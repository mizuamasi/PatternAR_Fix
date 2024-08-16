using UnityEngine;

public class CustomBoidManager : MonoBehaviour
{
    public static CustomBoidManager Instance { get; private set; }

    public BoidsManager boidsManager;
    public float spawnDistance = 5f;
    public float spawnRadius = 2f;
    public float minSpawnDistance = 0.5f;

    private Camera mainCamera;

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

    private void Start()
    {
        // Start時にメインカメラを取得
        mainCamera = GetMainCamera().GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found or doesn't have a Camera component!");
        }
    }

    public static GameObject GetMainCamera()
    {
        GameObject cameraObject = GameObject.FindWithTag("MainCamera");

        if (cameraObject != null)
        {
            return cameraObject;
        }

        Camera cameraComponent = Camera.main;
        if (cameraComponent != null)
        {
            return cameraComponent.gameObject;
        }

        Debug.LogWarning("MainCamera not found in the scene.");
        return null;
    }

    public void AddCustomBoidToFlock(CustomBoidObject customBoidObject)
    {
        
        // Start時にメインカメラを取得
        mainCamera = GetMainCamera().GetComponent<Camera>();
        if (boidsManager == null)
        {
            Debug.LogError("BoidsManager is not assigned!");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("Main camera is not assigned!");
            return;
        }

        CustomBoidParameters parameters = customBoidObject.GetCurrentParameters();
        if (parameters.scale == 0f)
        {
            parameters.scale = 0.1f;
            Debug.LogWarning("Boid scale was 0, set to default value 0.1");
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