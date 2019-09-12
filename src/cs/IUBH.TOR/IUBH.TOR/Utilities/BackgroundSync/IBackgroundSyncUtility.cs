namespace IUBH.TOR.Utilities.BackgroundSync
{
    public interface IBackgroundSyncUtility
    {
        /// <summary>
        /// Enables background synchronization. Should be called whenever
        /// the users signs in or the app is being started with valid
        /// credentials in the store. 
        /// </summary>
        void Enable();
        
        /// <summary>
        /// Disables background synchronization. Should be called whenever
        /// the user signs out.
        /// </summary>
        void Disable();
    }
}
