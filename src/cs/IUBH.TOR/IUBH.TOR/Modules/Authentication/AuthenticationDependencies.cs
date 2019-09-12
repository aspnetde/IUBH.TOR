using IUBH.TOR.Modules.Authentication.Services;

namespace IUBH.TOR.Modules.Authentication
{
    internal static class AuthenticationDependencies
    {
        public static void Register()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            container.Register<ICredentialValidator, CredentialValidator>();
        }
    }
}
