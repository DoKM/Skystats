using UnityEngine;
using System.Collections;

/**
 * This class attempts to force VERT- Field of view scaling.
 * By default, Unity uses the HOR+ technique.
 * 
 * http://en.wikipedia.org/wiki/Field_of_view_in_video_games#Scaling_methods
 */

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScaling : MonoBehaviour
{

    public float designTimeVerticalFieldOfView = 60;
    public int designTimeWidth = 1280;
    public int designTimeHeight = 720;

    private float hFOVInRads;

    private int prevWidth;
    private int prevHeight;

    void Start()
    {

        prevWidth = designTimeWidth;
        prevHeight = designTimeHeight;

        float aspectRatio = (float)designTimeWidth / (float)designTimeHeight;
        float vFOVInRads = designTimeVerticalFieldOfView * Mathf.Deg2Rad;
        hFOVInRads = 2f * Mathf.Atan(Mathf.Tan(vFOVInRads / 2f) * aspectRatio);

    }

    void Update()
  	{
        if (Screen.width != prevWidth || Screen.height != prevHeight)
        {

            float aspectRatio = (float)Screen.width / (float)Screen.height;

            float vFOVInRads = 2f * Mathf.Atan(Mathf.Tan(hFOVInRads / 2f) / aspectRatio);

            foreach (Camera cam in GameObject.FindObjectsOfType(typeof(Camera)))
            {
                cam.fieldOfView = vFOVInRads * Mathf.Rad2Deg;
            }
        }

    }
    
}