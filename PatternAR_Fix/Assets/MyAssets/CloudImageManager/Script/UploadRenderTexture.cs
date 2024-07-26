using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class UploadRenderTexture : MonoBehaviour
{
    public string gasUrl = "https://script.google.com/macros/s/AKfycbzY9koTi8XOyaGvA9UxyPwbNpWj87IOB4t8aEMdl2pxk-zdlXzwwpwoAQ6cWjUpqflC/exec"; // Google Apps ScriptのURLを設定
    public RenderTexture renderTexture; // アップロードするRenderTextureを設定
    public GameObject loadingIndicator; // ローディングインディケーターのゲームオブジェクトを設定

    public IEnumerator UploadPNG()
    {
        // ローディングインディケーターをアクティブにする
        loadingIndicator.SetActive(true);
        Debug.Log("Uploading");

        Texture2D tex = RenderTextureToTexture2D(renderTexture);
        byte[] bytes = tex.EncodeToPNG();
        string uuid = DeviceUUID.GetUUID();
        string base64Image = Convert.ToBase64String(bytes);
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        string imageName = uuid + "_" + timestamp + ".png";

        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("image", base64Image);
        form.AddField("imageName", imageName);
        form.AddField("imageType", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post(gasUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Uploaded successfully: " + responseText);
            }
        }

        // ローディングインディケーターを非アクティブにする
        loadingIndicator.SetActive(false);
    }

    private Texture2D RenderTextureToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }

    public void OnClick()
    {
        Debug.Log("Uploading");
        StartCoroutine(UploadPNG());
    }
}
