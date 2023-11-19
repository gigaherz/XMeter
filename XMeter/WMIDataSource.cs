using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.Versioning;
using static XMeter.DataTracker;

namespace XMeter
{
    [SupportedOSPlatform("windows")]
    public class WMIDataSource : IDataSource
    {
        private readonly ManagementObjectSearcher Searcher = new(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS" +
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");


        public IEnumerable<(string name, ulong recv, ulong sent, DateTime time)> ReadData()
        {
            foreach (ManagementObject adapter in Searcher.Get())
            {
                var name = (string)adapter["Name"];
                var recv = (ulong)adapter["BytesReceivedPerSec"];
                var sent = (ulong)adapter["BytesSentPerSec"];
                var time = DateTime.FromBinary((long)(ulong)adapter["Timestamp_Sys100NS"]).AddYears(1600);

                yield return (name, recv, sent, time);
            }
        }

        internal static IDataSource Construct()
        {
            return new WMIDataSource();
        }
    }
}
