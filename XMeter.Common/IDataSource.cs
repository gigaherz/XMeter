namespace XMeter.Common
{
    public interface IDataSource
    {
        public IEnumerable<(string name, ulong recv, ulong sent, DateTime time)> ReadData();
    }
}
