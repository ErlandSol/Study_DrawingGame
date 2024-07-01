using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform parent;
    [SerializeField] Transform parent2;
    Camera cam;
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(2))
        {
                if ((parent.transform.rotation.x + Input.mousePositionDelta.x > -12) && (parent.transform.rotation.x + Input.mousePositionDelta.x < 21))
                parent.transform.Rotate(Input.mousePositionDelta.y, 0, 0);
            //transform.position += new Vector3(0, 0, Input.mousePositionDelta.y/100);
        }
        if (Input.GetMouseButton(1))
        {
            //if ((transform.rotation.y + Input.mousePositionDelta.x > -12 ) && (transform.rotation.y + Input.mousePositionDelta.x < -12))
            parent2.transform.Rotate(0, Input.mousePositionDelta.x, 0);
            //transform.position += new Vector3(0, 0, Input.mousePositionDelta.y/100);
        }

        cam.fieldOfView -= Input.mouseScrollDelta.y;


        //transform.LookAt(target);
    }
}
