using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnonymizationTool.Export.SchulIT.Idp.Request
{
    public class UserAttributes
    {
        [JsonProperty("attributes")]
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
    }
}
