using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWind : MonoBehaviour
{
    public Vector3 _WindDirection = new Vector3(0.0f, 0.0f, -1.0f);
    public float _WindPower = 1.0f;
    public float _WindRotationSpeed = 1.0f;
    [Range(0.0f, 1.0f)]
    public float _WindRandomForce = 1.0f;
    float _WindTime;

    SPCR.SPCRJointDynamicsController[] _Controllers;

    private void OnEnable()
    {
        _Controllers = GetComponentsInChildren<SPCR.SPCRJointDynamicsController>();
    }

    float GetSin(float r)
    {
        return Mathf.Lerp(1.0f, (Mathf.Sin(r) * 0.5f + 0.5f), _WindRandomForce);
    }

    void Update()
    {
        _WindTime += Time.deltaTime;
        float WindForce =
            GetSin(_WindTime * _WindRotationSpeed) +
            GetSin(_WindTime * _WindRotationSpeed * 1.75f) * 0.5f +
            GetSin(_WindTime * _WindRotationSpeed * 3.50f) * 0.25f;
        var Wind = _WindDirection * (WindForce * _WindPower / 1.75f);

        foreach (var ctrl in _Controllers)
        {
            ctrl._WindForce = Wind;
        }
    }
}
