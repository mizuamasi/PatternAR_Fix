using UnityEngine;

public class CustomBoidManager : MonoBehaviour
{
    public static CustomBoidManager Instance { get; private set; }

    public BoidsManager boidsManager;
    public Camera mainCamera;
    public float spawnDistance = 5f;
    public float spawnRadius = 2f;
    public float minSpawnDistance = 0.5f;

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
        if (boidsManager == null)
        {
            Debug.LogError("BoidsManager is not assigned!");
            return;
        }

         CustomBoidParameters parameters = customBoidObject.GetCurrentParameters();
        // パラメータのscaleが正しく設定されていることを確認
        if (parameters.scale == 0f)
        {
            parameters.scale = 0.1f;  // デフォルト値を設定
            Debug.LogWarning("Boid scale was 0, set to default value 1");
        }

        Vector3 spawnPosition = CalculateSpawnPosition();
        Vector3 initialDirection = CalculateInitialDirection();

        boidsManager.AddCustomBoidToFlock(parameters, spawnPosition, initialDirection);
    }

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;

        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.z = Mathf.Max(randomOffset.z, minSpawnDistance);
        
        return cameraPosition + cameraForward * spawnDistance + randomOffset;
    }

    private Vector3 CalculateInitialDirection()
    {
        Quaternion randomRotation = Quaternion.LookRotation(Random.insideUnitSphere) * mainCamera.transform.rotation;
        return (randomRotation * Vector3.forward).normalized;
    }

    public void AddRandomCustomBoidToFlock()
    {
        CustomBoidObject[] customBoidObjects = FindObjectsOfType<CustomBoidObject>();
        if (customBoidObjects.Length > 0)
        {
            int randomIndex = Random.Range(0, customBoidObjects.Length);
            AddCustomBoidToFlock(customBoidObjects[randomIndex]);
        }
        else
        {
            Debug.LogWarning("No CustomBoidObject found in the scene.");
        }
    }
}