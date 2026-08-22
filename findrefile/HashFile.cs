using System;

namespace findrefile
{
    /// <summary>
    /// 表示一个被扫描到的文件及其 MD5 指纹与大小。
    /// </summary>
    public class HashFile
    {
        /// <summary>文件完整路径</summary>
        public string Path { get; set; }

        /// <summary>MD5 十六进制字符串（小写）</summary>
        public string Hash { get; set; }

        /// <summary>文件字节大小</summary>
        public long Size { get; set; }
    }
}
