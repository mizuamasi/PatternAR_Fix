using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;

public class BE2_Cst_InitializeRandomaizeTextureBlock : BE2_InstructionBase, I_BE2_Instruction {
    public RenderTexture texture;
    I_BE2_BlockSectionHeaderInput seedInput; 

    // 初期化処理が必要な場合はここに追加
    // protected override void OnStart() {
    //     base.OnStart(); // 必要に応じて基底クラスのOnStartを呼び出す
    //     RandomizeTexture(texture,10);
    // }

    public new void Function() {
        seedInput = Section0Inputs[0]; // シード値を最初のセクションから取得
        RandomizeTexture(texture, (int)(seedInput.FloatValue*10)); // シード値を使用してテクスチャをランダム化
    
        ExecuteNextInstruction();
    }

    // レンダーテクスチャをランダムに初期化する処理
    private void RandomizeTexture(RenderTexture texture,int seed) {
        Random.InitState(seed+10); // シード値でランダムジェネレータを初期化
        RenderTexture.active = texture;
        Texture2D tempTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        
        // ピクセルにランダムな色を設定
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                Color color = Random.value > 0.5f ? Color.white : Color.black;
                tempTexture.SetPixel(x, y, color);
            }
        }

        tempTexture.Apply();

        // レンダーテクスチャにテクスチャを書き込む
        Graphics.Blit(tempTexture, texture);
        RenderTexture.active = null; // クリーンアップ
        Destroy(tempTexture);
    }
}
