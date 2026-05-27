using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System;
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

        public string WindowTitle => Lang.WindowTitle;
        public string IntroText => Lang.IntroText;
        public string PriorityText => Lang.PriorityText;
        public string CacheGroupTitle => Lang.CacheGroupTitle;
        public string CacheGroupHint => Lang.CacheGroupHint;
        public string UpstreamNumaTitle => Lang.UpstreamNumaTitle;
        public string DownstreamNumaTitle => Lang.DownstreamNumaTitle;
        public string NumaGuidanceText => Lang.NumaGuidanceText;
        public string ThreadStrategyTitle => Lang.ThreadStrategyTitle;
        public string PreferPhysicalCoresText => Lang.PreferPhysicalCoresText;
        public string PreferPerformanceCoresText => Lang.PreferPerformanceCoresText;
        public string MemoryStrategyTitle => Lang.MemoryStrategyTitle;
        public string UseLargePagesText => Lang.UseLargePagesText;
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

            BuildDefaultNodes(UpstreamNodes);
            BuildDefaultNodes(DownstreamNodes);
            LoadModelToUi();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private static void BuildDefaultNodes(ObservableCollection<CPUNodeCardVM> nodes)
        {
            nodes.Clear();
            nodes.Add(new CPUNodeCardVM
            {
                NodeId = 0,
                GroupId = 0,
                MinThreadNum = 0,
                MaxThreadNum = 127,
                HasMemGB = 64,
                IsEnabled = true,
                IsSelected = true
            });
            nodes.Add(new CPUNodeCardVM
            {
                NodeId = 1,
                GroupId = 1,
                MinThreadNum = 128,
                MaxThreadNum = 255,
                HasMemGB = 64,
                IsEnabled = true
            });

            for (int i = 2; i < 8; i++)
            {
                nodes.Add(new CPUNodeCardVM
                {
                    NodeId = i,
                    IsEnabled = false
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
            _targetItem.PrimaryValueText = BuildPrimarySummary();
            _targetItem.Path = BuildSecondarySummary();
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
            _model.Save();
        }

        private void LoadModelToUi()
        {
            PreferPhysicalCores = _model.PreferPhysicalCores;
            PreferPerformanceCores = _model.PreferPerformanceCores;
            UseLargePages = _model.UseLargePages;

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
            return $"Upstream: {upstream.NodeLabel} | Downstream: {downstream.NodeLabel}";
        }

        private string BuildSecondarySummary()
        {
            CPUNodeCardVM upstream = UpstreamNodes.First(n => n.IsSelected);
            CPUNodeCardVM downstream = DownstreamNodes.First(n => n.IsSelected);
            return $"Upstream: {upstream.ResourceLabel} | Downstream: {downstream.ResourceLabel}";
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
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(ConfirmButtonText));

            FinishButtons.B2_1Text = CancelButtonText;
            FinishButtons.B2_2Text = ConfirmButtonText;
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
