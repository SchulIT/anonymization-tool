using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Messages;
using CsvHelper;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationTool.Export.Csv
{
    public class CsvExportService : IExportService
    {
        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void OnRaiseProgressChangedEvent(ProgressChangedEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        private const string FileFormat = "{0}.csv";

        private readonly IMessenger messenger;

        public CsvExportService(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public Task ExportAsync(IEnumerable<AnonymousStudent> students)
        {
            var msg = new SelectDirectoryDialogMessage();
            messenger.Send(msg);

            if (string.IsNullOrEmpty(msg.Path))
            {
                messenger.Send(new DialogMessage { Title = "Vorgang abgebrochen", Header = "Export abgebrochen", Text = "Es wurden keine Daten exportiert." });
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                var grades = students.Select(s => s.Grade).Distinct().ToList();
                var i = 0;

                foreach (var grade in grades)
                {
                    OnRaiseProgressChangedEvent(new ProgressChangedEventArgs(i, grades.Count, $"Exportiere CSV-Dateien ({i + 1}/{grades.Count})..."));
                    var path = Path.Combine(msg.Path, string.Format(FileFormat, grade));

                    var studentsInGrade = students.Where(s => s.Grade == grade).ToList();

                    using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        using (var csv = new CsvWriter(writer))
                        {
                            csv.WriteRecords(studentsInGrade);
                        }
                    }

                    i++;
                }
            });
        }
    }
}
