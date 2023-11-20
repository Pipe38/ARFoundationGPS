using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class ApplyTextureFromWeb : MonoBehaviour
{
    public string ImageURL;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DownloadImageFromWeb(ImageURL));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DownloadImageFromWeb(string imageUrl)
    {
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            // Get downloaded asset bundle
            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            Renderer myRenderer = GetComponent<Renderer>();

            myRenderer.material.mainTexture = texture;
        }

        yield return null;
    }
    
}
