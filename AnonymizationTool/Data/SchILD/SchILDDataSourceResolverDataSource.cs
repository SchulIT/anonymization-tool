using AnonymizationTool.Settings;
using Autofac.Features.Indexed;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public class SchILDDataSourceResolverDataSource : ISchILDDataSource
    {
        public bool CanConnect
        {
            get
            {
                var dataSource = GetDataSource(settingsService.Settings.SchILDConnection.Type);

                if(dataSource != null)
                {
                    return dataSource.CanConnect;
                }

                return false;
            }
        }

        private ISettingsService settingsService;
        private IIndex<DatabaseType, ISchILDDataSource> dataSources;

        public SchILDDataSourceResolverDataSource(ISettingsService settingsService, IIndex<DatabaseType, ISchILDDataSource> dataSources)
        {
            this.settingsService = settingsService;
            this.dataSources = dataSources;
        }

        public bool IsSupported(DatabaseType type)
        {
            return GetDataSource(type) != null;
        }

        public Task<IEnumerable<Student>> LoadStudentsAsync()
        {
            return GetDataSource(settingsService.Settings.SchILDConnection.Type).LoadStudentsAsync();
        }

        public Task TestConnectionAsync()
        {
            return GetDataSource(settingsService.Settings.SchILDConnection.Type).TestConnectionAsync();
        }

        private ISchILDDataSource GetDataSource(DatabaseType type)
        {
            ISchILDDataSource dataSource;

            if (dataSources.TryGetValue(type, out dataSource))
            {
                return dataSource;
            }

            return null;
        }
    }
}
