using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace AnonymizationTool.Settings.Export.SchulIT.Idp
{
    public class JsonIdpSettings : ObservableObject, IIdpSettings
    {
        private string endpoint;

        [JsonProperty("endpoint")]
        public string Endpoint
        {
            get => endpoint;
            set => Set(ref endpoint, value);
        }

        private string token;

        [JsonProperty("token")]
        public string Token
        {
            get => token;
            set => Set(ref token, value);
        }

        private string firstnameAttributeName;

        [JsonProperty("firstname_attribute")]
        public string FirstnameAttributeName
        {
            get => firstnameAttributeName;
            set => Set(ref firstnameAttributeName, value);
        }

        private string lastnameAttributeName;

        [JsonProperty("lastname_attribute")]
        public string LastnameAttributeName
        {
            get => lastnameAttributeName;
            set => Set(ref lastnameAttributeName, value);
        }

        private string emailAttribtueName;

        [JsonProperty("email_attribute")]
        public string EmailAttributeName
        {
            get => emailAttribtueName;
            set => Set(ref emailAttribtueName, value);
        }
    }
}
