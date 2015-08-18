using System;
using System.Collections.Generic;
using System.Management;

namespace XMeter2
{
    class NetTracker
    {
        public const double MaxSecondSpan = 3600;

        readonly static Dictionary<string, ulong> prevLastSend = new Dictionary<string, ulong>();
        readonly static Dictionary<string, ulong> prevLastRecv = new Dictionary<string, ulong>();
        readonly static Dictionary<string, DateTime> prevLastStamp = new Dictionary<string, DateTime>();
        readonly static ManagementObjectSearcher searcher =
            new ManagementObjectSearcher(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS" +
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");

        public static DateTime UpdateNetwork(out ulong bytesReceivedPerSec, out ulong bytesSentPerSec)
        {
            DateTime maxStamp = DateTime.MinValue;

            bytesReceivedPerSec = 0;
            bytesSentPerSec = 0;

            foreach (ManagementObject adapter in searcher.Get())
            {
                var name = (string)adapter["Name"];
                var recv = adapter["BytesReceivedPerSec"];
                var sent = adapter["BytesSentPerSec"];
                var curStamp = DateTime.FromBinary((long)(ulong)adapter["Timestamp_Sys100NS"]).AddYears(1600);

                if (curStamp > maxStamp)
                    maxStamp = curStamp;

                // XP seems to have uint32's there, but win7 has uint64's
                var curRecv = recv is uint ? (uint)recv : (ulong)recv;
                var curSend = sent is uint ? (uint)sent : (ulong)sent;

                var lstRecv = curRecv;
                var lstSend = curSend;
                var lstStamp = curStamp;

                if (prevLastRecv.ContainsKey(name)) lstRecv = prevLastRecv[name];
                if (prevLastSend.ContainsKey(name)) lstSend = prevLastSend[name];
                if (prevLastStamp.ContainsKey(name)) lstStamp = prevLastStamp[name];

                var diffRecv = (curRecv - lstRecv);
                var diffSend = (curSend - lstSend);
                var diffStamp = (curStamp - lstStamp);

                prevLastRecv[name] = curRecv;
                prevLastSend[name] = curSend;
                prevLastStamp[name] = curStamp;

                if (diffStamp <= TimeSpan.Zero)
                    continue;

                double diffSeconds = diffStamp.TotalSeconds;

                if (diffSeconds > MaxSecondSpan)
                    continue;

                bytesReceivedPerSec += (ulong)(diffRecv / diffSeconds);
                bytesSentPerSec += (ulong)(diffSend / diffSeconds);
            }

            return maxStamp;
        }
    }
}
