using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float hudRefreshRate = 1f;

    private float timer;

    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            float fps = (int)(1f / Time.unscaledDeltaTime);
            fps = Mathf.Clamp(fps, 3, Mathf.Infinity);
            fpsText.text = "<color=#FFFFFF>FPS:</color> " + fps;
            timer = Time.unscaledTime + hudRefreshRate;
        }
    }
}