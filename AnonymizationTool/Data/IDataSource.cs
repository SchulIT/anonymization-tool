using System.Threading.Tasks;

namespace AnonymizationTool.Data
{
    public interface IDataSource
    {
        bool CanConnect { get; }

        bool IsConnected { get; }

        Task ConnectAsync();

        Task DisconnectAsync();
    }
}
