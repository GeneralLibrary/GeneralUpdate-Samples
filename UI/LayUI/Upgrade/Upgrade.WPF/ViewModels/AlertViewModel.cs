using LayUI.Wpf.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upgrade.WPF.ViewModels
{
    public class AlertViewModel : BindableBase, ILayDialogAware
    {
        public event Action<ILayDialogResult> RequestClose;

        public void OnDialogClosed()
        {
             
        }
        private DelegateCommand _OkCommand;
        public DelegateCommand OkCommand =>
            _OkCommand ?? (_OkCommand = new DelegateCommand(ExecuteOkCommand));

        void ExecuteOkCommand()
        {
            RequestClose?.Invoke(new LayDialogResult());
        }
        public void OnDialogOpened(ILayDialogParameter parameters)
        {
             
        }
    }
}
