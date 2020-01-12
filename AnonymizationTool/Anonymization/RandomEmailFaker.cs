using AnonymizationTool.Settings;
using System;
using System.Linq;

namespace AnonymizationTool.Anonymization
{
    public class RandomEmailFaker : IEmailFaker
    {
        private const string characters = "abcdefghijklmnopqrstuvwxyz0123456789";

        private ISettingsService settingsService;
        private readonly Random random;

        public RandomEmailFaker(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            random = new Random();
        }

        public string GetEmail(string firstName, string lastName, int attempt)
        {
            // see https://stackoverflow.com/a/1344242
            return new string(Enumerable.Repeat(characters, settingsService.Settings.Email.RandomLength)
                    .Select(x => x[random.Next(settingsService.Settings.Email.RandomLength)])
                    .ToArray());
        }
    }
}
