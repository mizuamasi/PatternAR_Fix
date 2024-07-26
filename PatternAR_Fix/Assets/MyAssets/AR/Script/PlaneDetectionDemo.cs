using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneDetectionDemo : MonoBehaviour
{
    [SerializeField] private GameObject arObject;
    [SerializeField] private GameObject scanGuide;
    [SerializeField] private GameObject detectGuide;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool isDeteced;
    private Vector2 screenCenter;

    private void Start()
    {
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private void Update()
    {
        if (isDeteced)
        {
            return;
        }

        var isHit = arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);
        if (isHit)
        {
            //RayとARPlaneが衝突したところのPose
            var hitPose = hits[0].pose;
            detectGuide.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            detectGuide.SetActive(true);
            scanGuide.SetActive(false);
        }
        else
        {
            detectGuide.SetActive(false);
            scanGuide.SetActive(true);
        }
        
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (isHit)
                {
                    //RayとARPlaneが衝突したところのPose
                    var hitPose = hits[0].pose; 
                    //オブジェクトの配置
                    arObject.transform.position = hitPose.position;
                    arObject.SetActive(true);
                    var cameraPos = Camera.main.transform.position;
                    cameraPos.y = arObject.transform.position.y;
                    arObject.transform.LookAt(cameraPos);

                    //平面認識の機能をオフ
                    arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
                    foreach (ARPlane plane in arPlaneManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }

                    detectGuide.SetActive(false);
                    scanGuide.SetActive(false);
                    isDeteced = true;
                }
            }
        }
    }
}
