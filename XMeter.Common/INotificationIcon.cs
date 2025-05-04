using System.Drawing;
using System.Windows.Controls;
using System.Windows.Input;

namespace XMeter.Common
{
    public interface INotificationIcon
    {
        public object? DataContext { get; set; }
        public ICommand? LeftClickCommand{ get; set; }
        public ContextMenu ContextMenu { get; set; }
        public string ToolTipText { get; set; }
        public Icon? Icon { get; set; }

        void Dispose();
        void ForceCreate();
    }
}
