using System;
using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Shared.Domain;
using IUBH.TOR.Utilities.SecureStorage;

namespace IUBH.TOR.Modules.Shared.Services
{
    /// <summary>
    /// Offers the capability to store and retrieve credentials in a safe way.
    /// </summary>
    internal class CredentialStorage : ICredentialStorage
    {
        private readonly ISecureStorageUtility _secureStorageUtility;

        public const string SecureStorageUserNameKey = "CARE:UserName";
        public const string SecureStoragePasswordKey = "CARE:Password";

        public CredentialStorage(ISecureStorageUtility secureStorageUtility)
        {
            _secureStorageUtility = secureStorageUtility;
        }

        public async Task<Result> SaveCredentialsAsync(Credentials credentials)
        {
            try
            {
                await Task.WhenAll(
                    _secureStorageUtility.SetAsync(SecureStorageUserNameKey, credentials.UserName),
                    _secureStorageUtility.SetAsync(SecureStoragePasswordKey, credentials.Password)
                ).ConfigureAwait(false);

                return Result.Success;
            }
            catch (Exception e)
            {
                return Result.WithException(e);
            }
        }

        public async Task<Result<Credentials>> GetCredentialsAsync()
        {
            try
            {
                var getUserNameTask = _secureStorageUtility.GetAsync(SecureStorageUserNameKey);
                var getPasswordTask = _secureStorageUtility.GetAsync(SecureStoragePasswordKey);

                await Task.WhenAll(getPasswordTask, getUserNameTask).ConfigureAwait(false);

                // Both user name and password must be there. It's not likely to happen
                // that we only find one of them, but it's not impossible as we can't save
                // the data in a combined transaction. So we make sure here.
                if (getPasswordTask.Result == null || getUserNameTask.Result == null)
                {
                    return Result.WithError<Credentials>("User name or password not found.");
                }

                return Result.WithSuccess(
                    new Credentials(getUserNameTask.Result, getPasswordTask.Result)
                );
            }
            catch (Exception e)
            {
                return Result.WithException<Credentials>(e);
            }
        }
    }
}
