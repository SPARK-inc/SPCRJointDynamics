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
    [SerializeField, HideInInspector]
    private string uniqueGUIID;
    public string UniqueGUIID
    {
        get
        {
            if (string.IsNullOrEmpty(uniqueGUIID))
                GenerateNewID();
            return uniqueGUIID;
        }
    }

    [SerializeField, Range(0.0f, 5.0f)]
    float _Radius = 0.05f;
    public float RadiusRaw { get => _Radius; set =>_Radius = value; }
    public float Radius { get { return _Radius * Mathf.Abs(transform.localScale.x) * Mathf.Abs(transform.localScale.z); } }
    [SerializeField, Range(0.0f, 5.0f)]
    float _HeadRadiusScale = 1.0f;
    public float HeadRadiusScale { get { return _HeadRadiusScale; } set { _HeadRadiusScale = value; } }
    [SerializeField, Range(0.0f, 5.0f)]
    float _TailRadiusScale = 1.0f;
    public float TailRadiusScale { get { return _TailRadiusScale; } set { _TailRadiusScale = value; } }
    [SerializeField, Range(0.0f, 5.0f)]
    float _Height = 0.0f;
    public float HeightRaw { get => _Height; set => _Height = value; }
    [SerializeField, Range(0.0f, 1.0f)]
    float _Friction = 0.5f;
    [SerializeField, Range(0.0f, 1.0f)]
    float _PushOutRate = 1.0f;

    public Transform RefTransform { get; private set; }
    public float RadiusHead { get { return Radius * _HeadRadiusScale; } set { _HeadRadiusScale = value; } }
    public float RadiusTail { get { return Radius * _TailRadiusScale; } }
    public float Height { get { return _Height + (Mathf.Abs(transform.localScale.y) - 1); } set { _Height = value; } }
    public float Friction { get { return _Friction; } set { _Friction = value; } }
    public float PushOutRate { get { return _PushOutRate; } set { _PushOutRate = value; } }

    public bool IsCapsule { get { return Height > 0.0f; } }

    void Awake()
    {
        RefTransform = transform;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.Contains(gameObject))
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.gray;
#else
Gizmos.color = Color.gray;
#endif
        ResetTransform();
        var pos = transform.position;
        var rot = transform.rotation;

        if (IsCapsule)
        {
            var halfLength = Height / 2.0f;
            var up = Vector3.up * halfLength;
            var down = Vector3.down * halfLength;
            var right_head = Vector3.right * Radius * HeadRadiusScale;
            var right_tail = Vector3.right * Radius * TailRadiusScale;
            var forward_head = Vector3.forward * Radius * HeadRadiusScale;
            var forward_tail = Vector3.forward * Radius * TailRadiusScale;
            var top = pos + rot * up;
            var bottom = pos + rot * down;

            var mOld = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
            Gizmos.DrawLine(right_head - up, right_tail + up);
            Gizmos.DrawLine(-right_head - up, -right_tail + up);
            Gizmos.DrawLine(forward_head - up, forward_tail + up);
            Gizmos.DrawLine(-forward_head - up, -forward_tail + up);

            Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot);
            DrawWireArc(Radius * TailRadiusScale, 360);
            Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot);
            DrawWireArc(Radius * HeadRadiusScale, 360);

            Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.forward));
            DrawWireArc(Radius * TailRadiusScale, 180);
            Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward));
            DrawWireArc(Radius * TailRadiusScale, 180);
            Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward));
            DrawWireArc(Radius * HeadRadiusScale, 180);
            Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(-90, Vector3.forward));
            DrawWireArc(Radius * HeadRadiusScale, 180);

            Gizmos.matrix = mOld;
        }
        else
        {
            Gizmos.DrawWireSphere(pos, Radius);
        }
    }

    void ResetTransform()
    {
        if (transform.localScale.x == 0)
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        if (transform.localScale.y == 0)
            transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        if (transform.localScale.z == 0)
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
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

    public void Reset()
    {
        if (string.IsNullOrEmpty(uniqueGUIID))
            GenerateNewID();
    }

    void GenerateNewID()
    {
        uniqueGUIID = System.Guid.NewGuid().ToString();
    }

    public void SetGUIIIde(string guiiId)
    {
        uniqueGUIID = guiiId;
    }
}
