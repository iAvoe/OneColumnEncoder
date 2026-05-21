# 增删 Checklist 需要修改的文件顺序（按依赖关系排列）：

### 1. `Models/UILangProviderM.cs`
为新的条目添加多语言字符串 key（en / zh-cn / zh-tw 三份）

### 2. `Models/ChecklistProviderM.cs`
在对应方法（如 `GetToolsChecklist()`）中添加或移除 `ChecklistItemDefinitionM`

### 3. `ViewModels/Cards/XxxCardVM.cs`
- 如果新增的是 picked 这类需要外部驱动的条目，添加对应常量下标（如 `UpstreamPickedChecklistIndex = 3`）和更新方法（如 `SetToolPickedStatus`）
- 如果只是静态检查项，通常无需改动（`FillCollection` 已自动从 `ChecklistProviderM` 填充）

### 4. `Commands/SelectToolCmd.cs`（仅 picked 类条目）
在 `Execute` 中，对应 Zone 的 `ResetSelection` 之后，调用 VM 的更新方法修改 checklist `Status`

### 5. `ViewModels/MainVM.cs`
- `UpdateEncodingStartButtonsState()` — 加入新的条件判断
- `InitializeChecklistEntryStates()` — 如果该条目需要根据配置启用/禁用
- `SubToToolsChecklist()` / `UnsubFromToolsChecklist()` — 如果新卡片的 checklist 需要监听状态变化
- `RefreshCardsLanguage()` — 如果新增了卡片 VM

### 6. `Views/MainUI.xaml`（仅新增卡片时）
加入新的 `<comps:ValidationCard>` 或 `<comps:ChecklistContainer>` 并绑定 DataContext

**Lang → Provider → VM → Cmd → MainVM → XAML**