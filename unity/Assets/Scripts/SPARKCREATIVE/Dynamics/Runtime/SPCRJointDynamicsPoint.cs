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
        public float _MovableRadius = -1.0f;
        public GameObject MovableTarget { get; private set; }

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
        public int _MovableTargetIndex;

        public bool _UseForSurfaceCollision = true;

        public void Reset()
        {
            if (string.IsNullOrEmpty(UniqueGUIID))
                GenerateNewID();
        }

        void GenerateNewID()
        {
            uniqueGUIID = System.Guid.NewGuid().ToString();
        }

        public bool CreateMovableTargetPoint()
        {
            if (_MovableRadius > 0.0f)
            {
                if (this.transform.parent != null)
                {
                    if (this.transform.parent.parent != null)
                    {
                        MovableTarget = new GameObject(this.gameObject.name + "_target");
                        MovableTarget.transform.SetParent(this.transform.parent.parent);
                        MovableTarget.transform.position = this.transform.position;
                        MovableTarget.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        MovableTarget.transform.localEulerAngles = Vector3.zero;
                        return true;
                    }
                }
            }
            return false;
        }

        void OnDrawGizmos()
        {
            if (_MovableRadius > 0.0f)
            {
#if UNITY_EDITOR
                if (UnityEditor.Selection.Contains(gameObject))
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.gray;
#else
                Gizmos.color = Color.gray;
#endif
                Gizmos.DrawWireSphere(transform.position, _MovableRadius);
            }
        }
    }
}
