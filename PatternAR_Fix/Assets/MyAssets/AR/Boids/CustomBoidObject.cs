using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CustomBoidObject : MonoBehaviour
{
    [Header("Parameters")]
    public CustomBoidParameters parameters;

    [Header("UI Controls")]
    public Button addToFlockButton;
    public Button selectTextureButton;

    [Header("Preview")]
    public Renderer previewRenderer;

    public TextureSelector textureSelector;

    private void Start()
    {
        // textureSelector = FindObjectOfType<TextureSelector>(true); // includeInactive を true に設定
        // if (textureSelector != null)
        // {
        //     textureSelector.targetBoidObject = this;
        //     Debug.Log("TextureSelector found and assigned");
        // }
        // else
        // {
        //     Debug.LogError("TextureSelector not found in the scene!");
        // }
        // if (addToFlockButton != null)
        // {
        //     addToFlockButton.onClick.AddListener(AddToFlock);
        // }

        // if (selectTextureButton != null)
        // {
        //     selectTextureButton.onClick.AddListener(OpenTextureSelector);
        // }

        // textureSelector = FindObjectOfType<TextureSelector>();
        if (textureSelector != null)
        {
            textureSelector.targetBoidObject = this;
        }

        UpdatePreview();
    }

    public void AddToFlock()
    {
        Debug.Log("addtoflock");
        CustomBoidManager.Instance.AddCustomBoidToFlock(this);
    }

    private void UpdatePreview()
    {
        if (previewRenderer != null)
        {
            Material previewMaterial = new Material(previewRenderer.sharedMaterial);
            previewMaterial.SetColor("_BackColor", parameters.backColor);
            previewMaterial.SetColor("_BellyColor", parameters.bellyColor);
            previewMaterial.SetColor("_PatternColor", parameters.patternColor);
            
            if (parameters.customTexture != null)
            {
                previewMaterial.SetTexture("_MainTex", parameters.customTexture);
            }

            previewRenderer.material = previewMaterial;
            transform.localScale = Vector3.one * parameters.scale;
        }
    }

    private void OnValidate()
    {
        UpdatePreview();
    }

    public void SetCustomTexture(Texture2D texture)
    {
        parameters.customTexture = texture;
        UpdatePreview();
    }

    private void OpenTextureSelector()
    {
        Debug.Log("OpenTextureSelector called");
        if (textureSelector == null)
        {
            textureSelector = FindObjectOfType<TextureSelector>(true);
            Debug.Log(textureSelector != null ? "TextureSelector found" : "TextureSelector not found");
        }
        
        if (textureSelector != null)
        {
            textureSelector.gameObject.SetActive(true);
            textureSelector.PopulateTextureList();
        }
        else
        {
            Debug.LogError("TextureSelector is still null after trying to find it!");
        }
    }
}