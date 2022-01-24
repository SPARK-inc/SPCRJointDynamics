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

#define ENABLE_BURST

using UnityEngine;
using Unity.Jobs;
using System.Collections;
using System.Collections.Generic;

namespace SPCR
{
    [DefaultExecutionOrder(30001)]
    public class SPCRJointDynamicsJobManager : MonoBehaviour
    {
        static SPCRJointDynamicsJobManager _Instance;

        List<SPCRJointDynamicsController> _Controllers = new List<SPCRJointDynamicsController>();

        static public SPCRJointDynamicsJobManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    var obj = new GameObject("SPCRJointDynamicsJobManager");
                    DontDestroyOnLoad(obj);
                    _Instance = obj.AddComponent<SPCRJointDynamicsJobManager>();
                }
                return _Instance;
            }
        }

        static public void Push(SPCRJointDynamicsController ctrl)
        {
            Instance._Controllers.Add(ctrl);

            Instance._DynamicsBoneCount += ctrl.PointTbl.Length;
            Instance._DynamicsColliderCount += ctrl._ColliderTbl.Length;
        }

        static public void Pop(SPCRJointDynamicsController ctrl)
        {
            Instance._Controllers.Remove(ctrl);

            Instance._DynamicsBoneCount -= ctrl.PointTbl.Length;
            Instance._DynamicsColliderCount -= ctrl._ColliderTbl.Length;
        }

        private void Awake()
        {
            StartCoroutine("EndOfFrame");
        }

        private void OnDestroy()
        {
            StopCoroutine("EndOfFrame");

            _Instance = null;
        }

        void Update()
        {
            foreach (var ctrl in _Controllers)
            {
                ctrl.UpdateImpl();
            }

            foreach (var ctrl in _Controllers)
            {
                ctrl.PostUpdateImpl();
            }
        }

        void LateUpdate()
        {
            foreach (var ctrl in _Controllers)
            {
                ctrl.PreLateUpdateImpl();
            }

            foreach (var ctrl in _Controllers)
            {
                ctrl.LateUpdateImpl();
            }

            foreach (var ctrl in _Controllers)
            {
                ctrl.PostLateUpdateImpl();
            }
        }

        IEnumerator EndOfFrame()
        {
            var EoF = new WaitForEndOfFrame();

            for (; ; )
            {
                yield return EoF;

                foreach (var ctrl in _Controllers)
                {
                    ctrl.WaitLateUpdateImpl();
                }
            }
        }

        public int _DynamicsBoneCount;
        public int _DynamicsColliderCount;
    }
}
