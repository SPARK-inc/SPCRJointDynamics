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

//#define ENABLE_BURST
//#define ENABLE_JOBSYSTEM

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
#if ENABLE_JOBSYSTEM
using Unity.Jobs;
#endif//ENABLE_JOBSYSTEM

namespace SPCR
{
    public class SPCRJointDynamicsJob
    {
        const float EPSILON = 0.001f;

        public struct Point
        {
            public int Parent;
            public int Child;
            public int MovableLimitIndex;
            public float MovableLimitRadius;
            public float Weight;
            public float Mass;
            public float Resistance;
            public float Hardness;
            public float FrictionScale;
            public float SliderJointLength;
            public float FakeWavePower;
            public float FakeWaveFreq;
            public float ParentLength;
            public float StructuralShrinkVertical;
            public float StructuralStretchVertical;
            public float StructuralShrinkHorizontal;
            public float StructuralStretchHorizontal;
            public float ShearShrink;
            public float ShearStretch;
            public float BendingShrinkVertical;
            public float BendingStretchVertical;
            public float BendingShrinkHorizontal;
            public float BendingStretchHorizontal;
            public float WindForceScale;
            public Vector3 Gravity;
            public Vector3 BoneAxis;
            public Vector3 Position;
            public Vector3 Direction;
        }

        struct PointR
        {
            public int Parent;
            public int Child;
            public int MovableLimitIndex;
            public float MovableLimitRadius;
            public float Weight;
            public float Mass;
            public float Resistance;
            public float Hardness;
            public float FrictionScale;
            public float SliderJointLength;
            public float ParentLength;
            public float StructuralShrinkVertical;
            public float StructuralStretchVertical;
            public float StructuralShrinkHorizontal;
            public float StructuralStretchHorizontal;
            public float ShearShrink;
            public float ShearStretch;
            public float BendingShrinkVertical;
            public float BendingStretchVertical;
            public float BendingShrinkHorizontal;
            public float BendingStretchHorizontal;
            public float WindForceScale;
            public float FakeWavePower;
            public float FakeWaveFreq;
            public float ForceFadeRatio;
            public Vector3 Gravity;
            public Vector3 BoneAxis;
            public Vector3 InitialLocalScale;
            public Quaternion InitialLocalRotation;
            public Vector3 InitialLocalPosition;
        }

        struct PointRW
        {
            public Vector3 Position_ToTransform;
            public Vector3 Position_CurrentTransform;
            public Vector3 Position_PreviousTransform;
            public Vector3 Position_Current;
            public Vector3 Position_Previous;
            public Vector3 Direction_Previous;
            public Vector3 FakeWindDirection;
            public int GrabberIndex;
            public float GrabberDistance;
            public float Friction;
        }

        public struct Constraint
        {
            public int IsCollision;
            public SPCRJointDynamicsController.ConstraintType Type;
            public int IndexA;
            public int IndexB;
            public float Length;
        }

        struct ColliderR
        {
            public float Radius;
            public float RadiusTailScale;
            public float Height;
            public float Friction;
            public SPCRJointDynamicsCollider.ColliderForce ForceType;
        }

        struct ColliderRW
        {
            public Vector3 Position_Current;
            public Vector3 Direction_Current;
            public Vector3 Position_CurrentTransform;
            public Vector3 Position_PreviousTransform;
            public Quaternion Direction_CurrentTransform;
            public Quaternion Direction_PreviousTransform;
            public Matrix4x4 WorldToLocal;
            public Bounds LocalBounds;
            public float Radius;
            public int Enabled;
        }

        void ComputeCapsule(Vector3 SrcPos, Quaternion SrcRot, float SrcHeight, out Vector3 Head, out Vector3 Direction)
        {
            var dir = SrcRot * (Vector3.up * SrcHeight);
            var head = SrcPos - (dir * 0.5f);

            Head = head;
            Direction = dir;
        }

        void ComputeCapsule_Pos(Vector3 SrcPos, Quaternion SrcRot, float SrcHeight, out Vector3 Head, out Vector3 Tail)
        {
            var dir = SrcRot * (Vector3.up * SrcHeight);
            var head = SrcPos - (dir * 0.5f);

            Head = head;
            Tail = head + dir;
        }

        struct GrabberR
        {
            public float Radius;
            public float Force;
        }

        struct GrabberRW
        {
            public int Enabled;
            public Vector3 Position;
        }

        public struct AngleLimitConfig
        {
            public float angleLimit;
            public bool limitFromRoot;
        }

        bool _bInitialized = false;
#if ENABLE_JOBSYSTEM
        JobHandle _hJob = default;
#endif//ENABLE_JOBSYSTEM
        Transform _RootBone;
        Quaternion _PreviousRootRotation;
        Vector3 _PreviousRootPosition;
        int _PointCount;
        NativeArray<PointR> _PointsR;
        NativeArray<PointRW> _PointsRW;
        NativeArray<Vector3> _PointsP2T;
        NativeArray<Vector3> _MovableLimitTargets;
        NativeArray<Constraint> _Constraints;
        NativeArray<int> _SurfaceConstraints;

        Transform[] _PointTransforms;
        TransformAccessArray _PointTransformArray;

        Transform[] _MovableLimitTargetTransforms;
        TransformAccessArray _MovableLimitTargetTransformArray;

        SPCRJointDynamicsCollider[] _RefColliders;
        Transform[] _CollidersTransforms;
        TransformAccessArray _CollidersTransformArray;
        NativeArray<ColliderR> _CollidersR;
        NativeArray<ColliderRW> _CollidersRW;

        Transform[] _FlatPlanesTransform;
        NativeArray<Plane> _FlatPlanes;

        SPCRJointDynamicsPointGrabber[] _RefGrabbers;
        NativeArray<GrabberR> _GrabbersR;
        NativeArray<GrabberRW> _GrabbersRW;
        AngleLimitConfig _AngleLockConfig;
        float _FakeWaveCounter;

        public void SetPointDynamicsRatio(int Index, float Ratio)
        {
            if (!_bInitialized) return;
            if (!_PointsR.IsCreated) return;

            var ptR = _PointsR[Index];
            ptR.ForceFadeRatio = Ratio;
            _PointsR[Index] = ptR;
        }

        public bool Initialize(
            Transform RootBone,
            Point[] Points, Transform[] PointTransforms, Transform[] MovableLimitTargetTransforms,
            Constraint[][] Constraints,
            SPCRJointDynamicsCollider[] Colliders, SPCRJointDynamicsPointGrabber[] Grabbers,
            Transform[] FlatPlanes,
            SPCRJointDynamicsController.SPCRJointDynamicsSurfaceFace[] SurfaceConstraints,
            AngleLimitConfig AngleLockConfig)
        {
            Uninitialize();

            if (RootBone == null)
            {
                Debug.LogError("SPCRJointDynamics: RootBone is null!!!");
                return false;
            }

            _RootBone = RootBone;
            _PreviousRootRotation = _RootBone.rotation;
            _PreviousRootPosition = _RootBone.position;
            _PointCount = Points.Length;
            _PointTransforms = PointTransforms;
            _FlatPlanesTransform = FlatPlanes;
            _MovableLimitTargetTransforms = MovableLimitTargetTransforms;
            _AngleLockConfig = AngleLockConfig;

            _PointsR = new NativeArray<PointR>(_PointCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _PointsRW = new NativeArray<PointRW>(_PointCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _PointsP2T = new NativeArray<Vector3>(_PointCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < Points.Length; ++i)
            {
                var src = Points[i];

                var ptR = new PointR();
                var ptRW = new PointRW();

                ptR.Parent = src.Parent;
                ptR.Child = src.Child;

                ptR.MovableLimitIndex = src.MovableLimitIndex;
                ptR.MovableLimitRadius = src.MovableLimitRadius;
                ptR.Weight = src.Weight;
                ptR.Mass = src.Mass;
                ptR.Resistance = src.Resistance;
                ptR.Hardness = src.Hardness;
                ptR.FrictionScale = src.FrictionScale;

                ptR.SliderJointLength = src.SliderJointLength * 0.5f;
                ptR.ParentLength = src.ParentLength;

                ptR.StructuralShrinkHorizontal = src.StructuralShrinkHorizontal * 0.5f;
                ptR.StructuralStretchHorizontal = src.StructuralStretchHorizontal * 0.5f;
                ptR.StructuralShrinkVertical = src.StructuralShrinkVertical * 0.5f;
                ptR.StructuralStretchVertical = src.StructuralStretchVertical * 0.5f;
                ptR.ShearShrink = src.ShearShrink * 0.5f;
                ptR.ShearStretch = src.ShearStretch * 0.5f;
                ptR.BendingShrinkHorizontal = src.BendingShrinkHorizontal * 0.5f;
                ptR.BendingStretchHorizontal = src.BendingStretchHorizontal * 0.5f;
                ptR.BendingShrinkVertical = src.BendingShrinkVertical * 0.5f;
                ptR.BendingStretchVertical = src.BendingStretchVertical * 0.5f;
                ptR.ForceFadeRatio = 0.0f;

                ptR.Gravity = src.Gravity;
                ptR.WindForceScale = src.WindForceScale;
                ptR.FakeWavePower = src.FakeWavePower;
                ptR.FakeWaveFreq = src.FakeWaveFreq;
                ptR.BoneAxis = src.BoneAxis;

                ptR.InitialLocalScale = PointTransforms[i].localScale;
                ptR.InitialLocalRotation = PointTransforms[i].localRotation;
                ptR.InitialLocalPosition = PointTransforms[i].localPosition;

                ptRW.Position_CurrentTransform = src.Position;
                ptRW.Position_PreviousTransform = src.Position;
                ptRW.Position_Current = src.Position;
                ptRW.Position_Previous = src.Position;
                ptRW.Direction_Previous = src.Direction;

                ptRW.Friction = 0.5f;

                ptRW.GrabberIndex = -1;
                ptRW.GrabberDistance = 0.0f;

                _PointsR[i] = ptR;
                _PointsRW[i] = ptRW;
            }

            _PointTransformArray = new TransformAccessArray(_PointTransforms, 4);

            {
                _MovableLimitTargets = new NativeArray<Vector3>(MovableLimitTargetTransforms.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _MovableLimitTargetTransformArray = new TransformAccessArray(_MovableLimitTargetTransforms, 4);
            }

            {
                int TotalCount = 0;
                for (int i = 0; i < Constraints.Length; ++i)
                {
                    TotalCount += Constraints[i].Length;
                }
                int index = 0;
                _Constraints = new NativeArray<Constraint>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < Constraints.Length; ++i)
                {
                    var constraint = Constraints[i];
                    for (int j = 0; j < constraint.Length; ++j)
                    {
                        _Constraints[index++] = constraint[j];
                    }
                }
            }

            {
                int index = 0;
                _SurfaceConstraints = new NativeArray<int>(SurfaceConstraints.Length * 6, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < SurfaceConstraints.Length; ++i)
                {
                    var Src = SurfaceConstraints[i];

                    _SurfaceConstraints[index++] = Src.PointA._Index;
                    _SurfaceConstraints[index++] = Src.PointB._Index;
                    _SurfaceConstraints[index++] = Src.PointC._Index;
                    _SurfaceConstraints[index++] = Src.PointC._Index;
                    _SurfaceConstraints[index++] = Src.PointD._Index;
                    _SurfaceConstraints[index++] = Src.PointA._Index;
                }
            }

            {
                _RefColliders = Colliders;

                _CollidersTransforms = new Transform[_RefColliders.Length];
                for (int i = 0; i < _RefColliders.Length; ++i)
                {
                    _CollidersTransforms[i] = _RefColliders[i].transform;
                }
                _CollidersTransformArray = new TransformAccessArray(_CollidersTransforms, 4);

                _CollidersR = new NativeArray<ColliderR>(Colliders.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _CollidersRW = new NativeArray<ColliderRW>(Colliders.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < Colliders.Length; ++i)
                {
                    var src = Colliders[i];
                    if (src == null)
                    {
                        Debug.LogError("SPCRJointDynamics: ColliderR [" + i + "] is null!!!");
                        return false;
                    }

                    var colR = new ColliderR();
                    var colRW = new ColliderRW();

                    colR.Radius = src.Radius;
                    colR.RadiusTailScale = src.RadiusTailScale;
                    colR.Height = src.IsCapsule ? src.Height : 0.0f;
                    colR.Friction = src.Friction;
                    colR.ForceType = src._SurfaceColliderForce;

                    colRW.Enabled = src.isActiveAndEnabled ? 1 : 0;

                    colRW.Position_CurrentTransform = src.transform.position;
                    colRW.Position_PreviousTransform = src.transform.position;

                    colRW.Direction_CurrentTransform = src.transform.rotation;
                    colRW.Direction_PreviousTransform = src.transform.rotation;

                    ComputeCapsule(
                        colRW.Position_CurrentTransform, colRW.Direction_CurrentTransform, colR.Height,
                        out colRW.Position_Current, out colRW.Direction_Current);

                    colRW.LocalBounds = new Bounds();

                    _CollidersR[i] = colR;
                    _CollidersRW[i] = colRW;
                }
            }

            {
                _RefGrabbers = Grabbers;

                _GrabbersR = new NativeArray<GrabberR>(Grabbers.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _GrabbersRW = new NativeArray<GrabberRW>(Grabbers.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < Grabbers.Length; ++i)
                {
                    var src = _RefGrabbers[i];
                    if (src == null)
                    {
                        Debug.LogError("SPCRJointDynamics: GrabberR [" + i + "] is null!!!");
                        return false;
                    }

                    var grabber = new GrabberR();

                    grabber.Radius = src.Radius;
                    grabber.Force = src.Force;

                    _GrabbersR[i] = grabber;
                }
            }

            {
                _FlatPlanes = new NativeArray<Plane>(_FlatPlanesTransform.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            _bInitialized = true;

            return true;
        }

        public void Uninitialize()
        {
            if (!_bInitialized) return;
            _bInitialized = false;

            WaitJob();

            for (int i = 0; i < _PointsRW.Length; ++i)
            {
                var t = _PointTransforms[i];
                t.localPosition = _PointsR[i].InitialLocalPosition;
                t.localRotation = _PointsR[i].InitialLocalRotation;
                t.localScale = _PointsR[i].InitialLocalScale;
            }

            if (_Constraints.IsCreated)_Constraints.Dispose();
            if (_SurfaceConstraints.IsCreated) _SurfaceConstraints.Dispose();
            if (_GrabbersR.IsCreated) _GrabbersR.Dispose();
            if (_GrabbersRW.IsCreated) _GrabbersRW.Dispose();
            if (_CollidersR.IsCreated) _CollidersR.Dispose();
            if (_CollidersRW.IsCreated) _CollidersRW.Dispose();
            if (_MovableLimitTargets.IsCreated) _MovableLimitTargets.Dispose();
            if (_PointsR.IsCreated) _PointsR.Dispose();
            if (_PointsRW.IsCreated) _PointsRW.Dispose();
            if (_PointsP2T.IsCreated) _PointsP2T.Dispose();
            if (_FlatPlanes.IsCreated) _FlatPlanes.Dispose();

            if (_PointTransformArray.isCreated) _PointTransformArray.Dispose();
            if (_MovableLimitTargetTransformArray.isCreated) _MovableLimitTargetTransformArray.Dispose();
            if (_CollidersTransformArray.isCreated) _CollidersTransformArray.Dispose();
        }

        void WaitJob()
        {
#if ENABLE_JOBSYSTEM
            _hJob.Complete();
            _hJob = default;
#endif//ENABLE_JOBSYSTEM
        }

        public void Reset(Constraint[][] Constraints, Matrix4x4 LocalToWorldMatrix)
        {
            int index = 0;
            for (int i = 0; i < Constraints.Length; ++i)
            {
                var constraint = Constraints[i];
                for (int j = 0; j < constraint.Length; ++j)
                {
                    _Constraints[index++] = constraint[j];
                }
            }

            for (int i = 0; i < _PointsRW.Length; ++i)
            {
                var SrcT = _PointTransforms[i];
                var Dst = _PointsRW[i];
                Dst.Position_Current = SrcT.position;
                Dst.Position_Previous = SrcT.position;
                Dst.Position_CurrentTransform = SrcT.position;
                Dst.Position_PreviousTransform = SrcT.position;
                Dst.FakeWindDirection = Vector3.forward;
                _PointsRW[i] = Dst;
            }

            for (int i = 0; i < _RefColliders.Length; ++i)
            {
                var Src = _RefColliders[i];
                var SrcT = _RefColliders[i].transform;

                var Dst = _CollidersRW[i];

                Dst.Position_CurrentTransform = Dst.Position_PreviousTransform = SrcT.position;
                Dst.Direction_CurrentTransform = Dst.Direction_PreviousTransform = SrcT.rotation;

                ComputeCapsule(
                    Dst.Position_CurrentTransform,
                    Dst.Direction_CurrentTransform,
                    Src.Height,
                    out Dst.Position_Current,
                    out Dst.Direction_Current);

                _CollidersRW[i] = Dst;
            }

            for (int i = 0; i < _MovableLimitTargetTransforms.Length; ++i)
            {
                _MovableLimitTargets[i] = _MovableLimitTargetTransforms[i].position;
            }

            _PreviousRootRotation = _RootBone.rotation;
            _PreviousRootPosition = _RootBone.position;
        }

        public void TransformInitialize()
        {
            if (_MovableLimitTargetTransforms.Length > 0)
            {
                var Job = new JobCaptureTransformPosition();
                Job.Positions = _MovableLimitTargets;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_MovableLimitTargetTransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_MovableLimitTargetTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }

            {
                var Job = new JobTransformInitialize();
                Job.PointsR = _PointsR;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_PointTransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
        }

        public void WaitInitialize()
        {
            WaitJob();
        }

        public void PreSimulation()
        {
            {
                var Job = new JobCaptureCurrentPositionFromTransform();
                Job.PointsRW = _PointsRW;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_PointTransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
        }

        public void Simulation(
            Transform RootTransform,
            float RootSlideLimit, float RootRotateLimit,
            float ConstraintShrinkLimit,
            float StepTime,
            int SubSteps,
            Vector3 WindForce,
            bool EnableSurfaceCollision,
            float BlendRatio,
            bool IsFakeWave,
            float FakeWaveSpeed,
            float FakeWavePower,
            float CollisionScale,
            int Relaxation)
        {
            var IsPaused = StepTime <= 0.0f;

            if (!IsPaused)
            {
                _FakeWaveCounter += FakeWaveSpeed * StepTime;
            }

            // Plane
            for (int i = 0; i < _FlatPlanesTransform?.Length; ++i)
            {
                var plane = new Plane();
                plane.SetNormalAndPosition(_FlatPlanesTransform[i].up, _FlatPlanesTransform[i].position);
                _FlatPlanes[i] = plane;
            }

            // Grabber
            {
                for (int i = 0; i < _RefGrabbers.Length; ++i)
                {
                    var grab = _GrabbersRW[i];
                    grab.Enabled = _RefGrabbers[i].isActiveAndEnabled && _RefGrabbers[i].IsEnabled ? 1 : 0;
                    grab.Position = _RefGrabbers[i].RefTransform.position;
                    _GrabbersRW[i] = grab;
                }
            }

            // Collider
            {
                for (int i = 0; i < _RefColliders.Length; ++i)
                {
                    var coll = _CollidersRW[i];
                    coll.Enabled = _RefColliders[i].isActiveAndEnabled ? 1 : 0;
                    _CollidersRW[i] = coll;
                }

                var Job = new JobUpdateColliderTransform();
                Job.CollidersR = _CollidersR;
                Job.CollidersRW = _CollidersRW;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_CollidersTransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_CollidersTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }

            // Simulation
            {
                var RootPosition = RootTransform.position;
                var RootRotation = RootTransform.rotation;

                {
                    var Job = new JobExecuteSimulation();
                    Job.IsPaused = IsPaused;
                    Job.StepTime = StepTime;
                    Job.SubSteps = SubSteps;
                    Job.RootPosition = RootPosition;
                    Job.PreviousRootPosition = _PreviousRootPosition;
                    Job.RootSlideLimit = RootSlideLimit;
                    Job.RootRotation = RootRotation;
                    Job.PreviousRootRotation = _PreviousRootRotation;
                    Job.RootRotateLimit = RootRotateLimit;
                    Job.CollidersRW = _CollidersRW;
                    Job.CollidersR = _CollidersR;
                    Job.CollisionScale = CollisionScale;
                    Job.PointsP2T = _PointsP2T;
                    Job.PointsRW = _PointsRW;
                    Job.PointsR = _PointsR;
                    Job.GrabbersR = _GrabbersR;
                    Job.GrabbersRW = _GrabbersRW;
                    Job.MovableLimitTargets = _MovableLimitTargets;
                    Job.WindForce = WindForce;
                    Job.FlatPlanes = _FlatPlanes;
                    Job.EnableSurfaceCollision = EnableSurfaceCollision;
                    Job.SurfaceConstraints = _SurfaceConstraints;
                    Job.Relaxation = Relaxation;
                    Job.ConstraintShrinkLimit = ConstraintShrinkLimit;
                    Job.Constraints = _Constraints;
                    Job.BlendRatio = BlendRatio;
                    Job.IsFakeWave = IsFakeWave;
                    Job.FakeWaveSpeed = FakeWaveSpeed;
                    Job.FakeWavePower = FakeWavePower;
                    Job.FakeWaveCounter = _FakeWaveCounter;

#if ENABLE_JOBSYSTEM
                    _hJob = Job.Schedule(_hJob);
#else//ENABLE_JOBSYSTEM
                    JobSchedule(Job);
#endif//ENABLE_JOBSYSTEM
                }

                _PreviousRootPosition = RootPosition;
                _PreviousRootRotation = RootRotation;
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobExecuteSimulation
#if ENABLE_JOBSYSTEM
            : IJob
#else//ENABLE_JOBSYSTEM
            : Job
#endif//ENABLE_JOBSYSTEM
        {
            [ReadOnly] public bool IsPaused;
            [ReadOnly] public float StepTime;
            [ReadOnly] public int SubSteps;
            [ReadOnly] public Vector3 RootPosition;
            [ReadOnly] public Vector3 PreviousRootPosition;
            [ReadOnly] public float RootSlideLimit;
            [ReadOnly] public Quaternion RootRotation;
            [ReadOnly] public Quaternion PreviousRootRotation;
            [ReadOnly] public float RootRotateLimit;

            public NativeArray<ColliderRW> CollidersRW;
            [ReadOnly] public NativeArray<ColliderR> CollidersR;
            [ReadOnly] public float CollisionScale;

            public NativeArray<Vector3> PointsP2T;
            public NativeArray<PointRW> PointsRW;
            [ReadOnly] public NativeArray<PointR> PointsR;
            [ReadOnly] public NativeArray<GrabberR> GrabbersR;
            [ReadOnly] public NativeArray<GrabberRW> GrabbersRW;
            [ReadOnly] public NativeArray<Vector3> MovableLimitTargets;
            [ReadOnly] public Vector3 WindForce;
            [ReadOnly] public NativeArray<Plane> FlatPlanes;

            [ReadOnly] public bool EnableSurfaceCollision;
            [ReadOnly] public NativeArray<int> SurfaceConstraints;

            [ReadOnly] public int Relaxation;
            [ReadOnly] public float ConstraintShrinkLimit;
            [ReadOnly] public NativeArray<Constraint> Constraints;

            [ReadOnly] public float BlendRatio;
            [ReadOnly] public bool IsFakeWave;
            [ReadOnly] public float FakeWaveSpeed;
            [ReadOnly] public float FakeWavePower;
            [ReadOnly] public float FakeWaveCounter;

#if ENABLE_JOBSYSTEM
            void IJob.Execute()
#else//ENABLE_JOBSYSTEM
            public void Execute()
#endif//ENABLE_JOBSYSTEM
            {
                if (IsPaused)
                {
                    SubSteps = 1;
                }

                var FakeWaveFreq = FakeWaveCounter;

                // 移動オフセット
                var RootSlideOffset = Vector3.zero;
                var RootDeltaSlide = RootPosition - PreviousRootPosition;
                var RootDeltaSlideLength = RootDeltaSlide.magnitude;
                if ((RootSlideLimit >= 0.0f) && (RootDeltaSlideLength > RootSlideLimit))
                {
                    RootSlideOffset = RootDeltaSlide * (1.0f - RootSlideLimit / RootDeltaSlideLength);
                    RootSlideOffset /= SubSteps;
                }

                // 回転オフセット
                var RootRotationOffset = Quaternion.identity;
                var RootDeltaRotation = RootRotation * Quaternion.Inverse(PreviousRootRotation);
                var RotateAngle = Mathf.Acos(RootDeltaRotation.w) * 2.0f * Mathf.Rad2Deg;
                if ((RootRotateLimit >= 0.0f) && (Mathf.Abs(RotateAngle) > RootRotateLimit))
                {
                    Vector3 RotateAxis;
                    RootDeltaRotation.ToAngleAxis(out RotateAngle, out RotateAxis);

                    /*
                     * Fixed in 2022.1.X
                     * QUATERNION TOANGLEAXIS DOES NOT DEAL WITH SINGULARITY AT (0, 0, 0, -1)
                    */
                    if (float.IsNaN(RotateAxis.x) || float.IsNaN(RotateAxis.y) || float.IsNaN(RotateAxis.x) ||
                        float.IsInfinity(RotateAxis.x) || float.IsInfinity(RotateAxis.y) || float.IsInfinity(RotateAxis.x))
                    {
                        RotateAxis = Vector3.up;
                    }

                    var Angle = (RotateAngle > 0.0f) ? (RotateAngle - RootRotateLimit) : (RotateAngle + RootRotateLimit);
                    Angle /= SubSteps;
                    RootRotationOffset = Quaternion.AngleAxis(Angle, RotateAxis);
                }

                if (IsPaused)
                {
                    RootSlideOffset = RootDeltaSlide;
                    RootRotationOffset = RootDeltaRotation;
                }

                StepTime /= (float)SubSteps;

                for (var iSubStep = 1; iSubStep <= SubSteps; iSubStep++)
                {
                    var StepDelta = (float)iSubStep / (float)SubSteps;
                    var StepTime_x2_Half = StepTime * StepTime * 0.5f;

                    JobColliderUpdate(StepDelta);
                    JobPointUpdatePass1(StepDelta, StepTime_x2_Half, RootSlideOffset, RootRotationOffset);

                    if (!IsPaused)
                    {
                        FakeWaveFreq += FakeWaveSpeed * StepTime;

                        if (EnableSurfaceCollision)
                        {
                            JobSurfaceCollision();
                        }

                        for (int iRelax = Relaxation - 1; iRelax >= 0; --iRelax)
                        {
                            JobConstraintUpdate();
                        }
                    }

                    JobPointUpdatePass2(StepDelta, FakeWaveFreq);
                }
            }

            void JobColliderUpdate(float StepDelta)
            {
                for (int index = 0; index < CollidersR.Length; ++index)
                {
                    var colR = CollidersR[index];
                    var colRW = CollidersRW[index];

                    colRW.Radius = colR.Radius * CollisionScale;

                    var CurPosition = Vector3.Lerp(colRW.Position_PreviousTransform, colRW.Position_CurrentTransform, StepDelta);
                    var CurDirection = Quaternion.Slerp(colRW.Direction_PreviousTransform, colRW.Direction_CurrentTransform, StepDelta);

                    ComputeCapsule(
                        CurPosition, CurDirection, colR.Height,
                        out colRW.Position_Current, out colRW.Direction_Current);

                    Vector3 Corner, Center;

                    // Head
                    Corner = Vector3.one * colR.Radius;

                    // Current
                    Center = colRW.WorldToLocal.MultiplyPoint(colRW.Position_Current);
                    colRW.LocalBounds.SetMinMax(Center - Corner, Center + Corner);

                    // Tail
                    if (colR.Height > EPSILON)
                    {
                        Corner = Vector3.one * colR.Radius * colR.RadiusTailScale;
                        // Current
                        Center = colRW.WorldToLocal.MultiplyPoint(colRW.Position_Current + colRW.Direction_Current);
                        var bounds = new Bounds();
                        bounds.SetMinMax(Center - Corner, Center + Corner);
                        colRW.LocalBounds.Encapsulate(bounds);
                    }

                    CollidersRW[index] = colRW;
                }
            }

            void JobPointUpdatePass1(float StepDelta, float StepTime_x2_Half, Vector3 RootSlideOffset, Quaternion RootRotationOffset)
            {
                for (int index = 0; index < PointsR.Length; ++index)
                {
                    var ptR = PointsR[index];
                    var ptRW = PointsRW[index];

                    var CurrentTransformPosition = Vector3.Lerp(
                        ptRW.Position_PreviousTransform,
                        ptRW.Position_CurrentTransform,
                        StepDelta);

                    if (ptR.Weight <= EPSILON)
                    {
                        ptRW.Position_Previous = ptRW.Position_Current;
                        ptRW.Position_Current = CurrentTransformPosition;
                    }
                    else
                    {
                        ptRW.Position_Previous += RootSlideOffset;
                        ptRW.Position_Current += RootSlideOffset;

                        Vector3 Displacement = Vector3.zero;
                        if (!IsPaused)
                        {
                            var MoveDir = ptRW.Position_Current - ptRW.Position_Previous;

                            Vector3 ExternalForce = Vector3.zero;
                            ExternalForce += ptR.Gravity;
                            ExternalForce += WindForce * ptR.WindForceScale / ptR.Mass;
                            ExternalForce *= StepTime_x2_Half;

                            Displacement = MoveDir;
                            Displacement += ExternalForce;
                            Displacement *= ptR.Resistance;
                            Displacement *= 1.0f - Mathf.Clamp01(ptRW.Friction * ptR.FrictionScale);
                        }

                        ptRW.Position_Previous = ptRW.Position_Current;
                        ptRW.Position_Current += Displacement;
                        ptRW.Friction = 0.0f;

                        if (!IsPaused)
                        {
                            if (ptR.Hardness > 0.0f)
                            {
                                var Restore = (CurrentTransformPosition - ptRW.Position_Current) * ptR.Hardness;
                                ptRW.Position_Current += Restore;
                            }

                            if (ptR.ForceFadeRatio > 0.0f)
                            {
                                ptRW.Position_Current = Vector3.Lerp(ptRW.Position_Current, CurrentTransformPosition, ptR.ForceFadeRatio);
                            }

                            if (ptRW.GrabberIndex != -1)
                            {
                                var grR = GrabbersR[ptRW.GrabberIndex];
                                var grRW = GrabbersRW[ptRW.GrabberIndex];
                                if (grRW.Enabled == 0)
                                {
                                    ptRW.GrabberIndex = -1;
                                }
                                else
                                {
                                    var Vec = ptRW.Position_Current - grRW.Position;
                                    var Pos = grRW.Position + Vec.normalized * ptRW.GrabberDistance;
                                    ptRW.Position_Current += (Pos - ptRW.Position_Current) * grR.Force;
                                }
                            }
                            else
                            {
                                int NearIndex = -1;
                                float sqrNearRange = 1000.0f * 1000.0f;
                                for (int iGrabber = 0; iGrabber < GrabbersR.Length; ++iGrabber)
                                {
                                    var grR = GrabbersR[iGrabber];
                                    var grRW = GrabbersRW[iGrabber];

                                    if (grRW.Enabled != 0)
                                    {
                                        var Vec = grRW.Position - ptRW.Position_Current;
                                        var sqrVecLength = Vec.sqrMagnitude;
                                        if (sqrVecLength < grR.Radius * grR.Radius)
                                        {
                                            if (sqrVecLength < sqrNearRange)
                                            {
                                                sqrNearRange = sqrVecLength;
                                                NearIndex = iGrabber;
                                            }
                                        }
                                    }
                                }
                                if (NearIndex != -1)
                                {
                                    ptRW.GrabberIndex = NearIndex;
                                    ptRW.GrabberDistance = Mathf.Sqrt(sqrNearRange) / 2.0f;
                                }
                            }

                            if (ptR.MovableLimitIndex != -1)
                            {
                                var Target = MovableLimitTargets[ptR.MovableLimitIndex];
                                var Move = ptRW.Position_Current - Target;
                                var MoveLength = Move.magnitude;
                                if (MoveLength > ptR.MovableLimitRadius)
                                {
                                    ptRW.Position_Current = Target + Move / MoveLength * ptR.MovableLimitRadius;
                                }
                            }
                        }

                        for (int i = 0; i < FlatPlanes.Length; ++i)
                        {
                            var Distance = FlatPlanes[i].GetDistanceToPoint(ptRW.Position_Current);
                            if (Distance < 0.0f)
                            {
                                ptRW.Position_Current = FlatPlanes[i].ClosestPointOnPlane(ptRW.Position_Current);
                                ptRW.Friction = 0.3f;
                            }
                        }
                    }

                    PointsRW[index] = ptRW;
                }
            }

            void JobSurfaceCollision()
            {
                for (int index = 0; index < SurfaceConstraints.Length; index += 6)
                {
                    for (int i = 0; i < CollidersR.Length; i++)
                    {
                        var colR = CollidersR[i];
                        var colRW = CollidersRW[i];

                        if (colRW.Enabled == 0)
                            continue;

                        Vector3 intersectionPoint, pointOnCollider, pushOut;
                        float radius;

                        Vector3 colliderPosition = colRW.Position_Current;

                        for (int j = 0; j < 6; j += 3)
                        {
                            var indexA = SurfaceConstraints[index + j + 0];
                            var indexB = SurfaceConstraints[index + j + 1];
                            var indexC = SurfaceConstraints[index + j + 2];

                            var RWPtA = PointsRW[indexA];
                            var RWPtB = PointsRW[indexB];
                            var RWPtC = PointsRW[indexC];

                            if (CheckCollision(ref RWPtA, ref RWPtB, ref RWPtC, colliderPosition, ref colR, ref colRW, out intersectionPoint, out pointOnCollider, out radius, out pushOut))
                            {
                                Vector3 triangleCenter = CenterOfTheTriangle(RWPtA.Position_Current, RWPtB.Position_Current, RWPtC.Position_Current);
                                float pushOutmagnitude = pushOut.magnitude;
                                pushOut /= pushOutmagnitude;

                                for (int k = 0; k < 3; k++)
                                {
                                    var indexR = SurfaceConstraints[index + j + k];
                                    var rwPt = PointsRW[indexR];

                                    Vector3 centerToPoint = rwPt.Position_Current - triangleCenter;
                                    Vector3 intersectionToPoint = rwPt.Position_Current - intersectionPoint;

                                    float rate = centerToPoint.magnitude / intersectionToPoint.magnitude;
                                    rate = Mathf.Clamp01(Mathf.Abs(rate));
                                    Vector3 pushVec = pushOut * (radius - pushOutmagnitude);
                                    rwPt.Position_Current += pushVec * rate;

                                    PointsRW[indexR] = rwPt;
                                }
                            }
                        }
                    }
                }
            }

            void JobConstraintUpdate()
            {
                for (int index = 0; index < Constraints.Length; ++index)
                {
                    var constraint = Constraints[index];

                    var ptRA = PointsR[constraint.IndexA];
                    var ptRB = PointsR[constraint.IndexB];

                    float WeightA = ptRA.Weight;
                    float WeightB = ptRB.Weight;

                    if ((WeightA <= EPSILON) && (WeightB <= EPSILON))
                        continue;

                    var RWptA = PointsRW[constraint.IndexA];
                    var RWptB = PointsRW[constraint.IndexB];

                    var Direction = RWptB.Position_Current - RWptA.Position_Current;

                    float Distance = Direction.magnitude;

                    float ShrinkLength = constraint.Length;
                    float StretchLength = ShrinkLength;
                    switch (constraint.Type)
                    {
                    case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
                    case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
                    case SPCRJointDynamicsController.ConstraintType.Shear:
                        StretchLength += ptRA.SliderJointLength + ptRB.SliderJointLength;
                        break;
                    }

                    float Force = 0.0f;
                    if (Distance <= ShrinkLength)
                    {
                        Force = Distance - ShrinkLength;
                    }
                    else if (Distance >= StretchLength)
                    {
                        Force = Distance - StretchLength;
                    }

                    bool IsShrink = Force >= 0.0f;
                    float ConstraintPower;
                    switch (constraint.Type)
                    {
                    case SPCRJointDynamicsController.ConstraintType.Structural_Vertical:
                        ConstraintPower = IsShrink
                            ? (ptRA.StructuralShrinkVertical + ptRB.StructuralShrinkVertical)
                            : (ptRA.StructuralStretchVertical + ptRB.StructuralStretchVertical);
                        break;
                    case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
                        ConstraintPower = IsShrink
                            ? Mathf.Min(ConstraintShrinkLimit, ptRA.StructuralShrinkHorizontal + ptRB.StructuralShrinkHorizontal)
                            : (ptRA.StructuralStretchHorizontal + ptRB.StructuralStretchHorizontal);
                        break;
                    case SPCRJointDynamicsController.ConstraintType.Shear:
                        ConstraintPower = IsShrink
                            ? Mathf.Min(ConstraintShrinkLimit, ptRA.ShearShrink + ptRB.ShearShrink)
                            : (ptRA.ShearStretch + ptRB.ShearStretch);
                        break;
                    case SPCRJointDynamicsController.ConstraintType.Bending_Vertical:
                        ConstraintPower = IsShrink
                            ? (ptRA.BendingShrinkVertical + ptRB.BendingShrinkVertical)
                            : (ptRA.BendingStretchVertical + ptRB.BendingStretchVertical);
                        break;
                    case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
                        ConstraintPower = IsShrink
                            ? Mathf.Min(ConstraintShrinkLimit, ptRA.BendingShrinkHorizontal + ptRB.BendingShrinkHorizontal)
                            : (ptRA.BendingStretchHorizontal + ptRB.BendingStretchHorizontal);
                        break;
                    default:
                        ConstraintPower = 0.0f;
                        break;
                    }

                    if (ConstraintPower > 0.0f)
                    {
                        var Displacement = Direction.normalized * (Force * ConstraintPower);

                        float WightAB = WeightA + WeightB;
                        RWptA.Position_Current += Displacement * WeightA / WightAB;
                        RWptB.Position_Current -= Displacement * WeightB / WightAB;
                    }

                    if (constraint.IsCollision != 0)
                    {
                        float Friction = 0.0f;
                        for (int i = 0; i < CollidersR.Length; ++i)
                        {
                            var colRW = CollidersRW[i];
                            if (colRW.Enabled == 0)
                                continue;
                            var colR = CollidersR[i];

                            if (colR.Height > EPSILON)
                            {
                                if (Collision.PushoutFromCapsule(ref colR, ref colRW, ref RWptA.Position_Current))
                                {
                                    Friction = Mathf.Max(Friction, colR.Friction * 0.25f);
                                }
                                if (Collision.PushoutFromCapsule(ref colR, ref colRW, ref RWptB.Position_Current))
                                {
                                    Friction = Mathf.Max(Friction, colR.Friction * 0.25f);
                                }
                            }
                            else
                            {
                                if (Collision.PushoutFromSphere(ref colR, ref colRW, ref RWptA.Position_Current))
                                {
                                    Friction = Mathf.Max(Friction, colR.Friction * 0.25f);
                                }
                                if (Collision.PushoutFromSphere(ref colR, ref colRW, ref RWptB.Position_Current))
                                {
                                    Friction = Mathf.Max(Friction, colR.Friction * 0.25f);
                                }
                            }

                            float Radius;
                            Vector3 pointOnLine, pointOnCollider;
                            if (Collision.CollisionDetection(ref colR, ref colRW, RWptA.Position_Current, RWptB.Position_Current, out pointOnLine, out pointOnCollider, out Radius))
                            {
                                var Pushout = pointOnLine - pointOnCollider;
                                var PushoutDistance = Pushout.magnitude;

                                var pointDistance = (RWptB.Position_Current - RWptA.Position_Current).magnitude * 0.5f;
                                var rateP1 = Mathf.Clamp01((pointOnLine - RWptA.Position_Current).magnitude / pointDistance);
                                var rateP2 = Mathf.Clamp01((pointOnLine - RWptB.Position_Current).magnitude / pointDistance);

                                Pushout /= PushoutDistance;

                                Pushout *= Mathf.Max(Radius - PushoutDistance, 0.0f);
                                if (WeightA > EPSILON)
                                    RWptA.Position_Current += Pushout * rateP2;
                                if (WeightB > EPSILON)
                                    RWptB.Position_Current += Pushout * rateP1;

                                var Dot = Vector3.Dot(Vector3.up, (pointOnLine - pointOnCollider).normalized);
                                Friction = Mathf.Max(Friction, colR.Friction * Mathf.Clamp01(Dot));
                            }
                        }

                        RWptA.Friction = Mathf.Max(Friction, RWptA.Friction);
                        RWptB.Friction = Mathf.Max(Friction, RWptB.Friction);
                    }

                    PointsRW[constraint.IndexA] = RWptA;
                    PointsRW[constraint.IndexB] = RWptB;
                }
            }

            void JobPointUpdatePass2(float StepDelta, float FakeWaveFreq)
            {
                for (int index = 0; index < PointsR.Length; ++index)
                {
                    var ptR = PointsR[index];
                    var ptRW = PointsRW[index];

                    //if (EnablePointRayCollision)
                    //{
                    //    if ((ptRW.Position_Current - ptRW.Position_Previous).sqrMagnitude > 0.01f * 0.01f)
                    //    {
                    //        for (int i = 0; i < ColliderCount; ++i)
                    //        {
                    //            var colR = CollidersR[i];
                    //            var colRW = CollidersRW[i];
                    //            if (colRW.Enabled == 0)
                    //                continue;
                    //
                    //            float Radius;
                    //            Vector3 pointOnLine, pointOnCollider;
                    //            if (Collision.CollisionDetection(ref colR, ref colRW, ptRW.Position_Current, ptRW.Position_Previous, out pointOnLine, out pointOnCollider, out Radius))
                    //            {
                    //                //var Dot = Vector3.Dot(Vector3.up, (pointOnLine - pointOnCollider).normalized);
                    //                //ptRW.Friction = Mathf.Max(ptRW.Friction, colR.Friction * Mathf.Clamp01(Dot));
                    //                ptRW.Position_Current = pointOnLine;
                    //            }
                    //        }
                    //    }
                    //}

                    var CurrentTransformPosition = Vector3.Lerp(ptRW.Position_PreviousTransform, ptRW.Position_CurrentTransform, StepDelta);
                    ptRW.Position_ToTransform = Vector3.Lerp(
                        ptRW.Position_Current, CurrentTransformPosition,
                        Mathf.SmoothStep(0.0f, 1.0f, BlendRatio));

                    if (IsFakeWave)
                    {
                        if (ptR.Child == -1)
                        {
                            var A = PointsRW[ptR.Parent].Position_ToTransform;
                            var B = ptRW.Position_ToTransform;
                            Matrix4x4 mAxis = Matrix4x4.LookAt(A, B, Vector3.up);
                            ptRW.FakeWindDirection = Vector3.Lerp(
                                ptRW.FakeWindDirection,
                                new Vector3(mAxis.m01, mAxis.m11, mAxis.m21), 0.5f);

                            FakeWaveFreq += ptR.FakeWaveFreq;
                            var Power = Mathf.Sin(FakeWaveFreq) * FakeWavePower * ptR.FakeWavePower;
                            ptRW.Position_ToTransform += ptRW.FakeWindDirection * Power;
                        }
                    }

                    PointsRW[index] = ptRW;
                    PointsP2T[index] = ptRW.Position_ToTransform;
                }
            }

            void ComputeCapsule(Vector3 SrcPos, Quaternion SrcRot, float SrcHeight, out Vector3 Head, out Vector3 Direction)
            {
                var dir = SrcRot * (Vector3.up * SrcHeight);
                var head = SrcPos - (dir * 0.5f);

                Head = head;
                Direction = dir;
            }

            private Vector3 ApplySystemTransform(Vector3 Point, Vector3 Pivot, Vector3 RootSlideOffset, Quaternion RootRotationOffset)
            {
                return RootRotationOffset * (Point - Pivot) + Pivot + RootSlideOffset;
            }

            bool CheckCollision(ref PointRW RWPtA, ref PointRW RWPtB, ref PointRW RWPtC, Vector3 colliderPosition, ref ColliderR ColR, ref ColliderRW ColRW, out Vector3 intersectionPoint, out Vector3 pointOnCollider, out float radius, out Vector3 pushOut)
            {
                Ray ray;
                float enter;

                pointOnCollider = colliderPosition;
                radius = ColRW.Radius;

                bool isCapsuleCollider = ColR.Height > EPSILON;

                Vector3 triangleCenter = CenterOfTheTriangle(RWPtA.Position_Current, RWPtB.Position_Current, RWPtC.Position_Current);
                Vector3 colliderDirection = (colliderPosition - triangleCenter).normalized;

                Vector3 planeNormal = Vector3.Cross(RWPtB.Position_Current - RWPtA.Position_Current, RWPtC.Position_Current - RWPtA.Position_Current).normalized;
                float planeDistanceFromOrigin = -Vector3.Dot(planeNormal, RWPtA.Position_Current);
                planeNormal *= -1;

                float dot = Vector3.Dot(colliderDirection, planeNormal);

                if (isCapsuleCollider)
                {
                    float side = Vector3.Dot(ColRW.Direction_Current, planeNormal) * dot;
                    colliderDirection = planeNormal * dot * -1.0f;
                    Vector3 tempPointOnCollider = pointOnCollider;

                    radius = ColRW.Radius;
                    if (side < 0)
                    {
                        radius *= ColR.RadiusTailScale;
                        pointOnCollider = colliderPosition + ((ColRW.Direction_Current * 0.5f) * 2.0f);
                    }

                    ray = new Ray(pointOnCollider, colliderDirection.normalized);
                    if (Raycast(planeNormal * -1, planeDistanceFromOrigin, ray, out enter))
                    {
                        intersectionPoint = ray.GetPoint(enter);
                        if (TriangleContainsPoint(RWPtA.Position_Current, RWPtB.Position_Current, RWPtC.Position_Current, intersectionPoint))
                        {
                            pushOut = intersectionPoint - pointOnCollider;
                            Vector3 towardsCollider = pointOnCollider - intersectionPoint;
                            return towardsCollider.sqrMagnitude <= radius * radius;
                        }
                    }
                    else
                    {
                        if (ColR.ForceType != SPCRJointDynamicsCollider.ColliderForce.Off)
                        {
                            //Forcefully extrude the surface out of the collider
                            pointOnCollider = tempPointOnCollider;
                            Vector3 endVec = colliderPosition + ((ColRW.Direction_Current * 0.5f) * 2.0f);
                            Vector3 colDirVec = ColRW.Direction_Current;

                            if (ColR.ForceType == SPCRJointDynamicsCollider.ColliderForce.Pull)
                            {
                                Vector3 temp = pointOnCollider;
                                pointOnCollider = endVec;
                                endVec = temp;
                                colDirVec *= -1;
                            }

                            ray = new Ray(pointOnCollider, colDirVec);
                            if (Raycast(planeNormal * -1, planeDistanceFromOrigin, ray, out enter))
                            {
                                intersectionPoint = ray.GetPoint(enter);
                                if (TriangleContainsPoint(RWPtA.Position_Current, RWPtB.Position_Current, RWPtC.Position_Current, intersectionPoint))
                                {
                                    Vector3 intersecToEnd = endVec - intersectionPoint;
                                    Vector3 colliderToEndVec = endVec - pointOnCollider;
                                    Vector3 colldierToIntersectVec = intersectionPoint - pointOnCollider;

                                    float pointDot = Vector3.Dot(intersecToEnd, colldierToIntersectVec);
                                    float lineDot = Vector3.Dot(colliderToEndVec, colliderToEndVec);

                                    if (!(pointDot >= 0 && pointDot <= lineDot))
                                    {
                                        pushOut = Vector3.zero;
                                        return false;
                                    }

                                    float dotvalue = Vector3.Dot(colliderToEndVec, colldierToIntersectVec);
                                    float startToEndMag = Vector3.Dot(colliderToEndVec, colliderToEndVec);

                                    endVec = (pointOnCollider + colliderToEndVec.normalized * -radius);
                                    pushOut = intersectionPoint - endVec;

                                    pointOnCollider += colliderToEndVec * (dotvalue / startToEndMag);
                                    Vector3 towardsCollider = pointOnCollider - intersectionPoint;
                                    return towardsCollider.sqrMagnitude <= radius * radius;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //This is for Sphere collider
                    colliderDirection = planeNormal * dot * -1;
                    ray = new Ray(pointOnCollider, colliderDirection.normalized);

                    if (Raycast(planeNormal * -1, planeDistanceFromOrigin, ray, out enter))
                    {
                        intersectionPoint = ray.GetPoint(enter);
                        if (TriangleContainsPoint(RWPtA.Position_Current, RWPtB.Position_Current, RWPtC.Position_Current, intersectionPoint))
                        {
                            pushOut = intersectionPoint - pointOnCollider;
                            Vector3 towardsCollider = pointOnCollider - intersectionPoint;
                            return towardsCollider.sqrMagnitude <= radius * radius;
                        }
                    }
                }
                intersectionPoint = Vector3.zero;
                pushOut = Vector3.zero;
                return false;
            }

            bool Raycast(Vector3 normal, float planeDistance, Ray ray, out float enter)
            {
                float vdot = Vector3.Dot(ray.direction, normal);
                float ndot = -Vector3.Dot(ray.origin, normal) - planeDistance;

                if (Mathf.Abs(vdot) <= EPSILON)
                {
                    enter = 0.0f;
                    return false;
                }

                enter = ndot / vdot;
                return enter > 0.0f;
            }

            Vector3 CenterOfTheTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                return Vector3.Lerp(
                    Vector3.Lerp(v1, v2, 0.5f),
                    Vector3.Lerp(v1, v3, 0.5f),
                    0.5f);
            }

            bool TriangleContainsPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
            {
                a -= p;
                b -= p;
                c -= p;

                Vector3 u = Vector3.Cross(a, b);
                Vector3 v = Vector3.Cross(b, c);
                Vector3 w = Vector3.Cross(c, a);

                if (Vector3.Dot(u, v) < 0)
                    return false;
                if (Vector3.Dot(v, w) < 0)
                    return false;
                return true;
            }
        }

        public void PostSimulation()
        {
            {
                var Job = new JobApplySimlationResult();
                Job.PointsR = _PointsR;
                Job.PointsRW = _PointsRW;
                Job.PointsP2T = _PointsP2T;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_PointTransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }

            if (_AngleLockConfig.angleLimit >= 0)
            {
                var Job = new JobTransformAngleLock();
                Job.PointsR = _PointsR;
                Job.PointsRW = _PointsRW;
                Job.AngleLockConfig = _AngleLockConfig;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(Job);
#endif//ENABLE_JOBSYSTEM
            }
        }

        public void WaitSimulation()
        {
            WaitJob();
        }

        public void DrawGizmos_LateUpdatePoints()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _PointCount; ++i)
            {
                Gizmos.DrawSphere(_PointsRW[i].Position_CurrentTransform, 0.005f);
            }
        }

        public void DrawGizmos_Points(bool Draw3DGizmo)
        {
            if (Draw3DGizmo)
            {
                if (_PointTransforms == null) return;

                for (int i = 0; i < _PointTransforms.Length; ++i)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(_PointTransforms[i].position, 0.05f);
                    Gizmos.color = Color.blue;
                    DrawArrow(_PointTransforms[i].position, _PointTransforms[i].forward * 0.4f);
                    Gizmos.color = Color.green;
                    DrawArrow(_PointTransforms[i].position, _PointTransforms[i].up * 0.4f);
                    Gizmos.color = Color.red;
                    DrawArrow(_PointTransforms[i].position, _PointTransforms[i].right * 0.4f);
                }
            }
            else
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < _PointCount; ++i)
                {
                    Gizmos.DrawSphere(_PointsRW[i].Position_Current, 0.005f);
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        void DrawArrow(Vector3 pos, Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            Gizmos.DrawLine(pos, pos + direction);
            float arrowHeight = 0.15f;
            float coneAngle = 20.0f;

            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + coneAngle, 0, 0) * new Vector3(0, 0, 1);
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - coneAngle, 0, 0) * new Vector3(0, 0, 1);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + coneAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - coneAngle, 0) * new Vector3(0, 0, 1);

            Vector3 arrowPos = pos + direction;
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

        public void DrawGizmos_Constraints(int A, int B)
        {
            Gizmos.DrawLine(_PointTransforms[A].position, _PointTransforms[B].position);
        }

        public void DrawGizmos_Constraint(bool StructuralVertical, bool StructuralHorizontal, bool Shear, bool BendingVertical, bool BendingHorizontal)
        {
            //if (!Application.isPlaying) return;
            //if (_Constraints == null) return;
            //if (_PointsRW == null) return;

            //Gizmos.color = new Color(1.0f, 0.0f, 0.0f);

            //foreach (var constraint in _Constraints)
            //{
            //    for (int i = 0; i < constraint.Length; ++i)
            //    {
            //        var A = _PointsRW[constraint[i].IndexA];
            //        var B = _PointsRW[constraint[i].IndexB];
            //        Gizmos.DrawLine(A.Position, B.Position);
            //    }
            //}
        }

        public void DrawGizmos_ColliderEx()
        {
            var BoundsColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            for (int i = 0; i < _CollidersRW.Length; i++)
            {
                if (_CollidersRW[i].Enabled == 0)
                    continue;

                Vector3 CurHead, CurTail, OldHead, OldTail;

                ComputeCapsule_Pos(
                    _CollidersRW[i].Position_CurrentTransform,
                    _CollidersRW[i].Direction_CurrentTransform,
                    _CollidersR[i].Height,
                    out CurHead, out CurTail);
                ComputeCapsule_Pos(
                    _CollidersRW[i].Position_PreviousTransform,
                    _CollidersRW[i].Direction_PreviousTransform,
                    _CollidersR[i].Height,
                    out OldHead, out OldTail);

                Gizmos.color = Color.black;
                Gizmos.DrawLine(CurHead, OldHead);
                Gizmos.DrawLine(CurTail, OldTail);

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(OldHead, OldTail);

                var LocalToWorld = _CollidersRW[i].WorldToLocal.inverse;
                var BoundsMin = _CollidersRW[i].LocalBounds.min;
                var BoundsMax = _CollidersRW[i].LocalBounds.max;

                var BV000 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMin.x, BoundsMin.y, BoundsMin.z));
                var BV100 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMax.x, BoundsMin.y, BoundsMin.z));
                var BV010 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMin.x, BoundsMax.y, BoundsMin.z));
                var BV110 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMax.x, BoundsMax.y, BoundsMin.z));
                var BV001 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMin.x, BoundsMin.y, BoundsMax.z));
                var BV101 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMax.x, BoundsMin.y, BoundsMax.z));
                var BV011 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMin.x, BoundsMax.y, BoundsMax.z));
                var BV111 = LocalToWorld.MultiplyPoint(new Vector3(BoundsMax.x, BoundsMax.y, BoundsMax.z));

                Gizmos.color = BoundsColor;
                // X
                Gizmos.DrawLine(BV000, BV100);
                Gizmos.DrawLine(BV010, BV110);
                Gizmos.DrawLine(BV001, BV101);
                Gizmos.DrawLine(BV011, BV111);
                // Y
                Gizmos.DrawLine(BV000, BV010);
                Gizmos.DrawLine(BV100, BV110);
                Gizmos.DrawLine(BV001, BV011);
                Gizmos.DrawLine(BV101, BV111);
                // Z
                Gizmos.DrawLine(BV000, BV001);
                Gizmos.DrawLine(BV100, BV101);
                Gizmos.DrawLine(BV010, BV011);
                Gizmos.DrawLine(BV110, BV111);
            }
        }

        class Collision
        {
            public static bool PushoutFromSphere(Vector3 Center, float Radius, ref Vector3 point)
            {
                var direction = point - Center;
                var sqrDirectionLength = direction.sqrMagnitude;
                var radius = Radius;
                if (sqrDirectionLength > EPSILON)
                {
                    if (sqrDirectionLength < radius * radius)
                    {
                        var directionLength = Mathf.Sqrt(sqrDirectionLength);
                        point = Center + direction * radius / directionLength;
                        return true;
                    }
                }
                return false;
            }

            public static bool PushoutFromSphere(ref ColliderR colR, ref ColliderRW colRW, ref Vector3 point)
            {
                return PushoutFromSphere(colRW.Position_Current, colRW.Radius, ref point);
            }

            public static bool PushoutFromCapsule(ref ColliderR colR, ref ColliderRW colRW, ref Vector3 point)
            {
                var capsuleVec = colRW.Direction_Current;
                var capsuleVecNormal = capsuleVec.normalized;
                var capsulePos = colRW.Position_Current;
                var targetVec = point - capsulePos;
                var distanceOnVec = Vector3.Dot(capsuleVecNormal, targetVec);
                if (distanceOnVec <= EPSILON)
                {
                    return PushoutFromSphere(capsulePos, colRW.Radius, ref point);
                }
                else if (distanceOnVec >= colR.Height)
                {
                    return PushoutFromSphere(capsulePos + capsuleVec, colRW.Radius * colR.RadiusTailScale, ref point);
                }
                else
                {
                    var positionOnVec = capsulePos + (capsuleVecNormal * distanceOnVec);
                    var pushoutVec = point - positionOnVec;
                    var sqrPushoutDistance = pushoutVec.sqrMagnitude;
                    if (sqrPushoutDistance > EPSILON)
                    {
                        var Radius = colRW.Radius * Mathf.Lerp(1.0f, colR.RadiusTailScale, distanceOnVec / colR.Height);
                        if (sqrPushoutDistance < Radius * Radius)
                        {
                            var pushoutDistance = Mathf.Sqrt(sqrPushoutDistance);
                            point = positionOnVec + pushoutVec * Radius / pushoutDistance;
                            return true;
                        }
                    }
                    return false;
                }
            }

            public static bool CollisionDetection_Sphere(Vector3 center, float radius, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
            {
                var direction = point2 - point1;
                var directionLength = direction.magnitude;
                direction /= directionLength;

                var toCenter = center - point1;
                var dot = Vector3.Dot(direction, toCenter);
                var pointOnDirection = direction * Mathf.Clamp(dot, 0.0f, directionLength);

                pointOnCollider = center;
                pointOnLine = pointOnDirection + point1;
                Radius = radius;

                if ((pointOnCollider - pointOnLine).sqrMagnitude > radius * radius)
                {
                    return false;
                }

                return true;
            }

            public static bool CollisionDetection_Capsule(ref ColliderR colR, ref ColliderRW colRW, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
            {
                var capsuleDir = colRW.Direction_Current;
                var capsulePos = colRW.Position_Current;

                if (CollisionDetection_Sphere(capsulePos, colRW.Radius, point1, point2, out pointOnLine, out pointOnCollider, out Radius))
                    return true;
                if (CollisionDetection_Sphere(capsulePos + capsuleDir, colRW.Radius * colR.RadiusTailScale, point1, point2, out pointOnLine, out pointOnCollider, out Radius))
                    return true;

                var pointDir = point2 - point1;

                float t1, t2;
                var sqrDistance = ComputeNearestPoints(capsulePos, capsuleDir, point1, pointDir, out t1, out t2, out pointOnCollider, out pointOnLine);
                t1 = Mathf.Clamp01(t1);
                Radius = colRW.Radius * Mathf.Lerp(1.0f, colR.RadiusTailScale, t1);

                if (sqrDistance > Radius * Radius)
                {
                    pointOnCollider = Vector3.zero;
                    pointOnLine = Vector3.zero;
                    return false;
                }

                t2 = Mathf.Clamp01(t2);

                pointOnCollider = capsulePos + capsuleDir * t1;
                pointOnLine = point1 + pointDir * t2;

                return (pointOnCollider - pointOnLine).sqrMagnitude <= Radius * Radius;
            }

            public static bool CollisionDetection(ref ColliderR colR, ref ColliderRW colRW, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
            {
                if (colR.Height <= EPSILON)
                {
                    return CollisionDetection_Sphere(colRW.Position_Current, colRW.Radius, point1, point2, out pointOnLine, out pointOnCollider, out Radius);
                }
                else
                {
                    return CollisionDetection_Capsule(ref colR, ref colRW, point1, point2, out pointOnLine, out pointOnCollider, out Radius);
                }
            }

            public static float ComputeNearestPoints(Vector3 posP, Vector3 dirP, Vector3 posQ, Vector3 dirQ, out float tP, out float tQ, out Vector3 pointOnP, out Vector3 pointOnQ)
            {
                var n1 = Vector3.Cross(dirP, Vector3.Cross(dirQ, dirP));
                var n2 = Vector3.Cross(dirQ, Vector3.Cross(dirP, dirQ));

                tP = Vector3.Dot((posQ - posP), n2) / Vector3.Dot(dirP, n2);
                tQ = Vector3.Dot((posP - posQ), n1) / Vector3.Dot(dirQ, n1);
                pointOnP = posP + dirP * tP;
                pointOnQ = posQ + dirQ * tQ;

                return (pointOnQ - pointOnP).sqrMagnitude;
            }
        }

        interface Job
        {
            void Execute();
        }

        void JobSchedule<T>(T job)
             where T : Job
        {
            job.Execute();
        }

        interface JobParallelFor
        {
            void Execute(int index);
        }

        void JobSchedule<T>(int Count, T job)
             where T : JobParallelFor
        {
            for (int index = 0; index < Count; ++index)
            {
                job.Execute(index);
            }
        }

        interface JobParallelForTransform
        {
            void Execute(int index, Transform t);
        }

        void JobSchedule<T>(Transform[] t, T job)
             where T : JobParallelForTransform
        {
            for (int index = 0; index < t.Length; ++index)
            {
                job.Execute(index, t[index]);
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobTransformInitialize
#if ENABLE_JOBSYSTEM
            : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
            : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
            public NativeArray<PointR> PointsR;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var ptR = PointsR[index];

                transform.localScale = ptR.InitialLocalScale;
                transform.localRotation = ptR.InitialLocalRotation;
                transform.localPosition = ptR.InitialLocalPosition;
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobCaptureCurrentPositionFromTransform
#if ENABLE_JOBSYSTEM
            : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
            : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
            public NativeArray<PointRW> PointsRW;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var ptRW = PointsRW[index];
                ptRW.Position_PreviousTransform = PointsRW[index].Position_CurrentTransform;
                ptRW.Position_CurrentTransform = transform.position;
                PointsRW[index] = ptRW;
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobUpdateColliderTransform
#if ENABLE_JOBSYSTEM
            : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
            : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
            public NativeArray<ColliderRW> CollidersRW;
            [ReadOnly] public NativeArray<ColliderR> CollidersR;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var Src = CollidersR[index];
                var Dst = CollidersRW[index];

                Dst.Position_PreviousTransform = Dst.Position_CurrentTransform;
                Dst.Position_CurrentTransform = transform.position;

                if (Src.Height > EPSILON)
                {
                    Dst.Direction_PreviousTransform = Dst.Direction_CurrentTransform;
                    Dst.Direction_CurrentTransform = transform.rotation;
                }

                Dst.WorldToLocal = transform.worldToLocalMatrix;

                CollidersRW[index] = Dst;
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobCaptureTransformPosition
#if ENABLE_JOBSYSTEM
            : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
            : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
            public NativeArray<Vector3> Positions;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                Positions[index] = transform.position;
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobApplySimlationResult
#if ENABLE_JOBSYSTEM
            : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
            : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
            public NativeArray<PointRW> PointsRW;
            [ReadOnly] public NativeArray<PointR> PointsR;
            [ReadOnly] public NativeArray<Vector3> PointsP2T;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var ptR = PointsR[index];
                var ptRW = PointsRW[index];

                //if ((ptR.Weight >= EPSILON) && (ptR.Parent != -1))
                if (ptR.Weight >= EPSILON)
                {
                    var Direction = ptRW.Position_ToTransform - PointsP2T[ptR.Parent];
                    var RealLength = Direction.magnitude;
                    if (RealLength > EPSILON)
                    {
                        ptRW.Direction_Previous = Direction;
                        transform.position = ptRW.Position_ToTransform;
                        SetRotation(ref ptR, ref ptRW, transform);
                    }
                    else
                    {
                        ptRW.Position_ToTransform = PointsP2T[ptR.Parent] + ptRW.Direction_Previous;
                    }
                }
                else
                {
                    ptRW.Position_ToTransform = transform.position;
                    SetRotation(ref ptR, ref ptRW, transform);
                }

                PointsRW[index] = ptRW;
            }

#if ENABLE_JOBSYSTEM
            void SetRotation(ref PointR ptR, ref PointRW ptRW, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            void SetRotation(ref PointR ptR, ref PointRW ptRW, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                transform.localRotation = ptR.InitialLocalRotation;
                if (ptR.Child != -1)
                {
                    var Direction = PointsP2T[ptR.Child] - ptRW.Position_ToTransform;
                    if (Direction.sqrMagnitude > EPSILON)
                    {
                        Matrix4x4 mRotate = Matrix4x4.Rotate(transform.rotation);
                        Vector3 AimVector = mRotate * ptR.BoneAxis;
                        Quaternion AimRotation = Quaternion.FromToRotation(AimVector, Direction);
                        transform.rotation = AimRotation * transform.rotation;
                    }
                }
            }
        }

#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobTransformAngleLock
#if ENABLE_JOBSYSTEM
            : IJob
#else//ENABLE_JOBSYSTEM
            : Job
#endif//ENABLE_JOBSYSTEM
        {
            public NativeArray<PointRW> PointsRW;
            [ReadOnly] public NativeArray<PointR> PointsR;
            [ReadOnly] public AngleLimitConfig AngleLockConfig;

#if ENABLE_JOBSYSTEM
            void IJob.Execute()
#else//ENABLE_JOBSYSTEM
            public void Execute()
#endif//ENABLE_JOBSYSTEM
            {
                for (int index = 0; index < PointsR.Length; ++index)
                {
                    var ptR = PointsR[index];
                    if (ptR.Parent == -1)
                        continue;
                    var ptRW = PointsRW[index];

                    var ptRp = PointsR[ptR.Parent];
                    var ptRWp = PointsRW[ptR.Parent];

                    Vector3 superParentpos = (ptRp.Parent != -1) ? PointsRW[ptRp.Parent].Position_Current : ptRWp.Position_Current;

                    Vector3 boneDir = ptRW.Position_Current - ptRWp.Position_Current;
                    Vector3 parentBoneDir = ptRWp.Position_Current - superParentpos;

                    if ((parentBoneDir.magnitude == 0) || AngleLockConfig.limitFromRoot)
                    {
                        parentBoneDir = ptRW.Position_CurrentTransform - ptRWp.Position_CurrentTransform;
                    }

                    float angle = Vector3.Angle(parentBoneDir, boneDir);
                    float remainingAngle = angle - AngleLockConfig.angleLimit;

                    if (remainingAngle > 0.0f)
                    {
                        Vector3 axis = Vector3.Cross(parentBoneDir, boneDir);
                        ptRW.Position_Current = ptRWp.Position_Current + Quaternion.AngleAxis(-remainingAngle, axis) * boneDir;
                    }

                    PointsRW[index] = ptRW;
                }
            }
        }
    }
}
