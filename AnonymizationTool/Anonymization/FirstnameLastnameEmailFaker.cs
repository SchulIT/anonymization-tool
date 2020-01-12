using Unidecode.NET;

namespace AnonymizationTool.Anonymization
{
    public class FirstnameLastnameEmailFaker : IEmailFaker
    {
        private const string Format = "{0}.{1}{2}";


        public string GetEmail(string firstName, string lastName, int attempt)
        {
            return string.Format(Format, firstName, lastName, (attempt > 0 ? attempt.ToString() : ""))
                .Unidecode() // Transliterate ä -> ae, ü -> ue, ...
                .ToLower();
        }
    }
}
