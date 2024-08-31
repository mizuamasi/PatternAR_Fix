using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class DynamicARPlaneVisibilityController : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private string visibleText = "平面表示: オン";
    [SerializeField] private string hiddenText = "平面表示: オフ";
    private bool arePlanesVisible = true;

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePlanesVisibility);
            UpdateButtonText();
        }
        else
        {
            Debug.LogError("Toggle button is not assigned!");
        }
    }

    public void TogglePlanesVisibility()
    {
        ARPlaneManager planeManager = FindObjectOfType<ARPlaneManager>();
        if (planeManager != null)
        {
            arePlanesVisible = !arePlanesVisible;
            SetPlanesVisibility(planeManager, arePlanesVisible);
            UpdateButtonText();
        }
        else
        {
            Debug.LogWarning("ARPlaneManager not found in the scene!");
        }
    }

    private void SetPlanesVisibility(ARPlaneManager planeManager, bool visible)
    {
        planeManager.enabled = visible;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(visible);
        }
    }

    private void UpdateButtonText()
    {
        Text buttonText = toggleButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = arePlanesVisible ? visibleText : hiddenText;
        }
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePlanesVisibility);
        }
    }
}