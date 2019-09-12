using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Shared.Domain;

namespace IUBH.TOR.Modules.Authentication.Services
{
    public interface ICredentialValidator
    {
        /// <summary>
        /// Makes sure the given credentials are valid so they can
        /// be used to log in to the CARE system.
        /// </summary>
        Task<Result> ValidateAsync(Credentials credentials);
    }
}
