using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class ParallelismConfVM : BaseVM
    {
        private ParallelismConfLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);
        public ParallelismConfLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }

        private readonly ParallelismConfM _model;
        private readonly ToolItemCardVM _targetItem;

        public ObservableCollection<CPUNodeCardVM> UpstreamNodes { get; } = [];
        public ObservableCollection<CPUNodeCardVM> DownstreamNodes { get; } = [];

        public CloseModalCmd CloseCmd { get; }
        public ICommand ConfirmCmd { get; }
        public ICommand SelectUpstreamNodeCmd { get; }
        public ICommand SelectDownstreamNodeCmd { get; }
        public ButtonGroupVM FinishButtons { get; }

        private bool _preferUpstreamPhysicalCores;
        public bool PreferUpstreamPhysicalCores
        {
            get => _preferUpstreamPhysicalCores;
            set => SetProperty(ref _preferUpstreamPhysicalCores, value);
        }

        private bool _preferPhysicalCores = true;
        public bool PreferPhysicalCores
        {
            get => _preferPhysicalCores;
            set
            {
                if (!SetProperty(ref _preferPhysicalCores, value)) return;
                UpdateMaxThreadCount();
            }
        }

        private bool _preferPCoreCompute = true;
        public bool PreferPCoreCompute
        {
            get => _preferPCoreCompute;
            set => SetProperty(ref _preferPCoreCompute, value);
        }

        private bool _preferECoreLookahead = true;
        public bool PreferECoreLookahead
        {
            get => _preferECoreLookahead;
            set => SetProperty(ref _preferECoreLookahead, value);
        }

        private int _encoderThreadCount = Environment.ProcessorCount;
        public int EncoderThreadCount
        {
            get => _encoderThreadCount;
            set => SetProperty(ref _encoderThreadCount, ClampThreadCount(value, MaxThreadCount));
        }

        private int _maxThreadCount = Environment.ProcessorCount;
        public int MaxThreadCount
        {
            get => _maxThreadCount;
            private set
            {
                if (!SetProperty(ref _maxThreadCount, Math.Max(1, value))) return;
                ClampEncoderThreadCount();
                OnPropertyChanged(nameof(EncoderThreadTickLabels));
                OnPropertyChanged(nameof(EncoderThreadTickCount));
            }
        }

        private void ClampEncoderThreadCount()
        {
            EncoderThreadCount = ClampThreadCount(EncoderThreadCount, MaxThreadCount);
        }

        public static string WindowTitle => "1cenc Parallelism Settings";
        public string IntroText => Lang.IntroText;
        public string PriorityText => Lang.PriorityText;
        public string CacheGroupTitle => Lang.CacheGroupTitle;
        public static string CacheGroupHint => BuildCacheGroupHint();
        public string UpstreamNumaTitle => Lang.UpstreamNumaTitle;
        public string DownstreamNumaTitle => Lang.DownstreamNumaTitle;
        public string NumaGuidanceText => Lang.NumaGuidanceText;
        public string ThreadStrategyTitle => Lang.ThreadStrategyTitle;
        public string PreferUpstreamPhysCoresText => Lang.PreferUpstreamPhysCoresText;
        public string PreferDownstreamPhysCoresText => Lang.PreferDownstreamPhysCoresText;
        public string PreferPCoreComputeText => Lang.PreferPCoreComputeText;
        public string PreferECoreLookaheadText => Lang.PreferECoreLookaheadText;
        public string MemoryStrategyTitle => Lang.MemoryStrategyTitle;
        public string LargePagesUnavailableHintText => Lang.LargePagesUnavailableHintText;
        public string EncoderThreadCountText => Lang.EncoderThreadCountText;
        public List<string> EncoderThreadTickLabels => BuildThreadTickLabels();
        public int EncoderThreadTickCount => EncoderThreadTickLabels.Count;
        public string CancelButtonText => Lang.CancelButtonText;
        public string ConfirmButtonText => Lang.ConfirmButtonText;

        public ParallelismConfVM(Action closeAction, ToolItemCardVM targetItem)
        {
            _model = ParallelismConfM.LoadEffective();
            _targetItem = targetItem;
            Lang = new ParallelismConfLangProviderM(UILangProviderM.Current.LanguageCode);
            CloseCmd = new CloseModalCmd(closeAction);
            ConfirmCmd = new ActionCmd(_ =>
            {
                ApplySettingsToTarget();
                SaveModel();
                closeAction();
            });
            SelectUpstreamNodeCmd = new ActionCmd(p => SelectNode(UpstreamNodes, p as CPUNodeCardVM));
            SelectDownstreamNodeCmd = new ActionCmd(p =>
            {
                if (SelectNode(DownstreamNodes, p as CPUNodeCardVM))
                    UpdateMaxThreadCount();
            });
            FinishButtons = ButtonGroupVM.CreateTwoButton(CancelButtonText, ConfirmButtonText, CloseCmd, ConfirmCmd);

            BuildNodesFromTopology(UpstreamNodes);
            BuildNodesFromTopology(DownstreamNodes);
            LoadModelToUi();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private static void BuildNodesFromTopology(ObservableCollection<CPUNodeCardVM> nodes)
        {
            nodes.Clear();
            List<NumaNodeInfo> numaNodes = NumaTopologyH.GetNumaNodes();

            bool isFirst = true;
            foreach (NumaNodeInfo n in numaNodes)
            {
                nodes.Add(new CPUNodeCardVM
                {
                    NodeId = n.NodeId,
                    GroupId = n.Group,
                    MinThreadNum = n.MinThreadNum,
                    MaxThreadNum = n.MaxThreadNum,
                    HasMemGB = n.HasMemGB,
                    IsEnabled = true,
                    IsSelected = isFirst
                });
                isFirst = false;
            }
            // UI designed for 4 node cards, lacking is ugly
            while (nodes.Count < 4)
            {
                nodes.Add(new CPUNodeCardVM
                {
                    IsEnabled = false,
                    IsSelected = false
                });
            }
        }

        private static bool SelectNode(ObservableCollection<CPUNodeCardVM> zone, CPUNodeCardVM? targetNode)
        {
            if (targetNode is not { IsEnabled: true }) return false;
            foreach (CPUNodeCardVM node in zone)
                node.IsSelected = node == targetNode;
            return true;
        }

        private void ApplySettingsToTarget()
        {
            _targetItem.P2TextData = BuildSecondarySummary();
            _targetItem.P1TextData = BuildPrimarySummary();
        }

        private void SaveModel()
        {
            CPUNodeCardVM upstream = GetSelectedEnabledNode(UpstreamNodes);
            CPUNodeCardVM downstream = GetSelectedEnabledNode(DownstreamNodes);
            _model.UpstreamNodeId = upstream.NodeId;
            _model.DownstreamNodeId = downstream.NodeId;
            _model.PreferUpstreamPhysicalCores = PreferUpstreamPhysicalCores;
            _model.PreferPhysicalCores = PreferPhysicalCores;
            _model.PreferPCoreCompute = PreferPCoreCompute;
            _model.PreferECoreLookahead = PreferECoreLookahead;
            _model.EncoderThreadCount = EncoderThreadCount;
            _model.Save();
        }

        private void LoadModelToUi()
        {
            PreferUpstreamPhysicalCores = _model.PreferUpstreamPhysicalCores;
            PreferPhysicalCores = _model.PreferPhysicalCores;
            PreferPCoreCompute = _model.PreferPCoreCompute;
            PreferECoreLookahead = _model.PreferECoreLookahead;

            SelectById(UpstreamNodes, _model.UpstreamNodeId);
            SelectById(DownstreamNodes, _model.DownstreamNodeId);
            UpdateMaxThreadCount();
            EncoderThreadCount = _model.EncoderThreadCount;
        }

        private void UpdateMaxThreadCount()
        {
            CPUNodeCardVM? downstream = DownstreamNodes.FirstOrDefault(n => n.IsSelected && n.IsEnabled)
                ?? DownstreamNodes.FirstOrDefault(n => n.IsEnabled);

            MaxThreadCount = downstream == null
                ? Math.Max(1, Environment.ProcessorCount)
                : CpuSetsH.GetNodeProcessorCapacity(downstream.NodeId, PreferPhysicalCores);
        }

        private static void SelectById(ObservableCollection<CPUNodeCardVM> zone, int nodeId)
        {
            CPUNodeCardVM? target = zone.FirstOrDefault(n => n.NodeId == nodeId && n.IsEnabled);
            if (target is null) return;
            foreach (CPUNodeCardVM node in zone)
                node.IsSelected = node == target;
        }

        private string BuildPrimarySummary()
        {
            CPUNodeCardVM upstream = GetSelectedEnabledNode(UpstreamNodes);
            CPUNodeCardVM downstream = GetSelectedEnabledNode(DownstreamNodes);
            return $"{upstream.NodeId},{upstream.GroupId} → {downstream.NodeId},{downstream.GroupId}";
        }

        private string BuildSecondarySummary()
        {
            return BuildSecondarySummary(PreferUpstreamPhysicalCores, PreferPhysicalCores, EncoderThreadCount);
        }

        private static string BuildSecondarySummary(bool preferUpstreamPhysicalCores, bool preferPhysicalCores, int encoderThreadCount)
        {
            string upstreamIndicator = preferUpstreamPhysicalCores ? "U-Phys ON" : "U-Phys OFF";
            string encoderIndicator = preferPhysicalCores ? "E-Phys ON" : "E-Phys OFF";
            return $"{encoderThreadCount} ({upstreamIndicator}, {encoderIndicator})";
        }

        private List<string> BuildThreadTickLabels()
        {
            int max = Math.Max(1, MaxThreadCount);
            if (max <= 8)
                return Enumerable.Range(1, max).Select(v => v.ToString()).ToList();

            int step = GetThreadTickStep(max);
            List<int> values = [1];
            for (int value = step; value < max; value += step)
                values.Add(value);

            if (values[^1] != max) values.Add(max);
            return values.Select(v => v.ToString()).ToList();
        }

        private static int GetThreadTickStep(int maxThreadCount)
        {
            return maxThreadCount switch
            {
                <= 16 => 2,
                <= 32 => 4,
                <= 64 => 8,
                <= 128 => 16,
                _ => 32
            };
        }

        private static string BuildCacheGroupHint()
        {
            ParallelismConfLangProviderM lang = new(UILangProviderM.Current.LanguageCode);
            CpuTopologyH.CacheGroupInfo? cacheTopology = CpuTopologyH.GetCacheTopology();
            if (cacheTopology == null)
            {
                List<NumaNodeInfo> nodes = NumaTopologyH.GetNumaNodes();
                if (nodes.Count == 0) return string.Empty;

                int coresPerGroup = nodes.Count == 1
                    ? nodes[0].ProcessorCount
                    : nodes.Average(n => n.ProcessorCount) >= 8
                        ? 8
                        : Math.Max(1, nodes.Min(n => n.ProcessorCount));

                long cacheMbPerGroup = 32;

                return $"{lang["CorePerGroup"]}{coresPerGroup}{lang["CorePerGroup1"]}{cacheMbPerGroup}{lang["CorePerGroup2"]}";
            }

            return $"{lang["CorePerGroup"]}{cacheTopology.CoresPerGroup}{lang["CorePerGroup1alt"]}{cacheTopology.ThreadsPerGroup}{lang["CorePerGroup1alt1"]}{cacheTopology.CacheMbPerGroup}{lang["CorePerGroup2"]}";
        }

        private static int ClampThreadCount(int threadCount, int maxThreadCount)
        {
            return Math.Max(1, Math.Min(Math.Max(1, maxThreadCount), threadCount));
        }

        private static CPUNodeCardVM GetSelectedEnabledNode(ObservableCollection<CPUNodeCardVM> zone)
        {
            return zone.FirstOrDefault(n => n.IsSelected && n.IsEnabled)
                ?? zone.First(n => n.IsEnabled);
        }

        private void OnLanguageChanged()
        {
            Lang = new ParallelismConfLangProviderM(UILangProviderM.Current.LanguageCode);
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(IntroText));
            OnPropertyChanged(nameof(PriorityText));
            OnPropertyChanged(nameof(CacheGroupTitle));
            OnPropertyChanged(nameof(CacheGroupHint));
            OnPropertyChanged(nameof(UpstreamNumaTitle));
            OnPropertyChanged(nameof(DownstreamNumaTitle));
            OnPropertyChanged(nameof(NumaGuidanceText));
            OnPropertyChanged(nameof(ThreadStrategyTitle));
            OnPropertyChanged(nameof(PreferUpstreamPhysCoresText));
            OnPropertyChanged(nameof(PreferDownstreamPhysCoresText));
            OnPropertyChanged(nameof(PreferPCoreComputeText));
            OnPropertyChanged(nameof(PreferECoreLookaheadText));
            OnPropertyChanged(nameof(MemoryStrategyTitle));
            OnPropertyChanged(nameof(LargePagesUnavailableHintText));
            OnPropertyChanged(nameof(EncoderThreadCountText));
            OnPropertyChanged(nameof(EncoderThreadTickLabels));
            OnPropertyChanged(nameof(EncoderThreadTickCount));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(ConfirmButtonText));

            FinishButtons.B2_1Text = CancelButtonText;
            FinishButtons.B2_2Text = ConfirmButtonText;
        }

        public static void ApplySavedSettingsToCard(ToolItemCardVM targetItem)
        {
            ParallelismConfM model = ParallelismConfM.LoadEffective();
            List<NumaNodeInfo> numaNodes = NumaTopologyH.GetNumaNodes();
            int encoderThreadCount = CpuSetsH.ClampThreadCountForNode(
                model.DownstreamNodeId,
                model.PreferPhysicalCores,
                model.EncoderThreadCount);

            NumaNodeInfo? upstream =
                numaNodes.FirstOrDefault(n => n.NodeId == model.UpstreamNodeId)
                ?? numaNodes.FirstOrDefault();
            NumaNodeInfo? downstream =
                numaNodes.FirstOrDefault(n => n.NodeId == model.DownstreamNodeId)
                ?? (numaNodes.Count > 1 ? numaNodes[1] : upstream);

            if (upstream is null || downstream is null)
            {
                targetItem.P2TextData = BuildSecondarySummary(model.PreferUpstreamPhysicalCores, model.PreferPhysicalCores, encoderThreadCount);
                targetItem.P1TextData = "0,0 → 0,0";
                return;
            }

            string primary = $"{upstream.NodeId},{upstream.Group} → {downstream.NodeId},{downstream.Group}";

            string secondary = BuildSecondarySummary(model.PreferUpstreamPhysicalCores, model.PreferPhysicalCores, encoderThreadCount);

            targetItem.P2TextData = secondary;
            targetItem.P1TextData = primary;
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
