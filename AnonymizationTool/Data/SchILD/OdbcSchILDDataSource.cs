using AnonymizationTool.Common;
using AnonymizationTool.Settings;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public class OdbcSchILDDataSource : ISchILDDataSource
    {
        public bool CanConnect { get { return settingsService.Settings.SchILDConnection.Type == DatabaseType.Access && !string.IsNullOrEmpty(settingsService.Settings.SchILDConnection.ConnectionString); } }

        public bool IsConnected { get { return connection != null && connection.State == System.Data.ConnectionState.Open; } }


        private OdbcConnection connection;

        #region Events

        public event ConnectionStateChangedEventHandler<ISchILDDataSource> ConnectionStateChanged;

        private void RaiseConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs<ISchILDDataSource>(this, IsConnected));
        }

        #endregion

        private readonly ISettingsService settingsService;

        public OdbcSchILDDataSource(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public async Task ConnectAsync()
        {
            if(connection != null)
            {
                await DisconnectAsync();
            }

            connection = new OdbcConnection(settingsService.Settings.SchILDConnection.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            RaiseConnectionStateChanged();
        }

        public async Task DisconnectAsync()
        {
            if(connection == null)
            {
                return;
            }

            await connection.CloseAsync().ConfigureAwait(false);

            RaiseConnectionStateChanged();
        }

        public async Task<IEnumerable<Student>> LoadStudentsAsync()
        {
            // query all students
            var studentsCommand = connection.CreateCommand();
            studentsCommand.CommandText = "SELECT ID, Name, Vorname, Klasse, Geschlecht, SchulEmail FROM Schueler WHERE AktSchuljahr = (SELECT TOP 1 Schuljahr FROM EigeneSchule) AND AktAbschnitt = (SELECT TOP 1 SchuljahrAbschnitt FROM EigeneSchule)";

            var studentsReader = await studentsCommand.ExecuteReaderAsync();

            var students = new List<Student>();

            while (await studentsReader.ReadAsync())
            {
                var student = new Student
                {
                    Id = studentsReader["ID"].ToString(),
                    FirstName = studentsReader["Vorname"]?.ToString(),
                    LastName = studentsReader["Name"]?.ToString(),
                    Grade = studentsReader["Klasse"]?.ToString(),
                    Email = studentsReader["SchulEmail"]?.ToString(),
                    Gender = GetGender((byte)studentsReader["Geschlecht"])
                };

                students.Add(student);
            }

            return students;
        }

        private static Gender GetGender(int databaseGender)
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

        public bool IsSupported(DatabaseType type)
        {
            return type == DatabaseType.Access;
        }
    }
}
