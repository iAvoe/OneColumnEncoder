using OneColumnEncoder.CommonMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OneColumnEncoder.ViewModels
{
    public class StatusVM : BaseVM
    {
        private StatusType _currentStatus = StatusType.Waiting;
        public StatusType CurrentStatus
        {
            get => _currentStatus;
            set => SetProperty(ref _currentStatus, value);
        }
    }
}