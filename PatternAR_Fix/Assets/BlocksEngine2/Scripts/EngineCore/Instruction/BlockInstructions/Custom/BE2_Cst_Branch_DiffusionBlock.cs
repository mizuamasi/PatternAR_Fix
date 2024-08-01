using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;

public class Branch_DiffusionBlock : BE2_InstructionBase, I_BE2_Instruction
{
    public RenderTexture targetTexture;
    public RenderTexture conditionTexture;
    private RenderTexture bufferTexture; // ダブルバッファリング用の追加テクスチャ

    public Material processingMaterial;
    private Material multiplyMaterial;

    protected override void OnStart()
    {
        base.OnStart();
        // ダブルバッファ用のテクスチャを初期化
        bufferTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);
        bufferTexture.Create();

        // 乗算用マテリアルの初期化
        multiplyMaterial = new Material(Shader.Find("Hidden/Multiply"));
    }

    public new void Function()
    {
        if (Section0Inputs == null || Section0Inputs.Length == 0)
        {
            Debug.LogError("Section0Inputs is not initialized or empty.");
            return;
        }
        
        //string condition = Section0Inputs[2].StringValue; // "以上" または "未満"
        float threshold = float.Parse(Section0Inputs[2].StringValue);
        string colorValue = Section0Inputs[1].StringValue;
        float range = float.Parse(Section0Inputs[0].StringValue);


        float isSameColor = (colorValue == "自分とおなじ") ? 1.0f : ((colorValue == "自分とちがう") ? -1.0f : 0.0f);
        

        // 解像度と条件の設定
        processingMaterial.SetVector("_Resolution", new Vector2(targetTexture.width, targetTexture.height));
        processingMaterial.SetFloat("_Threshold", threshold);
        processingMaterial.SetFloat("_NeighborhoodSize", range);
        processingMaterial.SetVector("_TargetColor",colorValue == "白" ? new Vector4(1.0f,1.0f,1.0f,1.0f) : new Vector4(0.0f,0.0f,0.0f,1.0f));
        processingMaterial.SetFloat("_isSameColor", isSameColor);

        // 仮のテクスチャにターゲットテクスチャの内容をコピー
        Graphics.Blit(targetTexture, bufferTexture);

        // シェーダー処理を行い、結果をconditionTextureに保存
        Graphics.Blit(bufferTexture, conditionTexture, processingMaterial);

        // conditionTextureに対して乗算を行い、最終結果を保存
        multiplyMaterial.SetTexture("_SecondTex", conditionTexture);
        Graphics.Blit(conditionTexture, bufferTexture, multiplyMaterial);
        Graphics.Blit(bufferTexture, conditionTexture);

        ExecuteNextInstruction();
    }

    void OnDestroy()
    {
        // 使用したリソースのクリーンアップ
        if (bufferTexture != null)
            bufferTexture.Release();
    }
}
