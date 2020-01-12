
using System;

namespace AnonymizationTool.Data
{
    public class ConnectionStateChangedEventArgs<T> : EventArgs
        where T : IDataSource
    {
        public T DataSource { get; private set; }

        public bool IsConnected { get; private set; }

        public ConnectionStateChangedEventArgs(T dataSource, bool isConnected)
        {
            DataSource = dataSource;
            IsConnected = isConnected;
        }
    }
}
