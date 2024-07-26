using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TextureSelector : MonoBehaviour
{
    public GameObject texturePrefab;
    public Transform contentParent;
    public CustomBoidObject targetBoidObject;

    public ImageManager imageManager;

    
    void Awake()
    {
        Debug.Log("TextureSelector Awake");
    }

    void OnEnable()
    {
        Debug.Log("TextureSelector Enabled");
        PopulateTextureList();
    }
        

    private void Start()
    {
        //imageManager = FindObjectOfType<ImageManager>();
        gameObject.SetActive(false);
    }

    public void PopulateTextureList()
    {
        Debug.Log("PopulateTextureList called");
        if (contentParent == null)
        {
            Debug.LogError("Content Parent is not assigned!");
            return;
        }
        if (imageManager == null)
        {
            Debug.LogError("ImageManager is not found!");
            return;
        }
        // Clear existing content
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Populate with cached textures
        foreach (var texture in imageManager.GetCachedTextures())
        {
            GameObject textureObject = Instantiate(texturePrefab, contentParent);
            RawImage rawImage = textureObject.GetComponent<RawImage>();
            rawImage.texture = texture;

            Button button = textureObject.GetComponent<Button>();
            button.onClick.AddListener(() => ApplyTextureToTargetBoid(texture));
        }
    }

    private void ApplyTextureToTargetBoid(Texture2D texture)
    {
        if (targetBoidObject != null)
        {
            targetBoidObject.SetCustomTexture(texture);
        }
        gameObject.SetActive(false);
    }
}