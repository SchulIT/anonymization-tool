using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnonymizationTool.Data.Persistence
{
    public class MemoryDataSource : IPersistentDataSource
    {

        public bool IsInMemory { get { return true; } }

        public bool CanConnect { get { return true; } }

        private readonly List<AnonymousStudent> students = new List<AnonymousStudent>();

        public bool IsConnected { get; private set; }

        #region Events
        public event ConnectionStateChangedEventHandler<IPersistentDataSource> ConnectionStateChanged;

        private void OnConnectedStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs<IPersistentDataSource>(this, IsConnected));
        }
        #endregion

        public Task ConnectAsync()
        {
            IsConnected = true;
            OnConnectedStateChanged();

            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            OnConnectedStateChanged();

            return Task.CompletedTask;
        }

        public Task<List<AnonymousStudent>> LoadStudentsAsync()
        {
            return new Task<List<AnonymousStudent>>(() => students.ToList());
        }

        public void AddStudent(AnonymousStudent student)
        {
            students.Add(student);
        }

        public void UpdateStudent(AnonymousStudent student)
        {
            
        }

        public void RemoveStudent(AnonymousStudent student)
        {
            students.Remove(student);
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }

        public bool IsSupported(DatabaseType type)
        {
            return true;
        }

        public Task TestConnectionAsync(DatabaseType type, string connectionString)
        {
            return Task.CompletedTask;
        }
    }
}
