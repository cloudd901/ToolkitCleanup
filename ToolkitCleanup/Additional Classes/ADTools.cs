namespace FMOLAssistant
{
    using System.DirectoryServices.AccountManagement;

    public static class ADTools
    {
        public static bool ValidateCredentials(string user, string pass)
        {
            using PrincipalContext pc = new PrincipalContext(ContextType.Domain);
            return pc.ValidateCredentials(user, pass);
        }
    }
}