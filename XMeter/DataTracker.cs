using System;
using System.Collections.Generic;
using System.Linq;

namespace XMeter
{
    partial class DataTracker
    {
        public static readonly DataTracker Instance = new DataTracker();

        public IDataSource dataSource = IDataSource.CreateDataSource();

        public const double MaxSecondSpan = 3600;

        private struct TimeEntry
        {
            public DateTime TimeStamp;
            public ulong BytesSent;
            public ulong BytesRecv;

            public TimeEntry(DateTime timeStamp, ulong bytesSent, ulong bytesRecv)
            {
                TimeStamp = timeStamp;
                BytesSent = bytesSent;
                BytesRecv = bytesRecv;
            }

            internal void Deconstruct(out DateTime time, out ulong sent, out ulong recv)
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

        private readonly Dictionary<string, LinkedList<TimeEntry>> Adapters = new();

        public DateTime FirstTime => GetTime(p => p.First);
        public DateTime LastTime => GetTime(p => p.Last);

        private DateTime GetTime(Func<LinkedList<TimeEntry>, LinkedListNode<TimeEntry>> timeFunc)
        {
            return Adapters.Count > 0 ? Adapters.Values.Min(_points => (_points.Count > 0) ? timeFunc(_points).Value.TimeStamp : DateTime.Now) : DateTime.Now;
        }

        public void FetchData()
        {
            var unseen = new HashSet<string>(Adapters.Keys);
            foreach (var (name,recv,sent,time) in dataSource.ReadData())
            {
                var lastRecv = recv;
                var lastSent = sent;
                var lastTime = time;
                if (Adapters.TryGetValue(name, out var points))
                {
                    (lastTime, lastSent, lastRecv) = points.Last.Value;
                }
                else
                {
                    Adapters[name] = points = new LinkedList<TimeEntry>();
                }

                if (points.Count > 0 && time <= points.Last.Value.TimeStamp)
                    continue;

                //var diffRecv = recv - lastRecv;
                //var diffSent = sent - lastSent;
                //var diffTime = time - lastTime;

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

        public (double speedSend, double speedRecv) GetMaxSpeedBetween(DateTime startTime, DateTime endTime)
        {
            if (Adapters.Count == 0 || startTime >= endTime)
                return (0, 0);

            double accSent = 0;
            double accRecv = 0;
            foreach(var points in Adapters.Values)
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

#if false
                var startNext = start.Next ?? start;

                var startSent = Lerp(start.Value.TimeStamp, start.Value.BytesSent, startNext.Value.TimeStamp, startNext.Value.BytesSent, startTime);
                var endSent = Lerp(end.Value.TimeStamp, end.Value.BytesSent, endNext.Value.TimeStamp, endNext.Value.BytesSent, endTime);

                var startRecv = Lerp(start.Value.TimeStamp, start.Value.BytesRecv, startNext.Value.TimeStamp, startNext.Value.BytesRecv, startTime);
                var endRecv = Lerp(end.Value.TimeStamp, end.Value.BytesRecv, endNext.Value.TimeStamp, endNext.Value.BytesRecv, endTime);
                
                accSent += (endSent - startSent) / (endTime - startTime).TotalSeconds;
                accRecv += (endRecv - startRecv) / (endTime - startTime).TotalSeconds;
                adapters++;
#else
                double maxSent = 0;
                double maxRecv = 0;
                for (var time = start; time != endNext; time = time.Next)
                {
                    var dt = (time.Next.Value.TimeStamp - time.Value.TimeStamp).TotalSeconds;
                    var ds = (time.Next.Value.BytesSent - time.Value.BytesSent);
                    var dr = (time.Next.Value.BytesRecv - time.Value.BytesRecv);

                    var ss = dt > 0 ? ds / dt : 0;
                    var sr = dt > 0 ? dr / dt : 0;

                    maxSent = Math.Max(maxSent, ss);
                    maxRecv = Math.Max(maxRecv, sr);
                }

                accSent += maxSent;
                accRecv += maxRecv;
#endif
            }

            return (accSent, accRecv);
        }

        internal (double, double) GetMaxSpeed()
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
                    var ds = (start.Next.Value.BytesSent - start.Value.BytesSent);
                    var dr = (start.Next.Value.BytesRecv - start.Value.BytesRecv);
                    
                    var ss = dt > 0 ? ds / dt : 0;
                    var sr = dt > 0 ? dr / dt : 0;

                    maxSent = Math.Max(maxSent, ss);
                    maxRecv = Math.Max(maxRecv, sr);
                }
            }

            return (maxSent, maxRecv);
        }

        private ulong Lerp(DateTime time1, ulong value1, DateTime time2, ulong value2, DateTime time)
        {
            var abt = (time2 - time1).TotalSeconds;
            var att = (time - time1).TotalSeconds;
            var t = (att / abt);
            if (t < 0)
                return value1;
            if (t > 1)
                return value2;

            var d = (decimal)t;
            return (ulong)(value1 + d * (value2 - value1));
        }
    }
}
