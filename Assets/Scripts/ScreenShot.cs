using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    [Header("UI to Hide During Screenshot")]
    [SerializeField] private GameObject[] uiToHide;

    [Header("UI to Show During Screenshot")]
    [SerializeField] private GameObject[] uiToShow;

    public void TakeScreenshotToGallery()
    {
        StartCoroutine(CaptureAndSave());
    }

    private IEnumerator CaptureAndSave()
    {
        // Hide UI elements
        foreach (var go in uiToHide)
        {
            if (go != null) go.SetActive(false);
        }

        foreach(var go in uiToShow)
        {
            if (go != null) go.SetActive(true);
        }

        yield return new WaitForEndOfFrame();

        // Capture the screen
        Texture2D screenImage = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] imageBytes = screenImage.EncodeToPNG();
        Object.Destroy(screenImage);

        // Save to internal storage
        string filename = "screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, imageBytes);

#if UNITY_ANDROID
        // Copy to gallery path
        string galleryPath = Path.Combine("/storage/emulated/0/Pictures", filename);
        File.Copy(path, galleryPath, true);

        // Refresh Android gallery
        using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaClass mediaScannerConnection = new AndroidJavaClass("android.media.MediaScannerConnection"))
        {
            mediaScannerConnection.CallStatic("scanFile", activity, new string[] { galleryPath }, null, null);
        }

        Debug.Log("Screenshot saved to gallery: " + galleryPath);
#endif

        yield return new WaitForSeconds(0.3f); // Just in case

        // Show UI elements again
        foreach (var go in uiToHide)
        {
            if (go != null) go.SetActive(true);
        }

        foreach(var go in uiToShow)
        {
            if (go != null) go.SetActive(false);
        }
    }
}