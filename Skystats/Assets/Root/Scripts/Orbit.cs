using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public float orbitSpeed;
    public Vector3 offset;

    private void OnMouseDrag()
    {
        float rotX = Input.GetAxis("Mouse X") * orbitSpeed;
        //float rotY = Input.GetAxis("Mouse Y") * orbitSpeed;

        transform.Rotate(Vector3.down + offset, rotX);
        //transform.Rotate(Vector3.right + offset, rotY);
    }
}
