using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    float _Timer;

    // Update is called once per frame
    void Update()
    {
        _Timer += Time.deltaTime;
        if (_Timer > Mathf.PI) _Timer -= Mathf.PI * 2.0f;
        float angle = Mathf.Sin(_Timer) * 90.0f;
        transform.eulerAngles = new Vector3(0.0f, angle, 0.0f);
    }
}
