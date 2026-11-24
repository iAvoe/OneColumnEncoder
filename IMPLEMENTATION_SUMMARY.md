# 文件锁功能实现总结

## 概述
在 OneColumnEncoder 的设置中添加了一个新选项："编码时锁定源文件"，用于在压制过程中对导入的视频源文件应用写入锁保护，防止用户意外删除、移动或修改正在被编码的源文件。

## 功能特性

### 1. 锁定类型
- **写入锁（读共享）**：使用 `FileAccess.Read` + `FileShare.Read`
- 允许其他进程读取文件（包括分身功能）
- 禁止写入、删除、移动操作

### 2. 默认行为
- 默认值：`false`（禁用）
- 用户需要主动在设置中启用此功能

### 3. 文件系统支持
- **仅支持本地驱动器**（`DriveType.Fixed`）
- 自动跳过网络驱动器（`DriveType.Network`）
- 自动跳过未知类型驱动器（`DriveType.Unknown`）

### 4. 锁定范围
锁定以下文件：
- 主视频源文件（`UpstreamInputPath`）
- 原始视频源文件（`SourceVideoPath`）
- Concat/Repart 模式的所有源视频文件（`ConcatVideoSourcePaths`）

### 5. 模式支持

#### 单文件模式（Single）
- 编码开始时锁定源文件
- 编码结束或中断时解锁

#### 队列模式（Queue）
- 队列开始时一次性锁定所有任务的源文件
- 每个任务完成后逐个解锁该任务的源文件
- 队列全部完成或中断时解锁所有剩余文件

#### Concat/Repart 模式
- 同队列模式处理

### 6. 异常处理
- **用户中断/停止**：通过 `EnableCloseButton()` 中的 `ReleaseFileLocks()` 解锁
- **程序崩溃**：系统文件锁自动释放（依赖 `FileStream.Dispose()`）
- **权限不足**：静默失败，记录为"已禁用"

### 7. 用户反馈
在 EncodingMonitorModal 的运行日志中打印状态消息：
- `文件锁：已启用` - 成功锁定至少一个文件
- `文件锁：已禁用（设置关闭或文件已被锁定）` - 未锁定任何文件
- `文件锁：不可用（网络驱动器或不支持的文件系统）` - 所有文件都在网络驱动器上

消息在编码启动前打印，避免日志行错乱。

## 实现文件

### 1. 核心类
**FileManagement/FileLockManager.cs** - 文件锁管理器
- `TryLockFile(string)` - 尝试锁定单个文件
- `UnlockFile(string)` - 解锁单个文件
- `UnlockAll()` - 解锁所有文件
- `IsLocalDrive(string)` - 检查是否为本地驱动器
- 实现 `IDisposable` 确保资源释放

### 2. 配置模型
**Models/AppConfM.cs**
- 添加 `EncodingSettings` 类
- 属性：`LockSourceFilesWhileEncoding` (bool, 默认 false)

### 3. 设置定义
**Models/SettinglistProviderM.cs**
- 添加 `GetEncodingSettings()` 方法
- 在 `GetAllSettings()` 中引入新设置组

### 4. UI 本地化
**Models/Lang/UICaptionProvider.cs**
- 添加 `Groups.Encoding` 属性

**Models/Lang/AppConfLangProvider.cs**
- 添加设置组名称和设置项标签（英文、简体中文、繁体中文）
- `AppConf.Encoding` - "编码" / "編碼" / "Encoding"
- `Setting.Encoding.LockSourceFiles` - 设置描述

**Models/Lang/EncodingMonitorModalLangProvider.cs**
- 添加状态消息（英文、简体中文、繁体中文）
- `FileLockEnabled` - "文件锁：已启用"
- `FileLockDisabled` - "文件锁：已禁用"
- `FileLockUnavailable` - "文件锁：不可用"

### 5. ViewModel 集成
**ViewModels/AppConfVM.cs**
- 在 `BuildSettingsListing()` 的 `sourceMap` 中添加 `Encoding` 映射

**ViewModels/EncodingMonitorVM.cs**
- 添加字段：`_fileLockManager`
- 添加方法：
  - `TryApplyFileLocks()` - 单文件模式锁定
  - `TryApplyFileLocksForQueue()` - 队列模式一次性锁定
  - `ReleaseFileLocks()` - 释放所有锁
  - `CollectSourceFilePaths()` - 收集当前任务的源文件路径
  - `IsLocalDrive(string)` - 检查是否为本地驱动器
- 修改方法：
  - `RunEncodingAsync()` - 单文件模式在开始时锁定
  - `RunQueueEncodingAsync()` - 队列模式在开始时锁定所有文件
  - 队列循环中每个任务完成后解锁该任务的文件
  - `EnableCloseButton()` - 添加解锁调用

## 技术细节

### 文件锁实现
```csharp
var stream = new FileStream(
    filePath,
    FileMode.Open,
    FileAccess.Read,      // 读访问
    FileShare.Read);      // 允许其他进程读取
```

### 驱动器类型检测
```csharp
DriveInfo drive = new(Path.GetPathRoot(filePath));
return drive.DriveType != DriveType.Network 
    && drive.DriveType != DriveType.Unknown;
```

### 自动资源释放
- `FileLockManager` 实现 `IDisposable`
- 在 `EnableCloseButton()` 中调用 `ReleaseFileLocks()`
- 程序崩溃时依赖 CLR 的终结器自动释放

## 测试场景

### 应该测试的场景
1. ✅ 单文件编码 - 锁定和解锁
2. ✅ 队列模式 - 全锁定、逐个解锁
3. ✅ 用户中断编码 - 确保解锁
4. ✅ 网络驱动器 - 跳过锁定
5. ✅ 设置关闭 - 不锁定
6. ✅ 文件已被锁定 - 静默失败
7. ✅ Concat/Repart 模式 - 多文件锁定

### 边界情况
- 文件不存在 - 跳过
- 权限不足 - 静默失败
- 多个任务使用同一源文件 - HashSet 去重
- 程序崩溃 - 系统自动释放锁

## 兼容性
- 完全向后兼容（默认禁用）
- 不影响现有功能
- 读操作不受限制（支持分身功能）

## 注意事项
1. 文件锁仅在本地驱动器上有效
2. 锁定失败不会阻止编码继续（静默失败设计）
3. 用户可以通过任务管理器强制关闭程序来释放锁
4. 文件锁不能防止管理员权限的操作
