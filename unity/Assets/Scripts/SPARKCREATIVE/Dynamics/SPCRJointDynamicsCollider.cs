/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *  
 *  @author Noriyuki Hiromoto <hrmtnryk@sparkfx.jp>
*/

using UnityEngine;

public class SPCRJointDynamicsCollider : MonoBehaviour
{
    [SerializeField, Range(0.0f, 5.0f)]
    float _Radius = 0.05f;
    [SerializeField, Range(0.0f, 5.0f)]
    float _Height = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)]
    float _Friction = 0.5f;

    public Transform RefTransform { get; private set; }
    public float Radius { get { return _Radius; } }
    public float Height { get { return _Height; } }
    public float Friction { get { return _Friction; } }

    public bool IsCapsule { get { return _Height > 0.0f; } }

    void Awake()
    {
        RefTransform = transform;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        var pos = transform.position;
        var rot = transform.rotation;

        if (IsCapsule)
        {
            var halfLength = _Height / 2.0f;
            var up = Vector3.up * halfLength;
            var down = Vector3.down * halfLength;
            var right = Vector3.right * _Radius;
            var forward = Vector3.forward * _Radius;
            var top = pos + rot * up;
            var bottom = pos + rot * down;

            var mOld = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
            Gizmos.DrawLine(right - up, right + up);
            Gizmos.DrawLine(-right - up, -right + up);
            Gizmos.DrawLine(forward - up, forward + up);
            Gizmos.DrawLine(-forward - up, -forward + up);

            Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot);
            DrawWireArc(_Radius, 360);
            Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot);
            DrawWireArc(_Radius, 360);

            Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.forward));
            DrawWireArc(_Radius, 180);
            Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward));
            DrawWireArc(_Radius, 180);
            Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward));
            DrawWireArc(_Radius, 180);
            Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(-90, Vector3.forward));
            DrawWireArc(_Radius, 180);

            Gizmos.matrix = mOld;
        }
        else
        {
            Gizmos.DrawWireSphere(pos, _Radius);
        }
    }

    void DrawWireArc(float radius, float angle)
    {
        Vector3 from = Vector3.forward * radius;
        var step = Mathf.RoundToInt(angle / 15.0f);
        for (int i = 0; i <= angle; i += step)
        {
            var rad = (float)i * Mathf.Deg2Rad;
            var to = new Vector3(radius * Mathf.Sin(rad), 0, radius * Mathf.Cos(rad));
            Gizmos.DrawLine(from, to);
            from = to;
        }
    }
}
