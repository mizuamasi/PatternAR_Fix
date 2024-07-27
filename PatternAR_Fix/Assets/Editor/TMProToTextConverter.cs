using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class TMProToTextConverter : EditorWindow
{
    [MenuItem("Tools/Convert TMPro to Text")]
    public static void ShowWindow()
    {
        GetWindow<TMProToTextConverter>("TMPro to Text Converter");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Convert All TMPro to Text"))
        {
            ConvertAllTMProToText();
        }
    }

    void ConvertAllTMProToText()
    {
        TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            ConvertTMProToText(tmpText);
        }
    }

    void ConvertTMProToText(TextMeshProUGUI tmpText)
    {
        GameObject gameObject = tmpText.gameObject;

        // Create new Text component
        Text newText = gameObject.AddComponent<Text>();

        // Transfer properties
        newText.text = tmpText.text;
        newText.font = GetClosestFont(tmpText.font);
        newText.fontSize = Mathf.RoundToInt(tmpText.fontSize);
        newText.color = tmpText.color;
        newText.alignment = ConvertTextAlignment(tmpText.alignment);

        // Remove the old TMPro component
        DestroyImmediate(tmpText);

        Debug.Log("Converted " + gameObject.name + " from TMPro to Text");
    }

    Font GetClosestFont(TMP_FontAsset tmpFont)
    {
        // This is a simple implementation. You might want to improve this to find the best matching font.
        return Font.CreateDynamicFontFromOSFont(tmpFont.name, 14);
    }

    TextAnchor ConvertTextAlignment(TextAlignmentOptions tmpAlignment)
    {
        // This is a simplified conversion. You might need to handle more cases.
        if (tmpAlignment == TextAlignmentOptions.Center)
            return TextAnchor.MiddleCenter;
        if (tmpAlignment == TextAlignmentOptions.Left)
            return TextAnchor.MiddleLeft;
        if (tmpAlignment == TextAlignmentOptions.Right)
            return TextAnchor.MiddleRight;
        
        return TextAnchor.MiddleCenter; // Default
    }
}