using System.Threading.Tasks;

namespace IUBH.TOR.Utilities.SecureStorage
{
    /// <summary>
    /// Provides a way to securely store information that must be protected.
    /// </summary>
    public interface ISecureStorageUtility
    {
        Task<string> GetAsync(string key);
        Task SetAsync(string key, string value);
        
        /// <summary>
        /// Removes all the data saved in the Storage
        /// </summary>
        void Nuke();
    }
}
