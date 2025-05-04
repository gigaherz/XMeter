using System.Management;
using System.Runtime.Versioning;
using XMeter.Common;

namespace XMeter.Windows
{
    [SupportedOSPlatform("windows")]
    public class WMIDataSource : IDataSource
    {
        public static IDataSource Create()
        {
            return new WMIDataSource();
        }

        private readonly ManagementObjectSearcher Searcher = new(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS" +
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");


        public IEnumerable<(string name, ulong recv, ulong sent, DateTime time)> ReadData()
        {
            foreach (ManagementBaseObject adapter in Searcher.Get())
            {
                var name = (string)adapter["Name"];
                var recv = (ulong)adapter["BytesReceivedPerSec"];
                var sent = (ulong)adapter["BytesSentPerSec"];
                var time = DateTime.FromBinary((long)(ulong)adapter["Timestamp_Sys100NS"]).AddYears(1600);

                yield return (name, recv, sent, time);
            }
        }

        private WMIDataSource() { }

    }
}
