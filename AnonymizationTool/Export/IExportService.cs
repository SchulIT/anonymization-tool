using AnonymizationTool.Data.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Export
{
    public interface IExportService
    {
        event ProgressChangedEventHandler ProgressChanged;

        Task ExportAsync(IEnumerable<AnonymousStudent> students);
    }
}
