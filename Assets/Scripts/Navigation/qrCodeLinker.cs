using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class qrCodeLinker : MonoBehaviour
{
    [SerializeField] private Button qrCodeButton;
    [SerializeField] private string qrCodeUrl = "https://firebasestorage.googleapis.com/v0/b/inkar-135da.firebasestorage.app/o/QRcodes.pdf?alt=media&token=1b1757f9-0c60-4543-99f5-bbec36b9bfd3";

    private bool returningFromExternalLink = false;

    private void Start()
    {
        if (qrCodeButton != null)
            qrCodeButton.onClick.AddListener(OpenQRCodes);
    }

    public void OpenQRCodes()
    {
        Application.OpenURL(qrCodeUrl);
        returningFromExternalLink = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && returningFromExternalLink)
        {
            RefreshScene();
        }
    }

    private void RefreshScene()
    {
        // Reload the active scene to refresh all UI components, this avoids buttons becoming disconnected after the app is paused and resumed
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        returningFromExternalLink = false; // Reset bool
    }
}
