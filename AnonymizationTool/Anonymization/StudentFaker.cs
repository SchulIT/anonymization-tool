using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Settings;
using Autofac.Features.Indexed;
using static AnonymizationTool.Settings.IEmailSettings;

namespace AnonymizationTool.Anonymization
{
    public class StudentFaker : IStudentFaker
    {
        private const string EmailFormat = "{0}@{1}";


        private readonly INameFaker nameFaker;
        private readonly IIndex<AnonymizationType, IEmailFaker> emailFakers;

        private readonly ISettingsService settingsService;

        public StudentFaker(INameFaker nameFaker, IIndex<AnonymizationType, IEmailFaker> emailFakers, ISettingsService settingsService)
        {
            this.nameFaker = nameFaker;
            this.emailFakers = emailFakers;
            this.settingsService = settingsService;
        }

        public void FakeStudent(AnonymousStudent student, int attempt)
        {
            var emailFaker = emailFakers[settingsService.Settings.Email.Type];

            if (string.IsNullOrEmpty(student.AnonymousFirstName))
            {
                student.AnonymousFirstName = nameFaker.GetFirstName(student.Gender);
            }

            if (string.IsNullOrEmpty(student.AnonymousLastName))
            {
                student.AnonymousLastName = nameFaker.GetLastName();
            }

            if (string.IsNullOrEmpty(student.AnonymousEmail))
            {
                student.AnonymousEmail = string.Format(EmailFormat,
                    emailFaker.GetEmail(student.AnonymousFirstName, student.AnonymousLastName, attempt),
                    settingsService.Settings.Email.Domain
                );
            }
        }
    }
}
