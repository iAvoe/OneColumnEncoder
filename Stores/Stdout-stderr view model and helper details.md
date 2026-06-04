是的，利用命令行自身的覆盖机制（通过 `\r` 回车符）来防止进度行不断叠加，是保持 WPF 日志文本框干净、不爆内存的关键。

在 Windows 命令行（CMD）中，编码器（如 x264、x265、SVT-AV1）在更新进度时，输出的不是换行符（`\n`，`0x0A`），而是**回车符**（`\r`，`0x0D`）。它的作用是将光标移回当前行的行首，然后输出新内容覆盖旧内容。

但在 WPF 的 `TextBox` 或 `RichTextBox` 中，默认不支持这种光标复位覆盖的操作，它会把所有收到的字符一股脑往后拼接，导致进度行像盖楼一样不断垒高，瞬间产生数万行垃圾日志。

要在 WPF 中完美复现 CMD 的“原地刷新”效果，需要利用以下两种实现方案：

---

## 方案一：正则拦截过滤法（推荐：最省内存、UI最干净）

由于编码器的进度行（Progress Line）通常有非常明显的文本特征（例如包含 `fps`, `frame=`, `kb/s`, `%` 等关键字），我们可以在后台用正则表达式将其拦截，**不让它进入总日志缓存**，而是单独提取并更新到特定的“当前状态”标签中。

### 实现逻辑：

1. 在 `ErrorDataReceived` 回调中拿到整行文本。
2. 用正则表达式匹配该行是否属于“进度行”。
3. **如果是进度行**：直接解析出里面的关键数据（如 FPS、已耗时、剩余时间），将其赋值给界面底部的状态栏（即你 UI 图中最下方的 `已用时`、`预计剩余`、`编码器文件名` 等参数），然后**丢弃该行**。
4. **如果不是进度行**（属于核心配置信息或报错）：将其正常追加到多行文本框中。

```csharp
private void OnEncodingLogReceived(string rawLine)
{
    // 示例：判断是否包含 x264/x265 的进度特征
    if (rawLine.Contains("frames") && rawLine.Contains("fps"))
    {
        // 1. 解析进度数据，更新到界面下方的独立 TextBlock 中
        ParseAndUpdateBottomStatusBar(rawLine);
        return; // 2. 拦截，不让它进入多行日志文本框
    }

    // 普通日志或报错，正常追加
    AppendToLogTextBox(rawLine);
}

```

---

## 方案二：字符流手动解析法（1:1 还原 CMD 覆盖行为）

如果你希望在这 4 个多行文本框内**原地刷新**进度，就像真的 CMD 窗口一样，你需要自己处理 `\r` 字符。

由于 `Process.ErrorDataReceived` 是按行（以 `\n` 分隔）触发的，它可能会把带有 `\r` 的一段数据打包成一行扔给 C#。此时需要对字符串进行二次拆分：

### 实现逻辑：

1. 收到文本后，检查是否包含 `\r`。
2. 如果包含 `\r`，说明触发了复位覆盖。
3. 在更新 WPF 文本框时，执行“删除最后一行，再写入新行”的操作。

```csharp
private void AppendLogWithOverwrite(TextBox targetBox, string text)
{
    // 如果文本包含回车符 \r
    if (text.Contains("\r"))
    {
        var parts = text.Split('\r');
        
        // 最后一节是当前最新的进度内容
        string latestProgress = parts[^1]; 

        // 移除文本框的最后一行
        int lastLineIndex = targetBox.LineCount - 1;
        if (lastLineIndex >= 0)
        {
            int lineStartIndex = targetBox.GetCharacterIndexFromLineIndex(lastLineIndex);
            if (lineStartIndex >= 0)
            {
                // 将最后一行及其后面的内容全部截断
                targetBox.Text = targetBox.Text.Substring(0, lineStartIndex);
            }
        }
        
        // 追加最新的这一行进度
        targetBox.AppendText(latestProgress);
    }
    else
    {
        // 普通日志，直接换行追加
        targetBox.AppendText(text + Environment.NewLine);
    }
}

```

---

## 性能优化避坑指南

无论采用上述哪种方法，由于编码器刷新进度的频率极高（可能一秒触发几十次），**绝对不能直接频繁调用 `TextBox.AppendText()**`。

* **必须做定时合并（Throttling）**：建立一个后台线程安全队列（如 `ConcurrentQueue<string>`）。Log 事件只管往队列里扔（高吞吐、零阻塞）。
* **UI 定时器刷新**：用一个 `System.Windows.Threading.DispatcherTimer`，每隔 `100ms` 或 `200ms` 去队列里把这段时间积攒的日志一次性取出、合并、处理，然后刷新到 WPF UI 上。人的肉眼分不清 100ms 的延迟，这样可以降低 90% 以上的 UI 渲染开销。