/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 *  @author Hiromoto Noriyuki <hrmtnryk@sparkfx.jp>
*/

using UnityEngine;

namespace SPCR
{
    public class SPCRJointDynamicsPointGrabber : MonoBehaviour
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

        [SerializeField]
        bool _IsEnabled = true;
        [SerializeField, Range(0.0f, 5.0f)]
        float _Radius = 0.05f;
        [SerializeField, Range(0.0f, 1.0f)]
        float _Force = 0.5f;

        public Transform RefTransform { get; private set; }
        public bool IsEnabled { get { return enabled && _IsEnabled; } set { _IsEnabled = value; } }
        public float RadiusRaw { get => _Radius; set => _Radius = value; }
        public float Radius
        {
            get
            {
                return _Radius *
                    Mathf.Max(
                    new float[] {
                    Mathf.Abs(transform.localScale.x),
                    Mathf.Abs(transform.localScale.y),
                    Mathf.Abs(transform.localScale.z)});
            }
            set { _Radius = value; }
        }
        public float Force { get { return _Force; } set { _Force = value; } }

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
                Gizmos.color = new Color(0.8f, 1, 0.8f);
#else
            Gizmos.color = Color.black;
#endif
            ResetTransform();
            Gizmos.DrawWireSphere(transform.position, Radius);
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

        public void Reset()
        {
            if (string.IsNullOrEmpty(uniqueGUIID))
                GenerateNewID();
        }

        void GenerateNewID()
        {
            uniqueGUIID = System.Guid.NewGuid().ToString();
        }

        public void SetUniqueId(string guiiid)
        {
            if (uniqueGUIID.Equals(guiiid) || System.String.IsNullOrEmpty(guiiid))
                return;
            uniqueGUIID = guiiid;
        }
    }
}
