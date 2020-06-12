using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

public class AppManager : MonoBehaviour
{
    public CanvasRenderer sliderCanvasRenderer;

    public void Start() {
        FixElements();
        // Permission.RequestUserPermission(Permission.Camera);
    }

    void FixElements() {
        if (sliderCanvasRenderer)
            sliderCanvasRenderer.SetAlpha(0);
    }

    public void RequestLocationPermissions() {
        // Permission.RequestUserPermission(Permission.FineLocation);
        print("Requesting Location Permission");
    }

    public void GoToARScene() {
        SceneManager.LoadScene("WorldScaleAR");
    }

    public void Recalibrate() {
        print("Recalibrating Map..");
    }
}
