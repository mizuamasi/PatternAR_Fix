using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LoadImages : MonoBehaviour
{
    public static LoadImages Instance;
    public string gasUrl = "https://script.google.com/macros/s/AKfycbzY9koTi8XOyaGvA9UxyPwbNpWj87IOB4t8aEMdl2pxk-zdlXzwwpwoAQ6cWjUpqflC/exec";
    
    public List<Texture2D> loadedTextures = new List<Texture2D>();
    public bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator Start()
    {
        yield return StartCoroutine(LoadTexturesFromServer());
    }

    public IEnumerator LoadTexturesFromServer()
    {
        isLoading = true;
        string uuid = DeviceUUID.GetUUID();
        string url = gasUrl + "?uuid=" + uuid;

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            List<string> imageUrls = JsonUtility.FromJson<List<string>>(www.downloadHandler.text);
            foreach (string imageUrl in imageUrls)
            {
                yield return StartCoroutine(LoadImage(imageUrl));
            }
        }
        isLoading = false;
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            loadedTextures.Add(myTexture);
        }
    }
}