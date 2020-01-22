using AnonymizationTool.Settings;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public class AccessOdbcSchILDDataSource : AbstractSchILDDataSource
    {
        private const string SqlQuery = "SELECT ID, Name, Vorname, Klasse, Geschlecht, SchulEmail FROM Schueler WHERE AktSchuljahr = (SELECT TOP 1 Schuljahr FROM EigeneSchule) AND AktAbschnitt = (SELECT TOP 1 SchuljahrAbschnitt FROM EigeneSchule)";

        protected override DatabaseType Type { get { return DatabaseType.Access; } }

        public AccessOdbcSchILDDataSource(ISettingsService settingsService)
            : base(settingsService)
        {
        }

        public override async Task<IEnumerable<Student>> LoadStudentsAsync()
        {
            using (var connection = new OdbcConnection(GetConnectionString()))
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
                            Gender = GetGender((byte)studentsReader["Geschlecht"])
                        };

                        students.Add(student);
                    }

                    return students;
                }
            }
        }

        public override async Task TestConnectionAsync()
        {
            using (var connection = new OdbcConnection(GetConnectionString()))
            {
                await connection.OpenAsync();
            }
        }
    }
}
