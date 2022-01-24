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
    [DisallowMultipleComponent]
    public class SPCRJointDynamicsPoint : MonoBehaviour
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

        [Header("=== 物理設定項目 ===")]
        public float _Mass = 1.0f;
        public float _MovableLimitRadius = -1.0f;
        public GameObject MovableLimitTarget { get; private set; }

        public bool EnableMovableLimit { get { return _MovableLimitRadius >= 0.0f; } }

        public bool _UseForSurfaceCollision = true;

        [Header("=== 物理自動設定項目 ===")]
        public SPCRJointDynamicsPoint _RefChildPoint;
        public bool _IsFixed;
        [HideInInspector]
        public Vector3 _BoneAxis = new Vector3(-1.0f, 0.0f, 0.0f);
        [HideInInspector]
        public float _Depth;
        [HideInInspector]
        public int _Index;
        [HideInInspector]
        public int _MovableLimitTargetIndex;

        [HideInInspector]
        public Vector3 _LocalScale;
        [HideInInspector]
        public Vector3 _LocalPosition;
        [HideInInspector]
        public Quaternion _LocalRotation;

        public void Reset()
        {
            if (string.IsNullOrEmpty(UniqueGUIID))
                GenerateNewID();
        }

        void GenerateNewID()
        {
            uniqueGUIID = System.Guid.NewGuid().ToString();
        }

        public bool CreateMovableLimitTarget()
        {
            if (EnableMovableLimit)
            {
                if (this.transform.parent?.parent != null)
                {
                    MovableLimitTarget = new GameObject(this.gameObject.name + "@MovableLimitTarget");
                    MovableLimitTarget.transform.SetParent(this.transform.parent.parent);
                    MovableLimitTarget.transform.position = this.transform.position;
                    MovableLimitTarget.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    MovableLimitTarget.transform.localEulerAngles = Vector3.zero;
                    return true;
                }
            }
            return false;
        }

        void OnDrawGizmos()
        {
            if (EnableMovableLimit && (MovableLimitTarget != null))
            {
#if UNITY_EDITOR
                if (UnityEditor.Selection.Contains(gameObject))
                    Gizmos.color = Color.green;
                else
#endif//UNITY_EDITOR
                    Gizmos.color = Color.gray;

                Gizmos.DrawWireSphere(MovableLimitTarget.transform.position, _MovableLimitRadius);
            }
        }
    }
}
