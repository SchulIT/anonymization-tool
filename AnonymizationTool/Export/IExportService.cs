using AnonymizationTool.Data.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Export
{
    public interface IExportService
    {
        Task ExportAsync(string directory, IEnumerable<AnonymousStudent> students);
    }
}
