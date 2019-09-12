using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Shared.Domain;

namespace IUBH.TOR.Modules.Shared.Services
{
    /// <summary>
    /// Offers the capability to store and retrieve credentials in a safe way.
    /// </summary>
    public interface ICredentialStorage
    {
        Task<Result> SaveCredentialsAsync(Credentials credentials);
        Task<Result<Credentials>> GetCredentialsAsync();
    }
}
