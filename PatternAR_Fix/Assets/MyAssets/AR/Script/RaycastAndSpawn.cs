using UnityEngine;
using UnityEngine.UI;

public class RaycastAndSpawn : MonoBehaviour
{
    public Camera mainCamera;
    public Button activateButton;
    public GameObject prefabToSpawn;
    public float spawnHeight = 1.0f;

    private RaycastHit hitInfo;
    private bool isHit = false;

    void Start()
    {
        if (activateButton != null)
        {
            activateButton.gameObject.SetActive(false);
            activateButton.onClick.AddListener(OnButtonClick);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                isHit = true;
                activateButton.gameObject.SetActive(true);
            }
            else
            {
                isHit = false;
                activateButton.gameObject.SetActive(false);
            }
        }
    }

    void OnButtonClick()
    {
        if (isHit)
        {
            Vector3 spawnPosition = hitInfo.point + Vector3.up * spawnHeight;
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}
