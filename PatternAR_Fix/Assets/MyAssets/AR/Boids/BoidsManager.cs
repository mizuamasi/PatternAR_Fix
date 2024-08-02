using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        public float maxSpeed;
        public float minSpeed;
        [System.NonSerialized] public Vector3 targetPosition;
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
    private int maxBoids;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    public Camera mainCamera;
    public float cameraPullStrength = 1f;
    public float maxDistanceFromCamera = 20f;

    void Start()
    {
        InitializeComputeShader();
        InitializeBoidsWithDefaultTexture();
        InitializeFlockTargets();
        StartCoroutine(imageManager.GetImagesForBoids(initialBoidCount, OnTexturesDownloaded));
    }

    void InitializeComputeShader()
    {
        if (computeShader == null)
        {
            Debug.LogError("Compute Shader is not assigned. Please assign it in the inspector.");
            return;
        }

        kernelIndex = computeShader.FindKernel("CSMain");
        if (kernelIndex == -1)
        {
            Debug.LogError("Failed to find required kernel in the compute shader.");
            return;
        }

        boidList = new List<Boid>();
        boidObjects = new List<GameObject>();

        maxBoids = initialBoidCount;
        InitializeComputeBuffers();
    }

    void InitializeComputeBuffers()
    {
        int boidStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Boid));
        int flockTypeStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FlockType));

        boidBuffer = new ComputeBuffer(Mathf.Max(1, maxBoids), boidStride);
        positionBuffer = new ComputeBuffer(Mathf.Max(1, maxBoids), sizeof(float) * 3);
        directionBuffer = new ComputeBuffer(Mathf.Max(1, maxBoids), sizeof(float) * 3);
        flockTypeBuffer = new ComputeBuffer(Mathf.Max(1, flockTypes.Length), flockTypeStride);
    }

    void InitializeBoidsWithDefaultTexture()
    {
        textureAtlas = new Texture2D(atlasSize * 256, atlasSize * 256, TextureFormat.RGBA32, false);
        boidMaterial.SetTexture("_MainTex", textureAtlas);

        for (int i = 0; i < initialBoidCount; i++)
        {
            AddNewBoid();
        }
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

    public void AddCustomBoidToFlock(CustomBoidParameters parameters, Vector3 position, Vector3 direction)
    {
        if (boidList.Count >= maxBoids)
        {
            ResizeComputeBuffers();
        }

        Material boidMaterial = new Material(boidPrefab.GetComponent<Renderer>().sharedMaterial);
        SetMaterialProperties(boidMaterial, parameters);

        Boid newBoid = CreateNewBoid(position, direction, parameters);
        boidList.Add(newBoid);

        GameObject newBoidObject = CreateBoidObject(newBoid, position, direction, Vector3.one * parameters.scale, boidMaterial);
        boidObjects.Add(newBoidObject);

        UpdateComputeBuffers();
        
        Debug.Log($"Added new boid. Total boids: {boidList.Count}. FlockType: {newBoid.flockTypeIndex}");
    }

    private void SetMaterialProperties(Material material, CustomBoidParameters parameters)
    {
        material.SetColor("_BackColor", parameters.backColor);
        material.SetColor("_BellyColor", parameters.bellyColor);
        material.SetColor("_PatternBlackColor", parameters.patternBlackColor);
        material.SetColor("_PatternWhiteColor", parameters.patternWhiteColor);
        material.SetFloat("_ColorStrength", parameters.colorStrength);
        material.SetFloat("_PatternStrength", parameters.patternStrength);
        material.SetFloat("_Glossiness", parameters.glossiness);
        material.SetFloat("_Metallic", parameters.metallic);
        material.SetFloat("_NormalRotation", parameters.normalRotation);
        material.SetFloat("_AORotation", parameters.aoRotation);
        material.SetFloat("_RoughnessRotation", parameters.roughnessRotation);
        material.SetFloat("_NormalStrength", parameters.normalStrength);
        material.SetFloat("_AOStrength", parameters.aoStrength);
        material.SetFloat("_RoughnessStrength", parameters.roughnessStrength);

        if (parameters.customTexture != null)
        {
            material.SetTexture("_MainTex", parameters.customTexture);
        }
    }

    private Boid CreateNewBoid(Vector3 position, Vector3 direction, CustomBoidParameters parameters)
    {
        int flockTypeIndex = Random.Range(0, flockTypes.Length); // ランダムなFlockTypeを選択

        return new Boid
        {
            position = transform.InverseTransformPoint(position),
            velocity = direction.normalized * Random.Range(flockTypes[flockTypeIndex].minSpeed, flockTypes[flockTypeIndex].maxSpeed),
            direction = direction.normalized,
            uvOffset = new Vector2(Random.value, Random.value),
            tailSwingPhase = Random.value * Mathf.PI * 2.0f,
            tailFrequencyMultiplier = Random.value,
            tailPhaseOffset = Random.value,
            flockTypeIndex = flockTypeIndex
        };
    }

    private GameObject CreateBoidObject(Boid boid, Vector3 position, Vector3 direction, Vector3 scale, Material material)
    {
        GameObject newBoidObject = Instantiate(boidPrefab, position, Quaternion.LookRotation(direction), transform);
        newBoidObject.transform.localScale = scale;

        Renderer boidRenderer = newBoidObject.GetComponent<Renderer>();
        boidRenderer.material = material;

        BoidMeshColorizer colorizer = newBoidObject.GetComponent<BoidMeshColorizer>();
        if (colorizer != null)
        {
            colorizer.SetIndividualParameters(boid.tailFrequencyMultiplier, boid.tailPhaseOffset);
        }

        return newBoidObject;
    }

    void OnTexturesDownloaded(List<Texture2D> textures)
    {
        CreateTextureAtlas(textures);
        UpdateBoidTextures();
    }

    public void CreateTextureAtlas(List<Texture2D> textures)
    {
        // ... (CreateTextureAtlas implementation)
    }

    void UpdateBoidTextures()
    {
        // ... (UpdateBoidTextures implementation)
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

       //Debug.Log($"UpdateComputeBuffers: Boid count = {boidList.Count}, Positions count = {positions.Length}, Directions count = {directions.Length}");
        //Debug.Log(positions[0] +":pos " + directions[0] + ":direction");

    }

    void Update()
    {
        if (!isInitialized) return;

        UpdateFlockTargets();

        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetVector("minBounds", minBounds);
        computeShader.SetVector("maxBounds", maxBounds);
        computeShader.SetInt("boidCount", boidList.Count);
        computeShader.SetFloat("flowFieldStrength", flowFieldStrength);
        computeShader.SetFloat("flowFieldScale", flowFieldScale);
        computeShader.SetFloat("targetSeekStrength", targetSeekStrength);
        computeShader.SetFloat("time", Time.time);
        computeShader.SetVector("cameraPosition", mainCamera.transform.position);
        computeShader.SetFloat("cameraPullStrength", cameraPullStrength);
        computeShader.SetFloat("maxDistanceFromCamera", maxDistanceFromCamera);

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
            UpdateBoidObject(i, updatedBoids[i], positions[i], directions[i]);
        }
    }

    private void UpdateBoidObject(int index, Boid updatedBoid, Vector3 position, Vector3 direction)
    {
        boidList[index] = updatedBoid;

        GameObject boidObject = boidObjects[index];
        boidObject.transform.position = transform.TransformPoint(position);
        boidObject.transform.rotation = Quaternion.LookRotation(direction);
        
        Renderer renderer = boidObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.SetFloat("_TailSwingPhase", updatedBoid.tailSwingPhase);
            renderer.material.SetFloat("_TailFrequencyMultiplier", updatedBoid.tailFrequencyMultiplier);
            renderer.material.SetFloat("_TailPhaseOffset", updatedBoid.tailPhaseOffset);
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
        targetUpdateTimer += Time.deltaTime;
        if (targetUpdateTimer >= targetUpdateInterval)
        {
            for (int i = 0; i < flockTypes.Length; i++)
            {
                if (Random.value < 0.3f)
                {
                    FlockType flock = flockTypes[i];
                    flock.targetPosition = GetRandomTargetPosition();
                    flockTypes[i] = flock;
                }
            }
            targetUpdateTimer = 0f;
        }
    }

    Vector3 GetRandomTargetPosition()
    {
        return Random.insideUnitSphere * targetRadius;
    }

    void ResizeComputeBuffers()
    {
        maxBoids *= 2;

        if (boidBuffer != null) boidBuffer.Release();
        if (positionBuffer != null) positionBuffer.Release();
        if (directionBuffer != null) directionBuffer.Release();

        InitializeComputeBuffers();
    }

    void OnDestroy()
    {
        if (flockTypeBuffer != null) flockTypeBuffer.Release();
        if (boidBuffer != null) boidBuffer.Release();
        if (positionBuffer != null) positionBuffer.Release();
        if (directionBuffer != null) directionBuffer.Release();
    }

    private Texture2D ResizeTexture(Texture2D sourceTexture, int targetWidth, int targetHeight)
    {
        Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        resizedTexture.SetPixels(sourceTexture.GetPixels());
        resizedTexture.Apply();
        return resizedTexture;
    }
}