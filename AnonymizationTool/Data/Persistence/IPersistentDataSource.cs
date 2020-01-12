using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.Persistence
{
    public interface IPersistentDataSource : IDataSource
    {
        event ConnectionStateChangedEventHandler<IPersistentDataSource> ConnectionStateChanged;

        void AddStudent(AnonymousStudent student);

        void UpdateStudent(AnonymousStudent student);

        void RemoveStudent(AnonymousStudent student);

        Task SaveChangesAsync();

        Task<List<AnonymousStudent>> LoadStudentsAsync();
    }
}
