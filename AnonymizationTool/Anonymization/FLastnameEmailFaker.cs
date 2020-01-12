namespace AnonymizationTool.Anonymization
{
    public class FLastnameEmailFaker : IEmailFaker
    {
        private FirstnameLastnameEmailFaker firstnameLastnameFaker;

        public FLastnameEmailFaker(FirstnameLastnameEmailFaker firstnameLastnameEmailFaker)
        {
            this.firstnameLastnameFaker = firstnameLastnameEmailFaker;
        }

        public string GetEmail(string firstName, string lastName, int attempt)
        {
            return firstnameLastnameFaker.GetEmail(GetFirstLetter(firstName), lastName, attempt);
        }

        private string GetFirstLetter(string firstName)
        {
            if(string.IsNullOrEmpty(firstName))
            {
                return "";
            }

            return firstName[0].ToString();
        }
    }
}
