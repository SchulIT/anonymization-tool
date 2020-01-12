using AnonymizationTool.Common;
using AnonymizationTool.Settings;
using Bogus;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unidecode.NET;

namespace AnonymizationTool.Data.SchILD
{
    public class FakeSchILDDataService : ISchILDDataSource
    {
        private const string Locale = "de";

        public bool CanConnect { get { return true; } }

        public bool IsConnected { get; private set; }

        public event ConnectionStateChangedEventHandler<ISchILDDataSource> ConnectionStateChanged;

        private void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs<ISchILDDataSource>(this, IsConnected));
        }

        private readonly ISettingsService settingsService;
        private readonly ILogger<FakeSchILDDataService> logger;

        public FakeSchILDDataService(ISettingsService settingsService, ILogger<FakeSchILDDataService> logger)
        {
            this.settingsService = settingsService;
            this.logger = logger;
        }

        public Task ConnectAsync()
        {
            IsConnected = true;
            OnConnectionStateChanged();

            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            OnConnectionStateChanged();

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Student>> LoadStudentsAsync()
        {
            var grades = new List<string> { "5A", "6A", "EF", "Q1" };

            return Task.Run(() =>
            {
                var userId = 1;

                return new Faker<Student>(Locale)
                .UseSeed(1)
                    .CustomInstantiator(x => new Student { Id = (userId++).ToString() })
                    .RuleFor(s => s.Gender, f => f.PickRandom<Gender>())
                    .RuleFor(s => s.FirstName, (f, s) => f.Name.FirstName(s.Gender == Gender.Male ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female))
                    .RuleFor(s => s.LastName, (f, s) => f.Name.LastName())
                    .RuleFor(s => s.Email, (f, s) => $"{s.FirstName.Unidecode().ToLower()}.{s.LastName.Unidecode().ToLower()}@example.com")
                    .RuleFor(s => s.Grade, (f, s) => f.PickRandom(grades))
                    .GenerateLazy(50);
            });
        }
    }
}
