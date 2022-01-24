using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    public bool _RequireReset;
    float _Timer;

    void Start()
    {
        _Timer = -3.0f;
    }

    void Update()
    {
        if (_Timer > 0.0f)
        {
            _Timer -= Time.deltaTime;
            if (_Timer < 0.0f)
            {
                _Timer = -3.0f - _Timer;
                GetComponent<SPCR.SPCRJointDynamicsController>().FadeIn(1.0f);
            }
        }
        else
        {
            _Timer += Time.deltaTime;
            if (_Timer > 0.0f)
            {
                _Timer = +3.0f - _Timer;
                GetComponent<SPCR.SPCRJointDynamicsController>().FadeOut(1.0f);
            }
        }
    }
}
