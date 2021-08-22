using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float Xmin, Ymin, Xmax, Ymax;
    
    private void Start()
    {

    } 
    void Update()
    {
         if (target)
         {
              float Xcam = target.position.x + offset.x;
              float Ycam = target.position.y + offset.y;
              if (Xcam > Xmax) Xcam = Xmax; 
              if (Xcam < Xmin) Xcam = Xmin;
              if (Ycam > Ymax) Ycam = Ymax;
              if (Ycam < Ymin) Ycam = Ymin;
              transform.position = new Vector3 (Xcam, Ycam, offset.z); 
      // Camera follows the player with specified offset position
         }
    }
}
