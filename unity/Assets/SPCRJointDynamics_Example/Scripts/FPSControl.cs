using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSControl : MonoBehaviour
{
    [Range(15, 120)]
    public int FrameRate = 60;

    void Update()
    {
        Time.captureFramerate = FrameRate;
        Application.targetFrameRate = FrameRate;
    }
}
