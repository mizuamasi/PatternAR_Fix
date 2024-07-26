using UnityEngine;
using UnityEngine.UI;

public class CustomBoidManager : MonoBehaviour
{
    public static CustomBoidManager Instance { get; private set; }

    public BoidsManager boidsManager;
    public Camera mainCamera;  // インスペクターでメインカメラを割り当てる
    public float spawnDistance = 5f;  // カメラからの生成距離（奥に配置するため、大きめの値を設定）
    public float spawnRadius = 2f;  // 生成範囲の半径
    public float minSpawnDistance = 0.5f;  // 最小生成距離（0にならないようにする）

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCustomBoidToFlock(CustomBoidObject customBoidObject)
    {
        Debug.Log("AddCustomBoidToFlock called with CustomBoidObject");
        if (boidsManager == null)
        {
            Debug.LogError("BoidsManager is not assigned!");
            return;
        }
        if (boidsManager != null)
        {
             // カメラの位置と前方方向を取得
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 cameraForward = mainCamera.transform.forward;

            // カメラの奥にランダムな位置を生成
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.z = Mathf.Max(randomOffset.z, minSpawnDistance); // z軸の最小値を設定
            Vector3 spawnPosition = cameraPosition + cameraForward * spawnDistance + randomOffset;

            // カメラの方向を基準にしたランダムな回転を生成
            Quaternion randomRotation = Quaternion.LookRotation(Random.insideUnitSphere) * mainCamera.transform.rotation;

            // 速度をカメラの前方方向を基準に設定
            Vector3 initialVelocity = (randomRotation * Vector3.forward).normalized * Random.Range(boidsManager.minSpeed, boidsManager.maxSpeed);

            // CustomBoidParametersを取得
            CustomBoidParameters parameters = customBoidObject.parameters;

            // BoidsManagerのAddCustomBoidメソッドを呼び出す
            boidsManager.AddCustomBoid(spawnPosition, initialVelocity, parameters);

        }
        else
        {
            Debug.LogError("BoidsManager is not assigned to CustomBoidManager!");
        }
    }

    public void AddRandomCustomBoidToFlock()
    {
        Debug.Log("AddRandomCustomBoidToFlock called");
        CustomBoidObject[] customBoidObjects = FindObjectsOfType<CustomBoidObject>();
        Debug.Log($"Found {customBoidObjects.Length} CustomBoidObjects");
        if (customBoidObjects.Length > 0)
        {
            int randomIndex = Random.Range(0, customBoidObjects.Length);
            Debug.Log($"Selected CustomBoidObject at index {randomIndex}");
            AddCustomBoidToFlock(customBoidObjects[randomIndex]);
        }
        else
        {
            Debug.LogWarning("No CustomBoidObject found in the scene.");
        }
    }
}