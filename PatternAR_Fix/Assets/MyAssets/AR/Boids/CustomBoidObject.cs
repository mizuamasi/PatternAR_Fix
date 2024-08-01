using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CustomBoidObject : MonoBehaviour
{
    public GameObject CustomBOidObjectOriginak;
    [Header("Parameters")]
    public CustomBoidParameters parameters;

    [Header("UI Controls")]
    public Button addToFlockButton;
    public Button selectTextureButton;

    [Header("Preview")]
    public Renderer previewRenderer;

    public ImageGallery imageGallery;


    private void Awake()
    {
        //if (parameters == null)
        //{
        parameters = new CustomBoidParameters();
            // デフォルト値を設定
        //}
    }

    private void Start()
    {
        if (addToFlockButton != null)
        {
            addToFlockButton.onClick.AddListener(AddToFlock);
        }

        if (selectTextureButton != null)
        {
            selectTextureButton.onClick.AddListener(OpenImageGallery);
        }

        UpdatePreview();
    }

    public void AddToFlock()
    {
        if (CustomBoidManager.Instance != null)
        {
            CustomBoidManager.Instance.AddCustomBoidToFlock(this);
        }
        else
        {
            Debug.LogError("CustomBoidManager.Instance is null");
        }
    }

    public CustomBoidParameters GetCurrentParameters()
    {
        if (previewRenderer == null)
        {
            Debug.LogError("Preview Renderer is not assigned");
            return parameters;
        }

        Material mat = previewRenderer.sharedMaterial;
        parameters.backColor = mat.GetColor("_BackColor");
        parameters.bellyColor = mat.GetColor("_BellyColor");
        parameters.patternBlackColor = mat.GetColor("_PatternBlackColor");
        parameters.patternWhiteColor = mat.GetColor("_PatternWhiteColor");
        parameters.colorStrength = mat.GetFloat("_ColorStrength");
        parameters.patternStrength = mat.GetFloat("_PatternStrength");
        parameters.glossiness = mat.GetFloat("_Glossiness");
        parameters.metallic = mat.GetFloat("_Metallic");
        parameters.normalRotation = mat.GetFloat("_NormalRotation");
        parameters.aoRotation = mat.GetFloat("_AORotation");
        parameters.roughnessRotation = mat.GetFloat("_RoughnessRotation");
        parameters.normalStrength = mat.GetFloat("_NormalStrength");
        parameters.aoStrength = mat.GetFloat("_AOStrength");
        parameters.roughnessStrength = mat.GetFloat("_RoughnessStrength");
        parameters.customTexture = mat.GetTexture("_MainTex") as Texture2D;
        parameters.scale = transform.localScale.x;

        return parameters;
    }

    public void SetCustomTexture(Texture2D texture)
    {
        if (previewRenderer != null)
        {
            Material previewMaterial = previewRenderer.material;
            
            // 現在のパラメータを保存
            Color backColor = previewMaterial.GetColor("_BackColor");
            Color bellyColor = previewMaterial.GetColor("_BellyColor");
            Color patternBlackColor = previewMaterial.GetColor("_PatternBlackColor");
            Color patternWhiteColor = previewMaterial.GetColor("_PatternWhiteColor");
            float colorStrength = previewMaterial.GetFloat("_ColorStrength");
            float patternStrength = previewMaterial.GetFloat("_PatternStrength");
            float glossiness = previewMaterial.GetFloat("_Glossiness");
            float metallic = previewMaterial.GetFloat("_Metallic");
            float normalRotation = previewMaterial.GetFloat("_NormalRotation");
            float aoRotation = previewMaterial.GetFloat("_AORotation");
            float roughnessRotation = previewMaterial.GetFloat("_RoughnessRotation");
            float normalStrength = previewMaterial.GetFloat("_NormalStrength");
            float aoStrength = previewMaterial.GetFloat("_AOStrength");
            float roughnessStrength = previewMaterial.GetFloat("_RoughnessStrength");

            // テクスチャのみを変更
            previewMaterial.SetTexture("_MainTex", texture);

            // 保存したパラメータを再設定
            previewMaterial.SetColor("_BackColor", backColor);
            previewMaterial.SetColor("_BellyColor", bellyColor);
            previewMaterial.SetColor("_PatternBlackColor", patternBlackColor);
            previewMaterial.SetColor("_PatternWhiteColor", patternWhiteColor);
            previewMaterial.SetFloat("_ColorStrength", colorStrength);
            previewMaterial.SetFloat("_PatternStrength", patternStrength);
            previewMaterial.SetFloat("_Glossiness", glossiness);
            previewMaterial.SetFloat("_Metallic", metallic);
            previewMaterial.SetFloat("_NormalRotation", normalRotation);
            previewMaterial.SetFloat("_AORotation", aoRotation);
            previewMaterial.SetFloat("_RoughnessRotation", roughnessRotation);
            previewMaterial.SetFloat("_NormalStrength", normalStrength);
            previewMaterial.SetFloat("_AOStrength", aoStrength);
            previewMaterial.SetFloat("_RoughnessStrength", roughnessStrength);
        }

        parameters.customTexture = texture;
        UpdatePreview();
    }

    public void UpdatePreview()
    {
        if (previewRenderer != null)
        {
            Material previewMaterial = new Material(previewRenderer.sharedMaterial);
            previewMaterial.SetColor("_BackColor", parameters.backColor);
            previewMaterial.SetColor("_BellyColor", parameters.bellyColor);
            previewMaterial.SetColor("_PatternBlackColor", parameters.patternBlackColor);
            previewMaterial.SetColor("_PatternWhiteColor", parameters.patternWhiteColor);
            
            if (parameters.customTexture != null)
            {
                previewMaterial.SetTexture("_MainTex", parameters.customTexture);
            }

            previewRenderer.material = previewMaterial;
            transform.localScale = Vector3.one * parameters.scale;
        }
    }

    private void OpenImageGallery()
    {
        if (imageGallery != null)
        {
            imageGallery.Open(this);
        }
        else
        {
            Debug.LogError("ImageGallery is not assigned to CustomBoidObject!");
        }
    }

    private void OnValidate()
    {
        UpdatePreview();
    }
}