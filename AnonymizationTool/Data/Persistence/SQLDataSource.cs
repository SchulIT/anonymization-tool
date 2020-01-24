using AnonymizationTool.Settings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.Persistence
{
    public class SqlDataSource : IPersistentDataSource
    {
        private static string InMemoryConnectionString = "datasource=:memory:";

        public bool IsInMemory { get; private set; }

        public bool IsConnected { get { return context != null && context.Database != null; } }

        public bool CanConnect { get { return settingsService.Settings.DatabaseConnection.Type != DatabaseType.Access && !string.IsNullOrEmpty(settingsService.Settings.DatabaseConnection.ConnectionString); } }

        private SqlContext context = null;

        private ISettingsService settingsService;

        public event ConnectionStateChangedEventHandler<IPersistentDataSource> ConnectionStateChanged;

        private void RaiseConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs<IPersistentDataSource>(this, IsConnected));
        }

        public SqlDataSource(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public async Task ConnectAsync()
        {
            if(IsConnected)
            {
                await DisconnectAsync();
            }

            IsInMemory = false;
            context = new SqlContext(settingsService.Settings.DatabaseConnection.Type, settingsService.Settings.DatabaseConnection.ConnectionString);

            if(settingsService.Settings.DatabaseConnection.Type == DatabaseType.SQLite)
            {
                IsInMemory = !string.IsNullOrEmpty(settingsService.Settings.DatabaseConnection.ConnectionString) &&
                    settingsService.Settings.DatabaseConnection.ConnectionString.ToLower().Trim() == InMemoryConnectionString;

                await context.Database.OpenConnectionAsync().ConfigureAwait(false);
            }

            await context.Database.EnsureCreatedAsync().ConfigureAwait(false);

            RaiseConnectionStateChanged();
        }

        public async Task DisconnectAsync()
        {
            await context.DisposeAsync().ConfigureAwait(false);
            context = null;

            RaiseConnectionStateChanged();
        }

        public Task<List<AnonymousStudent>> LoadStudentsAsync()
        {
            return context.Students.Where(x => x.IsRemoved == false).ToListAsync();
        }

        public void AddStudent(AnonymousStudent student)
        {
            context.Students.Add(student);
        }

        public void UpdateStudent(AnonymousStudent student)
        {
            context.Students.Update(student);
        }

        public void RemoveStudent(AnonymousStudent student)
        {
            student.IsRemoved = true;
            context.Students.Update(student);
        }

        public Task SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }

        public bool IsSupported(DatabaseType type)
        {
            return type == DatabaseType.MSSQL || type == DatabaseType.MySQL || type == DatabaseType.SQLite;
        }

        public async Task TestConnectionAsync(DatabaseType type, string connectionString)
        {
            using (var context = new SqlContext(type, connectionString))
            {
                await context.Database.OpenConnectionAsync().ConfigureAwait(false);
            }
        }

        internal class SqlContext : DbContext
        {
            internal DbSet<AnonymousStudent> Students { get; set; }
            internal DbSet<AnonymousStudent> RemovedStudents { get; set; }

            private readonly DatabaseType type;
            private readonly string connectionString;

            public SqlContext(DatabaseType type, string connectionString)
            {
                this.type = type;
                this.connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                switch (type)
                {
                    case DatabaseType.MSSQL:
                        optionsBuilder.UseSqlServer(connectionString);
                        break;

                    case DatabaseType.MySQL:
                        optionsBuilder.UseMySql(connectionString);
                        break;

                    case DatabaseType.SQLite:
                        optionsBuilder.UseSqlite(connectionString);
                        break;

                    default:
                        throw new NotSupportedException("Access is not supported.");
                }
            }
        }
    }
}
