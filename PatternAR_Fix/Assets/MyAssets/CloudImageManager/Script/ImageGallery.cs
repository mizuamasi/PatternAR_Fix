using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class ImageGallery : MonoBehaviour
{
    public ScrollRect scrollRect;
    public GameObject buttonPrefab;
    public int columnsCount = 3;
    public float spacing = 10f;
    public ImageManager imageManager;
    public GameObject loadingObject; // ローディング表示用のオブジェクト

    public event Action<Texture2D> OnTextureSelected;

    public CustomBoidObject targetBoidObject;

    private List<Texture2D> cachedTextures = new List<Texture2D>();

    public float connectionTimeoutSeconds = 5f;
    private const string connectionCheckUrl = "https://www.google.com";

    private void Start()
    {
        LoadImagesAndCreateButtons();
    }

    public void LoadImagesAndCreateButtons()
    {
        cachedTextures = imageManager.GetCachedTextures();
        if (cachedTextures.Count > 0)
        {
            CreateButtons(cachedTextures);
        }
        StartCoroutine(LoadImagesCoroutine());
    }

    private IEnumerator LoadImagesCoroutine()
    {
        int additionalImagesNeeded = Math.Max(0, 30 - cachedTextures.Count);
        if (additionalImagesNeeded > 0)
        {
            loadingObject.SetActive(true);
            bool isConnected = false;
            yield return StartCoroutine(CheckInternetConnection(result => isConnected = result));
            
            if (isConnected)
            {
                yield return StartCoroutine(imageManager.GetImagesForBoids(additionalImagesNeeded, OnNewTexturesLoaded));
            }
            else
            {
                Debug.LogWarning("No internet connection. Using only cached images.");
                OnNewTexturesLoaded(new List<Texture2D>());
            }
            loadingObject.SetActive(false);
        }
    }

    private IEnumerator CheckInternetConnection(Action<bool> callback)
    {
        UnityWebRequest request = new UnityWebRequest(connectionCheckUrl);
        request.timeout = Mathf.RoundToInt(connectionTimeoutSeconds);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            callback(true);
        }
        else
        {
            Debug.LogWarning($"Internet connection check failed: {request.error}");
            callback(false);
        }
    }


    private bool CheckInternetConnection()
    {
        // インターネット接続をチェックするロジックをここに実装
        // 例: Unity の Ping クラスを使用する
        // この例では簡単のため、常に接続があると仮定しています
        return true;
    }

    private void OnNewTexturesLoaded(List<Texture2D> newTextures)
    {
        List<Texture2D> allTextures = new List<Texture2D>(cachedTextures);
        allTextures.AddRange(newTextures);
        CreateButtons(allTextures);
    }

    private void CreateButtons(List<Texture2D> textures)
    {
        // 既存のボタンをクリア
        foreach (Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }

        RectTransform contentTransform = scrollRect.content;
        float contentWidth = contentTransform.rect.width;
        float buttonWidth = (contentWidth - (columnsCount + 1) * spacing) / columnsCount;
        float buttonHeight = buttonWidth;

        for (int i = 0; i < textures.Count; i++)
        {
            int row = i / columnsCount;
            int col = i % columnsCount;

            GameObject buttonObj = Instantiate(buttonPrefab, contentTransform);
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            
            rectTransform.anchoredPosition = new Vector2(
                spacing + col * (buttonWidth + spacing) + buttonWidth / 2,
                -spacing - row * (buttonHeight + spacing) - buttonHeight / 2
            );

            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);

            Button button = buttonObj.GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => OnButtonClick(textures[index]));
        }

        float contentHeight = Mathf.Ceil(textures.Count / (float)columnsCount) * (buttonHeight + spacing) + spacing;
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, contentHeight);
    }

    private void OnButtonClick(Texture2D selectedTexture)
    {
        if (targetBoidObject != null)
        {
            targetBoidObject.SetCustomTexture(selectedTexture);
        }
        OnTextureSelected?.Invoke(selectedTexture);
        gameObject.SetActive(false);
    }

    public void Open(CustomBoidObject boidObject)
    {
        targetBoidObject = boidObject;
        gameObject.SetActive(true);
        LoadImagesAndCreateButtons();
    }

    public void ApplyTextureToCustomBoid(CustomBoidObject customBoid, Texture2D newTexture)
    {
        if (customBoid != null && newTexture != null)
        {
            Material boidMaterial = customBoid.GetComponent<Renderer>().material;
            
            // 現在のマテリアルのパラメータを保存
            Color backColor = boidMaterial.GetColor("_BackColor");
            Color bellyColor = boidMaterial.GetColor("_BellyColor");
            float colorStrength = boidMaterial.GetFloat("_ColorStrength");
            float patternStrength = boidMaterial.GetFloat("_PatternStrength");
            // ... 他の必要なパラメータも同様に保存

            // テクスチャのみを変更
            boidMaterial.SetTexture("_MainTex", newTexture);

            // 保存したパラメータを再設定
            boidMaterial.SetColor("_BackColor", backColor);
            boidMaterial.SetColor("_BellyColor", bellyColor);
            boidMaterial.SetFloat("_ColorStrength", colorStrength);
            boidMaterial.SetFloat("_PatternStrength", patternStrength);
            // ... 他の保存したパラメータも同様に再設定

            // CustomBoidObject のパラメータも更新
            customBoid.parameters.customTexture = newTexture;
        }
    }

    // このメソッドをボタンクリック時に呼び出す
    public void OnTextureButtonClicked(CustomBoidObject customBoid, Texture2D newTexture)
    {
        ApplyTextureToCustomBoid(customBoid, newTexture);
    }
}