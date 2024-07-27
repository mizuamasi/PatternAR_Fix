using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class BoidsManager : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material boidMaterial;
    public GameObject boidPrefab;
    public int initialBoidCount = 100;
    public int atlasSize = 4;
    public Vector3 minBounds = new Vector3(-10.0f, -10.0f, -10.0f);
    public Vector3 maxBounds = new Vector3(10.0f, 10.0f, 10.0f);
    public ImageManager imageManager;

    private int actualBoidCount = 100;

    public Color[] backColors;
    public Color[] bellyColors;
    [Range(0, 1)]
    public float colorStrength = 0.5f;
    [Range(0, 1)]
    public float patternStrength = 1.0f;

    private ComputeBuffer boidBuffer;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer directionBuffer;
    private List<Boid> boidList;
    private List<GameObject> boidObjects;
    private Texture2D textureAtlas;
    private bool isInitialized = false;
    private List<Texture2D> downloadedTextures;

    public Boid controlledBoid;
    public GameObject controlledBoidObject;
    private bool isControlledBoidInFlock = false;
    private Vector3 controlledBoidInitialPosition;

    public Texture2D defaultTexture;

    public float flowFieldStrength = 0.5f;
    public float flowFieldScale = 0.1f;
    public float targetSeekStrength = 0.2f;
    public float targetUpdateInterval = 10f;
    public float targetRadius = 20f;

    private float targetUpdateTimer;
    private ComputeBuffer flockTypeBuffer;

    [System.Serializable]
    public struct FlockType
    {
        public float separationWeight;
        public float alignmentWeight;
        public float cohesionWeight;
        public float separationRadius;
        public float alignmentRadius;
        public float cohesionRadius;
        [System.NonSerialized] public Vector3 targetPosition;
        public Color baseColor;
        public Color patternColor;
    }

    public FlockType[] flockTypes;

    [System.Serializable]
    public struct Boid
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector2 uvOffset;
        public Vector3 direction;
        public float tailSwingPhase;
        public float tailFrequencyMultiplier;
        public float tailPhaseOffset;
        public int flockTypeIndex;
    }

    private int kernelIndex;
    private int updateSingleBoidKernel;
    private int maxBoids;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    void Start()
    {
        if (defaultTexture == null)
        {
            Debug.LogError("Default texture is not set. Please assign a default texture in the inspector.");
            return;
        }

        if (defaultTexture.width != 256 || defaultTexture.height != 256)
        {
            Debug.LogError("Default texture must be 256x256 pixels.");
            return;
        }

        if (flockTypes == null || flockTypes.Length == 0)
        {
            Debug.LogError("FlockTypes are not set. Please set up flock types in the inspector.");
            return;
        }

        if (computeShader == null)
        {
            Debug.LogError("Compute Shader is not assigned. Please assign it in the inspector.");
            return;
        }

        kernelIndex = computeShader.FindKernel("CSMain");
        updateSingleBoidKernel = computeShader.FindKernel("UpdateSingleBoid");
        if (kernelIndex == -1 || updateSingleBoidKernel == -1)
        {
            Debug.LogError("Failed to find required kernels in the compute shader.");
            return;
        }

        boidList = new List<Boid>();
        boidObjects = new List<GameObject>();
        downloadedTextures = new List<Texture2D>();

        maxBoids = initialBoidCount;
        InitializeBoidsWithDefaultTexture();
        InitializeFlockTargets();

        StartCoroutine(imageManager.GetImagesForBoids(initialBoidCount, OnTexturesDownloaded));
    }

    void ResizeComputeBuffers()
    {
        maxBoids *= 2;

        if (boidBuffer != null) boidBuffer.Release();
        if (positionBuffer != null) positionBuffer.Release();
        if (directionBuffer != null) directionBuffer.Release();

        InitializeComputeBuffers();
    }

    void InitializeComputeBuffers()
    {
        int boidStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Boid));
        int flockTypeStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FlockType));

        boidBuffer = new ComputeBuffer(Mathf.Max(1, boidList.Count), boidStride);
        positionBuffer = new ComputeBuffer(Mathf.Max(1, boidList.Count), sizeof(float) * 3);
        directionBuffer = new ComputeBuffer(Mathf.Max(1, boidList.Count), sizeof(float) * 3);
        flockTypeBuffer = new ComputeBuffer(Mathf.Max(1, flockTypes.Length), flockTypeStride);
    }

    void InitializeBoidsWithDefaultTexture()
    {
        textureAtlas = new Texture2D(atlasSize * 256, atlasSize * 256, TextureFormat.RGBA32, false);
        Color[] defaultPixels = defaultTexture.GetPixels();

        for (int y = 0; y < atlasSize; y++)
        {
            for (int x = 0; x < atlasSize; x++)
            {
                textureAtlas.SetPixels(x * 256, y * 256, 256, 256, defaultPixels);
            }
        }
        textureAtlas.Apply();
        boidMaterial.SetTexture("_MainTex", textureAtlas);

        for (int i = 0; i < initialBoidCount; i++)
        {
            AddNewBoid();
        }
        InitializeComputeBuffers();
        UpdateComputeBuffers();
        isInitialized = true;
    }

    void AddNewBoid()
    {
        Vector3 initialVelocity = Random.insideUnitSphere.normalized * 2f;
        if (initialVelocity == Vector3.zero)
        {
            initialVelocity = new Vector3(1f, 0f, 0f);
        }

        int flockTypeIndex = Random.Range(0, flockTypes.Length);
        Boid newBoid = new Boid
        {
            position = transform.InverseTransformPoint(Random.insideUnitSphere * 10.0f),
            velocity = initialVelocity,
            direction = initialVelocity.normalized,
            tailSwingPhase = Random.value * Mathf.PI * 2.0f,
            tailFrequencyMultiplier = Random.value,
            tailPhaseOffset = Random.value,
            flockTypeIndex = flockTypeIndex
        };

        boidList.Add(newBoid);

        GameObject newBoidObject = Instantiate(boidPrefab, transform.TransformPoint(newBoid.position), Quaternion.LookRotation(newBoid.velocity), transform);
        Material boidMat = new Material(boidMaterial);
        Color backColor = backColors[Random.Range(0, backColors.Length)];
        Color bellyColor = bellyColors[Random.Range(0, bellyColors.Length)];
        boidMat.SetColor("_BackColor", backColor);
        boidMat.SetColor("_BellyColor", bellyColor);
        boidMat.SetFloat("_ColorStrength", colorStrength);
        boidMat.SetFloat("_PatternStrength", patternStrength);
        boidMat.SetFloat("_AtlasSize", atlasSize);

        int index = boidList.Count - 1;
        float uvX = (index % atlasSize) / (float)atlasSize;
        float uvY = (index / atlasSize) / (float)atlasSize;
        newBoid.uvOffset = new Vector2(uvX, uvY);
        boidMat.SetVector("_UvOffset", newBoid.uvOffset);

        newBoidObject.GetComponent<Renderer>().material = boidMat;

        BoidMeshColorizer colorizer = newBoidObject.GetComponent<BoidMeshColorizer>();
        if (colorizer == null)
        {
            colorizer = newBoidObject.AddComponent<BoidMeshColorizer>();
        }
        colorizer.SetIndividualParameters(newBoid.tailFrequencyMultiplier, newBoid.tailPhaseOffset);

        newBoidObject.name = $"Boid_FlockType_{flockTypeIndex}";

        boidObjects.Add(newBoidObject);
    }

    // CustomBoidManager.cs

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

            // BoidsManagerのAddCustomBoidToFlockメソッドを呼び出す
            boidsManager.AddCustomBoidToFlock(customBoidObject.GetComponent<Renderer>().material, spawnPosition, initialVelocity, Vector3.one * parameters.scale);
        }
        else
        {
            Debug.LogError("BoidsManager is not assigned to CustomBoidManager!");
        }
    }

    void OnTexturesDownloaded(List<Texture2D> textures)
    {
        downloadedTextures = textures;
        CreateTextureAtlas();
        UpdateBoidTextures();
    }

    public void CreateTextureAtlas()
    {
        int textureCount = downloadedTextures.Count;
        int atlasSize = Mathf.CeilToInt(Mathf.Sqrt(textureCount));

        textureAtlas = new Texture2D(atlasSize * 128, atlasSize * 128);

        for (int i = 0; i < textureCount; i++)
        {
            Texture2D resizedTexture = ResizeTexture(downloadedTextures[i], 128, 128);

            if (resizedTexture.width != 128 || resizedTexture.height != 128)
            {
                continue;
            }

            int atlasX = Mathf.Clamp(i % atlasSize, 0, atlasSize - 1) * 128;
            int atlasY = Mathf.Clamp(i / atlasSize, 0, atlasSize - 1) * 128;

            if (atlasX + 128 <= atlasSize * 128 && atlasY + 128 <= atlasSize * 128)
            {
                textureAtlas.SetPixels32(atlasX, atlasY, 128, 128, resizedTexture.GetPixels32());
            }
            else
            {
                Debug.LogError("Texture doesn't fit in atlas at: " + i);
            }

            Destroy(resizedTexture);
        }

        textureAtlas.Apply();
        boidMaterial.SetTexture("_MainTex", textureAtlas);
        
        for (int i = 0; i < boidObjects.Count; i++)
        {
            boidObjects[i].GetComponent<Renderer>().material.SetTexture("_MainTex", textureAtlas);
        }
    }

    void UpdateBoidTextures()
    {
        for (int i = 0; i < boidList.Count; i++)
        {
            Boid boid = boidList[i];
            boid.uvOffset = new Vector2((i % atlasSize) / (float)atlasSize, (i / atlasSize) / (float)atlasSize);
            boidList[i] = boid;

            boidObjects[i].GetComponent<Renderer>().material.SetVector("_UvOffset", boid.uvOffset);
        }
        UpdateComputeBuffers();
    }

    private Texture2D ResizeTexture(Texture2D sourceTexture, int targetWidth, int targetHeight)
    {
        Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        resizedTexture.SetPixels(sourceTexture.GetPixels());
        resizedTexture.Apply();
        return resizedTexture;
    }

    void UpdateComputeBuffers()
    {
        boidBuffer.SetData(boidList.ToArray());
        Vector3[] positions = boidList.Select(b => b.position).ToArray();
        Vector3[] directions = boidList.Select(b => b.direction).ToArray();
        positionBuffer.SetData(positions);
        directionBuffer.SetData(directions);
        flockTypeBuffer.SetData(flockTypes);

        computeShader.SetBuffer(kernelIndex, "boids", boidBuffer);
        computeShader.SetBuffer(kernelIndex, "positions", positionBuffer);
        computeShader.SetBuffer(kernelIndex, "directions", directionBuffer);
        computeShader.SetBuffer(kernelIndex, "flockTypes", flockTypeBuffer);
    }

    void Update()
    {
        if (!isInitialized) return;

        targetUpdateTimer += Time.deltaTime;
        if (targetUpdateTimer >= targetUpdateInterval)
        {
            UpdateFlockTargets();
            targetUpdateTimer = 0f;
        }

        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetVector("minBounds", minBounds);
        computeShader.SetVector("maxBounds", maxBounds);
        computeShader.SetInt("boidCount", boidList.Count);
        computeShader.SetFloat("flowFieldStrength", flowFieldStrength);
        computeShader.SetFloat("flowFieldScale", flowFieldScale);
        computeShader.SetFloat("targetSeekStrength", targetSeekStrength);
        computeShader.SetFloat("time", Time.time);

        UpdateComputeBuffers();

        computeShader.Dispatch(kernelIndex, Mathf.CeilToInt(boidList.Count / 64f), 1, 1);

        Vector3[] positions = new Vector3[boidList.Count];
        Vector3[] directions = new Vector3[boidList.Count];
        positionBuffer.GetData(positions);
        directionBuffer.GetData(directions);
        
        Boid[] updatedBoids = new Boid[boidList.Count];
        boidBuffer.GetData(updatedBoids);

        for (int i = 0; i < boidList.Count; i++)
        {
            Boid updatedBoid = updatedBoids[i];
            updatedBoid.position = positions[i];
            updatedBoid.direction = directions[i];
            boidList[i] = updatedBoid;

            boidObjects[i].transform.position = transform.TransformPoint(positions[i]);
            boidObjects[i].transform.rotation = Quaternion.LookRotation(directions[i]);
            
            Renderer renderer = boidObjects[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetFloat("_TailSwingPhase", updatedBoid.tailSwingPhase);
                renderer.material.SetFloat("_TailFrequencyMultiplier", updatedBoid.tailFrequencyMultiplier);
                renderer.material.SetFloat("_TailPhaseOffset", updatedBoid.tailPhaseOffset);
            }
        }
    }

    void InitializeFlockTargets()
    {
        for (int i = 0; i < flockTypes.Length; i++)
        {
            FlockType flock = flockTypes[i];
            flock.targetPosition = GetRandomTargetPosition();
            flockTypes[i] = flock;
        }
    }

    void UpdateFlockTargets()
    {
        for (int i = 0; i < flockTypes.Length; i++)
        {
            FlockType flock = flockTypes[i];
            if (Random.value < 0.3f)
            {
                flock.targetPosition = GetRandomTargetPosition();
            }
            flockTypes[i] = flock;
        }
    }

    public void UpdateControlledBoidParameters(Vector3 position, Vector3 velocity, float scale, Material material)
    {
        if (!isControlledBoidInFlock && controlledBoidObject != null)
        {
            controlledBoidObject.transform.position = position;
            controlledBoidObject.transform.rotation = Quaternion.LookRotation(velocity);
            controlledBoidObject.transform.localScale = Vector3.one * scale;
            controlledBoidObject.GetComponent<Renderer>().material = material;
        }
    }

    public void ToggleControlledBoid(Vector3 position, Vector3 velocity, float scale, Material material)
    {
        if (isControlledBoidInFlock)
        {
            int index = boidList.FindIndex(b => b.Equals(controlledBoid));
            if (index != -1)
            {
                boidList.RemoveAt(index);
                Destroy(boidObjects[index]);
                boidObjects.RemoveAt(index);
            }
            
            controlledBoidObject.transform.position = controlledBoidInitialPosition;
            controlledBoidObject.SetActive(true);
            
            isControlledBoidInFlock = false;
        }
        else
        {
            controlledBoid = new Boid
            {
                position = transform.InverseTransformPoint(position),
                velocity = velocity,
                direction = velocity.normalized,
                tailSwingPhase = Random.value * Mathf.PI * 2.0f,
                uvOffset = new Vector2(Random.value, Random.value),
                flockTypeIndex = Random.Range(0, flockTypes.Length),
                tailFrequencyMultiplier = Random.value,
                tailPhaseOffset = Random.value
            };
            
            boidList.Add(controlledBoid);
            
            GameObject newBoidObject = Instantiate(boidPrefab, position, Quaternion.LookRotation(velocity), transform);
            newBoidObject.transform.localScale = Vector3.one * scale;
            newBoidObject.GetComponent<Renderer>().material = material;
            boidObjects.Add(newBoidObject);
            
            controlledBoidInitialPosition = controlledBoidObject.transform.position;
            controlledBoidObject.SetActive(false);
            
            isControlledBoidInFlock = true;
        }

        UpdateComputeBuffers();
    }

    void OnDestroy()
    {
        if (flockTypeBuffer != null) flockTypeBuffer.Release();
        if (boidBuffer != null) boidBuffer.Release();
        if (positionBuffer != null) positionBuffer.Release();
        if (directionBuffer != null) directionBuffer.Release();
    }

    Vector3 GetRandomTargetPosition()
    {
        return Random.insideUnitSphere * targetRadius;
    }

    private void UpdateBoidInComputeShader(int index, Boid boid)
    {
        computeShader.SetInt("updateIndex", index);
        computeShader.SetVector("updatePosition", boid.position);
        computeShader.SetVector("updateVelocity", boid.velocity);
        computeShader.SetVector("updateUvOffset", boid.uvOffset);
        computeShader.SetVector("updateDirection", boid.direction);
        computeShader.SetFloat("updateTailSwingPhase", boid.tailSwingPhase);
        computeShader.SetFloat("updateTailFrequencyMultiplier", boid.tailFrequencyMultiplier);
        computeShader.SetFloat("updateTailPhaseOffset", boid.tailPhaseOffset);
        computeShader.SetInt("updateFlockTypeIndex", boid.flockTypeIndex);

        computeShader.Dispatch(updateSingleBoidKernel, 1, 1, 1);
    }
}

[System.Serializable]
public struct CustomBoidParameters
{
    public Color backColor;
    public Color bellyColor;
    public Color patternBlackColor;
    public Color patternWhiteColor;
    public float colorStrength;
    public float patternStrength;
    public float glossiness;
    public float metallic;
    public float normalRotation;
    public float aoRotation;
    public float roughnessRotation;
    public float normalStrength;
    public float aoStrength;
    public float roughnessStrength;
    public Texture2D customTexture;
    public float scale;
}