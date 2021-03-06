﻿using System.Threading.Tasks;

namespace AnonymizationTool.Data
{
    public interface IDataSource
    {
        bool CanConnect { get; }

        bool IsSupported(DatabaseType type);

        Task TestConnectionAsync(DatabaseType type, string connectionString);
    }
}
