using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class DownloadAndApply : MonoBehaviour
{
    public GameObject crabCubePrefab; 
    public Button button1;             
    public Button button2;            

    public Button button3;

    private Renderer cubeRenderer;

    void Start()
    {
        cubeRenderer = crabCubePrefab.GetComponent<Renderer>();

        button1.onClick.AddListener(() => LoadTattooFromURL("https://firebasestorage.googleapis.com/v0/b/inkar-135da.firebasestorage.app/o/tattoos%2Fcrabtattoo.png?alt=media&token=718b5c13-4215-4442-b296-daf096e32d86"));
        button2.onClick.AddListener(() => LoadTattooFromURL("https://firebasestorage.googleapis.com/v0/b/inkar-135da.firebasestorage.app/o/tattoos%2Fanchortattoo.png?alt=media&token=e2a7ac68-dae1-487d-a05f-1a9ca72f7c5d"));
        button3.onClick.AddListener(() => LoadTattooFromURL("https://firebasestorage.googleapis.com/v0/b/inkar-135da.firebasestorage.app/o/tattoos%2Fhearttattoo.png?alt=media&token=a3faa233-271e-4f7e-8315-ef1dec693fa3"));
    }

    public void LoadTattooFromURL(string imageUrl)
    {
        StartCoroutine(LoadAndApplyTexture(imageUrl));
    }

    private IEnumerator LoadAndApplyTexture(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            cubeRenderer.sharedMaterial.mainTexture = texture;

            Debug.Log("Texture applied, loading AR scene...");
            SceneManager.LoadScene("imageTrackingTattoo");
        }
    }
}
