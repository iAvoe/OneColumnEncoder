using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class CPUNodeCardVM : BaseVM
    {
        // Section 1: Text within card
        private int _nodeId;
        public int NodeId
        {
            get => _nodeId;
            set => SetProperty(ref _nodeId, value);
        }

        private int _groupId;
        public int GroupId
        {
            get => _groupId;
            set => SetProperty(ref _groupId, value);
        }

        // Section 2: small gray text under the card
        private int _minThreadNum;
        public int MinThreadNum
        {
            get => _minThreadNum;
            set => SetProperty(ref _minThreadNum, value);
        }
        private int _maxThreadNum;
        public int MaxThreadNum
        {
            get => _maxThreadNum;
            set => SetProperty(ref _maxThreadNum, value);
        }
        private int _hasMemGB;
        public int HasMemGB
        {
            get => _hasMemGB;
            set => SetProperty(ref _hasMemGB, value);
        }

        // Card selection or diabling (no NUMA node on this range)
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        // TODO: add NUMA detection here or in Helers
    }
}
