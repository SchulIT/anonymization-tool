using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public interface ISchILDDataSource : IDataSource
    {
        Task TestConnectionAsync();

        Task<IEnumerable<Student>> LoadStudentsAsync();
    }
}
