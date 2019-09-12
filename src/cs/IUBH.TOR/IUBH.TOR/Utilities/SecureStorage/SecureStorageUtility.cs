using System.Threading.Tasks;

namespace IUBH.TOR.Utilities.SecureStorage
{
    /// <summary>
    /// Provides a way to securely store information that must be protected.
    /// </summary>
    internal class SecureStorageUtility : ISecureStorageUtility
    {
        public Task<string> GetAsync(string key) => Xamarin.Essentials.SecureStorage.GetAsync(key);

        public Task SetAsync(string key, string value)
            => Xamarin.Essentials.SecureStorage.SetAsync(key, value);

        /// <summary>
        /// Removes all the data saved in the Storage
        /// </summary>
        public void Nuke() => Xamarin.Essentials.SecureStorage.RemoveAll();
    }
}
