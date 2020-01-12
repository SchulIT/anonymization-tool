namespace AnonymizationTool.Data
{
    public delegate void ConnectionStateChangedEventHandler<T>(T dataSource, ConnectionStateChangedEventArgs<T> args)
        where T : IDataSource;
}
