﻿using AnonymizationTool.Common;
using AnonymizationTool.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public abstract class AbstractSchILDDataSource : ISchILDDataSource
    {
        protected abstract DatabaseType Type { get; }

        public virtual bool CanConnect { get { return IsSupported(settingsService.Settings.SchILDConnection.Type) && !string.IsNullOrEmpty(settingsService.Settings.SchILDConnection.ConnectionString); } }

        private ISettingsService settingsService;

        public AbstractSchILDDataSource(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public bool IsSupported(DatabaseType type)
        {
            return type == Type;
        }

        public abstract Task<IEnumerable<Student>> LoadStudentsAsync();

        protected string GetConnectionString()
        {
            return settingsService.Settings.SchILDConnection.ConnectionString;
        }

        protected static Gender GetGender(int databaseGender)
        {
            switch (databaseGender)
            {
                case 3:
                    return Gender.Male;

                case 4:
                    return Gender.Female;

                default:
                    return Gender.Other;

            }
        }

        public abstract Task TestConnectionAsync(DatabaseType type, string connectionString);
    }
}
