using UnityEngine;

[System.Serializable]
public struct CustomBoidParameters
{
    public Color backColor;
    public Color bellyColor;
    public Color patternBlackColor;
    public Color patternWhiteColor;
    public float colorStrength;
    public float patternStrength;
    public float glossiness;
    public float metallic;
    public float normalRotation;
    public float aoRotation;
    public float roughnessRotation;
    public float normalStrength;
    public float aoStrength;
    public float roughnessStrength;
    public Texture2D customTexture;
    public float scale;
}