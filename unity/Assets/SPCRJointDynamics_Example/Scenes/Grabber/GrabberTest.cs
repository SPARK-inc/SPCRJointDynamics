using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberTest : MonoBehaviour
{
    Vector3 _position;
    float _offset_time;

    // Start is called before the first frame update
    void Start()
    {
        _position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var offset = new Vector3(0.0f, Mathf.Sin(_offset_time * 2.0f), 0.0f);
        transform.position = _position + offset;
        _offset_time += Time.deltaTime;
    }
}
