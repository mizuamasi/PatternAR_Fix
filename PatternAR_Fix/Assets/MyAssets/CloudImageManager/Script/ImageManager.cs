using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class ImageManager : MonoBehaviour
{
    public string gasUrl = "https://script.google.com/macros/s/AKfycbzY9koTi8XOyaGvA9UxyPwbNpWj87IOB4t8aEMdl2pxk-zdlXzwwpwoAQ6cWjUpqflC/exec";
    public int atlasSize = 4;
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    private Dictionary<string, string> imageUuidMap = new Dictionary<string, string>();

    public List<Texture2D> GetCachedTexturesForUuid(string uuid)
    {
        List<Texture2D> filteredTextures = new List<Texture2D>();
        foreach (var kvp in textureCache)
        {
            if (imageUuidMap.TryGetValue(kvp.Key, out string imageUuid) && imageUuid == uuid)
            {
                filteredTextures.Add(kvp.Value);
            }
        }
        return filteredTextures;
    }

    //private IEnumerator GetImageUrls(int count, List<string> imageUrls)
    // {
    //     string uuid = SystemInfo.deviceUniqueIdentifier;
    //     string url = $"{gasUrl}?uuid={uuid}&count={count}&random=true";

    //     using (UnityWebRequest www = UnityWebRequest.Get(url))
    //     {
    //         yield return www.SendWebRequest();

    //         if (www.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.LogError($"Error fetching image URLs: {www.error}");
    //         }
    //         else
    //         {
    //             string json = www.downloadHandler.text;
    //             ImageUrlsWrapper wrapper = JsonUtility.FromJson<ImageUrlsWrapper>(json);
    //             if (wrapper != null && wrapper.urls != null)
    //             {
    //                 imageUrls.AddRange(wrapper.urls);
    //                 foreach (string imageUrl in wrapper.urls)
    //                 {
    //                     imageUuidMap[imageUrl] = uuid;
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.LogError("Failed to parse JSON or urls is null");
    //             }
    //         }
    //     }
    // }

    public List<Texture2D> GetCachedTextures()
    {
        return new List<Texture2D>(textureCache.Values);
    }

    public IEnumerator GetImagesForBoids(int count, Action<List<Texture2D>> onTexturesDownloaded)
    {
        List<string> imageUrls = new List<string>();
        yield return StartCoroutine(GetImageUrls(count, imageUrls));

        List<Texture2D> textures = new List<Texture2D>();
        foreach (string imageUrl in imageUrls)
        {
            yield return StartCoroutine(DownloadTexture(imageUrl, textures));
        }

        onTexturesDownloaded(textures);
    }

    private IEnumerator GetImageUrls(int count, List<string> imageUrls)
    {
        string uuid = SystemInfo.deviceUniqueIdentifier;
        string url = $"{gasUrl}?uuid={uuid}&count={count}&random=true";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching image URLs: {www.error}");
            }
            else
            {
                string json = www.downloadHandler.text;
                ImageUrlsWrapper wrapper = JsonUtility.FromJson<ImageUrlsWrapper>(json);
                if (wrapper != null && wrapper.urls != null)
                {
                    imageUrls.AddRange(wrapper.urls);
                }
                else
                {
                    Debug.LogError("Failed to parse JSON or urls is null");
                }
            }
        }
    }

    private IEnumerator DownloadTexture(string url, List<Texture2D> textures)
    {
        url = ConvertToDirectDownloadLink(url);

        // キャッシュをチェック
        if (textureCache.TryGetValue(url, out Texture2D cachedTexture))
        {
            textures.Add(cachedTexture);
            Debug.Log($"Texture loaded from cache: {url}");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading texture from {url}: {www.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                textures.Add(texture);
                
                // キャッシュに追加
                textureCache[url] = texture;
                
                Debug.Log($"Texture downloaded and cached: {url}");
            }
        }
    }

    // private IEnumerator DownloadTexture(string url, List<Texture2D> textures)
    // {
    //     url = ConvertToDirectDownloadLink(url);

    //     using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
    //     {
    //         yield return www.SendWebRequest();

    //         if (www.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.LogError($"Error downloading texture from {url}: {www.error}");
    //         }
    //         else
    //         {
    //             Texture2D texture = DownloadHandlerTexture.GetContent(www);
    //             textures.Add(texture);
    //             Debug.Log($"Texture downloaded successfully from {url}");
    //         }
    //     }
    // }

    private string ConvertToDirectDownloadLink(string url)
    {
        if (url.Contains("drive.google.com") && url.Contains("/file/d/"))
        {
            string fileId = url.Split(new string[] { "/file/d/" }, StringSplitOptions.None)[1].Split('/')[0];
            return $"https://drive.google.com/uc?export=download&id={fileId}";
        }
        return url;
    }

    private void OnApplicationQuit()
    {
        ClearTextureCache();
    }

    private void ClearTextureCache(){
        foreach (var texture in textureCache.Values)
        {
            Destroy(texture);
        }
        textureCache.Clear();
    }
}



[System.Serializable]
public class ImageUrlsWrapper
{
    public List<string> urls;
}