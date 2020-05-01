using AnonymizationTool.Settings.Export.SchulIT.Idp;

namespace AnonymizationTool.Settings.Export
{
    public interface IExportSettings
    {
        IIdpSettings SchulITIdp { get; }
    }
}
