using System;
using System.Collections.Generic;
using System.Linq;
using XMeter.Common;

namespace XMeter
{
    public class DataTracker
    {
        public const double MaxSecondSpan = 3600;

        public static readonly DataTracker Instance = new();

        public static IDataSource DataSource { get; private set; }

        private struct TimeEntry(DateTime timeStamp, ulong bytesSent, ulong bytesRecv)
        {
            public DateTime TimeStamp = timeStamp;
            public ulong BytesSent = bytesSent;
            public ulong BytesRecv = bytesRecv;

            internal readonly void Deconstruct(out DateTime time, out ulong sent, out ulong recv)
            {
                time = TimeStamp;
                sent = BytesSent;
                recv = BytesRecv;
            }

            public static implicit operator TimeEntry((DateTime, ulong, ulong) value)
            {
                return new TimeEntry(value.Item1, value.Item2, value.Item3);
            }
        }

        private readonly Dictionary<string, LinkedList<TimeEntry>> Adapters = [];

        public DateTime FirstTime => GetTime(p => p.First);
        public DateTime LastTime => GetTime(p => p.Last);

        private DateTime GetTime(Func<LinkedList<TimeEntry>, LinkedListNode<TimeEntry>> timeFunc)
        {
            return Adapters.Count > 0 ? Adapters.Values.Min(_points => _points.Count > 0 ? timeFunc(_points).Value.TimeStamp : DateTime.Now) : DateTime.Now;
        }

        public void FetchData()
        {
            var unseen = new HashSet<string>(Adapters.Keys);
            foreach (var (name, recv, sent, time) in DataSource.ReadData())
            {
                if (!Adapters.TryGetValue(name, out var points))
                {
                    Adapters[name] = points = new LinkedList<TimeEntry>();
                }

                if (points.Count > 0 && time <= points.Last.Value.TimeStamp)
                    continue;

                points.AddLast((time, sent, recv));

                var totalSpan = points.Last.Value.TimeStamp - points.First.Value.TimeStamp;
                while (totalSpan.TotalSeconds > MaxSecondSpan && points.Count > 2)
                {
                    points.RemoveFirst();
                    totalSpan = points.Last.Value.TimeStamp - points.First.Value.TimeStamp;
                }

                unseen.Remove(name);
            }

            foreach (var name in unseen)
                Adapters.Remove(name);
        }

        public (double maxSend, double maxRecv, double minSend, double minRecv)
            GetMaxMinSpeedBetween(DateTime startTime, DateTime endTime)
        {
            if (Adapters.Count == 0 || startTime >= endTime)
                return (0, 0, 0, 0);

            double accSentMax= 0;
            double accRecvMax= 0;
            double accSentMin = 0;
            double accRecvMin = 0;
            foreach (var points in Adapters.Values)
            {
                if (points.Count < 2)
                    continue;

                if (startTime > points.Last.Value.TimeStamp || endTime < points.First.Value.TimeStamp)
                    continue;

                var start = points.First;
                while (start.Next.Value.TimeStamp < startTime && start.Next != null)
                    start = start.Next;

                var end = start;
                while (end.Value.TimeStamp < endTime && end.Next != null)
                    end = end.Next;
                var endNext = end.Next ?? points.Last;

                double maxSent = 0;
                double maxRecv = 0;
                double minSent = double.MaxValue;
                double minRecv = double.MaxValue;
                bool hasData = false;
                for (var time = start; time != endNext; time = time.Next)
                {
                    var dt = (time.Next.Value.TimeStamp - time.Value.TimeStamp).TotalSeconds;
                    var ds = time.Next.Value.BytesSent - time.Value.BytesSent;
                    var dr = time.Next.Value.BytesRecv - time.Value.BytesRecv;

                    var ss = dt > 0 ? ds / dt : 0;
                    var sr = dt > 0 ? dr / dt : 0;

                    maxSent = Math.Max(maxSent, ss);
                    maxRecv = Math.Max(maxRecv, sr);
                    minSent = Math.Min(minSent, ss);
                    minRecv = Math.Min(minRecv, sr);
                    hasData=true;
                }

                if (hasData)
                {
                    accSentMax += maxSent;
                    accRecvMax += maxRecv;
                    accSentMin += minSent;
                    accRecvMin += minRecv;
                }
            }

            return (accSentMax, accRecvMax, accSentMin, accRecvMin);
        }

        public (double, double) GetMaxSpeed()
        {
            if (Adapters.Count == 0)
                return (0, 0);

            double maxSent = 0;
            double maxRecv = 0;
            foreach (var points in Adapters.Values)
            {
                if (points.Count < 2)
                    continue;

                for (var start = points.First; start != points.Last; start = start.Next)
                {
                    var dt = (start.Next.Value.TimeStamp - start.Value.TimeStamp).TotalSeconds;
                    var ds = start.Next.Value.BytesSent - start.Value.BytesSent;
                    var dr = start.Next.Value.BytesRecv - start.Value.BytesRecv;

                    var ss = dt > 0 ? ds / dt : 0;
                    var sr = dt > 0 ? dr / dt : 0;

                    maxSent = Math.Max(maxSent, ss);
                    maxRecv = Math.Max(maxRecv, sr);
                }
            }

            return (maxSent, maxRecv);
        }
    }
}
