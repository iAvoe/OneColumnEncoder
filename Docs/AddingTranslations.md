# Adding a New Language

## Overview

This guide covers adding a new language (using **Portuguese Brazilian - pt-BR** as the example) to the translation system.

## Prerequisites

- C# and .NET knowledge
- Access to all provider files in `OneColumnEncoder\Models\Lang\`
- All keys translated for the target language (required for debug mode)

## Step-by-Step Implementation (`["pt-br"]` as example)

### 1. Language File Structure

The translation system consists of:

- **22 language provider files** — one per UI module
- **Common terms** — shared translations in `LangProviderBase.cs`
- **Language registration** — managed in `UICaptionProvider.cs`
- **Runtime detection** — handled in `App.xaml.cs`

### 2. Add Common Terms (Required First Step)

**File:** `LangProviderBase.cs`

Add Portuguese to the `CommonData` dictionary:

```csharp
["pt-br"] = new()
{
    ["Confirm"] = "Confirmar",
    ["Cancel"] = "Cancelar",
    ["Add"] = "Adicionar",
    ["Delete"] = "Excluir",
    ["Edit"] = "Editar",
    ["Save"] = "Salvar",
    ["Close"] = "Fechar",
    ["Yes"] = "Sim",
    ["No"] = "Não",
    ["Ok"] = "OK",
    ["Apply"] = "Aplicar",
    ["Reset"] = "Redefinir",
    ["Remove"] = "Remover",
    ["Select"] = "Selecionar",
    ["Clear"] = "Limpar",
    ["Default"] = "Padrão"
}
```

**Note:** These 20 common terms are shared across ALL providers and will be used as fallback translations.

### 3. Translate All Provider Files

Add `["pt-br"]` dictionary entries to **ALL 22 provider files**:

| Provider File | Number of Keys | Notes |
|--------------|----------------|-------|
| `UILangProvider.cs` | ~100 | Main UI - Most important |
| `AppConfLangProvider.cs` | 31 | Settings |
| `AppUsageLangProvider.cs` | 38 | Usage info |
| `AnalyzeSrcVideoCmdLangProvider.cs` | 9 | Analysis commands |
| `ClipRangeSelectorLangProvider.cs` | 22 | Clip selection |
| `ConfirmDialogLangProvider.cs` | 7 | Dialog boxes |
| `CpuSetsLangProvider.cs` | 6 | CPU configuration |
| `EncoderConfLangProvider.cs` | 42 | Encoder settings |
| `EncodingMonitorModalLangProvider.cs` | 65 | Monitoring |
| `FFProbeVideoAnalysisLangProvider.cs` | 5 | Video analysis |
| `FilenameScribeModalLangProvider.cs` | 23 | Filename generation |
| `FilterScribeModalLangProvider.cs` | 68 | Filter configuration |
| `ImgABPvLangProvider.cs` | 38 | Image comparison |
| `ParallelismConfLangProvider.cs` | 23 | Parallel processing |
| `QueueEditorLangProvider.cs` | 1 | Queue editing |
| `QueueSidebarLangProvider.cs` | 5 | Queue sidebar |
| `RepartLangProvider.cs` | 80 | Repartition management |
| `SrcFilePickerLangProvider.cs` | 6 | File selection |
| `SrcReviserLangProvider.cs` | 11 | Source revision |
| `StartEncCmdLangProvider.cs` | 21 | Encoding controls |
| `VideoSrcQueueLangProvider.cs` | 12 | Video queue |
| **TOTAL** | **~574 keys** | **mandatory 100% coverage** |

**Example Translation Pattern:**
```csharp
["pt-br"] = new()
{
    ["KeyName"] = "Translation in Portuguese",
    // ... all keys for this provider
}
```

### 4. Register the Language Code

**File:** `UICaptionProvider.cs`

Add the language code to the `Codes` array:

```csharp
public static readonly string[] Codes =
    ["en", "zh-cn", "zh-tw", "fr", "es", "ja", "ru", "de", "ko", "pt-br"];
```

### 5. Update Language Auto-Detection (Optional)

**File:** `App.xaml.cs`

Add culture mapping for auto-detection:

```csharp
private static string ResolveSupportedLanguageCode(string cultureName)
{
    return cultureName switch
    {
        "pt-br" or "pt" => "pt-br",
        // ... existing mappings
        _ => "en"
    };
}
```

---

## Important Guidelines

### Window Titles
- **Never** translate window titles — keep them as hardcoded English constants.

### Debug Mode
- `MissingTranslationException` is thrown in DEBUG mode for missing keys.
- **100% coverage is mandatory** for all translation files.

### Source Language
- English (`en`) is the baseline. All translations must be complete compared to English.

### Language Codes
- **lowercase** codes only. Include regional variant where applicable (e.g., `pt-br` not `pt`).

## Testing Translation

1. **Set language in debug mode:** `UILangProvider.SetLanguage("pt-br");`
2. **Verify no exceptions are thrown** (especially `MissingTranslationException`)
3. **Check all UI elements** display properly in Portuguese
4. **Test language switching** to ensure no hardcoded text remains

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Missing translation exception | See which key was missing on the Exception Window, and add it |
| Language not appearing in dropdown | Check `Codes` array in `UICaptionProvider.cs` |
| Some UI elements still in English | Check for hardcoded strings or missing provider files |
| Display name shows "pt-br (!Localized)" | Update `GetDisplayName` method in `UICaptionProvider.cs` |
