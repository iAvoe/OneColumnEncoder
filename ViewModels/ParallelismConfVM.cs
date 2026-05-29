using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private bool _preferPhysicalCores = true;
        public bool PreferPhysicalCores
        {
            get => _preferPhysicalCores;
            set => SetProperty(ref _preferPhysicalCores, value);
        }

        private bool _preferPerformanceCores = true;
        public bool PreferPerformanceCores
        {
            get => _preferPerformanceCores;
            set => SetProperty(ref _preferPerformanceCores, value);
        }

        private bool _useLargePages = true;
        public bool UseLargePages
        {
            get => _useLargePages;
            set => SetProperty(ref _useLargePages, value);
        }

        private int _encoderThreadCount = Environment.ProcessorCount;
        public int EncoderThreadCount
        {
            get => _encoderThreadCount;
            set => SetProperty(ref _encoderThreadCount, value);
        }

        public int MaxThreadCount { get; private set; } = Environment.ProcessorCount;

        public string WindowTitle => Lang.WindowTitle;
        public string IntroText => Lang.IntroText;
        public string PriorityText => Lang.PriorityText;
        public string CacheGroupTitle => Lang.CacheGroupTitle;
        public static string CacheGroupHint => BuildCacheGroupHint();
        public string UpstreamNumaTitle => Lang.UpstreamNumaTitle;
        public string DownstreamNumaTitle => Lang.DownstreamNumaTitle;
        public string NumaGuidanceText => Lang.NumaGuidanceText;
        public string ThreadStrategyTitle => Lang.ThreadStrategyTitle;
        public string PreferPhysicalCoresText => Lang.PreferPhysicalCoresText;
        public string PreferPerformanceCoresText => Lang.PreferPerformanceCoresText;
        public string MemoryStrategyTitle => Lang.MemoryStrategyTitle;
        public string UseLargePagesText => Lang.UseLargePagesText;
        public string EncoderThreadCountText => Lang.EncoderThreadCountText;
        public IEnumerable<string> EncoderThreadTickLabels => BuildThreadTickLabels();
        public string CancelButtonText => Lang.CancelButtonText;
        public string ConfirmButtonText => Lang.ConfirmButtonText;

        public ParallelismConfVM(Action closeAction, ToolItemCardVM targetItem)
        {
            _model = ParallelismConfM.Load();
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
            SelectDownstreamNodeCmd = new ActionCmd(p => SelectNode(DownstreamNodes, p as CPUNodeCardVM));
            FinishButtons = ButtonGroupVM.CreateTwoButton(CancelButtonText, ConfirmButtonText, CloseCmd, ConfirmCmd);

            BuildNodesFromTopology(UpstreamNodes);
            BuildNodesFromTopology(DownstreamNodes);
            LoadModelToUi();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private static void BuildNodesFromTopology(ObservableCollection<CPUNodeCardVM> nodes)
        {
            nodes.Clear();
            var numaNodes = NumaTopologyH.GetNumaNodes();

            bool isFirst = true;
            foreach (var numaNode in numaNodes)
            {
                nodes.Add(new CPUNodeCardVM
                {
                    NodeId = numaNode.NodeId,
                    GroupId = numaNode.Group,
                    MinThreadNum = numaNode.MinThreadNum,
                    MaxThreadNum = numaNode.MaxThreadNum,
                    HasMemGB = numaNode.HasMemGB,
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

        private static void SelectNode(ObservableCollection<CPUNodeCardVM> zone, CPUNodeCardVM? targetNode)
        {
            if (targetNode is not { IsEnabled: true }) return;
            foreach (CPUNodeCardVM node in zone)
                node.IsSelected = node == targetNode;
        }

        private void ApplySettingsToTarget()
        {
            _targetItem.P2TextData = BuildSecondarySummary();
            _targetItem.P1TextData = BuildPrimarySummary();
        }

        private void SaveModel()
        {
            CPUNodeCardVM upstream = UpstreamNodes.First(n => n.IsSelected);
            CPUNodeCardVM downstream = DownstreamNodes.First(n => n.IsSelected);
            _model.UpstreamNodeId = upstream.NodeId;
            _model.DownstreamNodeId = downstream.NodeId;
            _model.PreferPhysicalCores = PreferPhysicalCores;
            _model.PreferPerformanceCores = PreferPerformanceCores;
            _model.UseLargePages = UseLargePages;
            _model.EncoderThreadCount = EncoderThreadCount;
            _model.Save();
        }

        private void LoadModelToUi()
        {
            PreferPhysicalCores = _model.PreferPhysicalCores;
            PreferPerformanceCores = _model.PreferPerformanceCores;
            UseLargePages = _model.UseLargePages;

            var numaNodes = NumaTopologyH.GetNumaNodes();
            MaxThreadCount = numaNodes.Count > 0 ? numaNodes.Sum(n => n.ProcessorCount) : Environment.ProcessorCount;
            EncoderThreadCount = Math.Max(1, Math.Min(MaxThreadCount, _model.EncoderThreadCount));

            SelectById(UpstreamNodes, _model.UpstreamNodeId);
            SelectById(DownstreamNodes, _model.DownstreamNodeId);
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
            CPUNodeCardVM upstream = UpstreamNodes.First(n => n.IsSelected);
            CPUNodeCardVM downstream = DownstreamNodes.First(n => n.IsSelected);
            return $"{upstream.NodeId},{upstream.GroupId} → {downstream.NodeId},{downstream.GroupId}";
        }

        private string BuildSecondarySummary()
        {
            string clampIndicator = PreferPhysicalCores
                ? UILangProviderM.Current["ToolField.EncThreadClampOn"]
                : UILangProviderM.Current["ToolField.EncThreadClampOff"];
            return $"{EncoderThreadCount} {clampIndicator}";
        }

        private IEnumerable<string> BuildThreadTickLabels()
        {
            int max = MaxThreadCount;
            int tickCount = 8;
            var labels = new List<string>();
            for (int i = 0; i < tickCount; i++)
            {
                int val = 1 + i * (max - 1) / (tickCount - 1);
                labels.Add(val.ToString());
            }
            return labels;
        }

        private static string BuildCacheGroupHint()
        {
            var lang = ParallelismConfLangProviderM.Current;
            var cacheTopology = CpuTopologyH.GetCacheTopology();
            if (cacheTopology == null)
            {
                var nodes = NumaTopologyH.GetNumaNodes();
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
            OnPropertyChanged(nameof(PreferPhysicalCoresText));
            OnPropertyChanged(nameof(PreferPerformanceCoresText));
            OnPropertyChanged(nameof(MemoryStrategyTitle));
            OnPropertyChanged(nameof(UseLargePagesText));
            OnPropertyChanged(nameof(EncoderThreadCountText));
            OnPropertyChanged(nameof(EncoderThreadTickLabels));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(ConfirmButtonText));

            FinishButtons.B2_1Text = CancelButtonText;
            FinishButtons.B2_2Text = ConfirmButtonText;
        }

        public static void ApplySavedSettingsToCard(ToolItemCardVM targetItem)
        {
            var model = ParallelismConfM.Load();
            var numaNodes = NumaTopologyH.GetNumaNodes();

            int maxThreadCount = numaNodes.Count > 0
                ? numaNodes.Sum(n => n.ProcessorCount)
                : Environment.ProcessorCount;
            int encoderThreadCount = Math.Max(1, Math.Min(maxThreadCount, model.EncoderThreadCount));

            var upstream = numaNodes.FirstOrDefault(n => n.NodeId == model.UpstreamNodeId) ?? numaNodes[0];
            var downstream = numaNodes.FirstOrDefault(n => n.NodeId == model.DownstreamNodeId)
                ?? (numaNodes.Count > 1 ? numaNodes[1] : numaNodes[0]);

            string primary = $"{upstream.NodeId},{upstream.Group} → {downstream.NodeId},{downstream.Group}";

            string clampIndicator = model.PreferPhysicalCores
                ? UILangProviderM.Current["ToolField.EncThreadClampOn"]
                : UILangProviderM.Current["ToolField.EncThreadClampOff"];
            string secondary = $"{encoderThreadCount} {clampIndicator}";

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
