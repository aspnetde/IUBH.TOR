namespace IUBH.TOR.Utilities.Hud
{
    public interface IHudUtility
    {
        /// <summary>
        /// Shows a "Head Up Display" given the message provided. That can
        /// be used to block the UI, showing the message along with a "spinner"
        /// while some task is being processed in the background.
        /// </summary>
        void Show(string message);
        
        /// <summary>
        /// Dismisses the HUD.
        /// </summary>
        void Dismiss();
    }
}
