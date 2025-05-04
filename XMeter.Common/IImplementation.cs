using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace XMeter.Common
{
    public interface IImplementation
    {
        public IDataSource CreateDataSource();
        public INotificationIcon CreateNotificationIcon();
        public ISettings CreateSettings();
    }
}
