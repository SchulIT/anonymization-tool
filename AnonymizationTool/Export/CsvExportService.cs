using AnonymizationTool.Data.Persistence;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationTool.Export
{
    public class CsvExportService : IExportService
    {
        private const string FileFormat = "{0}.csv";

        public Task ExportAsync(string directory, IEnumerable<AnonymousStudent> students)
        {
            return Task.Run(() =>
            {
                var grades = students.Select(s => s.Grade).Distinct().ToList();

                foreach (var grade in grades)
                {
                    var path = Path.Combine(directory, string.Format(FileFormat, grade));

                    var studentsInGrade = students.Where(s => s.Grade == grade).ToList();

                    using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        using (var csv = new CsvWriter(writer))
                        {
                            csv.WriteRecords(studentsInGrade);
                        }
                    }
                }
            });
        }
    }
}
