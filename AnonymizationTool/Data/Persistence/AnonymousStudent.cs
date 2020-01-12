using AnonymizationTool.Common;
using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnonymizationTool.Data.Persistence
{
    public class AnonymousStudent
    {
        [Ignore]
        public int Id { get; set; }

        [Ignore]
        public string SchILDId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Grade { get; set; }

        [Ignore]
        public Gender Gender { get; set; }

        public string AnonymousFirstName { get; set; }

        public string AnonymousLastName { get; set; }

        public string AnonymousEmail { get; set; }

        [Ignore]
        public bool IsRemoved { get; set; }

        [NotMapped]
        [Ignore]
        public bool? IsMissingInSchILD { get; set; }
    }
}
