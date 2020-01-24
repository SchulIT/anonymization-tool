using AnonymizationTool.Settings;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public class MssqlSchILDDataSource : AbstractSchILDDataSource
    {
        private const string SqlQuery = "SELECT ID, Name, Vorname, Klasse, Geschlecht, SchulEmail FROM Schueler WHERE AktSchuljahr = (SELECT TOP 1 Schuljahr FROM EigeneSchule) AND AktAbschnitt = (SELECT TOP 1 SchuljahrAbschnitt FROM EigeneSchule)";

        protected override DatabaseType Type { get { return DatabaseType.MSSQL; } }

        public MssqlSchILDDataSource(ISettingsService settingsService)
            : base(settingsService)
        {
        }

        public override async Task<IEnumerable<Student>> LoadStudentsAsync()
        {
            using (var connection = new SqlConnection(GetConnectionString())) 
            {
                await connection.OpenAsync();

                // query all students
                var studentsCommand = connection.CreateCommand();
                studentsCommand.CommandText = SqlQuery;

                using (var studentsReader = await studentsCommand.ExecuteReaderAsync())
                {
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
                            Gender = GetGender((short)studentsReader["Geschlecht"])
                        };

                        students.Add(student);
                    }

                    return students;
                }
            }
        }

        public override async Task TestConnectionAsync(DatabaseType type, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
        }
    }
}
