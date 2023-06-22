/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 *  @author Hiromoto Noriyuki <hrmtnryk@sparkfx.jp>
 *          Nakajima Satoru <nakajima.satoru@spark-creative.co.jp>
 *          Piyush Nitnaware <nitnaware.piyush@spark-creative.co.jp>
*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SPCR
{
    [DefaultExecutionOrder(20000 - 1)]
    public class SPCRJointDynamicsController : MonoBehaviour
    {
        float _TimeScale = 1.0f;
        public float TimeScale { set { _TimeScale = value; } }

#if UNITY_EDITOR
        public enum eInspectorLang
        {
            日本語,
            English,
        }
        [HideInInspector]
        public eInspectorLang InspectorLang = eInspectorLang.日本語;
        public bool Opened_BaseSettings { get; set; }
        public bool Opened_PhysicsSettings { get; set; }
        public bool Opened_ConstraintSettings { get; set; }
        public bool Opened_AngleLockSettings { get; set; }
        public bool Opened_OptionSettings { get; set; }
        public bool Opened_PreSettings { get; set; }
#endif//UNITY_EDITOR

        public enum ConstraintType
        {
            Structural_Vertical,
            Structural_Horizontal,
            Shear,
            Bending_Vertical,
            Bending_Horizontal,
        }

        [Serializable]
        public class SPCRJointDynamicsConstraint
        {
            public ConstraintType _Type;
            public SPCRJointDynamicsPoint _PointA;
            public SPCRJointDynamicsPoint _PointB;
            public float _Length;

            public SPCRJointDynamicsConstraint(ConstraintType Type, SPCRJointDynamicsPoint PointA, SPCRJointDynamicsPoint PointB)
            {
                _Type = Type;
                _PointA = PointA;
                _PointB = PointB;
                UpdateLength();
            }

            public void UpdateLength()
            {
                _Length = (_PointA.transform.position - _PointB.transform.position).magnitude;
            }
        }

        [Serializable]
        public class SPCRJointDynamicsSurfaceFace
        {
            public SPCRJointDynamicsPoint PointA, PointB, PointC, PointD;
        }

        public enum eExecutionOrder
        {
            Default,
            AfterDefault,
        }

        public string Name;
        public eExecutionOrder ExecutionOrder = eExecutionOrder.Default;

        public Transform _RootTransform;
        public int _SearchPointDepth = 0;
        public SPCRJointDynamicsPoint[] _RootPointTbl = new SPCRJointDynamicsPoint[0];
        public Transform[] _PlaneLimitterTbl = new Transform[0];
        public SPCRJointDynamicsCollider[] _ColliderTbl = new SPCRJointDynamicsCollider[0];
        public SPCRJointDynamicsPointGrabber[] _PointGrabberTbl = new SPCRJointDynamicsPointGrabber[0];

        public int _Relaxation = 1;
        public int _SubSteps = 1;

        public bool _IsCancelResetPhysics = false;

        public bool _IsEnableSurfaceCollision = false;

        public AnimationCurve _MassScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _GravityScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _WindForceScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _ResistanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 5.0f) });
        public AnimationCurve _HardnessCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 5.0f), new Keyframe(1.0f, 0.1f) });
        public AnimationCurve _FrictionCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.7f), new Keyframe(1.0f, 0.7f) });
        public AnimationCurve _SliderJointLengthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });

        public AnimationCurve _AllShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.8f), new Keyframe(1.0f, 0.7f) });
        public AnimationCurve _AllStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.8f), new Keyframe(1.0f, 0.7f) });
        public AnimationCurve _StructuralShrinkVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _StructuralStretchVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _StructuralShrinkHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _StructuralStretchHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _ShearShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _ShearStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _BendingShrinkVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _BendingStretchVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _BendingShrinkHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _BendingStretchHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });

        public AnimationCurve _CollisionAnimateRadius = new AnimationCurve(new Keyframe[] { new Keyframe(0.7f, 0.0f), new Keyframe(1.0f, 1.0f) });
        public float _CollisionReturn_Start = 0.5f;
        public float _CollisionReturn_Speed = 0.5f;

        public Vector3 _Gravity = new Vector3(0.0f, -9.8f, 0.0f);
        public Vector3 _WindForce = new Vector3(0.0f, 0.0f, 0.0f);

        public bool _EnableWindForcePowerToAnimationBlendRatio = false;
        public AnimationCurve _WindForcePowerToAnimationBlendRatioCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });

        public float _RootSlideLimit = 5.0f;        // Negative value is disable
        public float _RootRotateLimit = 540.0f;     // Negative value is disable
        public float _ConstraintShrinkLimit = -1.0f;// Negative value is disable
        public float _ResetDelaySeconds = 0.0f;

        public float _AllShrink = 1.0f;
        public float _AllStretch = 1.0f;
        public float _StructuralShrinkVertical = 1.0f;
        public float _StructuralStretchVertical = 1.0f;
        public float _StructuralShrinkHorizontal = 1.0f;
        public float _StructuralStretchHorizontal = 1.0f;
        public float _ShearShrink = 1.0f;
        public float _ShearStretch = 1.0f;
        public float _BendingShrinkVertical = 1.0f;
        public float _BendingStretchVertical = 1.0f;
        public float _BendingShrinkHorizontal = 1.0f;
        public float _BendingStretchHorizontal = 1.0f;

        public bool _IsAllStructuralShrinkVertical = false;
        public bool _IsAllStructuralStretchVertical = false;
        public bool _IsAllStructuralShrinkHorizontal = true;
        public bool _IsAllStructuralStretchHorizontal = true;
        public bool _IsAllShearShrink = true;
        public bool _IsAllShearStretch = true;
        public bool _IsAllBendingShrinkVertical = true;
        public bool _IsAllBendingStretchVertical = true;
        public bool _IsAllBendingShrinkHorizontal = true;
        public bool _IsAllBendingStretchHorizontal = true;

        public bool _IsCollideStructuralVertical = true;
        public bool _IsCollideStructuralHorizontal = true;
        public bool _IsCollideShear = true;

        public enum eFade
        {
            None,
            In,
            Out,
        }
        eFade _eFade;
        float _FadeSec;
        float _FadeSecLength;
        float _FadeBlendRatio;
        float _CurrentBlendRatio;
        float _CollisionScale = 1.0f;

        public float _BlendRatio;

        [SerializeField]
        SPCRJointDynamicsPoint[] _PointTbl = new SPCRJointDynamicsPoint[0];
        public SPCRJointDynamicsPoint[] PointTbl { get => _PointTbl; set => _PointTbl = value; }
        [SerializeField]
        SPCRJointDynamicsConstraint[] _ConstraintsStructuralVertical = new SPCRJointDynamicsConstraint[0];
        public SPCRJointDynamicsConstraint[] ConstraintsStructuralVertical { get => _ConstraintsStructuralVertical; set => _ConstraintsStructuralVertical = value; }
        [SerializeField]
        SPCRJointDynamicsConstraint[] _ConstraintsStructuralHorizontal = new SPCRJointDynamicsConstraint[0];
        public SPCRJointDynamicsConstraint[] ConstraintsStructuralHorizontal { get => _ConstraintsStructuralHorizontal; set => _ConstraintsStructuralHorizontal = value; }
        [SerializeField]
        SPCRJointDynamicsConstraint[] _ConstraintsShear = new SPCRJointDynamicsConstraint[0];
        public SPCRJointDynamicsConstraint[] ConstraintsShear { get => _ConstraintsShear; set => _ConstraintsShear = value; }
        [SerializeField]
        SPCRJointDynamicsConstraint[] _ConstraintsBendingVertical = new SPCRJointDynamicsConstraint[0];
        public SPCRJointDynamicsConstraint[] ConstraintsBendingVertical { get => _ConstraintsBendingVertical; set => _ConstraintsBendingVertical = value; }
        [SerializeField]
        SPCRJointDynamicsConstraint[] _ConstraintsBendingHorizontal = new SPCRJointDynamicsConstraint[0];
        public SPCRJointDynamicsConstraint[] ConstraintsBendingHorizontal { get => _ConstraintsBendingHorizontal; set => _ConstraintsBendingHorizontal = value; }

        [SerializeField]
        SPCRJointDynamicsSurfaceFace[] _surfaceFacePoints = new SPCRJointDynamicsSurfaceFace[0];
        public SPCRJointDynamicsSurfaceFace[] SurfaceFacePoints { get => _surfaceFacePoints; set => _surfaceFacePoints = value; }

        public bool _IsLoopRootPoints = false;
        public bool _IsComputeStructuralVertical = true;
        public bool _IsComputeStructuralHorizontal = true;
        public bool _IsComputeShear = false;
        public bool _IsComputeBendingVertical = true;
        public bool _IsComputeBendingHorizontal = true;

        public bool _IsDebugDrawPointGizmo = false;
        public bool _IsDebugDraw_StructuralVertical = false;
        public bool _IsDebugDraw_StructuralHorizontal = false;
        public bool _IsDebugDraw_Shear = false;
        public bool _IsDebugDraw_BendingVertical = false;
        public bool _IsDebugDraw_BendingHorizontal = false;
        public bool _IsDebugDraw_SurfaceFace = false;
        public float _Debug_SurfaceNormalLength = 0.1f;
        public bool _IsDebugDraw_RuntimeColliderBounds = false;
        public bool _IsDebugDraw_Collider = true;

        [SerializeField]
        SPCRJointDynamicsJob.Constraint[][] _ConstraintTable;
        public SPCRJointDynamicsJob.Constraint[][] ConstraintTable { get => _ConstraintTable; set => _ConstraintTable = value; }

        [SerializeField]
        int _MaxPointDepth = 0;
        public int MaxPointDepth { get => _MaxPointDepth; set => _MaxPointDepth = value; }

        [SerializeField]
        public bool _IsPaused = false;

        [SerializeField]
        public int _StabilizationFrameRate = 60;

        public bool _UseLimitAngles = false;
        public int _LimitAngle = -1;// Negative value is disable
        public bool _LimitFromRoot = false;
        public AnimationCurve _LimitPowerCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f) });

        public bool _IsFakeWave = false;
        [NonSerialized]
        public float _FakeWaveSpeedScale = 1.0f;
        [NonSerialized]
        public float _FakeWavePowerScale = 1.0f;

        public float _FakeWaveSpeed = 10.0f;
        public float _FakeWavePower = 0.05f;
        public AnimationCurve _FakeWavePowerCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _FakeWaveFreqCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 2.0f) });

        float _RestDeltaTime;
        float _DelayTime;
        bool _RequireWarp = false;
        SPCRJointDynamicsJob _Job = new SPCRJointDynamicsJob();

        public bool RunValidityChecks()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            var HEAD = "SPCR_DATA_ERROR [[" + gameObject.name + "(" + Name + ") ]]: ";
            if (_RootTransform == null)
            {
                Debug.LogError(HEAD + "_RootTransform is Null!!!");
                return false;
            }

            for (int i = 0; i < _RootPointTbl.Length; ++i)
            {
                if (_RootPointTbl[i] == null)
                {
                    Debug.LogError(HEAD + "_RootPointTbl[" + i + "] is Null!!!");
                    return false;
                }
                foreach (var c in _RootPointTbl[i].gameObject.GetComponents<Component>())
                {
                    if (c == null)
                    {
                        Debug.LogError(HEAD + "_RootPointTbl[" + i + "] is Missing!!!");
                        return false;
                    }
                }
            }
            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                if (_PointTbl[i] == null)
                {
                    Debug.LogError(HEAD + "_PointTbl[" + i + "] is Null!!!");
                    return false;
                }
                foreach (var c in _PointTbl[i].gameObject.GetComponents<Component>())
                {
                    if (c == null)
                    {
                        Debug.LogError(HEAD + "_PointTbl[" + i + "] is Missing!!!");
                        return false;
                    }
                }
            }
            for (int i = 0; i < _ColliderTbl.Length; ++i)
            {
                if (_ColliderTbl[i] == null)
                {
                    Debug.LogError(HEAD + "_ColliderTbl[" + i + "] is null!!!");
                    return false;
                }
                foreach (var c in _ColliderTbl[i].gameObject.GetComponents<Component>())
                {
                    if (c == null)
                    {
                        Debug.LogError(HEAD + "_ColliderTbl[" + i + "] is Missing!!!");
                        return false;
                    }
                }
            }
            for (int i = 0; i < _PointGrabberTbl.Length; ++i)
            {
                if (_PointGrabberTbl[i] == null)
                {
                    Debug.LogError(HEAD + "_PointGrabberTbl[" + i + "] is null!!!");
                    return false;
                }
                foreach (var c in _PointGrabberTbl[i].gameObject.GetComponents<Component>())
                {
                    if (c == null)
                    {
                        Debug.LogError(HEAD + "_PointGrabberTbl[" + i + "] is Missing!!!");
                        return false;
                    }
                }
            }
            for (int i = 0; i < _PlaneLimitterTbl.Length; ++i)
            {
                if (_PlaneLimitterTbl[i] == null)
                {
                    Debug.LogError(HEAD + "_PlaneLimitterTbl[" + i + "] is null!!!");
                    return false;
                }
                foreach (var c in _PlaneLimitterTbl[i].gameObject.GetComponents<Component>())
                {
                    if (c == null)
                    {
                        Debug.LogError(HEAD + "_PlaneLimitterTbl[" + i + "] is Missing!!!");
                        return false;
                    }
                }
            }

            foreach (var c in _ConstraintsStructuralVertical)
            {
                if (!_PointTbl.Contains(c._PointA) || !_PointTbl.Contains(c._PointB))
                {
                    Debug.LogError(HEAD + "_ConstraintsStructuralVertical is Invalid!!!");
                    return false;
                }
            }
            foreach (var c in _ConstraintsStructuralHorizontal)
            {
                if (!_PointTbl.Contains(c._PointA) || !_PointTbl.Contains(c._PointB))
                {
                    Debug.LogError(HEAD + "_ConstraintsStructuralHorizontal is Invalid!!!");
                    return false;
                }
            }
            foreach (var c in _ConstraintsShear)
            {
                if (!_PointTbl.Contains(c._PointA) || !_PointTbl.Contains(c._PointB))
                {
                    Debug.LogError(HEAD + "_ConstraintsShear is Invalid!!!");
                    return false;
                }
            }
            foreach (var c in _ConstraintsBendingVertical)
            {
                if (!_PointTbl.Contains(c._PointA) || !_PointTbl.Contains(c._PointB))
                {
                    Debug.LogError(HEAD + "_ConstraintsBendingVertical is Invalid!!!");
                    return false;
                }
            }
            foreach (var c in _ConstraintsBendingHorizontal)
            {
                if (!_PointTbl.Contains(c._PointA) || !_PointTbl.Contains(c._PointB))
                {
                    Debug.LogError(HEAD + "_ConstraintsBendingHorizontal is Invalid!!!");
                    return false;
                }
            }

            foreach (var c in _surfaceFacePoints)
            {
                if (!_PointTbl.Contains(c.PointA) || !_PointTbl.Contains(c.PointB) || !_PointTbl.Contains(c.PointC) || !_PointTbl.Contains(c.PointD))
                {
                    Debug.LogError(HEAD + "_surfaceFacePoints is Invalid!!!");
                    return false;
                }
            }
#endif//DEVELOPMENT_BUILD || UNITY_EDITOR

            return true;
        }

        public void SetPointDynamicsRatio(int Index, float Ratio)
        {
            if (!isActiveAndEnabled) return;

            if ((Index < 0) || (Index >= _PointTbl.Length))
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                UnityEngine.Debug.Log("SetPointDynamicsRatio( " + Index + " ) <<< INVALID Index >>> " + gameObject.name + " [" + Name + "]");
#endif//DEVELOPMENT_BUILD || UNITY_EDITOR
                return;
            }

            _Job.SetPointDynamicsRatio(Index, Ratio);
        }

        public void Warp()
        {
            _RequireWarp = true;
        }

        void Awake()
        {
            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                var pt = _PointTbl[i];
                pt._LocalScale = pt.transform.localScale;
                pt._LocalPosition = pt.transform.localPosition;
                pt._LocalRotation = pt.transform.localRotation;
            }

            RunValidityChecks();
        }

        void OnEnable()
        {
            _DelayTime = _ResetDelaySeconds;
            InitializeAll();

            SPCRJointDynamicsJobManager.Push(this);
        }

        void OnDisable()
        {
            SPCRJointDynamicsJobManager.Pop(this);

            UninitializeAll();
        }

        public void InitializeBonePose()
        {
            if (!isActiveAndEnabled) return;

            _Job.TransformInitialize();
        }

        public void WaitInitializeBoneJobs()
        {
            if (!isActiveAndEnabled) return;

            _Job.WaitInitialize();
        }

        public void GetCurrentBoneTransform()
        {
            if (!isActiveAndEnabled) return;

            float DeltaTime = Time.deltaTime * _TimeScale;
            _RestDeltaTime += DeltaTime;

            switch (_eFade)
            {
            case eFade.In:
                _FadeBlendRatio = 1.0f - _FadeSec / _FadeSecLength;
                _FadeSec += DeltaTime;
                if (_FadeSec >= _FadeSecLength)
                {
                    _eFade = eFade.None;
                    _FadeBlendRatio = 0.0f;
                }
                break;
            case eFade.Out:
                _FadeBlendRatio = _FadeSec / _FadeSecLength;
                _FadeSec += DeltaTime;
                if (_FadeSec >= _FadeSecLength)
                {
                    _eFade = eFade.None;
                    _FadeBlendRatio = 1.0f;
                }
                break;
            }

            if (_DelayTime > 0.0f)
            {
                _DelayTime -= DeltaTime;
                if (_DelayTime <= 0.0f)
                {
                    CreationConstraintTable();
                    _Job.Reset(_ConstraintTable, _RootTransform.localToWorldMatrix);
                }
                else
                {
                    return;
                }
            }

            _Job.PreSimulation();
        }

        public void ExecuteSimulation()
        {
            if (!isActiveAndEnabled) return;
            if (_DelayTime > 0.0f) return;

            _CurrentBlendRatio = (1.0f - _FadeBlendRatio) * (1.0f - _BlendRatio);
            if (_EnableWindForcePowerToAnimationBlendRatio)
            {
                _CurrentBlendRatio *= 1.0f - Mathf.Clamp01(_WindForcePowerToAnimationBlendRatioCurve.Evaluate(_WindForce.magnitude));
            }
            _CurrentBlendRatio = 1.0f - _CurrentBlendRatio;

            var StabilizationFrameRate = (float)_StabilizationFrameRate;
            var ONE_STEP_DELTA_TIME = 1.0f / (StabilizationFrameRate + 1.0f);
            var LoopCount = Mathf.FloorToInt(_RestDeltaTime / ONE_STEP_DELTA_TIME);
            var ProcTime = LoopCount == 0 ? _RestDeltaTime : (float)LoopCount * ONE_STEP_DELTA_TIME;

            if (!_IsPaused)
            {
                if (_CurrentBlendRatio < _CollisionReturn_Start)
                {
                    _CollisionScale += _CollisionReturn_Speed * ProcTime;
                }
                else
                {
                    _CollisionScale -= _CollisionReturn_Speed * ProcTime;
                }
                _CollisionScale = Mathf.Clamp01(_CollisionScale);
            }

            _Job.Simulation(
                _RootTransform,
                _RequireWarp ? 0.0f : _RootSlideLimit * ProcTime,
                _RequireWarp ? 0.0f : _RootRotateLimit * ProcTime,
                _ConstraintShrinkLimit < 0.0f ? 1000000.0f : _ConstraintShrinkLimit,
                _IsPaused ? 0.0f : ProcTime,
                _SubSteps * Math.Max(1, LoopCount),
                _WindForce,
                _IsEnableSurfaceCollision,
                _CurrentBlendRatio,
                _IsFakeWave,
                _FakeWaveSpeed * _FakeWaveSpeedScale,
                _FakeWavePower * _FakeWavePowerScale,
                _CollisionScale,
                _Relaxation);

            _RestDeltaTime -= ProcTime;

            if (_RequireWarp)
            {
                _RequireWarp = false;
            }
        }

        public void ApplySimulationToBoneTransform()
        {
            if (!isActiveAndEnabled) return;
            if (_DelayTime > 0.0f) return;

            _Job.PostSimulation();
        }

        public void WaitBoneTransformJobs()
        {
            if (!isActiveAndEnabled) return;
            if (_DelayTime > 0.0f) return;

            _Job.WaitSimulation();
        }

        void InitializeAll()
        {
            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                var pt = _PointTbl[i];
                pt.transform.localScale = pt._LocalScale;
                pt.transform.localPosition = pt._LocalPosition;
                pt.transform.localRotation = pt._LocalRotation;
            }

            var MovableLimitTargets = new List<Transform>(_PointTbl.Length);
            var PointTransforms = new Transform[_PointTbl.Length];
            var Points = new SPCRJointDynamicsJob.Point[_PointTbl.Length];
            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                var src = _PointTbl[i];
                var rate = src._Depth / _MaxPointDepth;

                PointTransforms[i] = src.transform;

                Points[i].Parent = -1;
                Points[i].Child = -1;
                Points[i].Weight = src._IsFixed ? 0.0f : 1.0f;
                Points[i].Mass = Math.Max(0.01f, src._Mass * _MassScaleCurve.Evaluate(rate));
                Points[i].Resistance = 1.0f - Mathf.Clamp01(_ResistanceCurve.Evaluate(rate) * 0.01f);
                Points[i].Hardness = Mathf.Clamp01(_HardnessCurve.Evaluate(rate) * 0.01f);
                Points[i].Gravity = _Gravity * _GravityScaleCurve.Evaluate(rate);
                Points[i].WindForceScale = _WindForceScaleCurve.Evaluate(rate) * rate;
                Points[i].FrictionScale = _FrictionCurve.Evaluate(rate);
                Points[i].SliderJointLength = _SliderJointLengthCurve.Evaluate(rate);
                Points[i].FakeWavePower = _FakeWavePowerCurve.Evaluate(rate);
                Points[i].FakeWaveFreq = _FakeWaveFreqCurve.Evaluate(rate);
                Points[i].BoneAxis = src._BoneAxis;
                Points[i].Position = PointTransforms[i].position;
                Points[i].Direction = PointTransforms[i].parent.position - PointTransforms[i].position;
                Points[i].ParentLength = Points[i].Direction.magnitude;

                if (src.CreateMovableLimitTarget())
                {
                    Points[i].MovableLimitIndex = MovableLimitTargets.Count;
                    Points[i].MovableLimitRadius = src._MovableLimitRadius;
                    MovableLimitTargets.Add(src.MovableLimitTarget.transform);
                }
                else
                {
                    Points[i].MovableLimitIndex = -1;
                }

                var AllShrinkScale = _AllShrink * _AllShrinkScaleCurve.Evaluate(rate);
                var AllStretchScale = _AllStretch * _AllStretchScaleCurve.Evaluate(rate);

                if (_IsAllStructuralShrinkVertical)
                    Points[i].StructuralShrinkVertical = AllShrinkScale;
                else
                    Points[i].StructuralShrinkVertical = _StructuralShrinkVertical * _StructuralShrinkVerticalScaleCurve.Evaluate(rate);

                if (_IsAllStructuralStretchVertical)
                    Points[i].StructuralStretchVertical = AllStretchScale;
                else
                    Points[i].StructuralStretchVertical = _StructuralStretchVertical * _StructuralStretchVerticalScaleCurve.Evaluate(rate);

                if (_IsAllStructuralShrinkHorizontal)
                    Points[i].StructuralShrinkHorizontal = AllShrinkScale;
                else
                    Points[i].StructuralShrinkHorizontal = _StructuralShrinkHorizontal * _StructuralShrinkHorizontalScaleCurve.Evaluate(rate);

                if (_IsAllStructuralStretchHorizontal)
                    Points[i].StructuralStretchHorizontal = AllStretchScale;
                else
                    Points[i].StructuralStretchHorizontal = _StructuralStretchHorizontal * _StructuralStretchHorizontalScaleCurve.Evaluate(rate);

                if (_IsAllShearShrink)
                    Points[i].ShearShrink = AllShrinkScale;
                else
                    Points[i].ShearShrink = _ShearShrink * _ShearShrinkScaleCurve.Evaluate(rate);

                if (_IsAllShearStretch)
                    Points[i].ShearStretch = AllStretchScale;
                else
                    Points[i].ShearStretch = _ShearStretch * _ShearStretchScaleCurve.Evaluate(rate);

                if (_IsAllBendingShrinkVertical)
                    Points[i].BendingShrinkVertical = AllShrinkScale;
                else
                    Points[i].BendingShrinkVertical = _BendingShrinkVertical * _BendingShrinkVerticalScaleCurve.Evaluate(rate);

                if (_IsAllBendingStretchVertical)
                    Points[i].BendingStretchVertical = AllStretchScale;
                else
                    Points[i].BendingStretchVertical = _BendingStretchVertical * _BendingStretchVerticalScaleCurve.Evaluate(rate);

                if (_IsAllBendingShrinkHorizontal)
                    Points[i].BendingShrinkHorizontal = AllShrinkScale;
                else
                    Points[i].BendingShrinkHorizontal = _BendingShrinkHorizontal * _BendingShrinkHorizontalScaleCurve.Evaluate(rate);

                if (_IsAllBendingStretchHorizontal)
                    Points[i].BendingStretchHorizontal = AllStretchScale;
                else
                    Points[i].BendingStretchHorizontal = _BendingStretchHorizontal * _BendingStretchHorizontalScaleCurve.Evaluate(rate);
            }

            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                if (_PointTbl[i]._RefChildPoint != null)
                {
                    Points[i].Child = _PointTbl[i]._RefChildPoint._Index;
                }
            }

            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                if (_PointTbl[i]._RefChildPoint != null)
                {
                    Points[Points[i].Child].Parent = _PointTbl[i]._Index;
                }
            }

            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                if (_PointTbl[i]._RefParentPoint != null)
                {
                    Points[i].Parent = _PointTbl[i]._RefParentPoint._Index;
                }
            }

            CreationConstraintTable();

            if (!_Job.Initialize(
                _RootTransform,
                Points,
                PointTransforms,
                MovableLimitTargets.ToArray(),
                _ConstraintTable,
                _ColliderTbl,
                _PointGrabberTbl,
                _PlaneLimitterTbl,
                SurfaceFacePoints,
                GetAnglesConfig()))
            {
                enabled = false;
            }

            _RestDeltaTime += Time.deltaTime * _TimeScale;

            _eFade = eFade.None;
            _FadeSec = 0.0f;
            _FadeSecLength = 0.0f;
            _FadeBlendRatio = 0.0f;
            _FadeBlendRatio = 0.0f;

            _DelayTime = _ResetDelaySeconds;
        }

        void UninitializeAll()
        {
            _Job.Uninitialize();
        }

        void CreateConstraintStructuralVertical(SPCRJointDynamicsPoint Point, ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            Point._RefChildPoint = null;

            for (int i = 0; i < Point.transform.childCount; ++i)
            {
                var child = Point.transform.GetChild(i);
                var child_point = GetSPCRJointDynamicsPoint(child);
                if (child_point != null)
                {
                    Point._RefChildPoint = Point._ForceChildPoint != null ? Point._ForceChildPoint : child_point;
                    var LocalPosition = Point.transform.InverseTransformPoint(Point._RefChildPoint.transform.position);
                    Point._BoneAxis = LocalPosition.normalized;

                    ConstraintList.Add(new SPCRJointDynamicsConstraint(
                        ConstraintType.Structural_Vertical,
                        Point,
                        child_point));

                    CreateConstraintStructuralVertical(child_point, ref ConstraintList);
                }
            }
        }

        void ComputePointParameter(SPCRJointDynamicsPoint Point, int Depth)
        {
            _MaxPointDepth = Mathf.Max(_MaxPointDepth, Depth);

            Point._Depth = (float)Depth;
            Point._IsFixed = Point._Depth == 0;

            for (int i = 0; i < Point.transform.childCount; ++i)
            {
                var ChildPoint = GetSPCRJointDynamicsPoint(Point.transform.GetChild(i));
                if (ChildPoint != null)
                {
                    ComputePointParameter(ChildPoint, Depth + 1);
                }
            }
        }

        void SearchPoints(ref List<SPCRJointDynamicsPoint> list, GameObject target)
        {
            var pt = GetSPCRJointDynamicsPoint(target);
            if (pt == null) return;

            list.Add(pt);

            for (int i = 0; i < target.transform.childCount; ++i)
            {
                SearchPoints(ref list, target.transform.GetChild(i).gameObject);
            }
        }

        void SetMarking(GameObject target, SPCRJointDynamicsPoint ParentPt, bool bRoot, bool bParentDisable)
        {
            var pt = target.GetComponent<SPCRJointDynamicsPoint>();
            if (pt == null) return;

            pt._RefParentPoint = ParentPt;

            if (bParentDisable)
            {
                pt.DisableMark = true;
            }
            else if (pt.transform.GetComponent<SPCRJointDynamicsController>() != null)
            {
                pt.DisableMark = true;
            }
            else
            {
                pt.DisableMark = false;
            }

            for (int i = 0; i < target.transform.childCount; ++i)
            {
                SetMarking(target.transform.GetChild(i).gameObject, pt, false, pt.DisableMark);
            }
        }

        public void UpdateJointConnection()
        {
            foreach (var root in _RootPointTbl)
            {
                SetMarking(root.gameObject, null, true, false);
            }

            var PointAll = new List<SPCRJointDynamicsPoint>();
            foreach (var root in _RootPointTbl)
            {
                SearchPoints(ref PointAll, root.gameObject);
            }
            _PointTbl = PointAll.ToArray();
            for (int i = 0; i < _PointTbl.Length; ++i)
            {
                _PointTbl[i]._Index = i;
            }

            // All Points
            int HorizontalRootCount = _RootPointTbl.Length;

            // Compute PointParameter
            {
                _MaxPointDepth = 0;
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    ComputePointParameter(_RootPointTbl[i], 0);
                }
            }

            // Shear
            _ConstraintsShear = new SPCRJointDynamicsConstraint[0];
            {
                var ConstraintList = new List<SPCRJointDynamicsConstraint>();
                if (_IsLoopRootPoints)
                {
                    for (int i = 0; i < HorizontalRootCount; ++i)
                    {
                        CreationConstraintShear(
                            _RootPointTbl[(i + 0) % HorizontalRootCount],
                            _RootPointTbl[(i + 1) % HorizontalRootCount],
                            ref ConstraintList);
                    }
                }
                else
                {
                    for (int i = 0; i < HorizontalRootCount - 1; ++i)
                    {
                        CreationConstraintShear(
                            _RootPointTbl[i + 0],
                            _RootPointTbl[i + 1],
                            ref ConstraintList);
                    }
                }
                _ConstraintsShear = ConstraintList.ToArray();
            }

            // Bending Horizontal
            _ConstraintsBendingHorizontal = new SPCRJointDynamicsConstraint[0];
            {
                var ConstraintList = new List<SPCRJointDynamicsConstraint>();
                if (_IsLoopRootPoints)
                {
                    for (int i = 0; i < HorizontalRootCount; ++i)
                    {
                        CreationConstraintBendingHorizontal(
                            _RootPointTbl[(i + 0) % HorizontalRootCount],
                            _RootPointTbl[(i + 2) % HorizontalRootCount],
                            ref ConstraintList);
                    }
                }
                else
                {
                    for (int i = 0; i < HorizontalRootCount - 2; ++i)
                    {
                        CreationConstraintBendingHorizontal(
                            _RootPointTbl[i + 0],
                            _RootPointTbl[i + 2],
                            ref ConstraintList);
                    }
                }
                _ConstraintsBendingHorizontal = ConstraintList.ToArray();
            }

            // Bending Vertical
            _ConstraintsBendingVertical = new SPCRJointDynamicsConstraint[0];
            {
                var ConstraintList = new List<SPCRJointDynamicsConstraint>();
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    CreationConstraintBendingVertical(
                        _RootPointTbl[i],
                        ref ConstraintList);
                }
                _ConstraintsBendingVertical = ConstraintList.ToArray();
            }

            // Stracturarl Horizontal
            _ConstraintsStructuralHorizontal = new SPCRJointDynamicsConstraint[0];
            {
                var ConstraintList = new List<SPCRJointDynamicsConstraint>();
                if (_IsLoopRootPoints)
                {
                    for (int i = 0; i < HorizontalRootCount; ++i)
                    {
                        CreationConstraintHorizontal(
                            _RootPointTbl[(i + 0) % HorizontalRootCount],
                            _RootPointTbl[(i + 1) % HorizontalRootCount],
                            ref ConstraintList);
                    }
                }
                else
                {
                    for (int i = 0; i < HorizontalRootCount - 1; ++i)
                    {
                        CreationConstraintHorizontal(
                            _RootPointTbl[i + 0],
                            _RootPointTbl[i + 1],
                            ref ConstraintList);
                    }
                }
                _ConstraintsStructuralHorizontal = ConstraintList.ToArray();
            }

            // Vertical Structural
            _ConstraintsStructuralVertical = new SPCRJointDynamicsConstraint[0];
            {
                var ConstraintList = new List<SPCRJointDynamicsConstraint>();
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    CreateConstraintStructuralVertical(_RootPointTbl[i], ref ConstraintList);
                }
                _ConstraintsStructuralVertical = ConstraintList.ToArray();
            }

            _surfaceFacePoints = new SPCRJointDynamicsSurfaceFace[0];
            {
                var faceList = new List<SPCRJointDynamicsSurfaceFace>();
                for (int i = 0; i < HorizontalRootCount - 1; ++i)
                {
                    CreateSurfaceFace(_RootPointTbl[i], _RootPointTbl[i + 1], ref faceList);
                }
                if (_IsLoopRootPoints)
                {
                    CreateSurfaceFace(_RootPointTbl[HorizontalRootCount - 1], _RootPointTbl[0], ref faceList);
                }
                _surfaceFacePoints = faceList.ToArray();
            }

            CreationConstraintTable();
        }

        public void UpdateJointDistance()
        {
            for (int i = 0; i < _ConstraintsStructuralVertical.Length; ++i)
            {
                _ConstraintsStructuralVertical[i].UpdateLength();
            }
            for (int i = 0; i < _ConstraintsStructuralHorizontal.Length; ++i)
            {
                _ConstraintsStructuralHorizontal[i].UpdateLength();
            }
            for (int i = 0; i < _ConstraintsShear.Length; ++i)
            {
                _ConstraintsShear[i].UpdateLength();
            }
            for (int i = 0; i < _ConstraintsBendingVertical.Length; ++i)
            {
                _ConstraintsBendingVertical[i].UpdateLength();
            }
            for (int i = 0; i < _ConstraintsBendingHorizontal.Length; ++i)
            {
                _ConstraintsBendingHorizontal[i].UpdateLength();
            }
        }

        public void StretchBoneLength(float BoneStretchScale)
        {
            for (int i = 0; i < _ConstraintsStructuralVertical.Length; ++i)
            {
                var constraint = _ConstraintsStructuralVertical[i];
                var direction = constraint._PointB.transform.position - constraint._PointA.transform.position;
                constraint._PointB.transform.position = constraint._PointA.transform.position + direction * BoneStretchScale;
            }
            UpdateJointDistance();
        }

        public void DeleteJointConnection()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                _PointTbl = new SPCRJointDynamicsPoint[0];
                _ConstraintsStructuralVertical = new SPCRJointDynamicsConstraint[0];
                _ConstraintsStructuralHorizontal = new SPCRJointDynamicsConstraint[0];
                _ConstraintsShear = new SPCRJointDynamicsConstraint[0];
                _ConstraintsBendingVertical = new SPCRJointDynamicsConstraint[0];
                _ConstraintsBendingHorizontal = new SPCRJointDynamicsConstraint[0];
                _ConstraintTable = null;
            }
#endif
        }

        SPCRJointDynamicsPoint GetDynamicsPoint(Transform target, int depth)
        {
            var point = target.GetComponent<SPCRJointDynamicsPoint>();
            if (point != null)
            {
                if (depth == 0) return point;

                for (int i = 0; i < target.childCount; ++i)
                {
                    point = GetDynamicsPoint(target.GetChild(i), depth - 1);
                    if (point != null) return point;
                }
            }

            return null;
        }

        public void SearchRootPoints()
        {
            if (_RootTransform != null)
            {
                var PointList = new List<SPCRJointDynamicsPoint>();
                for (int i = 0; i < _RootTransform.transform.childCount; ++i)
                {
                    var child = _RootTransform.transform.GetChild(i);
                    var point = GetDynamicsPoint(child, _SearchPointDepth);
                    if (point != null)
                    {
                        PointList.Add(point);
                    }
                }
                _RootPointTbl = PointList.ToArray();
            }
        }

        SPCRJointDynamicsPoint GetNearestPoint(Vector3 Base, ref List<SPCRJointDynamicsPoint> Source, bool IsIgnoreY)
        {
            float NearestDistance = float.MaxValue;
            int NearestIndex = -1;
            for (int i = 0; i < Source.Count; ++i)
            {
                var Direction = Source[i].transform.position - Base;
                if (IsIgnoreY) Direction.y = 0.0f;
                var Distance = Direction.sqrMagnitude;
                if (NearestDistance > Distance)
                {
                    NearestDistance = Distance;
                    NearestIndex = i;
                }
            }
            var Point = Source[NearestIndex];
            Source.RemoveAt(NearestIndex);
            return Point;
        }

        public enum UpdateJointConnectionType
        {
            Default,
            SortNearPointXYZ,
            SortNearPointXZ,
            SortNearPointXYZ_FixedBeginEnd,
            SortNearPointXZ_FixedBeginEnd,
        }

        public void SortConstraintsHorizontalRoot(UpdateJointConnectionType Type)
        {
            switch (Type)
            {
            case UpdateJointConnectionType.Default:
                {
                }
                break;
            case UpdateJointConnectionType.SortNearPointXYZ:
                {
                    var SourcePoints = new List<SPCRJointDynamicsPoint>();
                    var EdgeA = _RootPointTbl[0];
                    for (int i = 1; i < _RootPointTbl.Length; ++i)
                    {
                        SourcePoints.Add(_RootPointTbl[i]);
                    }
                    var SortedPoints = new List<SPCRJointDynamicsPoint>();
                    SortedPoints.Add(EdgeA);
                    while (SourcePoints.Count > 0)
                    {
                        SortedPoints.Add(GetNearestPoint(
                            SortedPoints[SortedPoints.Count - 1].transform.position,
                            ref SourcePoints,
                            false));
                    }
                    _RootPointTbl = SortedPoints.ToArray();
                }
                break;
            case UpdateJointConnectionType.SortNearPointXZ:
                {
                    var SourcePoints = new List<SPCRJointDynamicsPoint>();
                    var EdgeA = _RootPointTbl[0];
                    for (int i = 1; i < _RootPointTbl.Length; ++i)
                    {
                        SourcePoints.Add(_RootPointTbl[i]);
                    }
                    var SortedPoints = new List<SPCRJointDynamicsPoint>();
                    SortedPoints.Add(EdgeA);
                    while (SourcePoints.Count > 0)
                    {
                        SortedPoints.Add(GetNearestPoint(
                            SortedPoints[SortedPoints.Count - 1].transform.position,
                            ref SourcePoints,
                            true));
                    }
                    _RootPointTbl = SortedPoints.ToArray();
                }
                break;
            case UpdateJointConnectionType.SortNearPointXYZ_FixedBeginEnd:
                {
                    var SourcePoints = new List<SPCRJointDynamicsPoint>();
                    var EdgeA = _RootPointTbl[0];
                    var EdgeB = _RootPointTbl[_RootPointTbl.Length - 1];
                    for (int i = 1; i < _RootPointTbl.Length - 1; ++i)
                    {
                        SourcePoints.Add(_RootPointTbl[i]);
                    }
                    var SortedPoints = new List<SPCRJointDynamicsPoint>();
                    SortedPoints.Add(EdgeA);
                    while (SourcePoints.Count > 0)
                    {
                        SortedPoints.Add(GetNearestPoint(
                            SortedPoints[SortedPoints.Count - 1].transform.position,
                            ref SourcePoints,
                            false));
                    }
                    SortedPoints.Add(EdgeB);
                    _RootPointTbl = SortedPoints.ToArray();
                }
                break;
            case UpdateJointConnectionType.SortNearPointXZ_FixedBeginEnd:
                {
                    var SourcePoints = new List<SPCRJointDynamicsPoint>();
                    var EdgeA = _RootPointTbl[0];
                    var EdgeB = _RootPointTbl[_RootPointTbl.Length - 1];
                    for (int i = 1; i < _RootPointTbl.Length - 1; ++i)
                    {
                        SourcePoints.Add(_RootPointTbl[i]);
                    }
                    var SortedPoints = new List<SPCRJointDynamicsPoint>();
                    SortedPoints.Add(EdgeA);
                    while (SourcePoints.Count > 0)
                    {
                        SortedPoints.Add(GetNearestPoint(
                            SortedPoints[SortedPoints.Count - 1].transform.position,
                            ref SourcePoints,
                            true));
                    }
                    SortedPoints.Add(EdgeB);
                    _RootPointTbl = SortedPoints.ToArray();
                }
                break;
            }

            UpdateJointConnection();
        }

        public void ImmediateParameterReflection()
        {
            UninitializeAll();
            InitializeAll();
        }

        public void ResetPhysics(float Delay, bool RebuildParameter = false)
        {
            if (!isActiveAndEnabled) return;
            if (_IsCancelResetPhysics) return;

            if (RebuildParameter)
            {
                ImmediateParameterReflection();
            }

            _DelayTime = Mathf.Max(Delay, 0.001f);
        }

        public void FadeIn(float fadeSec)
        {
            if (!isActiveAndEnabled) return;

            if (fadeSec <= 0.0f)
            {
                _eFade = eFade.None;
                _FadeSec = 0.0f;
                _FadeSecLength = 0.0f;
                _FadeBlendRatio = 0.0f;
            }
            else
            {
                _eFade = eFade.In;
                _FadeSec = fadeSec * (1.0f - _FadeBlendRatio);
                _FadeSecLength = fadeSec;
            }
        }

        public void FadeOut(float fadeSec)
        {
            if (!isActiveAndEnabled) return;

            if (fadeSec <= 0.0f)
            {
                _eFade = eFade.None;
                _FadeSec = 0.0f;
                _FadeSecLength = 0.0f;
                _FadeBlendRatio = 1.0f;
            }
            else
            {
                _eFade = eFade.Out;
                _FadeSec = fadeSec * _FadeBlendRatio;
                _FadeSecLength = fadeSec;
            }
        }

        SPCRJointDynamicsPoint GetSPCRJointDynamicsPoint(GameObject o)
        {
            var Point = o.transform.GetComponent<SPCRJointDynamicsPoint>();
            if (Point == null) return null;
            if (Point.DisableMark) return null;

            return Point;
        }

        SPCRJointDynamicsPoint GetSPCRJointDynamicsPoint(Transform o)
        {
            return GetSPCRJointDynamicsPoint(o.gameObject);
        }

        public SPCRJointDynamicsPoint GetChildJointDynamicsPoint(SPCRJointDynamicsPoint Parent)
        {
            if (Parent != null)
            {
                for (int i = 0; i < Parent.transform.childCount; ++i)
                {
                    var child = GetSPCRJointDynamicsPoint(Parent.transform.GetChild(i));
                    if (child != null)
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        void CreationConstraintHorizontal(
            SPCRJointDynamicsPoint PointA,
            SPCRJointDynamicsPoint PointB,
            ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            if ((PointA == null) || (PointB == null)) return;
            if (PointA == PointB) return;

            var childPointA = GetChildJointDynamicsPoint(PointA);
            var childPointB = GetChildJointDynamicsPoint(PointB);

            if ((childPointA != null) && (childPointB != null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Structural_Horizontal,
                    childPointA,
                    childPointB));

                CreationConstraintHorizontal(childPointA, childPointB, ref ConstraintList);
            }
            else if ((childPointA != null) && (childPointB == null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Structural_Horizontal,
                    childPointA,
                    PointB));
            }
            else if ((childPointA == null) && (childPointB != null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Structural_Horizontal,
                    PointA,
                    childPointB));
            }
        }

        void CreationConstraintShear(
            SPCRJointDynamicsPoint PointA,
            SPCRJointDynamicsPoint PointB,
            ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            if ((PointA == null) || (PointB == null)) return;
            if (PointA == PointB) return;

            var childPointA = GetChildJointDynamicsPoint(PointA);
            var childPointB = GetChildJointDynamicsPoint(PointB);
            var childPointA2 = GetChildJointDynamicsPoint(childPointA);
            var childPointB2 = GetChildJointDynamicsPoint(childPointB);
            var childPointA3 = GetChildJointDynamicsPoint(childPointA2);
            var childPointB3 = GetChildJointDynamicsPoint(childPointB2);

            if (childPointA != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Shear,
                    childPointA,
                    PointB));
            }
            else if (childPointA2 != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Shear,
                    childPointA2,
                    PointB));
            }
            else if (childPointA3 != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Shear,
                    childPointA3,
                    PointB));
            }

            if (childPointB != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Shear,
                    PointA,
                    childPointB));
            }
            else if (childPointB2 != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Shear,
                    PointA,
                    childPointB2));
            }
            else if (childPointB3 != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Shear,
                    PointA,
                    childPointB3));
            }
            CreationConstraintShear(childPointA, childPointB, ref ConstraintList);
        }

        void CreationConstraintBendingVertical(
            SPCRJointDynamicsPoint Point,
            ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            if (Point.transform.childCount != 1) return;
            var childA = Point.transform.GetChild(0);

            if (childA.childCount != 1) return;
            var childB = childA.transform.GetChild(0);

            var childPointA = GetSPCRJointDynamicsPoint(childA);
            var childPointB = GetSPCRJointDynamicsPoint(childB);

            if (childPointB != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Vertical,
                    Point,
                    childPointB));
            }

            if (childPointA != null)
            {
                CreationConstraintBendingVertical(childPointA, ref ConstraintList);
            }
        }

        void CreationConstraintBendingHorizontal(
            SPCRJointDynamicsPoint PointA,
            SPCRJointDynamicsPoint PointB,
            ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            if ((PointA == null) || (PointB == null)) return;
            if (PointA == PointB) return;

            var childPointA = GetChildJointDynamicsPoint(PointA);
            var childPointB = GetChildJointDynamicsPoint(PointB);

            if ((childPointA != null) && (childPointB != null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Horizontal,
                    childPointA,
                    childPointB));

                CreationConstraintBendingHorizontal(childPointA, childPointB, ref ConstraintList);
            }
            else if ((childPointA != null) && (childPointB == null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Horizontal,
                    childPointA,
                    PointB));
            }
            else if ((childPointA == null) && (childPointB != null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Horizontal,
                    PointA,
                    childPointB));
            }
        }

        bool FindSameIndexInList(List<SPCRJointDynamicsJob.Constraint> list, SPCRJointDynamicsJob.Constraint constraint)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].IndexA == constraint.IndexA) return true;
                if (list[i].IndexA == constraint.IndexB) return true;
                if (list[i].IndexB == constraint.IndexA) return true;
                if (list[i].IndexB == constraint.IndexB) return true;
            }
            return false;
        }

        void PushConstraintTable(List<List<SPCRJointDynamicsJob.Constraint>> ListTable, SPCRJointDynamicsJob.Constraint constraint)
        {
            for (int i = 0; i < ListTable.Count; ++i)
            {
                var table = ListTable[i];
                if (!FindSameIndexInList(table, constraint))
                {
                    table.Add(constraint);
                    return;
                }
            }

            {
                var list = new List<SPCRJointDynamicsJob.Constraint>();
                list.Add(constraint);
                ListTable.Add(list);
            }
        }

        void CreationConstraintTable()
        {
            var ConstraintTable = new List<List<SPCRJointDynamicsJob.Constraint>>();

            if (_IsComputeBendingHorizontal)
            {
                foreach (var src in _ConstraintsBendingHorizontal)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        IsCollision = 0,
                    });
                }
            }
            if (_IsComputeBendingVertical)
            {
                foreach (var src in _ConstraintsBendingVertical)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        IsCollision = 0,
                    });
                }
            }
            if (_IsComputeShear)
            {
                var IsCollide = _IsEnableSurfaceCollision || _IsCollideShear;
                foreach (var src in _ConstraintsShear)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        IsCollision = (!(src._PointA._IsFixed && src._PointB._IsFixed) && IsCollide) ? 1 : 0,
                    });
                }
            }
            if (_IsComputeStructuralHorizontal)
            {
                var IsCollide = _IsEnableSurfaceCollision || _IsCollideStructuralHorizontal;
                foreach (var src in _ConstraintsStructuralHorizontal)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        IsCollision = (!(src._PointA._IsFixed && src._PointB._IsFixed) && IsCollide) ? 1 : 0,
                    });
                }
            }
            if (_IsComputeStructuralVertical)
            {
                var IsCollide = _IsEnableSurfaceCollision || _IsCollideStructuralVertical;
                foreach (var src in _ConstraintsStructuralVertical)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        IsCollision = (!(src._PointA._IsFixed && src._PointB._IsFixed) && IsCollide) ? 1 : 0,
                    });
                }
            }

            _ConstraintTable = new SPCRJointDynamicsJob.Constraint[ConstraintTable.Count][];
            for (int i = 0; i < ConstraintTable.Count; ++i)
            {
                _ConstraintTable[i] = ConstraintTable[i].ToArray();
            }
        }

        void CreateSurfaceFace(SPCRJointDynamicsPoint PointA, SPCRJointDynamicsPoint PointB, ref List<SPCRJointDynamicsSurfaceFace> faceList)
        {
            if (PointA == null || PointB == null) return;
            if (PointA == PointB) return;

            var childPointA = GetChildJointDynamicsPoint(PointA);
            var childPointB = GetChildJointDynamicsPoint(PointB);

            if (childPointA != null && childPointB != null)
            {
                if (PointA._UseForSurfaceCollision && PointB._UseForSurfaceCollision && childPointA._UseForSurfaceCollision && childPointB._UseForSurfaceCollision)
                {
                    faceList.Add(new SPCRJointDynamicsSurfaceFace { PointA = PointA, PointB = PointB, PointC = childPointB, PointD = childPointA });
                }
                CreateSurfaceFace(childPointA, childPointB, ref faceList);
            }
        }

        void OnDrawGizms_Constraint(SPCRJointDynamicsConstraint[] constraints)
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < constraints.Length; i++)
                {
                    var constraint = constraints[i];
                    var pointA = constraint._PointA.transform.position;
                    var pointB = constraint._PointB.transform.position;
                    Gizmos.DrawLine(pointA, pointB);
                }
            }
            else
            {
                for (int i = 0; i < constraints.Length; i++)
                {
                    var constraint = constraints[i];
                    var pointA = constraint._PointA.transform.position;
                    var pointB = constraint._PointB.transform.position;
                    Gizmos.DrawLine(pointA, pointB);
                }
            }
        }

        void OnDrawGizmo_SurfaceFaces()
        {
            for (int i = 0; i < SurfaceFacePoints.Length; i++)
            {
                var clothPattern = SurfaceFacePoints[i];
                DebugDrawTriangle(clothPattern.PointA.transform.position, clothPattern.PointB.transform.position, clothPattern.PointC.transform.position);
                DebugDrawTriangle(clothPattern.PointC.transform.position, clothPattern.PointD.transform.position, clothPattern.PointA.transform.position);
            }
        }

        void DebugDrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v1);
            Vector3 center = FindCenterOfTheTriangle(v1, v2, v3);
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
            Gizmos.DrawLine(center, center + normal * _Debug_SurfaceNormalLength);
        }

        Vector3 FindCenterOfTheTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = Vector3.Lerp(a, b, 0.5f);
            Vector3 ac = Vector3.Lerp(a, c, 0.5f);
            return Vector3.Lerp(ab, ac, 0.5f);
        }

        SPCRJointDynamicsJob.AngleLimitConfig GetAnglesConfig()
        {
            if (!_UseLimitAngles)
                return new SPCRJointDynamicsJob.AngleLimitConfig { angleLimit = -1, limitFromRoot = _LimitFromRoot };
            return new SPCRJointDynamicsJob.AngleLimitConfig { angleLimit = _LimitAngle, limitFromRoot = _LimitFromRoot }; ;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (!this.enabled)
            {
                return;
            }

            _Job.DrawGizmos_LateUpdatePoints();

            Gizmos.color = Color.magenta;
            _Job.DrawGizmos_Points(_IsDebugDrawPointGizmo);

            if (_IsDebugDraw_RuntimeColliderBounds && _ColliderTbl.Length > 0)
            {
                _Job.DrawGizmos_ColliderEx();
            }

            if (_IsDebugDraw_StructuralVertical)
            {
                Gizmos.color = new Color(0.8f, 0.4f, 0.4f);
                OnDrawGizms_Constraint(_ConstraintsStructuralVertical);
                _Job.DrawGizmos_Constraint(true, false, false, false, false);
            }
            if (_IsDebugDraw_StructuralHorizontal)
            {
                Gizmos.color = new Color(0.4f, 0.8f, 0.4f);
                OnDrawGizms_Constraint(_ConstraintsStructuralHorizontal);
                _Job.DrawGizmos_Constraint(false, true, false, false, false);
            }
            if (_IsDebugDraw_Shear)
            {
                Gizmos.color = new Color(0.4f, 0.4f, 0.8f);
                OnDrawGizms_Constraint(_ConstraintsShear);
                _Job.DrawGizmos_Constraint(false, false, true, false, false);
            }
            if (_IsDebugDraw_BendingVertical)
            {
                Gizmos.color = new Color(0.8f, 0.8f, 0.4f);
                OnDrawGizms_Constraint(_ConstraintsBendingVertical);
                _Job.DrawGizmos_Constraint(false, false, false, true, false);
            }
            if (_IsDebugDraw_BendingHorizontal)
            {
                Gizmos.color = new Color(0.4f, 0.8f, 0.8f);
                OnDrawGizms_Constraint(_ConstraintsBendingHorizontal);
                _Job.DrawGizmos_Constraint(false, false, false, false, true);
            }

            if (_IsDebugDraw_SurfaceFace && _IsEnableSurfaceCollision)
            {
                Gizmos.color = Color.red;
                OnDrawGizmo_SurfaceFaces();
                OnDrawGizms_Constraint(_ConstraintsStructuralVertical);
                OnDrawGizms_Constraint(_ConstraintsStructuralHorizontal);
                OnDrawGizms_Constraint(_ConstraintsShear);
            }
        }
#endif//UNITY_EDITOR
    }
}
