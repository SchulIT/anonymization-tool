using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.Persistence
{
    public interface IPersistentDataSource : IDataSource
    {
        bool IsConnected { get; }

        event ConnectionStateChangedEventHandler<IPersistentDataSource> ConnectionStateChanged;

        Task ConnectAsync();

        Task DisconnectAsync();

        void AddStudent(AnonymousStudent student);

        void UpdateStudent(AnonymousStudent student);

        void RemoveStudent(AnonymousStudent student);

        Task SaveChangesAsync();

        Task<List<AnonymousStudent>> LoadStudentsAsync();
    }
}
