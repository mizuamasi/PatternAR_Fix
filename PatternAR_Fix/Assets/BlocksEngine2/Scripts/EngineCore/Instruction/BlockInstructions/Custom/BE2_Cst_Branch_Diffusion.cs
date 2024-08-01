using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;

public class Branch_Diffusion : BE2_InstructionBase, I_BE2_Instruction
{
    public RenderTexture targetTexture;
    public RenderTexture conditionTexture;
    private RenderTexture bufferTexture; // ダブルバッファリング用の追加テクスチャ

    public Material processingMaterial;
    private Material multiplyMaterial;

    protected override void OnStart()
{
    base.OnStart();
    Debug.Log("Branch_Diffusion OnStart called");

    if (targetTexture == null)
    {
        Debug.LogError("targetTexture is null in OnStart");
        return;
    }

    // ダブルバッファ用のテクスチャを初期化
    bufferTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);
    bufferTexture.Create();
    Debug.Log("bufferTexture created");

    // 乗算用マテリアルの初期化
    multiplyMaterial = new Material(Shader.Find("Custom/Multiply"));
    if (multiplyMaterial == null)
    {
        Debug.LogError("Failed to create multiplyMaterial");
    }
    else
    {
        Debug.Log("multiplyMaterial created");
    }

    if (processingMaterial == null)
    {
        Debug.LogError("processingMaterial is null in OnStart");
    }

    if (conditionTexture == null)
    {
        Debug.LogError("conditionTexture is null in OnStart");
    }
}

    public new void Function()
{
    try
    {
        Debug.Log("Branch_Diffusion Function called");

        if (Section0Inputs == null || Section0Inputs.Length == 0)
        {
            Debug.LogError("Section0Inputs is not initialized or empty.");
            return;
        }

        if (targetTexture == null || conditionTexture == null || bufferTexture == null)
        {
            Debug.LogError("One or more textures are null");
            return;
        }

        if (processingMaterial == null || multiplyMaterial == null)
        {
            Debug.LogError("One or more materials are null");
            return;
        }

        float threshold = float.Parse(Section0Inputs[2].StringValue);
        string colorValue = Section0Inputs[1].StringValue;
        float range = float.Parse(Section0Inputs[0].StringValue);

        Debug.Log($"Parsed values: threshold={threshold}, colorValue={colorValue}, range={range}");

        float isSameColor = (colorValue == "自分とおなじ") ? 1.0f : ((colorValue == "自分とちがう") ? -1.0f : 0.0f);
    
        // 解像度と条件の設定
        processingMaterial.SetVector("_Resolution", new Vector2(targetTexture.width, targetTexture.height));
        processingMaterial.SetFloat("_Threshold", threshold);
        processingMaterial.SetFloat("_NeighborhoodSize", range);
        processingMaterial.SetVector("_TargetColor", colorValue == "白" ? new Vector4(1.0f,1.0f,1.0f,1.0f) : new Vector4(0.0f,0.0f,0.0f,1.0f));
        processingMaterial.SetFloat("_isSameColor", isSameColor);

        Debug.Log("Material properties set");

        // 仮のテクスチャにターゲットテクスチャの内容をコピー
        Graphics.Blit(targetTexture, bufferTexture);
        Debug.Log("Blit to bufferTexture completed");

        // シェーダー処理を行い、結果をconditionTextureに保存
        Graphics.Blit(bufferTexture, conditionTexture, processingMaterial);
        Debug.Log("Shader processing completed");

        // conditionTextureに対して乗算を行い、最終結果を保存
        multiplyMaterial.SetTexture("_SecondTex", conditionTexture);
        Graphics.Blit(conditionTexture, bufferTexture, multiplyMaterial);
        Graphics.Blit(bufferTexture, conditionTexture);
        Debug.Log("Final processing completed");

        ExecuteNextInstruction();
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Error in Branch_Diffusion.Function: {e.Message}\nStackTrace: {e.StackTrace}");
        ExecuteNextInstruction();
    }
}

    void OnDestroy()
    {
        Debug.Log("Branch_Diffusion OnDestroy called");
        // 使用したリソースのクリーンアップ
        if (bufferTexture != null)
        {
            bufferTexture.Release();
            Debug.Log("bufferTexture released");
        }
    }
}