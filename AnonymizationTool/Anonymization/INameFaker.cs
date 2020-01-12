using AnonymizationTool.Common;

namespace AnonymizationTool.Anonymization
{
    public interface INameFaker
    {
        string GetFirstName(Gender gender);

        string GetLastName();
    }
}
