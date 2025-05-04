using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media;

namespace XMeter.Common
{
    public interface IFloatingWindow
    {
        public bool SeparateFromTaskbar { get; set; }
        public Brush PopupBackground { get; set; }
        public Brush AccentBackground { get; set; }
        public Brush TextColor { get; set; }
        public Color TextShadow {get;set;}
    }
}
