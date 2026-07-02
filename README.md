# JpgVerifyTool - JPG 文件校验工具

A lightweight WinForms tool for verifying JPG files against a CSV record list. Built for production line use.

---

## 功能简介

在产线测试过程中，根据 CSV 文件中记录的 PID 号和步骤号，校验目标文件夹中对应的 JPG 测试截图是否存在，并统计每个 PID+Step 对应的图片数量。

---

## 功能特性

- ✅ 读取 CSV 文件中的 PID 和步骤号记录
- ✅ 精确匹配（不做模糊匹配，防止误匹配）
- ✅ 统计每个 PID+Step 匹配到的 JPG 文件数量
- ✅ 自动检测并提示 CSV 中的重复记录
- ✅ 支持递归扫描子文件夹（可选）
- ✅ 支持手动输入 UNC 网络路径（如 `\\server\share\folder`）
- ✅ 生成 Summary.csv（汇总报告）和 Details.csv（详细清单）
- ✅ 生成 run.log（本次运行元信息）
- ✅ 单文件 exe，无需安装，开箱即用

---

## 文件命名规则

工具根据以下命名规则匹配 JPG 文件：

```
ZH1_MUL201_20250920040558_PIDV_80471796U03N_80471796L03N.jpg
│    │         │           │        │
│    │         │           │        └─ PID + 模式(U/L) + 步骤号 + N
│    │         │           └─ 固定标识
│    │         └─ 时间戳（忽略）
│    └─ 设备编号（忽略）
└─ 站点（忽略）
```

**匹配规则：** 只匹配 PID 和步骤号，其他字段忽略。

| 字段 | 是否参与匹配 |
|---|---|
| PID | ✅ 精确匹配 |
| 步骤号 | ✅ 精确匹配 |
| 站点 | ❌ 忽略 |
| 设备编号 | ❌ 忽略 |
| 时间戳 | ❌ 忽略 |
| 模式 (U/L) | ❌ 忽略 |

---

## CSV 文件格式

```
PID,Step
80471796,3
12345678,5
99999999,1
```

- 第一行为表头（自动跳过）
- 第一列：PID（精确匹配，不支持模糊搜索）
- 第二列：步骤号
- 支持编码：UTF-8、UTF-8 BOM、GBK（中文 Windows 默认）

---

## 输出报告

每次运行在 `Log/` 文件夹下创建一个子文件夹：

```
Log/
└── {CSV文件名}_{yyyyMMdd_HHmmss}/
    ├── Summary.csv    ← 主报告（每条记录一行，含是否存在和数量）
    ├── Details.csv    ← 详细清单（每个匹配文件一行）
    └── run.log        ← 本次运行元信息
```

### Summary.csv 示例

```
PID,Step,Exists,Count
80471796,3,Y,2
12345678,5,Y,1
99999999,1,N,0
```

### Details.csv 示例

```
PID,Step,FileName
80471796,3,ZH1_MUL201_20250920040558_PIDV_80471796U03N_80471796L03N.jpg
80471796,3,ZH1_MUL201_20250920041022_PIDV_80471796U03N_80471796L03N.jpg
12345678,5,ZH1_MUL201_20250920050000_PIDV_12345678U05N_12345678L05N.jpg
99999999,1,(NOT FOUND)
```

---

## 使用方法

1. 双击运行 `JpgVerifyTool.exe`
2. 选择或输入 CSV 文件路径
3. 选择或输入 JPG 文件夹路径
4. 根据需要勾选"包含子文件夹"
5. 点击"开始校验"
6. 校验完成后点击"打开 Log 文件夹"查看报告

---

## 系统要求

| 项目 | 要求 |
|---|---|
| 操作系统 | Windows 10 / Windows 11 |
| .NET Framework | 4.7.2（Windows 10/11 自带，无需安装）|
| 内存 | 无特殊要求 |
| 磁盘空间 | 约 10MB |

---

## 项目结构

```
JpgVerifyTool/
├── Forms/
│   ├── MainForm.cs              # 主界面逻辑
│   └── MainForm.Designer.cs     # 界面布局
├── Core/
│   ├── ExcelReader.cs           # CSV 文件读取
│   ├── FileScanner.cs           # JPG 文件扫描
│   ├── Matcher.cs               # 匹配逻辑（正则）
│   └── ReportWriter.cs          # 报告生成
├── Models/
│   ├── PidRecord.cs             # CSV 记录数据类
│   └── MatchResult.cs           # 匹配结果数据类
├── AppConfig.cs                 # 统一配置管理
├── App.config                   # 外部配置文件（开发用）
├── Program.cs                   # 程序入口
└── README.md
```

---

## 开发环境

| 项目 | 说明 |
|---|---|
| 语言 | C# |
| 框架 | .NET Framework 4.7.2 |
| IDE | Visual Studio |
| UI | WinForms |
| NuGet 包 | Costura.Fody（单文件打包）|

---

## 配置说明

`AppConfig.cs` 中包含两种配置模式：

```csharp
// 开发调试时：false（从 App.config 读取）
// 发布时：    true（使用内嵌默认值，无需 .config 文件）
private const bool UseEmbeddedConfig = true;
```

可配置项包括：软件名称、版本号、开发者信息、Log 目录、CSV 列索引等。

---

## License

Internal use only. © Michael Xiang
```

---
