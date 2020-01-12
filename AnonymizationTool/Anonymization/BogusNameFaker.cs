using AnonymizationTool.Common;
using AnonymizationTool.Settings;
using Bogus;
using Microsoft.Extensions.Logging;

namespace AnonymizationTool.Anonymization
{
    public class BogusNameFaker : INameFaker
    {
        private const string Locale = "de"; 

        private readonly Faker faker;
        private readonly ISettingsService settingsService;

        private readonly ILogger<BogusNameFaker> logger;

        public BogusNameFaker(ISettingsService settingsService, ILogger<BogusNameFaker> logger)
        {
            this.settingsService = settingsService;
            this.logger = logger;

            faker = new Faker(Locale);
        }

        public string GetFirstName(Gender gender)
        {
            if(settingsService.Settings.KeepGender == false)
            {
                return faker.Name.FirstName(null);
            }

            return faker.Name.FirstName(GetBogusGender(gender));
        }

        public string GetLastName()
        {
            return faker.Name.LastName();
        }

        private Bogus.DataSets.Name.Gender? GetBogusGender(Gender gender)
        {
            switch(gender)
            {
                case Gender.Female:
                    return Bogus.DataSets.Name.Gender.Female;

                case Gender.Male:
                    return Bogus.DataSets.Name.Gender.Male;

                default:
                    return null;
            }
        }
    }
}
