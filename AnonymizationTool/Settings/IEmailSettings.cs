namespace AnonymizationTool.Settings
{
    public interface IEmailSettings
    {
        public enum AnonymizationType
        {
            /// <summary>
            /// Completely random string (you should specify the length)
            /// </summary>
            Random = 0,

            /// <summary>
            /// Applies the format firstname.lastname@domain
            /// </summary>
            FirstnameLastname = 1,

            /// <summary>
            /// Applies the format f.lastname@domain, with f being the first letter of the firstname
            /// </summary>
            FLastname = 2
        }

        AnonymizationType Type { get; set; }

        string Domain { get; set; }

        int RandomLength { get; set; }
    }
}
