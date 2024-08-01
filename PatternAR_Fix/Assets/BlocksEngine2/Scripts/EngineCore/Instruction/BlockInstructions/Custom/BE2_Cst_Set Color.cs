using UnityEngine;
using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Core;

public class SetColorBlock : BE2_InstructionBase, I_BE2_Instruction
{
    public RenderTexture targetTexture;
    public RenderTexture conditionTexture;

    public new void Function()
    {
        UpdateTextureColorBasedOnCondition(Section0Inputs[0].StringValue);
        ExecuteNextInstruction();
    }

    private void UpdateTextureColorBasedOnCondition(string colorValue)
    {
        // 日本語での色の判定
        bool setWhite = (colorValue == "白");
        Color targetColor = setWhite ? Color.white : Color.black;

        // conditionTextureからマスクを作成
        Texture2D maskTexture = new Texture2D(conditionTexture.width, conditionTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = conditionTexture;
        maskTexture.ReadPixels(new Rect(0, 0, conditionTexture.width, conditionTexture.height), 0, 0);
        maskTexture.Apply();

        // targetTextureに対する変更
        RenderTexture.active = targetTexture;
        Texture2D tempTexture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
        tempTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        tempTexture.Apply();

        for (int x = 0; x < targetTexture.width; x++)
        {
            for (int y = 0; y < targetTexture.height; y++)
            {
                float maskValue = maskTexture.GetPixel(x, y).r;  // マスクの赤要素を取得
                if (maskValue == 1.0f)  // 赤要素が1.0の場合のみ色を更新
                {
                    tempTexture.SetPixel(x, y, targetColor);
                }
            }
        }

        tempTexture.Apply();
        Graphics.Blit(tempTexture, targetTexture);
        RenderTexture.active = null;

        // 使用したテクスチャを破棄
        Object.Destroy(tempTexture);
        Object.Destroy(maskTexture);
    }
}
