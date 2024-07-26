using UnityEngine;
using UnityEngine.UI;

public class BoidController : MonoBehaviour
{
    public BoidsManager boidsManager;
    public GameObject controlledBoidPrefab;
    public Transform controlPosition;
    
    public Slider scaleSlider;
    public Slider speedSlider;
    public Button toggleButton;

    [Header("Color Picker")]
    public Image colorDisplayImage;
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    private GameObject controlledBoid;
    private Material controlledBoidMaterial;

    void Start()
    {
        controlledBoid = Instantiate(controlledBoidPrefab, controlPosition.position, Quaternion.identity);
        controlledBoidMaterial = new Material(controlledBoid.GetComponent<Renderer>().sharedMaterial);
        controlledBoid.GetComponent<Renderer>().material = controlledBoidMaterial;

        boidsManager.controlledBoidObject = controlledBoid;

        scaleSlider.onValueChanged.AddListener(UpdateScale);
        speedSlider.onValueChanged.AddListener(UpdateSpeed);
        toggleButton.onClick.AddListener(ToggleBoid);

        redSlider.onValueChanged.AddListener(_ => UpdateColor());
        greenSlider.onValueChanged.AddListener(_ => UpdateColor());
        blueSlider.onValueChanged.AddListener(_ => UpdateColor());

        UpdateColor();
        UpdateBoidParameters();
    }

    void UpdateScale(float scale)
    {
        UpdateBoidParameters();
    }

    void UpdateSpeed(float speed)
    {
        UpdateBoidParameters();
    }

    void UpdateColor()
    {
        Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        colorDisplayImage.color = newColor;
        controlledBoidMaterial.SetColor("_BackColor", newColor);
        UpdateBoidParameters();
    }

    void UpdateBoidParameters()
    {
        Vector3 position = controlPosition.position;
        Vector3 velocity = controlPosition.forward * speedSlider.value;
        float scale = scaleSlider.value;

        boidsManager.UpdateControlledBoidParameters(position, velocity, scale, controlledBoidMaterial);
    }

    void ToggleBoid()
    {
        Vector3 position = controlPosition.position;
        Vector3 velocity = controlPosition.forward * speedSlider.value;
        float scale = scaleSlider.value;

        boidsManager.ToggleControlledBoid(position, velocity, scale, controlledBoidMaterial);
    }
}