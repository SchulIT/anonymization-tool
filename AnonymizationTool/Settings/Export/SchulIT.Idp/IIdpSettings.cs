using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymizationTool.Settings.Export.SchulIT.Idp
{
    public interface IIdpSettings
    {
        string Endpoint { get; set; }

        string Token { get; set; }

        string FirstnameAttributeName { get; set; }

        string LastnameAttributeName { get; set; }

        string EmailAttributeName { get; set; }
    }
}
