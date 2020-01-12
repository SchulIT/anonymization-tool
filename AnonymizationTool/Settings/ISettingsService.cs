using System.Threading.Tasks;

namespace AnonymizationTool.Settings
{
    public interface ISettingsService
    {
        ISettings Settings { get; }

        event SettingsChangedEventHandler Changed;

        Task SaveAsync();
    }
}
