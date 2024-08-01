using UnityEngine;
using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Core;

public class InitializeToRed : BE2_InstructionBase, I_BE2_Instruction
{
    public RenderTexture targetTexture;

    public new void Function()
    {
        // RenderTextureをアクティブに設定
        RenderTexture.active = targetTexture;

        // テクスチャを赤色で初期化するための一時的なTexture2Dを作成
        Texture2D tempTexture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
        Color fillColor = Color.white;
        // 赤色でテクスチャを塗りつぶし
        for (int x = 0; x < tempTexture.width; x++)
        {
            for (int y = 0; y < tempTexture.height; y++)
            {
                tempTexture.SetPixel(x, y, fillColor);
            }
        }
        tempTexture.Apply();

        // レンダーテクスチャにテクスチャを適用
        Graphics.Blit(tempTexture, targetTexture);
        RenderTexture.active = null; // クリーンアップ

        // 一時的なテクスチャを削除
        Object.Destroy(tempTexture);

        ExecuteNextInstruction();
    }
}
