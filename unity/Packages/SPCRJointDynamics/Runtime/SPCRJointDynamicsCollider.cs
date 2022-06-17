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

namespace SPCR
{
    public class SPCRJointDynamicsCollider : MonoBehaviour
    {
        public enum ColliderForce
        {
            Off,
            Push,
            Pull
        }

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

        public float _Radius = 0.05f;
        public float RadiusRaw { get => _Radius; set => _Radius = value; }
        public float Radius
        {
            get
            {
                var scale = transform.lossyScale;
                return _Radius * ((scale.x + scale.y + scale.z) / 3.0f);
            }
        }

        public float _RadiusTailScale = 1.0f;
        public float RadiusTailScaleRaw { get => _RadiusTailScale; set => _RadiusTailScale = value; }
        public float RadiusTailScale
        {
            get
            {
                return _RadiusTailScale;
            }
        }

        public float _Height = 0.0f;
        public float HeightRaw { get => _Height; set => _Height = value; }
        public float Height
        {
            get
            {
                return _Height + (Mathf.Abs(transform.localScale.y) - 1);
            }
        }

        public float _Friction = 0.5f;
        public float FrictionRaw { get => _Friction; set => _Friction = value; }
        public float Friction
        {
            get
            {
                return _Friction;
            }
        }

        public ColliderForce _SurfaceColliderForce = ColliderForce.Off;

        public bool _ShowColiiderGizmo = true;

        public Transform RefTransform { get; private set; }

        public bool IsCapsule { get { return Height > 0.0f; } }

        void Awake()
        {
            RefTransform = transform;
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

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!_ShowColiiderGizmo)
                return;

            var bRegistered = false;
            var RequireDrawGizmo = false;
            foreach (var ctrl in GetComponentsInParent<SPCRJointDynamicsController>())
            {
                foreach (var col in ctrl._ColliderTbl)
                {
                    if (this == col)
                    {
                        bRegistered = true;
                        if (ctrl._IsDebugDraw_Collider)
                        {
                            RequireDrawGizmo = true;
                        }
                    }
                }
            }

            if (!RequireDrawGizmo && bRegistered)
                return;

            if (bRegistered)
                if (UnityEditor.Selection.Contains(gameObject))
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = new Color(0.6f, 0.6f, 0.8f);
            else
                Gizmos.color = Color.gray;

            ResetTransform();

            var pos = transform.position;
            var rot = transform.rotation;

            if (IsCapsule)
            {
                var halfLength = Height / 2.0f;
                var up = Vector3.up * halfLength;
                var down = Vector3.down * halfLength;
                var right_head = Vector3.right * Radius;
                var right_tail = Vector3.right * Radius * RadiusTailScale;
                var forward_head = Vector3.forward * Radius;
                var forward_tail = Vector3.forward * Radius * RadiusTailScale;
                var top = pos + rot * up;
                var bottom = pos + rot * down;

                var mOld = Gizmos.matrix;

                Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
                Gizmos.DrawLine(right_head - up, right_tail + up);
                Gizmos.DrawLine(-right_head - up, -right_tail + up);
                Gizmos.DrawLine(forward_head - up, forward_tail + up);
                Gizmos.DrawLine(-forward_head - up, -forward_tail + up);

                Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot);
                DrawWireArc(Radius * RadiusTailScale, 360);
                Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot);
                DrawWireArc(Radius, 360);

                Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.forward));
                DrawWireArc(Radius * RadiusTailScale, 180);
                Gizmos.matrix = Matrix4x4.Translate(top) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward));
                DrawWireArc(Radius * RadiusTailScale, 180);
                Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward));
                DrawWireArc(Radius, 180);
                Gizmos.matrix = Matrix4x4.Translate(bottom) * Matrix4x4.Rotate(rot * Quaternion.AngleAxis(-90, Vector3.forward));
                DrawWireArc(Radius, 180);

                Gizmos.matrix = mOld;
            }
            else
            {
                Gizmos.DrawWireSphere(pos, Radius);
            }

            if (IsCapsule)
            {
                if (UnityEditor.Selection.Contains(gameObject))
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.gray;

                switch (_SurfaceColliderForce)
                {
                case ColliderForce.Off:
                    break;
                case ColliderForce.Push:
                    DrawArrow(transform.position + transform.up * Height * 0.5f, -transform.up);
                    break;
                case ColliderForce.Pull:
                    DrawArrow(transform.position - transform.up * Height * 0.5f, transform.up);
                    break;
                }
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

        void DrawArrow(Vector3 pos, Vector3 direction)
        {
            Gizmos.DrawLine(pos, pos + direction * Height);
            float arrowHeight = 0.15f;
            float coneAngle = 20.0f;

            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + coneAngle, 0, 0) * new Vector3(0, 0, 1);
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - coneAngle, 0, 0) * new Vector3(0, 0, 1);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + coneAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - coneAngle, 0) * new Vector3(0, 0, 1);

            Vector3 arrowPos = pos + direction * Height;
            up = arrowPos + up * arrowHeight;
            down = arrowPos + down * arrowHeight;
            right = arrowPos + right * arrowHeight;
            left = arrowPos + left * arrowHeight;


            Gizmos.DrawLine(arrowPos, up);
            Gizmos.DrawLine(arrowPos, down);
            Gizmos.DrawLine(arrowPos, right);
            Gizmos.DrawLine(arrowPos, left);

            Gizmos.DrawLine(up, right);
            Gizmos.DrawLine(right, down);
            Gizmos.DrawLine(down, left);
            Gizmos.DrawLine(left, up);

            arrowPos = arrowPos - direction * (arrowHeight - 0.01f);
            Gizmos.DrawLine(arrowPos, up);
            Gizmos.DrawLine(arrowPos, down);
            Gizmos.DrawLine(arrowPos, right);
            Gizmos.DrawLine(arrowPos, left);
        }
#endif//UNITY_EDITOR
    }
}
