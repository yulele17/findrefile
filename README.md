# findrefile · 重复文件查找工具

一个用 C# / WinForms 写的 Windows 小工具，用来扫描指定文件夹（含子目录），找出内容完全相同的重复文件，并安全地删除它们。

## 功能特性
- 基于 MD5 的内容比对，准确识别重复文件（不只同名/同大小）
- 大小预分组 + 64KB 快速指纹预筛，只对疑似重复的文件计算完整 MD5，扫描更快
- 多线程并行扫描，本地硬盘按 CPU 核数自动并发
- 后台扫描（BackgroundWorker），带进度显示，可随时「取消扫描」
- 勾选要删除的文件，默认丢进**回收站**（可一键切换为永久删除）
- 删除过程带实时进度条
- 无边框自定义界面，支持按文件大小排序、隔行变色

## 使用方法
1. 用 Visual Studio 2022 打开 `findrefile.sln`（目标框架 .NET Framework 4.8）。
2. 编译生成（Release / AnyCPU）。
3. 运行 `findrefile.exe`，选择要扫描的文件夹，点「扫描」。
4. 扫描完成后，勾选要清理的重复文件，点「删除选中」。

> 说明：删除默认进入回收站，可在工具栏勾选「永久删除」切换为直接删除。

## 目录结构
- `findrefile/` — 主项目（C# 源码）
  - `Form1.cs` / `Form1.Designer.cs` — 界面与交互
  - `DuplicateDetector.cs` — 重复文件检测核心逻辑
  - `HashFile.cs` — 文件哈希/大小模型
- `findrefile.sln` — 解决方案文件
- `generate_icon.py` — 程序图标生成脚本

## 技术要点
- 检测流程：枚举文件 → 按大小分组 → 64KB 头尾指纹快速预筛 → 仅对指纹相同的文件算完整 MD5 → 合并结果
- 安全删除：`Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile` 进回收站；`File.Delete` 永久删除
- 界面：无边框窗体 + 自定义标题栏，DataGridView 展示结果，支持列排序

## 编译
需要 .NET Framework 4.8 与 Visual Studio 2022「.NET 桌面开发」工作负载。
