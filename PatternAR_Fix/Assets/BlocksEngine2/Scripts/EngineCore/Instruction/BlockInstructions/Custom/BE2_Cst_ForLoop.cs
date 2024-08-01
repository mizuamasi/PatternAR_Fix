using UnityEngine;
using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Core;

public class RepeatBlock : BE2_InstructionBase, I_BE2_Instruction
{
    private int repeatCount = 5;  // ユーザーが設定する繰り返し回数

    private int currentCount = 0;  // 現在の繰り返し回数

    protected override void OnStart()
    {
        base.OnStart();
        // ダブルバッファ用のテクスチャを初期化
        currentCount = 0;  // 繰り返し回数の初期化
        if (int.TryParse(Section0Inputs[0].StringValue, out int count))
        {
            repeatCount = count;  // 変換成功した場合
        }
        else
        {
            repeatCount = 0;  // 変換失敗した場合は0に設定
        }
    }

    public new void Function()
    {
        RepeatProcess();
        ExecuteNextInstruction();
    }

    void RepeatProcess()
    {
        if (currentCount < repeatCount)
        {
            // ここで繰り返し実行したい処理を呼び出す
            ExecuteSection(0);  // 子ブロックの実行
            currentCount++;
            // 次のフレームで再び同じメソッドを呼び出す
            Invoke("RepeatProcess", 0.5f);  // 少し遅延を持たせて再帰的に呼び出す
        }
        else
        {
            // 全ての繰り返しが終了したら次のブロックを実行
            ExecuteNextInstruction();
        }
    }
}
