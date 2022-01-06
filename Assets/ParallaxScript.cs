using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScript : MonoBehaviour
{

    public float ParallaxEffect;
    private Transform cam;
    private Vector3 lastCamPosition;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        lastCamPosition = cam.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPosition;
        transform.position += delta * ParallaxEffect;
        lastCamPosition = cam.position;
    }
}
