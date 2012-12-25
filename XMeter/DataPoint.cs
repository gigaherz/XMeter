using System;

namespace XMeter
{
    class DataPoint
    {
        public DateTime TimeStamp { get; set; }
        public DataSize DownloadSpeed { get; set; }
        public DataSize UploadSpeed { get; set; }

        public DataPoint(DateTime maxStamp, ulong bytesReceivedPerSec, ulong bytesSentPerSec)
        {
            TimeStamp = maxStamp;
            DownloadSpeed = new DataSize(bytesReceivedPerSec);
            UploadSpeed = new DataSize(bytesSentPerSec);
        }
    }
}