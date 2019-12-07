namespace Controllers.UI
{
    public interface IUIController
    {
        void HandleGameFinished(short index);

        bool AreSettingsVisible();

        void ToggleSettings();

        void OpenPrivacyPolicy();
    }
}