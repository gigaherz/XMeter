
namespace XMeter2
{
    static class Util
    {
        public static string FormatUSize(ulong bytes)
        {
            double dbytes = bytes;

            if (bytes < 1024)
                return bytes.ToString() + " B/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return dbytes.ToString("#0.00") + " KB/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return dbytes.ToString("#0.00") + " MBs/s";

            dbytes /= 1024.0;

            // Maybe... someday...
            return dbytes.ToString("#0.00") + " GBs/s";
        }
    }
}
