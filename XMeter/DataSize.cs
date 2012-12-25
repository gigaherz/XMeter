using System;

namespace XMeter
{
    internal struct DataSize : IComparable<DataSize>, IEquatable<DataSize>
    {
        public static DataSize MinValue = new DataSize(ulong.MinValue);
        public static DataSize MaxValue = new DataSize(ulong.MaxValue);

        private readonly ulong bytes;

        public ulong Bytes
        {
            get { return bytes; }
        }

        public double KiloBytesBinary
        {
            get { return bytes/(1024.0); }
        }

        public double MegaBytesBinary
        {
            get { return bytes/(1024.0*1024.0); }
        }

        public double GigaBytesBinary
        {
            get { return bytes/(1024.0*1024.0*1024.0); }
        }

        public double TeraBytesBinary
        {
            get { return bytes/(1024.0*1024.0*1024.0*1024.0); }
        }

        public double KiloBytesDecimal
        {
            get { return bytes / (1000.0); }
        }

        public double MegaBytesDecimal
        {
            get { return bytes / (1000000.0); }
        }

        public double GigaBytesDecimal
        {
            get { return bytes / (1000000.0 * 1000.0); }
        }

        public double TeraBytesDecimal
        {
            get { return bytes / (1000000.0 * 1000000.0); }
        }

        public DataSize(ulong bytes)
        {
            this.bytes = bytes;
        }

        public int CompareTo(DataSize other)
        {
            if (this < other) return -1;
            if (this > other) return 1;
            return 0;
        }

        public bool Equals(DataSize other)
        {
            return bytes == other.bytes;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is DataSize && Equals((DataSize)obj);
        }

        public override int GetHashCode()
        {
            return bytes.GetHashCode();
        }

        public static bool operator >(DataSize a, DataSize b)
        {
            return a.bytes > b.bytes;
        }

        public static bool operator <(DataSize a, DataSize b)
        {
            return a.bytes < b.bytes;
        }

        public static bool operator <=(DataSize a, DataSize b)
        {
            return a.bytes <= b.bytes;
        }

        public static bool operator >=(DataSize a, DataSize b)
        {
            return a.bytes >= b.bytes;
        }

        public static bool operator ==(DataSize a, DataSize b)
        {
            return a.bytes == b.bytes;
        }

        public static bool operator !=(DataSize a, DataSize b)
        {
            return a.bytes != b.bytes;
        }

        public static DataSize operator -(DataSize a, DataSize b)
        {
            return new DataSize(a.bytes - b.bytes);
        }

        public static DataSize Max(DataSize a, DataSize b)
        {
            return a > b ? a : b;
        }

        public static DataSize Min(DataSize a, DataSize b)
        {
            return a < b ? a : b;
        }
        
        public override string ToString()
        {
            double dbytes = bytes;

            if (bytes < 1024)
                return string.Format("{0:0.00} Bytes/s", dbytes);

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return string.Format("{0:0.00} KB/s", dbytes);

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return string.Format("{0:0.00} GB/s", dbytes);

            dbytes /= 1024.0;

            // Maybe... someday...
            return string.Format("{0:0.00} GB/s", dbytes);
        }
    }
}