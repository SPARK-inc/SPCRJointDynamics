/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 *  @author Hiromoto Noriyuki <hiromoto.noriyuki@spark-creative.co.jp>
 *          Piyush Nitnaware <nitnaware.piyush@spark-creative.co.jp>
*/

using UnityEngine;
using System.Collections.Generic;

namespace SPCR
{
    [DisallowMultipleComponent, DefaultExecutionOrder(20000)]
    [RequireComponent(typeof(SPCRJointDynamicsJobManager_Step1))]
    [RequireComponent(typeof(SPCRJointDynamicsJobManager_Step2))]
    [RequireComponent(typeof(SPCRJointDynamicsJobManager_Step3))]
    [RequireComponent(typeof(SPCRJointDynamicsJobManager_Step4))]
    public class SPCRJointDynamicsJobManager : MonoBehaviour
    {
        public int _OrderStepCount = 0;
        public List<SPCRJointDynamicsController>[] _Controllers;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        static void OnInitializeOnEnterPlayMode()
        {
            _bDestroyed = false;
            _Instance = null;
        }
#endif//UNITY_EDITOR

        static bool _bDestroyed = false;
        static SPCRJointDynamicsJobManager _Instance;
        static public SPCRJointDynamicsJobManager Instance
        {
            get
            {
                if (!_bDestroyed)
                {
                    if (_Instance == null)
                    {
                        _Instance = GameObject.FindObjectOfType<SPCRJointDynamicsJobManager>();
                        if (_Instance == null)
                        {
                            var obj = new GameObject("SPCRJointDynamicsJobManager");
                            GameObject.DontDestroyOnLoad(obj);
                            _Instance = obj.AddComponent<SPCRJointDynamicsJobManager>();
                            _Instance.Initialize();
                        }
                    }
                }
                return _Instance;
            }
        }

        static public void Push(SPCRJointDynamicsController ctrl)
        {
            if (_bDestroyed) return;

            Instance._Controllers[(int)ctrl.ExecutionOrder].Add(ctrl);

            Instance._DynamicsBoneCount += ctrl.PointTbl.Length;
            Instance._DynamicsColliderCount += ctrl._ColliderTbl.Length;
        }

        static public void Pop(SPCRJointDynamicsController ctrl)
        {
            if (_bDestroyed) return;

            Instance._Controllers[(int)ctrl.ExecutionOrder].Remove(ctrl);

            Instance._DynamicsBoneCount -= ctrl.PointTbl.Length;
            Instance._DynamicsColliderCount -= ctrl._ColliderTbl.Length;
        }

        private void OnDestroy()
        {
            _bDestroyed = true;

            _Instance._Controllers[0].Clear();
            _Instance._Controllers[1].Clear();

            _Instance._DynamicsBoneCount = 0;
            _Instance._DynamicsColliderCount = 0;

            _Instance = null;
        }

        void Initialize()
        {
            _OrderStepCount = System.Enum.GetValues(typeof(SPCRJointDynamicsController.eExecutionOrder)).Length;
            _Controllers = new List<SPCRJointDynamicsController>[_OrderStepCount];
            for (int i = 0; i < _OrderStepCount; ++i)
            {
                _Controllers[i] = new List<SPCRJointDynamicsController>();
            }
        }

        public void Update_Step1()
        {
            for (int i = 0; i < _OrderStepCount; ++i)
            {
                foreach (var ctrl in _Controllers[i])
                {
                    ctrl.InitializeBonePose();
                }
            }
        }

        public void Update_Step2()
        {
            for (int i = 0; i < _OrderStepCount; ++i)
            {
                foreach (var ctrl in _Controllers[i])
                {
                    ctrl.WaitInitializeBoneJobs();
                }
            }
        }

        public void Step1()
        {
            var list = _Controllers[(int)SPCRJointDynamicsController.eExecutionOrder.Default];
            foreach (var ctrl in list)
            {
                ctrl.GetCurrentBoneTransform();
            }
        }

        public void Step2()
        {
            var list = _Controllers[(int)SPCRJointDynamicsController.eExecutionOrder.Default];
            foreach (var ctrl in list)
            {
                ctrl.ExecuteSimulation();
            }
        }

        public void Step3()
        {
            var list = _Controllers[(int)SPCRJointDynamicsController.eExecutionOrder.Default];
            foreach (var ctrl in list)
            {
                ctrl.ApplySimulationToBoneTransform();
            }
        }

        public void Step4()
        {
            {
                var list = _Controllers[(int)SPCRJointDynamicsController.eExecutionOrder.Default];
                foreach (var ctrl in list)
                {
                    ctrl.WaitBoneTransformJobs();
                }
            }
            {
                var list = _Controllers[(int)SPCRJointDynamicsController.eExecutionOrder.AfterDefault];
                foreach (var ctrl in list)
                {
                    ctrl.GetCurrentBoneTransform();
                    ctrl.ExecuteSimulation();
                    ctrl.ApplySimulationToBoneTransform();
                    ctrl.WaitBoneTransformJobs();
                }
            }
        }

        public int _DynamicsBoneCount;
        public int _DynamicsColliderCount;
    }
}
