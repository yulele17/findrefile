using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace findrefile
{
    /// <summary>
    /// 重复文件探测核心逻辑（纯逻辑，不依赖 UI 与数据库）。
    /// 提速策略：
    ///   1. 先按“文件大小”预分组 —— 大小唯一的文件绝不可能是重复，直接排除；
    ///   2. 对“大小相同”的文件并行做 64KB 快速指纹，绝大多数是内容不同，直接筛掉，不必读全文；
    ///   3. 仅对“指纹相同”的文件计算完整 MD5，并多线程并行，充分利用多核（按文件粒度并行，负载更均衡）；
    ///   4. 完整/部分读取均使用顺序扫描提示(SequentialScan)+大缓冲，减少磁盘寻道；
    ///   5. 枚举阶段直接返回 FileInfo，省去一次 stat 系统调用。
    /// </summary>
    public class DuplicateDetector
    {
        private readonly Action<string> _status;
        private const int PartialChunk = 65536; // 64KB 快速指纹

        /// <summary>
        /// 最大并行度。0 = 自动（网络/可移动盘单线程，本地盘按 CPU 核数）。
        /// 机械硬盘上若发现变慢，可手动设为 2~4 以避免磁头来回寻道。
        /// </summary>
        public static int MaxConcurrency { get; set; } = 0;

        public DuplicateDetector(Action<string> status = null)
        {
            _status = status;
        }

        // 第二、三遍使用的候选条目
        private class Cand
        {
            public long Size;
            public string Path;
            public string Partial; // 64KB 指纹
            public string Full;    // 完整 MD5（小文件=Partial 直接定稿；大文件待算）
        }

        public List<HashFile> Scan(string root, Func<bool> isCancelled = null)
        {
            int concurrency = ResolveConcurrency(root);

            // 第一遍：收集 (大小, 路径)。直接拿 FileInfo，省一次 stat。
            var bySize = new Dictionary<long, List<string>>();
            int scanned = 0;
            foreach (var info in EnumerateFilesSafe(new DirectoryInfo(root)))
            {
                if (isCancelled != null && isCancelled()) break;
                try
                {
                    long size = info.Length;
                    if (!bySize.ContainsKey(size))
                        bySize[size] = new List<string>();
                    bySize[size].Add(info.FullName);
                }
                catch
                {
                    // 跳过无法读取的文件信息
                }
                finally
                {
                    scanned++;
                    _status?.Invoke(string.Format("已枚举 {0} 个文件…", scanned));
                }
            }

            // 仅保留“大小相同”的组，并扁平化为候选
            var dupGroups = bySize.Where(g => g.Value.Count >= 2).ToList();
            var cands = new List<Cand>();
            foreach (var g in dupGroups)
                foreach (var fp in g.Value)
                    cands.Add(new Cand { Size = g.Key, Path = fp });

            int total = cands.Count;
            int done = 0;

            // 第二遍：并行计算 64KB 指纹。小文件指纹即完整哈希，直接定稿。
            Parallel.ForEach(cands,
                new ParallelOptions { MaxDegreeOfParallelism = concurrency },
                c =>
                {
                    if (isCancelled != null && isCancelled()) return;
                    try
                    {
                        string ph = ComputePartial(c.Path);
                        c.Partial = ph;
                        if (c.Size <= PartialChunk) c.Full = ph; // 小文件完成
                    }
                    catch
                    {
                        // 跳过无法读取/被占用的文件
                    }
                    int d = System.Threading.Interlocked.Increment(ref done);
                    _status?.Invoke(string.Format("正在校验 {0}/{1}", d, total));
                });

            // 第三遍：仅对“指纹相同且为大文件”的子集算完整 MD5（按文件粒度并行，负载更均衡）
            var needFull = cands.Where(c => c.Full == null)
                                 .GroupBy(c => c.Size.ToString() + "|" + c.Partial)
                                 .Where(g => g.Count() >= 2)
                                 .ToList();
            Parallel.ForEach(needFull,
                new ParallelOptions { MaxDegreeOfParallelism = concurrency },
                g =>
                {
                    if (isCancelled != null && isCancelled()) return;
                    foreach (var c in g)
                    {
                        try { c.Full = ComputeMd5(c.Path); }
                        catch { }
                    }
                });

            // 汇总：只保留有完整哈希的（即真正可能为重复的文件）
            return cands
                .Where(c => c.Full != null)
                .Select(c => new HashFile { Path = c.Path, Size = c.Size, Hash = c.Full })
                .ToList();
        }

        private static int ResolveConcurrency(string root)
        {
            if (MaxConcurrency > 0) return MaxConcurrency;
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(root) ?? root);
                if (drive.DriveType == DriveType.Network || drive.DriveType == DriveType.Removable)
                    return 1;
            }
            catch { }
            return Environment.ProcessorCount;
        }

        public static string ComputeMd5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan))
            {
                return ToHex(md5.ComputeHash(stream));
            }
        }

        private static string ComputePartial(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan))
            {
                int chunk = (int)Math.Min(PartialChunk, stream.Length);
                byte[] buf = new byte[chunk];
                int read = stream.Read(buf, 0, chunk);
                return ToHex(md5.ComputeHash(buf, 0, read));
            }
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static IEnumerable<FileInfo> EnumerateFilesSafe(DirectoryInfo root)
        {
            if (root == null) yield break;
            IEnumerable<FileInfo> files = null;
            try { files = root.EnumerateFiles(); } catch { files = null; }
            if (files != null)
            {
                foreach (var f in files) yield return f;
            }
            IEnumerable<DirectoryInfo> dirs = null;
            try { dirs = root.EnumerateDirectories(); } catch { dirs = null; }
            if (dirs != null)
            {
                foreach (var d in dirs)
                {
                    foreach (var f in EnumerateFilesSafe(d))
                        yield return f;
                }
            }
        }
    }
}
