using UnityEngine;
using UnityEngine.UI;
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
    public GameObject loadingObject;

    public event Action<Texture2D> OnTextureSelected;

    private CustomBoidObject targetBoidObject;
    private List<Texture2D> displayedTextures = new List<Texture2D>();
    private int currentImageIndex = 0;
    private const int batchSize = 10;

    private void Start()
    {
        StartCoroutine(LoadImagesCoroutine());
    }

    public void Open(CustomBoidObject boidObject)
    {
        targetBoidObject = boidObject;
        gameObject.SetActive(true);
        if (displayedTextures.Count == 0)
        {
            StartCoroutine(LoadImagesCoroutine());
        }
    }

    private IEnumerator LoadImagesCoroutine()
    {
        loadingObject.SetActive(true);
        
        while (true)
        {
            int remainingImages = Math.Max(0, 30 - currentImageIndex);
            int imagesToLoad = Math.Min(batchSize, remainingImages);
            
            if (imagesToLoad == 0)
            {
                break;
            }

            yield return StartCoroutine(imageManager.GetImagesForBoids(imagesToLoad, OnNewTexturesLoaded));
            
            yield return new WaitForSeconds(0.1f);
        }

        loadingObject.SetActive(false);
    }

    private void OnNewTexturesLoaded(List<Texture2D> newTextures)
    {
        foreach (var texture in newTextures)
        {
            if (!displayedTextures.Contains(texture))
            {
                displayedTextures.Add(texture);
                CreateButton(texture, currentImageIndex);
                currentImageIndex++;
            }
        }
        UpdateContentSize();
    }

    private void CreateButton(Texture2D texture, int index)
    {
        RectTransform contentTransform = scrollRect.content;
        float contentWidth = contentTransform.rect.width;
        float buttonWidth = (contentWidth - (columnsCount + 1) * spacing) / columnsCount;
        float buttonHeight = buttonWidth;

        int row = index / columnsCount;
        int col = index % columnsCount;

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
        buttonImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => OnButtonClick(texture));
    }

    private void UpdateContentSize()
    {
        RectTransform contentTransform = scrollRect.content;
        float contentWidth = contentTransform.rect.width;
        float buttonWidth = (contentWidth - (columnsCount + 1) * spacing) / columnsCount;
        float buttonHeight = buttonWidth;

        float contentHeight = Mathf.Ceil(displayedTextures.Count / (float)columnsCount) * (buttonHeight + spacing) + spacing;
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, contentHeight);
    }

    private void OnButtonClick(Texture2D selectedTexture)
    {
        if (targetBoidObject != null)
        {
            bool success = ApplyTextureToCustomBoid(targetBoidObject, selectedTexture);
            if (success)
            {
                Debug.Log($"Texture applied to Boid: {selectedTexture.name}");
                OnTextureSelected?.Invoke(selectedTexture);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Failed to apply texture to Boid. Check if the Boid has the necessary components.");
            }
        }
        else
        {
            Debug.LogWarning("No target Boid object set.");
        }
    }

    public bool ApplyTextureToCustomBoid(CustomBoidObject customBoid, Texture2D newTexture)
    {
        if (customBoid == null || newTexture == null)
        {
            Debug.LogError("CustomBoid or new texture is null.");
            return false;
        }

        if (customBoid.previewRenderer == null)
        {
            Debug.LogError("Preview Renderer is not assigned in CustomBoidObject.");
            return false;
        }

        try
        {
            customBoid.SetCustomTexture(newTexture);
            Debug.Log($"Texture successfully applied to Boid: {newTexture.name}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error applying texture to Boid: {e.Message}");
            return false;
        }
    }

    public void ClearGallery()
    {
        foreach (Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }
        displayedTextures.Clear();
        currentImageIndex = 0;
    }
}