using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public interface ISchILDDataSource : IDataSource
    {
        Task<IEnumerable<Student>> LoadStudentsAsync();
    }
}
