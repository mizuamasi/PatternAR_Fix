using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(Renderer))]
public class BoidMeshColorizer : MonoBehaviour
{
    public float tailFrequency = 2f;
    public float tailAmplitude = 0.5f;

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Color[] colors;
    private Material material;

    private float tailFrequencyMultiplier = 1f;
    private float tailPhaseOffset = 0f;

    void Awake()
    {
        SetupMesh();
        material = GetComponent<Renderer>().material;
    }

    void SetupMesh()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("Mesh not found on the object.");
            return;
        }

        originalVertices = mesh.vertices;
        colors = new Color[mesh.vertexCount];

        Vector3[] normals = mesh.normals;

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        // Find the min and max Z values
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (originalVertices[i].z < minZ) minZ = originalVertices[i].z;
            if (originalVertices[i].z > maxZ) maxZ = originalVertices[i].z;
        }

        float zRange = maxZ - minZ;
        if (Mathf.Approximately(zRange, 0f))
        {
            Debug.LogWarning("Z range is zero. Using default colors.");
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                colors[i] = Color.white;
            }
        }
        else
        {
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                // Normalize Z position (0 at the tail, 1 at the head)
                float normalizedZ = (originalVertices[i].z - minZ) / zRange;

                // Calculate tail swing phase
                float tailSwingPhase = Mathf.Sin(normalizedZ * tailFrequency * Mathf.PI) * tailAmplitude;

                colors[i] = new Color(
                    tailSwingPhase * 0.5f + 0.5f, // R: 尻尾の振りのフェーズ（-0.5 to 0.5 を 0 to 1 に変換）
                    normals[i].x * 0.5f + 0.5f,   // G: 法線のX成分
                    normals[i].y * 0.5f + 0.5f,   // B: 法線のY成分
                    normals[i].z * 0.5f + 0.5f    // A: 法線のZ成分
                );
            }
        }

        mesh.colors = colors;
    }

    public void SetIndividualParameters(float frequencyMultiplier, float phaseOffset)
    {
        if (material == null)
        {
            material = GetComponent<Renderer>().material;
        }

        tailFrequencyMultiplier = Mathf.Lerp(0.8f, 1.2f, frequencyMultiplier);
        tailPhaseOffset = phaseOffset * Mathf.PI * 2f;
        
        if (material != null)
        {
            material.SetFloat("_TailFrequencyMultiplier", tailFrequencyMultiplier);
            material.SetFloat("_TailPhaseOffset", tailPhaseOffset);
        }
        else
        {
            Debug.LogError("Material is null in BoidMeshColorizer.SetIndividualParameters");
        }
    }

    void Update()
    {
        if (material != null)
        {
            //material.SetFloat("_Time", Time.time);
            material.SetFloat("_TailFrequency", tailFrequency);
            material.SetFloat("_TailAmplitude", tailAmplitude);
        }
    }
}