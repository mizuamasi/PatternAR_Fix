using UnityEngine;
using UnityEngine.UI;

public class BoidController : MonoBehaviour
{
    public BoidsManager boidsManager;
    public CustomBoidManager customBoidManager;
    public GameObject controlledBoidPrefab;
    public Transform controlPosition;
    
    public Slider scaleSlider;
    public Slider speedSlider;
    public Button toggleButton;
    public Button addToFlockButton;

    [Header("Color Picker")]
    public Image colorDisplayImage;
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    private GameObject controlledBoidObject;
    private Material controlledBoidMaterial;
    private CustomBoidParameters controlledBoidParameters;
    private bool isControlledBoidInFlock = false;

    void Start()
    {
        InitializeControlledBoid();
        SetupUIListeners();
        UpdateBoidParameters();
    }

    void InitializeControlledBoid()
    {
        controlledBoidObject = Instantiate(controlledBoidPrefab, controlPosition.position, Quaternion.identity);
        controlledBoidMaterial = new Material(controlledBoidObject.GetComponent<Renderer>().sharedMaterial);
        controlledBoidObject.GetComponent<Renderer>().material = controlledBoidMaterial;

        controlledBoidParameters = new CustomBoidParameters{
            scale = 1f
        };
    }

    void SetupUIListeners()
    {
        scaleSlider.onValueChanged.AddListener(UpdateScale);
        speedSlider.onValueChanged.AddListener(UpdateSpeed);
        toggleButton.onClick.AddListener(ToggleBoid);
        addToFlockButton.onClick.AddListener(AddControlledBoidToFlock);

        redSlider.onValueChanged.AddListener(_ => UpdateColor());
        greenSlider.onValueChanged.AddListener(_ => UpdateColor());
        blueSlider.onValueChanged.AddListener(_ => UpdateColor());
    }

    void UpdateScale(float scale)
    {
        controlledBoidParameters.scale = 0.1f;
        UpdateBoidParameters();
    }

    void UpdateSpeed(float speed)
    {
        // Speed is not directly a part of CustomBoidParameters, but you might want to use it when adding to flock
        UpdateBoidParameters();
    }

    void UpdateColor()
    {
        Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        colorDisplayImage.color = newColor;
        controlledBoidMaterial.SetColor("_BackColor", newColor);
        controlledBoidParameters.backColor = newColor;
        UpdateBoidParameters();
    }

    void UpdateBoidParameters()
    {
        if (!isControlledBoidInFlock)
        {
            controlledBoidObject.transform.position = controlPosition.position;
            controlledBoidObject.transform.rotation = Quaternion.LookRotation(controlPosition.forward);
            controlledBoidObject.transform.localScale = Vector3.one * controlledBoidParameters.scale;

            // Update other material properties here
            controlledBoidMaterial.SetColor("_BackColor", controlledBoidParameters.backColor);
            // Set other properties...
        }
    }

    void ToggleBoid()
    {
        isControlledBoidInFlock = !isControlledBoidInFlock;

        if (isControlledBoidInFlock)
        {
            AddControlledBoidToFlock();
        }
        else
        {
            RemoveControlledBoidFromFlock();
        }
    }

    void AddControlledBoidToFlock()
    {
        if (customBoidManager != null)
        {
            // Create a CustomBoidObject on the fly
            GameObject tempObject = new GameObject("TempCustomBoid");
            CustomBoidObject tempCustomBoid = tempObject.AddComponent<CustomBoidObject>();
            tempCustomBoid.parameters = controlledBoidParameters;

            customBoidManager.AddCustomBoidToFlock(tempCustomBoid);

            // Destroy the temporary object
            Destroy(tempObject);

            controlledBoidObject.SetActive(false);
            isControlledBoidInFlock = true;
        }
        else
        {
            Debug.LogError("CustomBoidManager is not assigned!");
        }
    }

    void RemoveControlledBoidFromFlock()
    {
        // This functionality is not implemented in BoidsManager yet
        // You might want to add a method in BoidsManager to remove a specific boid

        controlledBoidObject.SetActive(true);
        isControlledBoidInFlock = false;
        UpdateBoidParameters();
    }

    // You might want to add methods to update other CustomBoidParameters properties
}