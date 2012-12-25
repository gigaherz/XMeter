using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace XMeter
{
    class Meter : IDisposable
    {
        private const double MaxSecondSpan = 3600;

        public readonly Queue<DataPoint> DataPoints = new Queue<DataPoint>();

        private TimeSpan TotalSpan
        {
            get
            {
                return DataPoints.First().TimeStamp - DataPoints.Last().TimeStamp;
            }
        }

        public DataSize LastMinSpeed { get; private set; }
        public DataSize LastMaxSpeed { get; private set; }

        private readonly Dictionary<string, DataPoint> previousValues = new Dictionary<string, DataPoint>();

        private const string WmiQuery = "SELECT Name, BytesReceivedPerSec, BytesSentPerSec FROM Win32_PerfRawData_Tcpip_NetworkInterface";
        readonly ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiQuery);

        public DateTime LastCheck;

        public Meter()
        {
            searcher.Options.UseAmendedQualifiers = false;
            searcher.Options.Rewindable = false;
        }

        public void UpdateSpeeds()
        {
            var currentCheck = DateTime.Now;

            ReadStatistics(currentCheck);

            RemoveOldDataPoints();

            UpdateBounds();

            LastCheck = currentCheck;
        }

        private void UpdateBounds()
        {
            var minSpeed = DataSize.MaxValue;
            var maxSpeed = DataSize.MinValue;

            foreach (var ts in DataPoints)
            {
                minSpeed = DataSize.Min(minSpeed, DataSize.Min(ts.DownloadSpeed, ts.UploadSpeed));
                maxSpeed = DataSize.Max(maxSpeed, DataSize.Max(ts.UploadSpeed, ts.UploadSpeed));
            }

            LastMaxSpeed = maxSpeed;
            LastMinSpeed = minSpeed;
        }

        private void RemoveOldDataPoints()
        {
            while (TotalSpan.TotalSeconds > MaxSecondSpan && DataPoints.Count > 1)
            {
                DataPoints.Dequeue();
            }
        }

        private void ReadStatistics(DateTime currentTime)
        {
            ulong bytesReceivedPerSec = 0;
            ulong bytesSentPerSec = 0;

            var elapsed = (currentTime - LastCheck).TotalSeconds;

            foreach (ManagementObject adapter in searcher.Get())
            {
                var name = adapter["Name"].ToString();
                var recv = adapter["BytesReceivedPerSec"];
                var sent = adapter["BytesSentPerSec"];

                // XP seems to have uint32's there, but win7 has uint64's
                var curRecv = recv is uint ? (uint) recv : (ulong) recv;
                var curSend = sent is uint ? (uint) sent : (ulong) sent;

                ulong prevRecv = curRecv;
                ulong prevSend = curSend;

                DataPoint point;
                if (previousValues.TryGetValue(name, out point))
                {
                    prevRecv = point.DownloadSpeed.Bytes;
                    prevSend = point.UploadSpeed.Bytes;
                }
                
                previousValues[name] = new DataPoint(currentTime, curRecv, curSend);

                bytesReceivedPerSec += (ulong) ((curRecv - prevRecv)/elapsed);
                bytesSentPerSec += (ulong) ((curSend - prevSend)/elapsed);
            }

            DataPoints.Enqueue(new DataPoint(currentTime, bytesReceivedPerSec, bytesSentPerSec));
        }

        public void Dispose()
        {
            searcher.Dispose();
        }
    }
}
