# OneColumnEncoder

一款基於 .NET 9/WPF 的次時代智慧影片編碼輔助工具。主要工作流程圍繞“導入工具和編碼器、導入影片或腳本源、分析源影片、訂製編碼命令、自訂平行計算策略、現代化編碼監控、中斷與封裝”展開。

<p align="center"><img src="WireframeMockups/logo.png" alt="Logo" width="200"></p>

## 功能概述

- **源驗證：**
    - 根據 ffprobe 讀數驗證源影片
    - 提供包含成功、警告和錯誤狀態的源檢查清單與強行繞過功能
- **高級平行處理：**在指定執行緒數量的基礎上，支持逐物理核心和 NUMA 節點綁定設置
- **編碼器參數配置：**
    - 提供方便的 UI 控制項來控制影片壓製參數，包括標籤頁菜單、滑條、下拉菜單等元素
    - 提供單圖壓制 A/B 對比預覽器，即時檢查參數配置對任意幀的影響
- **自動化與自訂：**
    - 自動編寫基本的 AviSynth、VapourSynth 腳本（影片解碼與管道輸出）
    - 為 FFMPEG-VS-AVS 生成 VFR→CFR 濾鏡、HDR→SDR+色彩空間轉換濾鏡、縮放濾鏡、SAR 修復濾鏡
    - 提供 FFMPEG-VS-AVS 濾鏡編輯器
    - 自動生成影片壓製參數，並生成音訊壓製命令
    - 提供可動態編輯的隊列模式
- **取段打樣：**基於 FFMPEG-VS-AVS 命令行實現的片段截取壓製
- **高級監控：**監控每個應用程式的記憶體使用情況，分別查看上游和下游（編碼器）日誌
- **中斷控制：**可中斷上游或下遊程序（編碼器）以一致地退出編碼會話
- **覆防寫：**基於待覆蓋檔案大小的冷卻時間防呆設計

## 軟體截圖

本軟體支持多語言，但為減少圖片數量而在此統一用了英語文本截圖。圖片中可能會有一些過時的 UI 元素或文本，但整體布局和功能區域劃分仍然適用。請以實際使用版本為準。

<p align="center"><img src="WireframeMockups/1-Main-Page.png" alt="Main Window" width="600"><br>
<img src="WireframeMockups/2-Script-Scribe.png" alt="Script Scribe Window" width="500"><br>
<img src="WireframeMockups/3-Encoder-Settings.webp" alt="Encoder Setting Window" width="900"><br>
<img src="WireframeMockups/4-Parallelism-Setting.png" alt="Parallelism Setting Window" width="400"><br>
<img src="WireframeMockups/5-Clip-Sampler.png" alt="Clip Sampler Window" width="400"><br>
<img src="WireframeMockups/6-Encoding-Monitor.png" alt="Encoding Monitor Window" width="650"><br>
<img src="WireframeMockups/7-Warning-Modal-OW-Guard.png" alt="Warning Modal & Overwrite Protection" width="350"><br></p>

## 運行要求

- Windows 10/11 x64
  - 推薦 1809 / 21H2（LTSC）或更高版本，最低 1607

### 下載編碼相關工具

你可以根據此教學來尋得合適的工具：
- [Encoding Tools Download Tutorial](https://github.com/iAvoe/encoding-tools-download-tutorial)

或者著急要用的話，也可以無視上述教學並使用這個壓縮包的工具（不推薦但可以）
- [Google Drive / ffmepg-ffprobe-x264-x265-SVTAV1 Pkg.](https://drive.google.com/file/d/1DNrVBoJtmYka0LiorjuWDgeGxfnq62PM/view?usp=sharing)
- [Baidu Netdisk / ffmpeg-ffprobe-x264-x265-SVTAV1 Pkg.](https://pan.baidu.com/s/1I-WsIBdr2syvHmkhdLGdHA?pwd=ya32)提取碼：ya32

> 最少只需一個上遊程序 + 一個下遊程序

**支持的管道上遊程序（解碼與濾鏡工具）**：
- ffmpeg
- vspipe（支持 API 3.0、4.0 自動識別）
- avs2yuv
- avs2pipemod
- SVFI

**支持的管道下遊程序（編碼器）**：
- x264 core 165 或更新
- x265 v4.2 或更新
- SVT-AV1 v4.1 或更新

> 使用最新版本的編碼器有助於提高性能（速度、畫質、壓縮率），以及更小的記憶體洩漏機率

## 圖示使用

- Azure icons: [azureicons.com](https://www.azureicons.com)
- 單色小圖示 by NiewBie: [GitHub/Niewbie](https://github.com/Nieobie/Game-Icon-Pack)

---

## 驗證狀態

**系統**：
- Windows 10 22H2
- Windows 11 25H2（感謝 [Lofu](https://github.com/Ronifue) 幫助驗證）

**硬體**：
- Core i5 7600k（4C4T）
- Ryzen 9 9900X（2CCD 12C24T）
- EPYC 7R13（6CCD 48C96T）
- Intel i7 14700K（感謝 Whithost 幫助驗證）

**高負載**
- 在無人值守的情況下完成了 30+ 4K 影片的長隊列壓製的測試成功

## 在地化狀態

- **支持範圍：** 英語、簡體、繁體
- **僅機器翻譯：** 法語、西班牙語、日語、俄語
- 如需提供翻譯，請 fork 此倉庫，在 `Models/XxxLangProviderM` 中添加新的語言條目，並提交 pull request。
  - README 的翻譯並非強制要求

---

## 打賞資訊

開發這些工具並不容易。如果這套工具提高了你的效率，那麼不妨贊助或推廣一下。

<p align="center"><img src="WireframeMockups/bmc_qr.png" alt="支持一下 -_-" width="300"><br>
<img src="WireframeMockups/pp_tip_qr.png" alt="支持一下 =_=" width="300"><br>
<img src="WireframeMockups/qr_ap.png" alt="支持一下 w_w" width="300"><br>
<img src="WireframeMockups/qr_wx.png" alt="支持一下 @_@" width="300"></p>

## 項目狀態

### 已完成

#### 應用框架與主界面

- WPF 應用啟動、主窗口和主界面布局已經實現，入口在 `App.xaml.cs`、`MainWindow.xaml`、`Views/MainUI.xaml`
- `MainVM` 負責主界面模組編排，包括工具區、源導入區、分析區、檢查卡、編碼設置區和啟動區
- 模態窗口導航、遮罩狀態、關閉命令和基礎命令模型已經實現
- 多語言切換機制已經接入主要界面、卡片、按鈕和模態窗文本

#### 工具導入與選擇

- 支持導入、替換、刪除和選擇外部工具
- 已定義並支持的上游工具包括 `ffmpeg.exe`、`vspipe.exe`、`avs2yuv.exe`、`avs2pipemod.exe`、`one_line_shot_args.exe`
- 已定義並支持的編碼器包括 `x264.exe`、`x265.exe`、`svtav1encapp.exe`
- 已定義並支持的分析和依賴項包括 `ffprobe.exe`、`avisynth.dll`
- 工具版本檢測、檔案名校驗、工具分區、預設選擇和依賴/來源相容性刷新已經實現

#### 源導入與腳本生成

- 支持導入普通影片源、AviSynth 腳本、VapourSynth 腳本和 SVFI ini 源
- 支持源文件路徑持久化，並在啟動時回填仍存在的源文件
- 一鍵生成 AviSynth / VapourSynth 腳本已經實現，會寫出 `.avs` 和 `.vpy` 文件並回填到腳本源卡片
- 源類型與上游工具之間的選擇同步已經實現
  - 例如 `vspipe.exe` 對應 `.vpy`，`avs2yuv.exe` / `avs2pipemod.exe` 對應 `.avs`

#### ffprobe 源分析與源檢查

- `ffprobe` JSON 分析已經實現，並可複製原始分析結果
- 源檢查卡已經解析並展示 progressive、位深、幀率、SAR、色彩元數據、chroma 等檢查項
- 源檢查問題查看、檢查清單狀態刷新和手動繞過已經接入主流程

#### 編碼前置檢查

- 編碼前置檢查卡已經實現硬體和軟體檢查項
- 已包含 AC 電源、輸出目錄、磁碟空間、寫權限、輸出文件覆蓋、AviSynth / L-SMASH 相關檢查
- 編碼前置問題查看、重新評估和手動繞過已經接入啟動按鈕狀態

#### 編碼參數配置

- x264 / x265 / SVT-AV1 的 CRF / ABR 基礎參數配置已經實現
- 編碼預設、關鍵幀間隔和部分第三方參數開關已經實現並持久化
- 編碼設置卡片會顯示當前編碼參數摘要
- 編碼管線會根據當前配置生成對應編碼器參數

#### 編碼命令生成與啟動

- Y4M 管線命令生成已經實現，支持多種上游工具輸出到 x264 / x265 / SVT-AV1
- 命令生成會結合 ffprobe 資訊自動補充部分幀數、色彩、range、chroma、lookahead 等參數
- 啟動編碼前會彈出命令確認窗口，確認後進入編碼監控窗口

#### 採樣片段

- 支持按時間或幀號選擇片段，並支持時間與幀號轉換
- 採樣片段會以 sample 模式打開編碼監控流程
- SVFI / OneLineShotArgs 當前不支持採樣片段，主界面已有禁用提示

#### 編碼監控與進程執行

- 支持啟動上游進程和編碼器進程，並將上游 stdout 管道傳給編碼器 stdin
- 支持讀取上遊程序與下遊程序（編碼器）stderr、日誌摺疊、保存日誌、查看編碼命令和調整日誌字號
- 支持編碼進度、已寫幀數、當前/預計輸出大小、耗時、剩餘時間和完成時間估算
- 支持記憶體占用、工作集峰值、Page Fault、記憶體壓力和記憶體範圍條統計
- 支持中斷上游或編碼器進程，編碼完成後才能關閉窗口

#### 並行基礎能力

- NUMA 節點枚舉、CPU 拓撲讀取、CPU Sets 分配和編碼器執行緒數限制已有實現
- 並行設置可以保存並應用到編碼監控啟動的上遊程序與下遊程序（編碼器）進程
- 編碼器執行緒數會傳給 x264 / x265 參數，SVT-AV1 當前沒有執行緒參數生成

#### 應用配置與持久化

- 應用配置、工具路徑、源路徑、編碼參數和並行參數的 JSON 保存/載入基礎邏輯已經實現
- 語言配置已經接入保存和載入

#### 輸出檔案名工具

- 支持路徑預覽、剪貼簿、非法字元、保留名和長度檢查
- 確認後會回填輸出設置卡片

#### UI 組件

- 卡片、按鈕組、下拉菜單、設置項容器、檢查清單、整數滑塊、片段範圍選擇器、記憶體範圍條和列文本組件已有實現
- 當前使用到的單向 UI 轉換器已經滿足現有綁定需求

#### 編碼參數配置細節

- `EncoderConfM.CustomParams` 會被保存，且已經被 `EncodingPipelineH` 拼入最終編碼命令
- "自訂參數"區域不再是第三方開關匯總，而是直接讀寫一個自由文本參數實現
- x264 / x265 / SVT-AV1 的參數覆蓋範圍仍有限，但已堪用

#### 腳本編輯器

- 腳本編輯器窗口、AVS / VPY 編輯區、複製完整腳本和複製輸入輸出片段
- "另存為文件"已實現
- "確認"已實現腳本保存和回填
  - 為簡化邏輯，實現和一鍵生成按鈕一樣會同時保存 AVS 和 VPY 腳本

#### 主界面 Best Practices 檢查卡

- `BestPracsSelfCheckCardVM` 是自查參考卡，不參與編碼啟動前的阻塞條件
- 無 `RunAllChecks()`、無 `IsBypassed`、無 Inspect/Bypass 按鈕
- 通過 `Subtitle` 屬性在 UI 上明確標識為僅供參考

#### 應用設定 → 文件覆蓋

文件覆蓋設置會在展示壓制命令並確認後，如果輸出文件已存在，則追加覆蓋確認彈出視窗，並按被覆蓋檔案大小延遲啟用確認按鈕

---

### 未驗證

#### 隊列模式

- 驗證 `ffmpeg`、`vspipe`、`avs2yuv`、`avs2pipemod` 隊列路線下的基本與帶濾鏡壓製行為

---

### 完全沒做

#### 暫無

---

### 死胡同

#### P-Core / E-Core 最佳化

由於需要修改上遊程序和編碼器原始碼，該功能無法實現
- UI 中 P-Core / E-Core 相關複選框處於禁用狀態
- 這本質上屬於 CISC 與 RISC 處理器廠商的職責，不是用戶或應用層應該做的事情

#### Large Pages 實現

由於需要修改上遊程序和編碼器原始碼，該功能無法實現

#### 自動生成關鍵幀 qpfile

由於行為過於複雜且耗時，實現失敗
- 必須使用比影片編碼器自帶方法更好的轉場檢測功能
- 比影片編碼器更好的轉場檢測功能更慢，會大幅增加整體壓制耗時
- 最終預期的壓縮率和畫質提升平均只有（不經過該功能壓制結果的）5%

---

### 主要原始碼位置

- `Commands/`：用戶操作命令、模態窗打開關閉、保存載入和編碼啟動入口
- `Helpers/`：編碼管線、ffprobe 分析、工具檢測、腳本模板、檔案名校驗、CPU / NUMA / 權限等輔助邏輯
- `Models/`：配置模型、工具定義、語言資源、檢查清單和數據 DTO
- `ViewModels/`：主界面、模態窗和卡片狀態管理
- `Views/`：WPF 窗口和界面 XAML
- `Components/`：復用 UI 控制項
- `Converters/`：WPF 綁定轉換器

#### 測試與工程化

- 當前沒有單元測試、集成測試或自動化 UI 測試項目
  - 許多方案還未完全支持 .Net 9.0 的項目
- README 中尚未包含構建、運行、依賴工具準備和典型工作流說明（不過已在用法說明窗口 / AppUsageModal 中提供）

---

## 確認窗口（ConfirmationModal）出現位置

- 啟動編碼前的命令確認，以及輸出文件覆蓋確認：`Commands/StartEncCmd.cs`
- 採樣片段啟動前的命令確認：`ViewModels/SampleClipVM.cs`
- 編碼監控裡的“查看編碼命令”：`ViewModels/EncodingMonitorVM.cs`
- 腳本生成後的複製/保存結果提示：`ViewModels/ScriptScribeVM.cs`、`Commands/SaveLoad/OneClickScriptGenCmd.cs`
- 源分析和檢查結果提示：`Commands/AnalyzeSrcVideoCmd.cs`、`Commands/CopyRawAnalysisCmd.cs`、`Commands/InspectEncProblemsCmd.cs`、`Commands/InspectSrcProblemsCmd.cs`
- 工具導入/文件選擇時的二次確認：`Commands/ImportToolCmd.cs`、`Helpers/SourceFilePickerH.cs`

## 設定資料儲存位置

所有持久化設定資料以 **JSON 檔案** 形式儲存在 `{應用程式基目錄}\1cenc\` 下：

| 檔案 | 內容 |
|------|------|
| `appconfig.json` | 應用設定（覆蓋保護設定、語言選擇） |
| `appdata.json` | 工具路徑/版本/大小、來源影片路徑、輸出目錄 |
| `encodingconf.json` | 編碼器參數（CRF/ABR、關鍵幀、預設、x264/x265/SVT-AV1 自訂參數） |
| `parallelismconfig.json` | 並行設定（NUMA 節點 ID、CPU 偏好、執行緒數） |

**持久化基類：**`Helpers\SaveLoadBaseH.cs` 的設定模型繼承自 `SaveLoadBaseH<T>`，透過 `Save()` / `Load()` 提供 JSON 序列化/反序列化。

**其他持久化資料（使用者選擇路徑，不在 `\1cenc\` 中）：**
- 生成的腳本檔案（`.avs` / `.vpy` / `.txt`）經由 `ViewModels\FilterScribeVM.cs` 和 `Commands\SaveLoad\OneClickScriptGenCmd.cs`
- stderr 日誌檔案（`upstream-stderr.txt`、`downstream-stderr.txt`）儲存到輸出目錄，經由 `ViewModels\EncodingMonitorVM.cs`

---

## 免責聲明

### 責任限制

因使用或無法使用本軟體所導致的任何直接、間接、附帶、特殊或後果性損害（包括但不限於商業利潤損失、業務中斷、電腦系統損壞、數據遺失及商譽損害），即使已被告知此類損害的可能性，開發者亦不承擔任何責任。使用者應自行承擔使用本軟體的一切風險。

### 關於硬體損壞的說明

影片壓製屬於長時間持續高負載 CPU 計算任務。在此場景下，包括但不限於以下情況可能對硬體造成損害：
- 散熱器安裝不當、超頻不穩定或電壓設置過高，可能導致處理器加速老化、電氣短路等硬體故障
- 極端負載可能導致系統無響應、藍屏當機，進而造成數據損壞或遺失

### 本軟體提供的保護措施

1. **x265 壓力測試預設**：提供用於驗證系統穩定性的 x265 壓力測試預設。但該測試的實際負載取決於輸入影片的內容複雜度，建議使用與目標壓製任務一致的測試影片作為源文件進行驗證。
    - 由於該測試的壓力可能會比 Prime95 等傳統壓力測試工具更加暴烈，因此運行該測試本身就存在一定風險，只建議在全程監控溫度和系統狀態的前提下運行，並且在開始測試前先做好數據備份。
2. **避免進程優先度控制**：本軟體不會提高編碼任務的進程優先度，以確保在編碼器無響應時，操作系統及其他程序仍能正常響應。
3. **文件覆蓋保護**：在編碼啟動前，如果輸出文件已存在，會彈出確認窗口並根據被覆蓋檔案大小延遲啟用確認按鈕，以避免誤操作導致的數據遺失。

### 建議用戶採取的保護措施

1. **使用可靠的散熱設備**：普通的散熱風扇難以承受長時間全速運行的損耗，建議選用品質可靠的方案。
2. **合理配置超頻策略**：應以長期高負載穩定運行為目標調整 CPU 與記憶體的超頻參數，而非僅追求短時爆發性能。
3. **使用不間斷電源（UPS）**：高負載任務運行時突然斷電對硬體極為危險，建議配備 UPS 以爭取手動保存並關機的緩衝時間。
4. **注意電腦所處環境的濕度**：高濕度環境容易造成電氣短路，尤其在高負載、長時間運行的情況下更需注意。

---

## 為什麼不做 Linux 支持 / 為什麼用 WPF

本軟體對 Windows 專有 API 依賴較深，這些 API 構成了核心功能的基礎，因此天然綁定在 Windows 平台上：

1. **WPF 呈現層**：WPF 無官方 Linux 支持
2. **Windows 內核 API（P/Invoke 調用 kernel32.dll）**
   - **CPU Sets**：將編碼進程綁定到指定物理核心，避開超執行緒虛擬核心
   - **NUMA 拓撲**：枚舉並指定編碼器使用的 NUMA 節點，使用戶看到的可視化與編碼器所見一致
   - **進程枚舉**：編碼監控中的子進程追蹤
   - **電源狀態**：編碼前檢查是否使用市電供電
   - **記憶體資訊**：獲取物理記憶體總量並按 NUMA 節點比例分配估算
3. **psapi.dll**：編碼監控功能的工作集/記憶體壓力統計

這些 API 覆蓋了並行調度、硬體檢測、進程監控、編碼前置檢查等關鍵環節，是跨平台 UI 框架無法替代的。

既然底層已經鎖死在 Windows API 上，選擇 WPF 是最自然的結果——它提供了原生 Windows 桌面集成（包括防止打開溢出畫面窗口的功能）、成熟的 MVVM 數據綁定生態，且無需引入瀏覽器內核或第三方依賴。跨平台框架解決的是 UI 層的移植問題，但無法解決底層 API 的相容，反而會增加一層抽象成本和測試負擔。

綜上所述，最適合其它平台的方案是在該平台上用對應原生技術棧重新實現本項目邏輯。由於已有完整原始碼和 Agent 輔助編程工具，同時為了降低門檻，本項目使用了 Apache 2.0 許可條款，因此重新開發的難度已然降低。
