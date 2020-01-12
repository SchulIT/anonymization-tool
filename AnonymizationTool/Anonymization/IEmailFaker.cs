namespace AnonymizationTool.Anonymization
{
    public interface IEmailFaker
    {
        string GetEmail(string firstName, string lastName, int attempt);
    }
}
