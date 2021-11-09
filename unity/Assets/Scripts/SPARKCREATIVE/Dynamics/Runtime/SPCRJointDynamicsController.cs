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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SPCR
{
    [DefaultExecutionOrder(10000)]
    public class SPCRJointDynamicsController : MonoBehaviour
    {
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

        public enum UpdateTiming
        {
            LateUpdate,
            FixedUpdate,
        }

        [Serializable]
        public class SPCRJointDynamicsConstraint
        {
            public ConstraintType _Type;
            public SPCRJointDynamicsPoint _PointA;
            public SPCRJointDynamicsPoint _PointB;
            public SPCRJointDynamicsPoint _PointC; // Mid Point
            public float _Length;
            public float _LengthACB;

            public SPCRJointDynamicsConstraint(ConstraintType Type, SPCRJointDynamicsPoint PointA, SPCRJointDynamicsPoint PointB, SPCRJointDynamicsPoint PointC = null)
            {
                _Type = Type;
                _PointA = PointA;
                _PointB = PointB;
                _PointC = PointC;
                UpdateLength();
            }

            public void UpdateLength()
            {
                _Length = (_PointA.transform.position - _PointB.transform.position).magnitude;

                if (_PointC != null)
                {
                    _LengthACB = (_PointA.transform.position - _PointC.transform.position).magnitude;
                    _LengthACB += (_PointC.transform.position - _PointB.transform.position).magnitude;
                }
                else
                {
                    _LengthACB = _Length;
                }
            }
        }

        [Serializable]
        public class SPCRJointDynamicsSurfaceFace
        {
            public SPCRJointDynamicsPoint PointA, PointB, PointC, PointD;
        }

        public string Name;

        public Transform _RootTransform;
        public SPCRJointDynamicsPoint[] _RootPointTbl = new SPCRJointDynamicsPoint[0];

        public SPCRJointDynamicsCollider[] _ColliderTbl = new SPCRJointDynamicsCollider[0];

        public SPCRJointDynamicsPointGrabber[] _PointGrabberTbl = new SPCRJointDynamicsPointGrabber[0];

        public UpdateTiming _UpdateTiming = UpdateTiming.LateUpdate;

        public int _Relaxation = 3;
        public int _SubSteps = 1;

        public bool _IsEnableFloorCollision = true;
        public float _FloorHeight = 0.02f;

        public bool _IsEnableColliderCollision = true;

        public bool _IsCancelResetPhysics = false;

        public bool _IsEnableSurfaceCollision = false;
        public int _SurfaceCollisionDivision = 1;

        public AnimationCurve _MassScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _GravityScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _WindForceScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _ResistanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.05f) });
        public AnimationCurve _HardnessCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });
        public AnimationCurve _FrictionCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.7f), new Keyframe(1.0f, 0.7f) });
        public AnimationCurve _SliderJointLengthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });
        public AnimationCurve _SliderJointSpringCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.05f), new Keyframe(1.0f, 0.01f) });

        public AnimationCurve _AllShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve _AllStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
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

        public Vector3 _Gravity = new Vector3(0.0f, -9.8f, 0.0f);
        public Vector3 _WindForce = new Vector3(0.0f, 0.0f, 0.0f);

        public float _SpringK = 1.0f;

        public float _RootSlideLimit = -1.0f;   // Negative value is disable
        public float _RootRotateLimit = -1.0f;  // Negative value is disable

        public int _DetailHitDivideMax = 0;

        public float _AllShrink = 1.0f;
        public float _AllStretch = 1.0f;
        public float _StructuralShrinkVertical = 1.0f;
        public float _StructuralStretchVertical = 1.0f;
        public float _StructuralShrinkHorizontal = 1.0f;
        public float _StructuralStretchHorizontal = 1.0f;
        public float _ShearShrink = 1.0f;
        public float _ShearStretch = 1.0f;
        [FormerlySerializedAs("_BendingingShrinkVertical")]
        public float _BendingShrinkVertical = 1.0f;
        [FormerlySerializedAs("_BendingingStretchVertical")]
        public float _BendingStretchVertical = 1.0f;
        [FormerlySerializedAs("_BendingingShrinkHorizontal")]
        public float _BendingShrinkHorizontal = 1.0f;
        [FormerlySerializedAs("_BendingingStretchHorizontal")]
        public float _BendingStretchHorizontal = 1.0f;

        public bool _IsAllStructuralShrinkVertical = false;
        public bool _IsAllStructuralStretchVertical = true;
        public bool _IsAllStructuralShrinkHorizontal = true;
        public bool _IsAllStructuralStretchHorizontal = true;
        public bool _IsAllShearShrink = true;
        public bool _IsAllShearStretch = true;
        [FormerlySerializedAs("_IsAllBendingingShrinkVertical")]
        public bool _IsAllBendingShrinkVertical = true;
        [FormerlySerializedAs("_IsAllBendingingStretchVertical")]
        public bool _IsAllBendingStretchVertical = true;
        [FormerlySerializedAs("_IsAllBendingingShrinkHorizontal")]
        public bool _IsAllBendingShrinkHorizontal = true;
        [FormerlySerializedAs("_IsAllBendingingStretchHorizontal")]
        public bool _IsAllBendingStretchHorizontal = true;

        public bool _IsCollideStructuralVertical = true;
        public bool _IsCollideStructuralHorizontal = true;
        public bool _IsCollideShear = true;
        public bool _IsCollideBendingVertical = false;
        public bool _IsCollideBendingHorizontal = false;

        public bool _UseLimitAngles = false;
        public int _LimitAngle = -1;// Negative value is disable
        public bool _LimitFromRoot = false;
        public AnimationCurve _LimitPowerCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f) });

        public bool _IsReferToAnimation;
        public bool _IsPreventBoneTwist;
        public bool _IsLateUpdateStabilization;
        public int _StabilizationFrameRate = 60;
        float _TimeRest;

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
        public float _Debug_SurfaceNormalLength = 0.25f;
        public bool _IsDebugDraw_RuntimeColliderBounds = false;

        [SerializeField]
        SPCRJointDynamicsJob.Constraint[][] _ConstraintTable;
        public SPCRJointDynamicsJob.Constraint[][] ConstraintTable { get => _ConstraintTable; set => _ConstraintTable = value; }

        [SerializeField]
        int _MaxPointDepth = 0;
        public int MaxPointDepth { get => _MaxPointDepth; set => _MaxPointDepth = value; }

        [SerializeField]
        public bool _IsPaused = false;

        float _Delay;
        SPCRJointDynamicsJob _Job = new SPCRJointDynamicsJob();

#if UNITY_EDITOR
        [SerializeField]
        public List<SPCRJointDynamicsPoint> _SubDivInsertedPoints = new List<SPCRJointDynamicsPoint>();

        [SerializeField]
        public List<SPCRJointDynamicsPoint> _SubDivOriginalPoints = new List<SPCRJointDynamicsPoint>();
#endif

        void Awake()
        {
            var MovableTargetPoints = new List<Transform>(_PointTbl.Length);
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
                Points[i].Resistance = 1.0f - Mathf.Clamp01(_ResistanceCurve.Evaluate(rate));
                Points[i].Hardness = Mathf.Clamp01(_HardnessCurve.Evaluate(rate));
                Points[i].Gravity = _Gravity * _GravityScaleCurve.Evaluate(rate);
                Points[i].WindForceScale = _WindForceScaleCurve.Evaluate(rate) * rate;
                Points[i].FrictionScale = _FrictionCurve.Evaluate(rate);
                Points[i].LimitPower = _LimitPowerCurve.Evaluate(rate);
                Points[i].SliderJointLength = _SliderJointLengthCurve.Evaluate(rate);
                Points[i].SliderJointSpring = _SliderJointSpringCurve.Evaluate(rate);
                Points[i].BoneAxis = src._BoneAxis;
                Points[i].Position = PointTransforms[i].position;
                Points[i].OldPosition = PointTransforms[i].position;
                Points[i].LocalPosition = PointTransforms[i].rotation * -PointTransforms[i].localPosition;
                Points[i].Rotation = PointTransforms[i].rotation;
                Points[i].InitialPosition = _RootTransform.InverseTransformPoint(PointTransforms[i].position);
                Points[i].PreviousDirection = PointTransforms[i].parent.position - PointTransforms[i].position;
                Points[i].ParentLength = Points[i].PreviousDirection.magnitude;
                Points[i].LocalRotation = PointTransforms[i].localRotation;

                if (src.CreateMovableTargetPoint())
                {
                    Points[i].MobableTargetIndex = MovableTargetPoints.Count;
                    Points[i].MobableTargetRadius = src._MovableRadius;
                    MovableTargetPoints.Add(src.MovableTarget.transform);
                }
                else
                {
                    Points[i].MobableTargetIndex = -1;
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
                if (_PointTbl[i]._RefChildPoint == null) continue;

                Points[i].Child = _PointTbl[i]._RefChildPoint._Index;
                Points[Points[i].Child].Parent = _PointTbl[i]._Index;
            }

            CreationConstraintTable();

            List<SPCRJointDynamicsJob.SurfaceFaceConstraints> surfaceConstraints = GetSurfaceFaceConstraints();

            _Job.Initialize(
                _RootTransform,
                Points,
                PointTransforms,
                MovableTargetPoints.ToArray(),
                _ConstraintTable,
                _ColliderTbl,
                _PointGrabberTbl,
                surfaceConstraints.ToArray(),
                _IsReferToAnimation && (_UpdateTiming == UpdateTiming.LateUpdate), _IsPreventBoneTwist);

            _Delay = 0.1f;
            _TimeRest = 0.0f;
        }

        SPCRJointDynamicsJob.AngleLimitConfig GetAnglesConfig()
        {
            if (!_UseLimitAngles)
                return new SPCRJointDynamicsJob.AngleLimitConfig { angleLimit = -1, limitFromRoot = _LimitFromRoot };
            return new SPCRJointDynamicsJob.AngleLimitConfig { angleLimit = _LimitAngle, limitFromRoot = _LimitFromRoot }; ;
        }

        void OnDestroy()
        {
            _Job.Uninitialize();
        }

        void FixedUpdate()
        {
            if (_UpdateTiming == UpdateTiming.FixedUpdate)
            {
                UpdateImpl(Time.fixedDeltaTime);
            }
        }

        void Update()
        {
            _Job.RestoreTransform();
        }

        void LateUpdate()
        {
            if (_UpdateTiming == UpdateTiming.LateUpdate)
            {
                if (_IsLateUpdateStabilization)
                {
                    float DELTA = 1.0f / ((float)_StabilizationFrameRate * 1.1f);
                    if (Time.deltaTime < DELTA)
                    {
                        UpdateImpl(Time.deltaTime);
                        _TimeRest = 0.0f;
                    }
                    else
                    {
                        _TimeRest += Time.deltaTime;
                        while (_TimeRest >= DELTA)
                        {
                            UpdateImpl(DELTA);
                            _TimeRest -= DELTA;
                        }
                    }
                }
                else
                {
                    const float MAX_DELTA_TIME = 1.0f / 10.0f;
                    UpdateImpl(Math.Min(Time.deltaTime, MAX_DELTA_TIME));
                }
            }
        }

        void UpdateImpl(float DeltaTime)
        {
            if (_Delay > 0.0f)
            {
                _Delay -= DeltaTime;
                _TimeRest = 0.0f;
                _Job.Reset();
                return;
            }

            var StepTime = _IsPaused ? 0.0f : DeltaTime;

            _Job.Execute(
                _RootTransform, _RootSlideLimit, _RootRotateLimit,
                StepTime, _SubSteps, Time.fixedDeltaTime,
                _WindForce,
                _Relaxation, _SpringK,
                _IsEnableFloorCollision, _FloorHeight,
                _DetailHitDivideMax,
                _IsEnableColliderCollision,
                _IsEnableSurfaceCollision,
                _SurfaceCollisionDivision,
                GetAnglesConfig());
        }

        void CreateConstraintStructuralVertical(SPCRJointDynamicsPoint Point, ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            for (int i = 0; i < Point.transform.childCount; ++i)
            {
                var child = Point.transform.GetChild(i);
                var child_point = child.gameObject.GetComponent<SPCRJointDynamicsPoint>();
                if (child_point != null)
                {
                    Point._RefChildPoint = child_point;
                    var LocalPosition = Point.transform.InverseTransformPoint(child_point.transform.position);
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
                var ChildPoint = Point.transform.GetChild(i).gameObject.GetComponent<SPCRJointDynamicsPoint>();
                if (ChildPoint != null)
                {
                    ComputePointParameter(ChildPoint, Depth + 1);
                }
            }
        }

        public void UpdateJointConnection()
        {
            var PointAll = new List<SPCRJointDynamicsPoint>();
            foreach (var root in _RootPointTbl)
            {
                PointAll.AddRange(root.gameObject.GetComponentsInChildren<SPCRJointDynamicsPoint>());
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
                            _RootPointTbl[(i + 1) % HorizontalRootCount],
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
                            _RootPointTbl[i + 1],
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

        public void ResetPhysics(float Delay)
        {
            if (_IsCancelResetPhysics) return;

            _Job.Restore();
            _Delay = Delay;
        }

        public SPCRJointDynamicsPoint GetChildJointDynamicsPoint(SPCRJointDynamicsPoint Parent)
        {
            if (Parent != null)
            {
                for (int i = 0; i < Parent.transform.childCount; ++i)
                {
                    var child = Parent.transform.GetChild(i).GetComponent<SPCRJointDynamicsPoint>();
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

            var childPointA = childA.GetComponent<SPCRJointDynamicsPoint>();
            var childPointB = childB.GetComponent<SPCRJointDynamicsPoint>();

            if (childPointB != null)
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Vertical,
                    Point,
                    childPointB,
                    childPointA));
            }

            if (childPointA != null)
            {
                CreationConstraintBendingVertical(childPointA, ref ConstraintList);
            }
        }

        void CreationConstraintBendingHorizontal(
            SPCRJointDynamicsPoint PointA,
            SPCRJointDynamicsPoint PointB,
            SPCRJointDynamicsPoint PointC,
            ref List<SPCRJointDynamicsConstraint> ConstraintList)
        {
            if ((PointA == null) || (PointB == null)) return;
            if (PointA == PointB) return;

            var childPointA = GetChildJointDynamicsPoint(PointA);
            var childPointB = GetChildJointDynamicsPoint(PointB);
            var childPointC = (PointC == null) ? null : GetChildJointDynamicsPoint(PointC);

            if (childPointC == null)
            {
                childPointC = PointC;
            }

            if ((childPointA != null) && (childPointB != null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Horizontal,
                    childPointA,
                    childPointB,
                    childPointC));

                CreationConstraintBendingHorizontal(childPointA, childPointB, childPointC, ref ConstraintList);
            }
            else if ((childPointA != null) && (childPointB == null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Horizontal,
                    childPointA,
                    PointB,
                    childPointC));
            }
            else if ((childPointA == null) && (childPointB != null))
            {
                ConstraintList.Add(new SPCRJointDynamicsConstraint(
                    ConstraintType.Bending_Horizontal,
                    PointA,
                    childPointB,
                    childPointC));
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
                        StretchLength = src._LengthACB,
                        IsCollision = (!src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideBendingHorizontal) ? 1 : 0,
                    });
                }
            }
            if (_IsComputeStructuralHorizontal)
            {
                foreach (var src in _ConstraintsStructuralHorizontal)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        StretchLength = src._LengthACB,
                        IsCollision = (!src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideStructuralHorizontal) ? 1 : 0,
                    });
                }
            }
            if (_IsComputeShear)
            {
                foreach (var src in _ConstraintsShear)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        StretchLength = src._LengthACB,
                        IsCollision = (!src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideShear) ? 1 : 0,
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
                        StretchLength = src._LengthACB,
                        IsCollision = (!src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideBendingVertical) ? 1 : 0,
                    });
                }
            }
            if (_IsComputeStructuralVertical)
            {
                foreach (var src in _ConstraintsStructuralVertical)
                {
                    PushConstraintTable(ConstraintTable, new SPCRJointDynamicsJob.Constraint()
                    {
                        Type = src._Type,
                        IndexA = src._PointA._Index,
                        IndexB = src._PointB._Index,
                        Length = src._Length,
                        StretchLength = src._LengthACB,
                        IsCollision = (!src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideStructuralVertical) ? 1 : 0,
                    });
                }
            }

            _ConstraintTable = new SPCRJointDynamicsJob.Constraint[ConstraintTable.Count][];
            for (int i = 0; i < ConstraintTable.Count; ++i)
            {
                _ConstraintTable[i] = ConstraintTable[i].ToArray();
            }
        }

        List<SPCRJointDynamicsJob.SurfaceFaceConstraints> GetSurfaceFaceConstraints()
        {
            List<SPCRJointDynamicsJob.SurfaceFaceConstraints> faceConstraints = new List<SPCRJointDynamicsJob.SurfaceFaceConstraints>();
            for (int i = 0; i < SurfaceFacePoints.Length; i++)
            {
                faceConstraints.Add(new SPCRJointDynamicsJob.SurfaceFaceConstraints
                {
                    IndexA = SurfaceFacePoints[i].PointA._Index,
                    IndexB = SurfaceFacePoints[i].PointB._Index,
                    IndexC = SurfaceFacePoints[i].PointC._Index,
                    IndexD = SurfaceFacePoints[i].PointD._Index,
                });
            }

            return faceConstraints;
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
            for (int i = 0; i < constraints.Length; i++)
            {
                var constraint = constraints[i];
                var pointA = constraint._PointA.transform.position;
                var pointB = constraint._PointB.transform.position;
                Gizmos.DrawLine(pointA, pointB);
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

        public void OnDrawGizmos()
        {
            if (!this.enabled)
            {
                return;
            }

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
            }
            if (_IsDebugDraw_StructuralHorizontal)
            {
                Gizmos.color = new Color(0.4f, 0.8f, 0.4f);
                OnDrawGizms_Constraint(_ConstraintsStructuralHorizontal);
            }
            if (_IsDebugDraw_Shear)
            {
                Gizmos.color = new Color(0.4f, 0.4f, 0.8f);
                OnDrawGizms_Constraint(_ConstraintsShear);
            }
            if (_IsDebugDraw_BendingVertical)
            {
                Gizmos.color = new Color(0.8f, 0.8f, 0.4f);
                OnDrawGizms_Constraint(_ConstraintsBendingVertical);
            }
            if (_IsDebugDraw_BendingHorizontal)
            {
                Gizmos.color = new Color(0.4f, 0.8f, 0.8f);
                OnDrawGizms_Constraint(_ConstraintsBendingHorizontal);
            }

            if (_IsDebugDraw_SurfaceFace)
            {
                Gizmos.color = Color.red;
                OnDrawGizmo_SurfaceFaces();
            }
        }
    }
}
