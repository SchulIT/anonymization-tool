using AnonymizationTool.Common;

namespace AnonymizationTool.Data.SchILD
{
    public class Student
    {
        public string Id { get; internal set; }

        public string FirstName { get; internal set; }

        public string LastName { get; internal set; }

        public string Email { get; internal set; }

        public Gender Gender { get; internal set; }

        public string Grade { get; internal set; }
    }
}
