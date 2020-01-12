using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.SchILD
{
    public interface ISchILDDataSource : IDataSource
    {
        event ConnectionStateChangedEventHandler<ISchILDDataSource> ConnectionStateChanged;

        Task<IEnumerable<Student>> LoadStudentsAsync();
    }
}
