using AnonymizationTool.Settings.Export.SchulIT.Idp;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.ComponentModel;

namespace AnonymizationTool.Settings.Export
{
    public class JsonExportSettings : ObservableObject, IExportSettings
    {

        [JsonProperty("schulit_idp")]
        public IIdpSettings SchulITIdp { get; } = new JsonIdpSettings();

        public JsonExportSettings()
        {
            var idpSettings = SchulITIdp as JsonIdpSettings;
            if(idpSettings != null)
            {
                idpSettings.PropertyChanged += OnPropertyChanged;
            }
        }


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged();
        }
    }
}
