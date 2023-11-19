using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XMeter
{
    public interface IDataSource
    {
        public IEnumerable<(string name, ulong recv, ulong sent, DateTime time)> ReadData();

        public static IDataSource CreateDataSource()
        {
            if (OperatingSystem.IsWindows())
            {
                return WMIDataSource.Construct();
            }
            else
            {
                throw new NotSupportedException("Data parsing not supported for platform " + RuntimeInformation.OSDescription);
            }
        }

    }
}
