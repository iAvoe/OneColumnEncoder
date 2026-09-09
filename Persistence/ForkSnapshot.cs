using OneColumnEncoder.Models.Analysis;
using System.IO;

namespace OneColumnEncoder.Persistence;

/// <summary>
/// One-shot state handoff used by Fork. It is intentionally separate from the
/// user-facing import/export format because it is only consumed by a child
/// process during startup.
/// </summary>
internal sealed class ForkSnapshot
{
    public const string CommandLineOption = "--fork-snapshot";

    public AppDataM AppData { get; set; } = new();
    public AppConfM AppConf { get; set; } = new();
    public List<ForkCardState> Cards { get; set; } = [];
    public string[] QueueSourcePaths { get; set; } = [];
    public string[] ConcatSourcePaths { get; set; } = [];
    public RepartPlanM? RepartPlan { get; set; }
    public VideoAnalysisM Analysis { get; set; } = new();
    public string FfmpegFilterArgs { get; set; } = string.Empty;
    public string RepartAvsFilterInput { get; set; } = string.Empty;
    public string RepartVpyFilterInput { get; set; } = string.Empty;
    public bool IsDurationFilterEnabled { get; set; }
    public int MinVideoDurationSeconds { get; set; } = 30;
    public int QueueIncludedCount { get; set; }
    public int QueueExcludedCount { get; set; }
    public string QueueJsonPath { get; set; } = string.Empty;
    public string ExcludedQueueJsonPath { get; set; } = string.Empty;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string Write(ForkSnapshot snapshot)
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"1cenc-fork-{Environment.ProcessId}-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, JsonOptions));
        return path;
    }

    public static ForkSnapshot? LoadFromCommandLine()
    {
        string[] args = Environment.GetCommandLineArgs();
        int optionIndex = Array.FindIndex(args, arg =>
            string.Equals(arg, CommandLineOption, StringComparison.OrdinalIgnoreCase));
        if (optionIndex < 0 || optionIndex + 1 >= args.Length) return null;

        string path = args[optionIndex + 1];
        try
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<ForkSnapshot>(File.ReadAllText(path))
                : null;
        }
        catch
        {
            return null;
        }
        finally
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }
    }

    public static Process StartChild(string snapshotPath)
    {
        string processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("The current process path is unavailable.");
        string[] commandLine = Environment.GetCommandLineArgs();
        ProcessStartInfo startInfo = new()
        {
            FileName = processPath,
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = true
        };

        // dotnet run hosts the app in dotnet.exe, so preserve the managed DLL
        // argument when developing instead of trying to execute dotnet itself.
        if (Path.GetFileNameWithoutExtension(processPath)
                .Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            string? assemblyPath = commandLine.Skip(1).FirstOrDefault(arg =>
                arg.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new InvalidOperationException("The managed application path is unavailable.");
            startInfo.ArgumentList.Add(assemblyPath);
        }

        startInfo.ArgumentList.Add(CommandLineOption);
        startInfo.ArgumentList.Add(snapshotPath);
        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("The forked process could not be started.");
    }

    public void ApplyTo(AppDataM appData, AppConfM appConf)
    {
        appData.Tools = AppData.Tools;
        appData.Encoding = AppData.Encoding;
        appData.MuxTracksBySource = AppData.MuxTracksBySource;
        appData.BrowseHistory = AppData.BrowseHistory;
        appData.IsMiniUpstreamsZone = AppData.IsMiniUpstreamsZone;
        appData.IsMiniEncodersZone = AppData.IsMiniEncodersZone;
        appData.IsMiniAnalyticsZone = AppData.IsMiniAnalyticsZone;
        appData.IsMiniDependenciesZone = AppData.IsMiniDependenciesZone;
        appData.IsMiniVideoSrcImportZone = AppData.IsMiniVideoSrcImportZone;
        appData.IsMiniScriptSrcImportZone = AppData.IsMiniScriptSrcImportZone;
        appData.IsMiniEncodingConfZone = AppData.IsMiniEncodingConfZone;
        appData.IsMiniSrcValidationCard = AppData.IsMiniSrcValidationCard;
        appData.IsMiniEncTermsCard = AppData.IsMiniEncTermsCard;
        appData.IsMiniBestPracticesCard = AppData.IsMiniBestPracticesCard;
        appData.IsMiniToolsImportCard = AppData.IsMiniToolsImportCard;
        appData.IsMiniStartEncodingZone = AppData.IsMiniStartEncodingZone;
        appData.IsDurationFilterEnabled = AppData.IsDurationFilterEnabled;
        appData.MinVideoDurationSeconds = AppData.MinVideoDurationSeconds;

        appConf.Reimport = AppConf.Reimport;
        appConf.InitLang = AppConf.InitLang;
        appConf.Overwrite = AppConf.Overwrite;
        appConf.Lang = AppConf.Lang;
        appConf.Font = AppConf.Font;
        appConf.Logs = AppConf.Logs;
        appConf.AudioMux = AppConf.AudioMux;
        appConf.AutoMux = AppConf.AutoMux;
        appConf.TextEditor = AppConf.TextEditor;
    }
}

internal sealed class ForkCardState
{
    public string Zone { get; set; } = string.Empty;
    public string? DefinitionKey { get; set; }
    public string P1TextData { get; set; } = string.Empty;
    public string? P1TooltipText { get; set; }
    public string P2TextData { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public bool IsEnabled { get; set; } = true;
}
