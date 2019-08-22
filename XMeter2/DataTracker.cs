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
        public static readonly DataTracker Instance = new DataTracker();

        private static readonly ManagementObjectSearcher Searcher =
            new ManagementObjectSearcher(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS" +
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");

        public const double MaxSecondSpan = 3600;

        private readonly Dictionary<string, ulong> PrevLastSend = new Dictionary<string, ulong>();
        private readonly Dictionary<string, ulong> PrevLastRecv = new Dictionary<string, ulong>();
        private readonly Dictionary<string, DateTime> PrevLastStamp = new Dictionary<string, DateTime>();

        public LinkedList<(DateTime TimeStamp, ulong Bytes)> SendPoints { get; } = new LinkedList<(DateTime TimeStamp, ulong Bytes)>();
        public LinkedList<(DateTime TimeStamp, ulong Bytes)> RecvPoints { get; } = new LinkedList<(DateTime TimeStamp, ulong Bytes)>();

        public (ulong send, ulong recv) CurrentSpeed =>
            (SendPoints.Count > 0 && RecvPoints.Count > 0) ? (SendPoints.Last.Value.Bytes, RecvPoints.Last.Value.Bytes) : (0, 0);
        public (DateTime send, DateTime recv) CurrentTime =>
            (SendPoints.Count > 0 && RecvPoints.Count > 0) ? (SendPoints.Last.Value.TimeStamp, RecvPoints.Last.Value.TimeStamp) : (DateTime.Now, DateTime.Now);

        public (DateTime send, DateTime recv) FirstTime => 
            (SendPoints.Count > 0 && RecvPoints.Count > 0) ? (SendPoints.First.Value.TimeStamp, RecvPoints.First.Value.TimeStamp) : (DateTime.Now, DateTime.Now);

        public (ulong send, ulong recv) MaxSpeed => (SendPoints.Count > 0 ? (SendPoints.Select(s => s.Bytes).Max(), RecvPoints.Select(s => s.Bytes).Max()) : (0,0));

        public void FetchData()
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

        private DateTime UpdateNetwork(out ulong bytesReceivedPerSec, out ulong bytesSentPerSec)
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

        public DataTracker Simplify(int maxPoints)
        {
            var dt = new DataTracker();

            if (SendPoints.Count == 0)
                return dt;

            double points = SendPoints.Count;

            double last = 0;
            int at = 0;
            DateTime atdt = DateTime.MinValue;
            double acc = 0;
            TimeSpan time = TimeSpan.Zero;
            for (var current = SendPoints.Last; current != null; current = current.Previous, at++)
            {
                var prev = current.Next;
                if (prev == null)
                {
                    dt.SendPoints.AddFirst(current.Value);
                    atdt = current.Value.TimeStamp;
                    continue;
                }
                var ts = prev.Value.TimeStamp - current.Value.TimeStamp;
                var vl = prev.Value.Bytes - current.Value.Bytes;
                acc += vl;
                time += ts;

                double prog = Math.Floor(at * points / maxPoints);
                if (prog > last)
                {
                    dt.SendPoints.AddFirst((atdt, (ulong)(acc/time.TotalSeconds)));
                    atdt = current.Value.TimeStamp;
                    acc = 0;

                    last = prog;
                }
            }
            if(time > TimeSpan.Zero)
            {
                dt.SendPoints.AddFirst((atdt, (ulong)(acc / time.TotalSeconds)));
            }

            last = 0;
            at = 0;
            atdt = DateTime.MinValue;
            acc = 0;
            time = TimeSpan.Zero;
            for (var current = RecvPoints.Last; current != null; current = current.Previous, at++)
            {
                var prev = current.Next;
                if (prev == null)
                {
                    dt.RecvPoints.AddFirst(current.Value);
                    atdt = current.Value.TimeStamp;
                    continue;
                }
                var ts = prev.Value.TimeStamp - current.Value.TimeStamp;
                var vl = prev.Value.Bytes - current.Value.Bytes;
                acc += vl;
                time += ts;

                double prog = Math.Floor(at * points / maxPoints);
                if (prog > last)
                {
                    dt.RecvPoints.AddFirst((atdt, (ulong)(acc / time.TotalSeconds)));
                    atdt = current.Value.TimeStamp;
                    acc = 0;

                    last = prog;
                }
            }
            if (time > TimeSpan.Zero)
            {
                dt.RecvPoints.AddFirst((atdt, (ulong)(acc / time.TotalSeconds)));
            }

            return dt;
        }
    }
}
