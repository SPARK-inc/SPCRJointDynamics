/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 *  @author Hiromoto Noriyuki <hiromoto.noriyuki@spark-creative.co.jp>
 *          Nakajima Satoru <nakajima.satoru@spark-creative.co.jp>
 *          Piyush Nitnaware <nitnaware.piyush@spark-creative.co.jp>
*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SPCR
{
    [DefaultExecutionOrder(20000 - 8)]
    public class SPCRJointDynamicsHelper : MonoBehaviour
    {
        SPCRJointDynamicsController[] _Controllers;
        Vector3 _PrevFramePosition;
        Quaternion _PrevFrameRotation;

        [Header("=== Auto warp ===")]
        public Transform _AutoWarpTarget;
        public float _AutoWarpMoveThreshold = 0.0f;
        public float _AutoWarpEularRotateThreshold = 0.0f;

        private void Awake()
        {
            _Controllers = gameObject.GetComponentsInChildren<SPCRJointDynamicsController>();

            if (_AutoWarpTarget != null)
            {
                _PrevFramePosition = _AutoWarpTarget.position;
                _PrevFrameRotation = _AutoWarpTarget.rotation;
            }
        }

        private void LateUpdate()
        {
            if (_AutoWarpTarget != null)
            {
                var RequireWarp = false;

                var CurrFramePosition = _AutoWarpTarget.position;
                var CurrFrameRotation = _AutoWarpTarget.rotation;

                if (_AutoWarpMoveThreshold > 0.0f)
                {
                    var DiffPosition = Vector3.Distance(CurrFramePosition, _PrevFramePosition);
                    if (DiffPosition < _AutoWarpMoveThreshold * Time.deltaTime)
                    {
                        RequireWarp = true;
                    }
                }

                if (_AutoWarpEularRotateThreshold > 0.0f)
                {
                    var DiffRotation = CurrFrameRotation * Quaternion.Inverse(_PrevFrameRotation);
                    var DiffEular = Mathf.Acos(DiffRotation.w) * 2.0f * Mathf.Rad2Deg;
                    if (DiffEular > 180.0f) DiffEular -= 360.0f;
                    if (Mathf.Abs(DiffEular) > _AutoWarpEularRotateThreshold * Time.deltaTime)
                    {
                        RequireWarp = true;
                    }
                }

                _PrevFramePosition = CurrFramePosition;
                _PrevFrameRotation = CurrFrameRotation;

                if (RequireWarp)
                {
                    foreach (var ctrl in _Controllers)
                    {
                        ctrl.Warp();
                    }
                }
            }
        }

        public void AllReset(float Delay, bool RebuildParameter = false)
        {
            foreach (var ctrl in _Controllers)
            {
                ctrl.ResetPhysics(Delay, RebuildParameter);
            }
        }

        public void AllEnable()
        {
            foreach (var ctrl in _Controllers)
            {
                ctrl.enabled = true;
            }
        }

        public void AllDisable()
        {
            foreach (var ctrl in _Controllers)
            {
                ctrl.enabled = false;
            }
        }
    }
}
