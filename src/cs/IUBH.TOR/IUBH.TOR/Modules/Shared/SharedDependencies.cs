using IUBH.TOR.Modules.Shared.Services;

namespace IUBH.TOR.Modules.Shared
{
    internal static class SharedDependencies
    {
        public static void Register()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            container.Register<ICredentialStorage, CredentialStorage>();
        }
    }
}
