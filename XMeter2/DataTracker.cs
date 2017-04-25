using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace XMeter2
{
    class DataTracker
    {
        public const double MaxSecondSpan = 3600;

        private static readonly Dictionary<string, ulong> PrevLastSend = new Dictionary<string, ulong>();
        private static readonly Dictionary<string, ulong> PrevLastRecv = new Dictionary<string, ulong>();
        private static readonly Dictionary<string, DateTime> PrevLastStamp = new Dictionary<string, DateTime>();
        private static readonly ManagementObjectSearcher Searcher =
            new ManagementObjectSearcher(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS" +
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");

        public static LinkedList<(DateTime TimeStamp, ulong Bytes)> SendPoints { get; } = new LinkedList<(DateTime TimeStamp, ulong Bytes)>();
        public static LinkedList<(DateTime TimeStamp, ulong Bytes)> RecvPoints { get; } = new LinkedList<(DateTime TimeStamp, ulong Bytes)>();

        public static (ulong send, ulong recv) CurrentSpeed => 
            (SendPoints.Last.Value.Bytes,
            RecvPoints.Last.Value.Bytes);
        public static (DateTime send, DateTime recv) CurrentTime =>
            (SendPoints.Last.Value.TimeStamp,
            RecvPoints.Last.Value.TimeStamp);

        public static (DateTime send, DateTime recv) FirstTime =>
            (SendPoints.First.Value.TimeStamp,
            RecvPoints.First.Value.TimeStamp);

        public static (ulong send, ulong recv) MaxSpeed => 
            (SendPoints.Select(s => s.Bytes).Max(),
            RecvPoints.Select(s => s.Bytes).Max());

        public static void FetchData()
        {
            var maxStamp = UpdateNetwork(out ulong bytesReceivedPerSec, out ulong bytesSentPerSec);

            void AddData(LinkedList<(DateTime TimeStamp, ulong Bytes)> points, ulong bytesPerSec)
            {
                points.AddLast((maxStamp, bytesPerSec));

                var totalSpan = points.Last.Value.TimeStamp - points.First.Value.TimeStamp;
                while (totalSpan.TotalSeconds > MaxSecondSpan && points.Count > 1)
                {
                    points.RemoveFirst();
                    totalSpan = points.Last.Value.TimeStamp - points.First.Value.TimeStamp;
                }
            }

            AddData(SendPoints, bytesSentPerSec);
            AddData(RecvPoints, bytesReceivedPerSec);
        }

        public static DateTime UpdateNetwork(out ulong bytesReceivedPerSec, out ulong bytesSentPerSec)
        {
            var maxStamp = DateTime.MinValue;

            bytesReceivedPerSec = 0;
            bytesSentPerSec = 0;

            foreach (ManagementObject adapter in Searcher.Get())
            {
                var name = (string)adapter["Name"];
                var recv = adapter["BytesReceivedPerSec"];
                var sent = adapter["BytesSentPerSec"];
                var curStamp = DateTime.FromBinary((long)(ulong)adapter["Timestamp_Sys100NS"]).AddYears(1600);

                if (curStamp > maxStamp)
                    maxStamp = curStamp;

                // XP seems to have uint32's there, but win7 has uint64's
                var curRecv = recv as uint? ?? (ulong)recv;
                var curSend = sent as uint? ?? (ulong)sent;

                var lstRecv = curRecv;
                var lstSend = curSend;
                var lstStamp = curStamp;

                if (PrevLastRecv.ContainsKey(name)) lstRecv = PrevLastRecv[name];
                if (PrevLastSend.ContainsKey(name)) lstSend = PrevLastSend[name];
                if (PrevLastStamp.ContainsKey(name)) lstStamp = PrevLastStamp[name];

                var diffRecv = curRecv - lstRecv;
                var diffSend = curSend - lstSend;
                var diffStamp = curStamp - lstStamp;

                PrevLastRecv[name] = curRecv;
                PrevLastSend[name] = curSend;
                PrevLastStamp[name] = curStamp;

                if (diffStamp <= TimeSpan.Zero)
                    continue;

                var diffSeconds = diffStamp.TotalSeconds;

                if (diffSeconds > MaxSecondSpan)
                    continue;

                bytesReceivedPerSec += (ulong)(diffRecv / diffSeconds);
                bytesSentPerSec += (ulong)(diffSend / diffSeconds);
            }

            return maxStamp;
        }
    }
}
