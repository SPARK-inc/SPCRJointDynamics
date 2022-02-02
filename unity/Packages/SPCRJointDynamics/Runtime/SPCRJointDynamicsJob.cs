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
#define ENABLE_JOBSYSTEM

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace SPCR
{
    public unsafe class SPCRJointDynamicsJob
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
            public Quaternion LocalRotation;
            public Quaternion Rotation;
            public Vector3 Position;
            public Vector3 OldPosition;
            public Vector3 PreviousDirection;
        }

        struct PointRead
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
            public Vector3 Gravity;
            public Vector3 BoneAxis;
            public Vector3 ParentBoneAxis;
            public Quaternion LocalRotation;
            public Quaternion Rotation;
            public Vector3 InitialLocalScale;
            public Quaternion InitialLocalRotation;
            public Vector3 InitialLocalPosition;
            public Vector3 InitialRootSpacePosition;
        }

        struct PointReadWrite
        {
            public Vector3 FinalPosition;
            public Vector3 CurrentTransformPosition;
            public Vector3 Position;
            public Vector3 OldPosition;
            public Vector3 OriginalOldPosition;
            public Vector3 PreviousDirection;
            public Vector3 PushOutForce;
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

        struct Collider
        {
            public float Radius;
            public float RadiusTailScale;
            public float Height;
            public float Friction;
            public SPCRJointDynamicsCollider.ColliderForce ForceType;
        }

        struct ColliderEx
        {
            public Vector3 Position;
            public Vector3 Direction;
            public Vector3 OldPosition;
            public Vector3 OldDirection;
            public Vector3 SourcePosition;
            public Vector3 SourceDirection;
            public Matrix4x4 WorldToLocal;
            public Bounds LocalBounds;
            public int Enabled;
        }

        struct Grabber
        {
            public float Radius;
            public float Force;
        }

        struct GrabberEx
        {
            public int Enabled;
            public Vector3 Position;
        }

        public struct AngleLimitConfig
        {
            public float angleLimit;
            public bool limitFromRoot;
        }

        public struct SurfaceFaceConstraints
        {
            public int IndexA;
            public int IndexB;
            public int IndexC;
            public int IndexD;
        }

#if ENABLE_JOBSYSTEM
        JobHandle _hJob = default;
#endif//ENABLE_JOBSYSTEM
        Transform _RootBone;
        Vector3 _OldRootPosition;
        Vector3 _OldRootScale;
        Quaternion _OldRootRotation;
        int _PointCount;
        NativeArray<PointRead> _PointsR;
        NativeArray<PointReadWrite> _PointsRW;
        NativeArray<Vector3> _MovableLimitTargets;
        NativeArray<Constraint>[] _Constraints;
        NativeArray<SurfaceFaceConstraints> _SurfaceConstraints;
        Transform[] _PointTransforms;
        TransformAccessArray _TransformArray;
        Transform[] _MovableLimitTargetTransforms;
        TransformAccessArray _MovableLimitTargetTransformArray;
        SPCRJointDynamicsCollider[] _RefColliders;
        NativeArray<Collider> _Colliders;
        NativeArray<ColliderEx> _ColliderExs;
        SPCRJointDynamicsPointGrabber[] _RefGrabbers;
        NativeArray<Grabber> _Grabbers;
        NativeArray<GrabberEx> _GrabberExs;
        NativeArray<Plane> _FlatPlanes;
        AngleLimitConfig _AngleLockConfig;

        public bool Initialize(
            Transform RootBone,
            Point[] Points, Transform[] PointTransforms, Transform[] MovableLimitTargetTransforms,
            Constraint[][] Constraints,
            SPCRJointDynamicsCollider[] Colliders, SPCRJointDynamicsPointGrabber[] Grabbers,
            int FlatPlanesCount,
            SurfaceFaceConstraints[] SurfaceConstraints,
            AngleLimitConfig angleLockConfig)
        {
            _RootBone = RootBone;
            _PointCount = Points.Length;
            _OldRootPosition = _RootBone.position;
            _OldRootRotation = _RootBone.rotation;
            _OldRootScale = _RootBone.lossyScale;
            _PointTransforms = PointTransforms;
            _MovableLimitTargetTransforms = MovableLimitTargetTransforms;
            _AngleLockConfig = angleLockConfig;

            var PointsR = new PointRead[_PointCount];
            var PointsRW = new PointReadWrite[_PointCount];
            for (int i = 0; i < Points.Length; ++i)
            {
                var src = Points[i];
                PointsR[i].Parent = src.Parent;
                PointsR[i].Child = src.Child;

                PointsR[i].MovableLimitIndex = src.MovableLimitIndex;
                PointsR[i].MovableLimitRadius = src.MovableLimitRadius;
                PointsR[i].Weight = src.Weight;
                PointsR[i].Mass = src.Mass;
                PointsR[i].Resistance = src.Resistance;
                PointsR[i].Hardness = src.Hardness;
                PointsR[i].FrictionScale = src.FrictionScale;

                PointsR[i].SliderJointLength = src.SliderJointLength * 0.5f;
                PointsR[i].ParentLength = src.ParentLength;

                PointsR[i].StructuralShrinkHorizontal = src.StructuralShrinkHorizontal * 0.5f;
                PointsR[i].StructuralStretchHorizontal = src.StructuralStretchHorizontal * 0.5f;
                PointsR[i].StructuralShrinkVertical = src.StructuralShrinkVertical * 0.5f;
                PointsR[i].StructuralStretchVertical = src.StructuralStretchVertical * 0.5f;
                PointsR[i].ShearShrink = src.ShearShrink * 0.5f;
                PointsR[i].ShearStretch = src.ShearStretch * 0.5f;
                PointsR[i].BendingShrinkHorizontal = src.BendingShrinkHorizontal * 0.5f;
                PointsR[i].BendingStretchHorizontal = src.BendingStretchHorizontal * 0.5f;
                PointsR[i].BendingShrinkVertical = src.BendingShrinkVertical * 0.5f;
                PointsR[i].BendingStretchVertical = src.BendingStretchVertical * 0.5f;

                PointsR[i].Gravity = src.Gravity;
                PointsR[i].WindForceScale = src.WindForceScale;
                PointsR[i].BoneAxis = src.BoneAxis;
                PointsR[i].ParentBoneAxis = src.Parent == -1 ? Vector3.zero : (src.Position - Points[src.Parent].Position).normalized;
                PointsR[i].Rotation = src.Rotation;
                PointsR[i].LocalRotation = src.LocalRotation;
                PointsR[i].InitialLocalScale = PointTransforms[i].localScale;
                PointsR[i].InitialLocalRotation = PointTransforms[i].localRotation;
                PointsR[i].InitialLocalPosition = PointTransforms[i].localPosition;
                PointsR[i].InitialRootSpacePosition = _RootBone.worldToLocalMatrix.MultiplyPoint3x4(PointTransforms[i].position);

                PointsRW[i].CurrentTransformPosition = src.Position;
                PointsRW[i].Position = src.Position;
                PointsRW[i].OldPosition = src.OldPosition;
                PointsRW[i].OriginalOldPosition = src.OldPosition;
                PointsRW[i].PreviousDirection = src.PreviousDirection;
                PointsRW[i].Friction = 0.5f;
                PointsRW[i].GrabberIndex = -1;
                PointsRW[i].GrabberDistance = 0.0f;
            }

            _PointsR = new NativeArray<PointRead>(_PointCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _PointsR.CopyFrom(PointsR);
            _PointsRW = new NativeArray<PointReadWrite>(_PointCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _PointsRW.CopyFrom(PointsRW);

            _MovableLimitTargets = new NativeArray<Vector3>(MovableLimitTargetTransforms.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _MovableLimitTargetTransformArray = new TransformAccessArray(_MovableLimitTargetTransforms);

            _TransformArray = new TransformAccessArray(_PointTransforms);

            _Constraints = new NativeArray<Constraint>[Constraints.Length];
            for (int i = 0; i < Constraints.Length; ++i)
            {
                var src = Constraints[i];
                _Constraints[i] = new NativeArray<Constraint>(src.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _Constraints[i].CopyFrom(src);
            }

            _SurfaceConstraints = new NativeArray<SurfaceFaceConstraints>(SurfaceConstraints.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _SurfaceConstraints.CopyFrom(SurfaceConstraints);

            _RefColliders = Colliders;
            var ColliderR = new Collider[Colliders.Length];
            var ColliderExR = new ColliderEx[Colliders.Length];
            for (int i = 0; i < Colliders.Length; ++i)
            {
                var src = Colliders[i];
                if (src == null)
                {
                    Debug.LogError("SPCRJointDynamics: Collider [" + i + "] is null!!!");
                    return false;
                }
                if (src.IsCapsule)
                {
                    ColliderR[i].Height = src.Height;
                }
                else
                {
                    ColliderR[i].Height = 0.0f;
                }
                ColliderR[i].Radius = src.Radius;
                ColliderR[i].RadiusTailScale = src.RadiusTailScale;
                ColliderR[i].Friction = src.Friction;
                ColliderR[i].ForceType = src._SurfaceColliderForce;
                ColliderExR[i].Enabled = src.isActiveAndEnabled ? 1 : 0;
                ColliderExR[i].Position = ColliderExR[i].OldPosition = src.transform.position;
                ColliderExR[i].Direction = ColliderExR[i].OldDirection = src.transform.rotation * Vector3.up * src.Height;
                ColliderExR[i].LocalBounds = new Bounds();
            }
            _Colliders = new NativeArray<Collider>(Colliders.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _Colliders.CopyFrom(ColliderR);
            _ColliderExs = new NativeArray<ColliderEx>(Colliders.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _ColliderExs.CopyFrom(ColliderExR);

            _RefGrabbers = Grabbers;
            var GrabberR = new Grabber[Grabbers.Length];
            for (int i = 0; i < Grabbers.Length; ++i)
            {
                var src = Grabbers[i];
                if (src == null)
                {
                    Debug.LogError("SPCRJointDynamics: Grabber [" + i + "] is null!!!");
                    return false;
                }
                GrabberR[i].Radius = src.Radius;
                GrabberR[i].Force = src.Force;
            }
            _Grabbers = new NativeArray<Grabber>(Grabbers.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _Grabbers.CopyFrom(GrabberR);

            _GrabberExs = new NativeArray<GrabberEx>(Grabbers.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            _FlatPlanes = new NativeArray<Plane>(FlatPlanesCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            return true;
        }

        public void Uninitialize()
        {
            for (int i = 0; i < _PointsRW.Length; ++i)
            {
                var t = _PointTransforms[i];
                t.localPosition = _PointsR[i].InitialLocalPosition;
                t.localRotation = _PointsR[i].InitialLocalRotation;
                t.localScale = _PointsR[i].InitialLocalScale;
            }

            _FlatPlanes.Dispose();
            _GrabberExs.Dispose();
            _Grabbers.Dispose();
            _ColliderExs.Dispose();
            _Colliders.Dispose();
            for (int i = 0; i < _Constraints.Length; ++i)
            {
                _Constraints[i].Dispose();
            }
            _MovableLimitTargets.Dispose();
            _MovableLimitTargetTransformArray.Dispose();
            _TransformArray.Dispose();
            _PointsR.Dispose();
            _PointsRW.Dispose();
            _SurfaceConstraints.Dispose();
        }

        public void Reset(bool ResetToTPose, Matrix4x4 LocalToWorldMatrix)
        {
            var pPointsR = (PointRead*)_PointsR.GetUnsafePtr();
            var pPointsRW = (PointReadWrite*)_PointsRW.GetUnsafePtr();
            if (ResetToTPose)
            {
                for (int i = 0; i < _PointsRW.Length; ++i)
                {
                    pPointsRW[i].Position = LocalToWorldMatrix.MultiplyPoint3x4(pPointsR[i].InitialRootSpacePosition);
                }
            }
            else
            {
                for (int i = 0; i < _PointsRW.Length; ++i)
                {
                    var t = _PointTransforms[i];
                    pPointsRW[i].Position = t.position;
                }
            }

            for (int i = 0; i < _PointsRW.Length; ++i)
            {
                pPointsRW[i].OldPosition = pPointsRW[i].Position;
                pPointsRW[i].CurrentTransformPosition = pPointsRW[i].Position;
                pPointsRW[i].PushOutForce = Vector3.zero;
            }

            var pColloderExs = (ColliderEx*)_ColliderExs.GetUnsafePtr();
            for (int i = 0; i < _RefColliders.Length; ++i)
            {
                var Src = _RefColliders[i];
                pColloderExs[i].Position = pColloderExs[i].OldPosition = Src.transform.position;
                pColloderExs[i].Direction = pColloderExs[i].OldDirection = Src.transform.rotation * Vector3.up * Src.Height;
            }

            var pMovable = (Vector3*)_MovableLimitTargets.GetUnsafePtr();
            for (int i = 0; i < _MovableLimitTargetTransforms.Length; ++i)
            {
                pMovable[i] = _MovableLimitTargetTransforms[i].position;
            }

            _OldRootPosition = _RootBone.position;
            _OldRootRotation = _RootBone.rotation;
            _OldRootScale = _RootBone.lossyScale;
        }

        public void TransformInitialize(bool IsCaptureAnimationTransform)
        {
            if (IsCaptureAnimationTransform)
            {
                var Job = new JobTransformInitialize();
                Job.pRPoints = (PointRead*)_PointsR.GetUnsafePtr();
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_TransformArray);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
        }

        public void WaitInitialize(bool _EnableCaptureAnimationTransform)
        {
            if (_EnableCaptureAnimationTransform)
            {
#if ENABLE_JOBSYSTEM
                _hJob.Complete();
                _hJob = default;
#else//ENABLE_JOBSYSTEM
            //
#endif//ENABLE_JOBSYSTEM
            }
        }

        public void PreSimulation(bool IsCaptureAnimationTransform)
        {
            if (IsCaptureAnimationTransform)
            {
                var Job = new JobCaptureCurrentTransformPositionFromTransform();
                Job.pRWPoints = (PointReadWrite*)_PointsRW.GetUnsafePtr();
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_TransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
            else
            {
                var Job = new JobCaptureCurrentTransformPosition();
                Job.pRWPoints = (PointReadWrite*)_PointsRW.GetUnsafePtr();
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_TransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
        }

        const int _InnerJobCount = 4;

        public void Simulation(
            Transform RootTransform,
            float RootSlideLimit, float RootRotateLimit,
            float StepTime,
            int SubSteps,
            Vector3 WindForce,
            int Relaxation,
            Plane[] FlatPlanes,
            bool EnableCollision,
            bool EnableSurfaceCollision,
            int SurfaceCollisionDivision,
            float BlendRatio,
            bool IsCaptureAnimationTransform)
        {
            var IsPaused = StepTime <= 0.0f;
            if (IsPaused)
            {
                SubSteps = 1;
            }

            var RootPosition = RootTransform.position;
            var RootRotation = RootTransform.rotation;
            var RootScale = RootTransform.lossyScale;

            var RootSlide = RootPosition - _OldRootPosition;
            var SystemOffset = Vector3.zero;
            var SlideLength = RootSlide.magnitude;
            if (RootSlideLimit >= 0.0f && SlideLength > RootSlideLimit)
            {
                SystemOffset = RootSlide * (1.0f - RootSlideLimit / SlideLength);
                SystemOffset /= SubSteps;
            }

            var RootDeltaRotation = RootRotation * Quaternion.Inverse(_OldRootRotation);
            var RotateAngle = Mathf.Acos(RootDeltaRotation.w) * 2.0f * Mathf.Rad2Deg;
            Quaternion SystemRotation = Quaternion.identity;
            if (RootRotateLimit >= 0.0f && Mathf.Abs(RotateAngle) > RootRotateLimit)
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
                SystemRotation = Quaternion.AngleAxis(Angle, RotateAxis);
            }

            if (IsPaused)
            {
                SystemOffset = RootSlide;
                SystemRotation = RootDeltaRotation;
            }

            var pRPoints = (PointRead*)_PointsR.GetUnsafePtr();
            var pRWPoints = (PointReadWrite*)_PointsRW.GetUnsafePtr();
            var pRMovableLimitTargets = (Vector3*)_MovableLimitTargets.GetUnsafePtr();
            var pColliders = (Collider*)_Colliders.GetUnsafePtr();
            var pColliderExs = (ColliderEx*)_ColliderExs.GetUnsafePtr();
            var pGrabbers = (Grabber*)_Grabbers.GetUnsafePtr();
            var pSurfaceFaces = (SurfaceFaceConstraints*)_SurfaceConstraints.GetUnsafePtr();
            var pGrabberExs = (GrabberEx*)_GrabberExs.GetUnsafePtr();

            var GrabberCount = _RefGrabbers.Length;
            var ColliderCount = EnableCollision ? _RefColliders.Length : 0;

            // Grabber
            for (var i = 0; i < GrabberCount; ++i)
            {
                var pDst = pGrabberExs + i;

                pDst->Enabled = (_RefGrabbers[i].isActiveAndEnabled && _RefGrabbers[i].IsEnabled) ? 1 : 0;
                pDst->Position = _RefGrabbers[i].RefTransform.position;
            }

            _FlatPlanes.CopyFrom(FlatPlanes);

            var TempBounds = new Bounds();

            StepTime /= SubSteps;

            for (var iSubStep = 1; iSubStep <= SubSteps; iSubStep++)
            {
                var SubDelta = (float)iSubStep / SubSteps;

                var ColliderDelta = 1.0f / (SubSteps - iSubStep + 1.0f);
                for (var i = 0; i < ColliderCount; ++i)
                {
                    var pDst = pColliderExs + i;
                    var Src = _RefColliders[i];

                    pDst->Enabled = Src.isActiveAndEnabled ? 1 : 0;
                    if (pDst->Enabled == 0)
                    {
                        continue;
                    }

                    if (iSubStep == 1)
                    {
                        var SrcT = Src.RefTransform;
                        if (Src.Height <= EPSILON)
                        {
                            pDst->SourcePosition = SrcT.position;
                        }
                        else
                        {
                            pDst->SourcePosition = SrcT.position - (pDst->Direction * 0.5f);
                        }

                        pDst->SourceDirection = SrcT.rotation * Vector3.up * Mathf.Clamp(Src.Height, 0.01f, Src.Height);

                        pDst->WorldToLocal = SrcT.worldToLocalMatrix;
                    }

                    pDst->OldPosition = pDst->Position;
                    pDst->OldDirection = pDst->Direction;

                    pDst->Position = Vector3.Lerp(pDst->OldPosition, pDst->SourcePosition, ColliderDelta);
                    pDst->Direction = Vector3.Lerp(pDst->OldDirection, pDst->SourceDirection, ColliderDelta);

                    Vector3 Corner, Center;

                    // Head
                    Corner = Vector3.one * Src.Radius;

                    // Current
                    Center = pDst->WorldToLocal.MultiplyPoint(pDst->Position);
                    pDst->LocalBounds.SetMinMax(Center - Corner, Center + Corner);

                    // Old
                    Center = pDst->WorldToLocal.MultiplyPoint(pDst->OldPosition);
                    TempBounds.SetMinMax(Center - Corner, Center + Corner);
                    pDst->LocalBounds.Encapsulate(TempBounds);

                    // Tail
                    if (Src.Height > EPSILON)
                    {
                        Corner = Vector3.one * Src.Radius * Src.RadiusTailScale;
                        // Current
                        Center = pDst->WorldToLocal.MultiplyPoint(pDst->Position + pDst->Direction);
                        TempBounds.SetMinMax(Center - Corner, Center + Corner);
                        pDst->LocalBounds.Encapsulate(TempBounds);
                        // Old
                        Center = pDst->WorldToLocal.MultiplyPoint(pDst->OldPosition + pDst->OldDirection);
                        TempBounds.SetMinMax(Center - Corner, Center + Corner);
                        pDst->LocalBounds.Encapsulate(TempBounds);
                    }
                }

                var RootMatrix = Matrix4x4.TRS(
                    Vector3.Lerp(_OldRootPosition, RootPosition, SubDelta),
                    Quaternion.Slerp(_OldRootRotation, RootRotation, SubDelta),
                    Vector3.Lerp(_OldRootScale, RootScale, SubDelta));

                {
                    var Job = new JobPointUpdate();
                    Job.RootMatrix = RootMatrix;
                    Job.OldRootPosition = _OldRootPosition;
                    Job.GrabberCount = _RefGrabbers.Length;
                    Job.pGrabbers = pGrabbers;
                    Job.pGrabberExs = pGrabberExs;
                    Job.pRPoints = pRPoints;
                    Job.pRWPoints = pRWPoints;
                    Job.pRMovableLimitTargets = pRMovableLimitTargets;
                    Job.WindForce = WindForce;
                    Job.StepTime_x2_Half = StepTime * StepTime * 0.5f;
                    Job.SystemOffset = SystemOffset;
                    Job.SystemRotation = SystemRotation;
                    Job.IsPaused = IsPaused;
                    Job.BlendRatio = BlendRatio;
                    Job.FlatPlanes = (Plane*)_FlatPlanes.GetUnsafePtr();
                    Job.FlatPlaneCount = _FlatPlanes.Length;
                    Job.IsCaptureAnimationTransform = IsCaptureAnimationTransform;
#if ENABLE_JOBSYSTEM
                    _hJob = Job.Schedule(_PointCount, _InnerJobCount, _hJob);
#else//ENABLE_JOBSYSTEM
                    JobSchedule(_PointCount, Job);
#endif//ENABLE_JOBSYSTEM
                }

                if (!IsPaused)
                {
                    //if (EnablePointCollision && (DetailHitDivideMax > 0))
                    //{
                    //    var Job = new JobMovingCollisionPoint();
                    //    Job.pRPoints = pRPoints;
                    //    Job.pRWPoints = pRWPoints;
                    //    Job.pColliders = pColliders;
                    //    Job.pColliderExs = pColliderExs;
                    //    Job.ColliderCount = ColliderCount;
                    //    Job.DivideMax = DetailHitDivideMax;
                    //    _hJob = Job.Schedule(_PointCount, _InnerJobCount, _hJob);
                    //}

                    //if (EnablePointCollision)
                    //{
                    //    var Job = new JobCollisionPoint();
                    //    Job.pRWPoints = pRWPoints;
                    //    Job.pColliders = pColliders;
                    //    Job.pColliderExs = pColliderExs;
                    //    Job.ColliderCount = ColliderCount;
                    //    Job.IsEnableCollider = EnablePointCollision;
                    //    _hJob = Job.Schedule(_PointCount, _InnerJobCount, _hJob);
                    //}

                    for (var i = Relaxation - 1; i >= 0; --i)
                    {
                        foreach (var constraint in _Constraints)
                        {
                            var Job = new JobConstraintUpdate();
                            Job.IsCollision = (i % 2) == 0;
                            Job.pConstraints = (Constraint*)constraint.GetUnsafePtr();
                            Job.pRPoints = pRPoints;
                            Job.pRWPoints = pRWPoints;
                            Job.pColliders = pColliders;
                            Job.pColliderExs = pColliderExs;
                            Job.ColliderCount = ColliderCount;
#if ENABLE_JOBSYSTEM
                            _hJob = Job.Schedule(constraint.Length, _InnerJobCount, _hJob);
#else//ENABLE_JOBSYSTEM
                            JobSchedule(constraint.Length, Job);
#endif//ENABLE_JOBSYSTEM
                        }
                    }

                    if (EnableSurfaceCollision)
                    {
                        var Job = new JobSurfaceCollision();
                        Job.pRPoints = pRPoints;
                        Job.pRWPoints = pRWPoints;
                        Job.pSurfaceConstraint = pSurfaceFaces;
                        Job.pColliders = pColliders;
                        Job.pColliderExs = pColliderExs;
                        Job.ColliderCount = ColliderCount;
                        Job.CollisionDivision = Mathf.Clamp(SurfaceCollisionDivision, 1, SurfaceCollisionDivision);
#if ENABLE_JOBSYSTEM
                        _hJob = Job.Schedule(_SurfaceConstraints.Length, _InnerJobCount, _hJob);
#else//ENABLE_JOBSYSTEM
                        JobSchedule(_SurfaceConstraints.Length, Job);
#endif//ENABLE_JOBSYSTEM
                    }
                }
            }

            _OldRootPosition = RootPosition;
            _OldRootRotation = RootRotation;
            _OldRootScale = RootScale;
        }

        public void PostSimulation()
        {
            {
                var Job = new JobApplySimlationResultToTransform();
                Job.pRPoints = (PointRead*)_PointsR.GetUnsafePtr();
                Job.pRWPoints = (PointReadWrite*)_PointsRW.GetUnsafePtr();
                Job.angleLockConfig = _AngleLockConfig;
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_TransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_PointTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
            if (_MovableLimitTargetTransforms.Length > 0)
            {
                var Job = new JobCaptureTransformPosition();
                Job.pPositions = (Vector3*)_MovableLimitTargets.GetUnsafePtr();
#if ENABLE_JOBSYSTEM
                _hJob = Job.Schedule(_MovableLimitTargetTransformArray, _hJob);
#else//ENABLE_JOBSYSTEM
                JobSchedule(_MovableLimitTargetTransforms, Job);
#endif//ENABLE_JOBSYSTEM
            }
        }

        public void WaitSimulation()
        {
#if ENABLE_JOBSYSTEM
            _hJob.Complete();
            _hJob = default;
#else//ENABLE_JOBSYSTEM
            //
#endif//ENABLE_JOBSYSTEM
        }

        public void DrawGizmos_Points(bool Draw3DGizmo)
        {
            if (Draw3DGizmo)
            {
                if (_PointTransforms == null) return;

                for (int i = 0; i < _PointTransforms.Length; ++i)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(_PointTransforms[i].position, 0.06f);
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
                    Gizmos.DrawSphere(_PointsRW[i].Position, 0.005f);
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
            for (int i = 0; i < _ColliderExs.Length; i++)
            {
                if (_ColliderExs[i].Enabled == 0)
                    continue;

                var CurHead = _ColliderExs[i].Position;
                var CurTail = _ColliderExs[i].Position + _ColliderExs[i].Direction;
                var OldHead = _ColliderExs[i].OldPosition;
                var OldTail = _ColliderExs[i].OldPosition + _ColliderExs[i].OldDirection;

                Gizmos.color = Color.black;
                Gizmos.DrawLine(CurHead, OldHead);
                Gizmos.DrawLine(CurTail, OldTail);

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(OldHead, OldTail);

                var LocalToWorld = _ColliderExs[i].WorldToLocal.inverse;
                var BoundsMin = _ColliderExs[i].LocalBounds.min;
                var BoundsMax = _ColliderExs[i].LocalBounds.max;

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

        interface JobParallelFor
        {
            void Execute(int index);
        }

        void JobSchedule<T>(int Count, T job)
             where T : JobParallelFor
        {
            for (int i = 0; i < Count; ++i)
            {
                job.Execute(i);
            }
        }

        interface JobParallelForTransform
        {
            void Execute(int index, Transform t);
        }

        void JobSchedule<T>(Transform[] t, T job)
             where T : JobParallelForTransform
        {
            for (int i = 0; i < t.Length; ++i)
            {
                job.Execute(i, t[i]);
            }
        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobPointUpdate : IJobParallelFor
#else//ENABLE_JOBSYSTEM
        struct JobPointUpdate : JobParallelFor
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public int GrabberCount;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Grabber* pGrabbers;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public GrabberEx* pGrabberExs;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointRead* pRPoints;
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointReadWrite* pRWPoints;
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Vector3* pRMovableLimitTargets;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public Matrix4x4 RootMatrix;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public Vector3 OldRootPosition;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public Vector3 WindForce;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public float StepTime_x2_Half;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public Vector3 SystemOffset;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public Quaternion SystemRotation;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public bool IsPaused;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public float BlendRatio;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Plane* FlatPlanes;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public int FlatPlaneCount;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public bool IsCaptureAnimationTransform;

            private Vector3 ApplySystemTransform(Vector3 Point, Vector3 Pivot)
            {
                return SystemRotation * (Point - Pivot) + Pivot + SystemOffset;
            }

#if ENABLE_JOBSYSTEM
            void IJobParallelFor.Execute(int index)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index)
#endif//ENABLE_JOBSYSTEM
            {
                var pR = pRPoints + index;
                var pRW = pRWPoints + index;

                if (pR->Weight <= EPSILON)
                {
                    pRW->OriginalOldPosition = pRW->Position;
                    pRW->OldPosition = ApplySystemTransform(pRW->Position, OldRootPosition);
                    pRW->Position = RootMatrix.MultiplyPoint3x4(pR->InitialRootSpacePosition);
                    pRW->Friction = 0.0f;
                    return;
                }

                pRW->OriginalOldPosition = pRW->Position;
                pRW->OldPosition = ApplySystemTransform(pRW->OldPosition, OldRootPosition);
                pRW->Position = ApplySystemTransform(pRW->Position, OldRootPosition);

                if (pR->MovableLimitIndex != -1)
                {
                    var Target = pRMovableLimitTargets[pR->MovableLimitIndex];
                    var Move = pRW->Position - Target;
                    var MoveLength = Move.magnitude;
                    if (MoveLength > pR->MovableLimitRadius)
                    {
                        pRW->Position = Target + Move / MoveLength * pR->MovableLimitRadius;
                    }
                }

                Vector3 Displacement = Vector3.zero;
                if (!IsPaused)
                {
                    var MoveDir = pRW->Position - pRW->OldPosition;

                    Vector3 ExternalForce = Vector3.zero;
                    ExternalForce += pR->Gravity;
                    ExternalForce += WindForce * pR->WindForceScale / pR->Mass;
                    ExternalForce *= StepTime_x2_Half;

                    Displacement = MoveDir;
                    Displacement += ExternalForce;
                    Displacement += pRW->PushOutForce;
                    Displacement *= pR->Resistance;
                    Displacement *= 1.0f - Mathf.Clamp01(pRW->Friction * pR->FrictionScale);
                }

                pRW->PushOutForce = Vector3.zero;
                pRW->OldPosition = pRW->Position;
                pRW->Position += Displacement;
                pRW->Friction = 0.0f;

                if (!IsPaused)
                {
                    if (pR->Hardness > 0.0f)
                    {
                        if (IsCaptureAnimationTransform)
                        {
                            pRW->Position += (pRW->CurrentTransformPosition - pRW->Position) * pR->Hardness;
                        }
                        else
                        {
                            var Target = RootMatrix.MultiplyPoint3x4(pR->InitialRootSpacePosition);
                            pRW->Position += (Target - pRW->Position) * pR->Hardness;
                        }
                    }

                    if (pRW->GrabberIndex != -1)
                    {
                        Grabber* pGR = pGrabbers + pRW->GrabberIndex;
                        GrabberEx* pGRW = pGrabberExs + pRW->GrabberIndex;
                        if (pGRW->Enabled == 0)
                        {
                            pRW->GrabberIndex = -1;
                            return;
                        }

                        var Vec = pRW->Position - pGRW->Position;
                        var Pos = pGRW->Position + Vec.normalized * pRW->GrabberDistance;
                        pRW->Position += (Pos - pRW->Position) * pGR->Force;
                    }
                    else
                    {
                        int NearIndex = -1;
                        float sqrNearRange = 1000.0f * 1000.0f;
                        for (int i = 0; i < GrabberCount; ++i)
                        {
                            Grabber* pGR = pGrabbers + i;
                            GrabberEx* pGRW = pGrabberExs + i;

                            if (pGRW->Enabled == 0)
                                continue;

                            var Vec = pGRW->Position - pRW->Position;
                            var sqrVecLength = Vec.sqrMagnitude;
                            if (sqrVecLength < pGR->Radius * pGR->Radius)
                            {
                                if (sqrVecLength < sqrNearRange)
                                {
                                    sqrNearRange = sqrVecLength;
                                    NearIndex = i;
                                }
                            }
                        }
                        if (NearIndex != -1)
                        {
                            pRW->GrabberIndex = NearIndex;
                            pRW->GrabberDistance = Mathf.Sqrt(sqrNearRange) / 2.0f;
                        }
                    }
                }

                for (int i = 0; i < FlatPlaneCount; ++i)
                {
                    var Distance = FlatPlanes[i].GetDistanceToPoint(pRW->Position);
                    if (Distance < 0.0f)
                    {
                        pRW->Position = FlatPlanes[i].ClosestPointOnPlane(pRW->Position);
                        pRW->Friction = 0.3f;
                    }
                }

                pRW->FinalPosition = Vector3.Lerp(
                    pRW->Position, pRW->CurrentTransformPosition,
                    Mathf.SmoothStep(0.0f, 1.0f, BlendRatio));
            }
        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobSurfaceCollision : IJobParallelFor
#else//ENABLE_JOBSYSTEM
        struct JobSurfaceCollision : JobParallelFor
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointRead* pRPoints;
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointReadWrite* pRWPoints;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public SurfaceFaceConstraints* pSurfaceConstraint;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Collider* pColliders;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public ColliderEx* pColliderExs;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public int ColliderCount;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public int CollisionDivision;

#if ENABLE_JOBSYSTEM
            void IJobParallelFor.Execute(int index)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index)
#endif//ENABLE_JOBSYSTEM
            {
                var pSurfaceFace = pSurfaceConstraint + index;

                NativeArray<int> triangleArray = new NativeArray<int>(6, Allocator.Temp);
                triangleArray[0] = pSurfaceFace->IndexA;
                triangleArray[1] = pSurfaceFace->IndexB;
                triangleArray[2] = pSurfaceFace->IndexC;

                triangleArray[3] = pSurfaceFace->IndexC;
                triangleArray[4] = pSurfaceFace->IndexD;
                triangleArray[5] = pSurfaceFace->IndexA;

                for (int i = 0; i < ColliderCount; i++)
                {
                    Collider* pCollider = pColliders + i;

                    ColliderEx* pColliderEx = pColliderExs + i;
                    if (pColliderEx->Enabled == 0)
                        continue;

                    Vector3 intersectionPoint, pointOnCollider, pushOut;
                    float radius;

                    for (int division = 1; division <= CollisionDivision; division++)
                    {
                        Vector3 colliderPosition = Vector3.Lerp(pColliderEx->OldPosition, pColliderEx->Position, division / CollisionDivision);

                        for (int j = 0; j < triangleArray.Length; j += 3)
                        {
                            var RWPtA = pRWPoints + triangleArray[j + 0];
                            var RWPtB = pRWPoints + triangleArray[j + 1];
                            var RWPtC = pRWPoints + triangleArray[j + 2];

                            if (CheckCollision(RWPtA, RWPtB, RWPtC, colliderPosition, pCollider, pColliderEx, out intersectionPoint, out pointOnCollider, out radius, out pushOut))
                            {
                                Vector3 triangleCenter = CenterOfTheTriangle(RWPtA->Position, RWPtB->Position, RWPtC->Position);
                                float pushOutmagnitude = pushOut.magnitude;
                                pushOut /= pushOutmagnitude;

                                for (int k = 0; k < 3; k++)
                                {
                                    var rwPt = pRWPoints + triangleArray[j + k];
                                    Vector3 centerToPoint = rwPt->Position - triangleCenter;
                                    Vector3 intersectionToPoint = rwPt->Position - intersectionPoint;

                                    float rate = centerToPoint.magnitude / intersectionToPoint.magnitude;
                                    rate = Mathf.Clamp01(Mathf.Abs(rate));
                                    Vector3 pushVec = pushOut * (radius - pushOutmagnitude);
                                    rwPt->Position += pushVec * rate;
                                }
                            }
                        }
                    }
                }
            }

            bool CheckCollision(PointReadWrite* RWPtA, PointReadWrite* RWPtB, PointReadWrite* RWPtC, Vector3 colliderPosition, Collider* pCollider, ColliderEx* pColliderEx, out Vector3 intersectionPoint, out Vector3 pointOnCollider, out float radius, out Vector3 pushOut)
            {
                Ray ray;
                float enter;

                pointOnCollider = colliderPosition;
                radius = pCollider->Radius;

                bool isCapsuleCollider = pCollider->Height > EPSILON;

                Vector3 triangleCenter = CenterOfTheTriangle(RWPtA->Position, RWPtB->Position, RWPtC->Position);
                Vector3 colliderDirection = (colliderPosition - triangleCenter).normalized;

                Vector3 planeNormal = Vector3.Cross(RWPtB->Position - RWPtA->Position, RWPtC->Position - RWPtA->Position).normalized;
                float planeDistanceFromOrigin = -Vector3.Dot(planeNormal, RWPtA->Position);
                planeNormal *= -1;

                float dot = Vector3.Dot(colliderDirection, planeNormal);

                if (isCapsuleCollider)
                {
                    float side = Vector3.Dot(pColliderEx->Direction, planeNormal) * dot;
                    colliderDirection = planeNormal * dot * -1;
                    Vector3 tempPointOnCollider = pointOnCollider;

                    radius = pCollider->Radius;
                    if (side < 0)
                    {
                        radius *= pCollider->RadiusTailScale;
                        pointOnCollider = colliderPosition + ((pColliderEx->Direction * 0.5f) * 2);
                    }

                    ray = new Ray(pointOnCollider, colliderDirection.normalized);
                    if (Raycast(planeNormal * -1, planeDistanceFromOrigin, ray, out enter))
                    {
                        intersectionPoint = ray.GetPoint(enter);
                        if (TriangleContainsPoint(RWPtA->Position, RWPtB->Position, RWPtC->Position, intersectionPoint))
                        {
                            pushOut = intersectionPoint - pointOnCollider;
                            Vector3 towardsCollider = pointOnCollider - intersectionPoint;
                            return towardsCollider.sqrMagnitude <= radius * radius;
                        }
                    }
                    else
                    {
                        if (pCollider->ForceType != SPCRJointDynamicsCollider.ColliderForce.Off)
                        {
                            //Forcefully extrude the surface out of the collider
                            pointOnCollider = tempPointOnCollider;
                            Vector3 endVec = colliderPosition + ((pColliderEx->Direction * 0.5f) * 2);
                            Vector3 colDirVec = pColliderEx->Direction;

                            if (pCollider->ForceType == SPCRJointDynamicsCollider.ColliderForce.Pull)
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
                                if (TriangleContainsPoint(RWPtA->Position, RWPtB->Position, RWPtC->Position, intersectionPoint))
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
                        if (TriangleContainsPoint(RWPtA->Position, RWPtB->Position, RWPtC->Position, intersectionPoint))
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
                Vector3 v12 = Vector3.Lerp(v1, v2, 0.5f);
                Vector3 v13 = Vector3.Lerp(v1, v3, 0.5f);
                return Vector3.Lerp(v12, v13, 0.5f);
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

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobConstraintUpdate : IJobParallelFor
#else//ENABLE_JOBSYSTEM
        struct JobConstraintUpdate : JobParallelFor
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public bool IsCollision;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Constraint* pConstraints;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointRead* pRPoints;
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointReadWrite* pRWPoints;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Collider* pColliders;
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public ColliderEx* pColliderExs;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public int ColliderCount;

#if ENABLE_JOBSYSTEM
            void IJobParallelFor.Execute(int index)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index)
#endif//ENABLE_JOBSYSTEM
            {
                var constraint = pConstraints + index;
                var RptA = pRPoints + constraint->IndexA;
                var RptB = pRPoints + constraint->IndexB;

                float WeightA = RptA->Weight;
                float WeightB = RptB->Weight;

                if ((WeightA <= EPSILON) && (WeightB <= EPSILON)) return;

                var RWptA = pRWPoints + constraint->IndexA;
                var RWptB = pRWPoints + constraint->IndexB;

                var Direction = RWptB->Position - RWptA->Position;

                float Distance = Direction.magnitude;

                float ShrinkLength = constraint->Length;
                float StretchLength = ShrinkLength;
                switch (constraint->Type)
                {
                case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
                case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
                case SPCRJointDynamicsController.ConstraintType.Shear:
                    StretchLength += RptA->SliderJointLength + RptB->SliderJointLength;
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
                switch (constraint->Type)
                {
                case SPCRJointDynamicsController.ConstraintType.Structural_Vertical:
                    ConstraintPower = IsShrink
                        ? (RptA->StructuralShrinkVertical + RptB->StructuralShrinkVertical)
                        : (RptA->StructuralStretchVertical + RptB->StructuralStretchVertical);
                    break;
                case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
                    ConstraintPower = IsShrink
                        ? (RptA->StructuralShrinkHorizontal + RptB->StructuralShrinkHorizontal)
                        : (RptA->StructuralStretchHorizontal + RptB->StructuralStretchHorizontal);
                    break;
                case SPCRJointDynamicsController.ConstraintType.Shear:
                    ConstraintPower = IsShrink
                        ? (RptA->ShearShrink + RptB->ShearShrink)
                        : (RptA->ShearStretch + RptB->ShearStretch);
                    break;
                case SPCRJointDynamicsController.ConstraintType.Bending_Vertical:
                    ConstraintPower = IsShrink
                        ? (RptA->BendingShrinkVertical + RptB->BendingShrinkVertical)
                        : (RptA->BendingStretchVertical + RptB->BendingStretchVertical);
                    break;
                case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
                    ConstraintPower = IsShrink
                        ? (RptA->BendingShrinkHorizontal + RptB->BendingShrinkHorizontal)
                        : (RptA->BendingStretchHorizontal + RptB->BendingStretchHorizontal);
                    break;
                default:
                    ConstraintPower = 0.0f;
                    break;
                }

                if (ConstraintPower > 0.0f)
                {
                    var Displacement = Direction.normalized * (Force * ConstraintPower);

                    float WightAB = WeightA + WeightB;
                    RWptA->Position += Displacement * WeightA / WightAB;
                    RWptB->Position -= Displacement * WeightB / WightAB;
                }

                if (constraint->IsCollision == 0) return;
                if (!IsCollision) return;

                float Friction = 0.0f;
                for (int i = 0; i < ColliderCount; ++i)
                {
                    Collider* pCollider = pColliders + i;
                    ColliderEx* pColliderEx = pColliderExs + i;
                    if (pColliderEx->Enabled == 0)
                        continue;

                    if (pCollider->Height > EPSILON)
                    {
                        if (PushoutFromCapsule(pCollider, pColliderEx, ref RWptA->Position))
                        {
                            Friction = Mathf.Max(Friction, pCollider->Friction * 0.25f);
                        }
                        if (PushoutFromCapsule(pCollider, pColliderEx, ref RWptB->Position))
                        {
                            Friction = Mathf.Max(Friction, pCollider->Friction * 0.25f);
                        }
                    }
                    else
                    {
                        if (PushoutFromSphere(pCollider, pColliderEx, ref RWptA->Position))
                        {
                            Friction = Mathf.Max(Friction, pCollider->Friction * 0.25f);
                        }
                        if (PushoutFromSphere(pCollider, pColliderEx, ref RWptB->Position))
                        {
                            Friction = Mathf.Max(Friction, pCollider->Friction * 0.25f);
                        }
                    }

                    float Radius;
                    Vector3 pointOnLine, pointOnCollider;
                    if (CollisionDetection(pCollider, pColliderEx, RWptA->Position, RWptB->Position, out pointOnLine, out pointOnCollider, out Radius))
                    {
                        var Pushout = pointOnLine - pointOnCollider;
                        var PushoutDistance = Pushout.magnitude;

                        var pointDistance = (RWptB->Position - RWptA->Position).magnitude * 0.5f;
                        var rateP1 = Mathf.Clamp01((pointOnLine - RWptA->Position).magnitude / pointDistance);
                        var rateP2 = Mathf.Clamp01((pointOnLine - RWptB->Position).magnitude / pointDistance);

                        Pushout /= PushoutDistance;
#if true
                        var PushoutForce = Mathf.Max(Radius - PushoutDistance, 0.0f);
                        var PushoutRateR = PushoutForce * 0.5f;
                        var PushoutRateV = (PushoutForce - PushoutRateR) * 0.5f;
                        var PushoutR = Pushout * PushoutRateR;
                        var PushoutF = Pushout * PushoutRateV;
                        if (WeightA > EPSILON)
                        {
                            RWptA->Position += PushoutR * rateP2;
                            RWptA->PushOutForce += PushoutF * rateP2;
                        }
                        if (WeightB > EPSILON)
                        {
                            RWptB->Position += PushoutR * rateP1;
                            RWptB->PushOutForce += PushoutF * rateP1;
                        }
#else
                        Pushout *= Mathf.Max(Radius - PushoutDistance, 0.0f);
                        if (WeightA > EPSILON)
                            RWptA->Position += Pushout * rateP2;
                        if (WeightB > EPSILON)
                            RWptB->Position += Pushout * rateP1;
#endif

                        var Dot = Vector3.Dot(Vector3.up, (pointOnLine - pointOnCollider).normalized);
                        Friction = Mathf.Max(Friction, pCollider->Friction * Mathf.Clamp01(Dot));
                    }
                }

                RWptA->Friction = Mathf.Max(Friction, RWptA->Friction);
                RWptB->Friction = Mathf.Max(Friction, RWptB->Friction);
            }

            bool PushoutFromSphere(Vector3 Center, float Radius, ref Vector3 point)
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

            bool PushoutFromSphere(Collider* pCollider, ColliderEx* pColliderEx, ref Vector3 point)
            {
                return PushoutFromSphere(pColliderEx->Position, pCollider->Radius, ref point);
            }

            bool PushoutFromCapsule(Collider* pCollider, ColliderEx* pColliderEx, ref Vector3 point)
            {
                var capsuleVec = pColliderEx->Direction;
                var capsuleVecNormal = capsuleVec.normalized;
                var capsulePos = pColliderEx->Position;
                var targetVec = point - capsulePos;
                var distanceOnVec = Vector3.Dot(capsuleVecNormal, targetVec);
                if (distanceOnVec <= EPSILON)
                {
                    return PushoutFromSphere(capsulePos, pCollider->Radius, ref point);
                }
                else if (distanceOnVec >= pCollider->Height)
                {
                    return PushoutFromSphere(capsulePos + capsuleVec, pCollider->Radius * pCollider->RadiusTailScale, ref point);
                }
                else
                {
                    var positionOnVec = capsulePos + (capsuleVecNormal * distanceOnVec);
                    var pushoutVec = point - positionOnVec;
                    var sqrPushoutDistance = pushoutVec.sqrMagnitude;
                    if (sqrPushoutDistance > EPSILON)
                    {
                        var Radius = pCollider->Radius * Mathf.Lerp(1.0f, pCollider->RadiusTailScale, distanceOnVec / pCollider->Height);
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

            bool CollisionDetection_Sphere(Vector3 center, float radius, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
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

            bool CollisionDetection_Capsule(Collider* pCollider, ColliderEx* pColliderEx, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
            {
                var capsuleDir = pColliderEx->Direction;
                var capsulePos = pColliderEx->Position;

                if (CollisionDetection_Sphere(capsulePos, pCollider->Radius, point1, point2, out pointOnLine, out pointOnCollider, out Radius))
                    return true;
                if (CollisionDetection_Sphere(capsulePos + capsuleDir, pCollider->Radius * pCollider->RadiusTailScale, point1, point2, out pointOnLine, out pointOnCollider, out Radius))
                    return true;

                var pointDir = point2 - point1;

                float t1, t2;
                var sqrDistance = ComputeNearestPoints(capsulePos, capsuleDir, point1, pointDir, out t1, out t2, out pointOnCollider, out pointOnLine);
                t1 = Mathf.Clamp01(t1);
                Radius = pCollider->Radius * Mathf.Lerp(1.0f, pCollider->RadiusTailScale, t1);

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

            bool CollisionDetection(Collider* pCollider, ColliderEx* pColliderEx, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
            {
                if (pCollider->Height <= EPSILON)
                {
                    return CollisionDetection_Sphere(pColliderEx->Position, pCollider->Radius, point1, point2, out pointOnLine, out pointOnCollider, out Radius);
                }
                else
                {
                    return CollisionDetection_Capsule(pCollider, pColliderEx, point1, point2, out pointOnLine, out pointOnCollider, out Radius);
                }
            }

            float ComputeNearestPoints(Vector3 posP, Vector3 dirP, Vector3 posQ, Vector3 dirQ, out float tP, out float tQ, out Vector3 pointOnP, out Vector3 pointOnQ)
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

        //#if ENABLE_JOBSYSTEM
        //#if ENABLE_BURST
        //        [Unity.Burst.BurstCompile]
        //#endif//ENABLE_BURST
        //        struct JobMovingCollisionPoint : IJobParallelFor
        //#else//ENABLE_JOBSYSTEM
        //        struct JobMovingCollisionPoint : JobParallelFor
        //#endif//ENABLE_JOBSYSTEM
        //        {
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly, NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public PointRead* pRPoints;
        //#if ENABLE_JOBSYSTEM
        //            [NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public PointReadWrite* pRWPoints;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly, NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public Collider* pColliders;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly, NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public ColliderEx* pColliderExs;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly]
        //#endif//ENABLE_JOBSYSTEM
        //            public int ColliderCount;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly]
        //#endif//ENABLE_JOBSYSTEM
        //            public int DivideMax;
        //
        //#if ENABLE_JOBSYSTEM
        //            void IJobParallelFor.Execute(int index)
        //#else//ENABLE_JOBSYSTEM
        //            public void Execute(int index)
        //#endif//ENABLE_JOBSYSTEM
        //            {
        //                var pR = pRPoints + index;
        //                var pRW = pRWPoints + index;
        //
        //                if (pR->Weight <= EPSILON)
        //                {
        //                    return;
        //                }
        //
        //                var ray = new Ray();
        //
        //                for (int i = 0; i < ColliderCount; ++i)
        //                {
        //                    Collider* pCollider = pColliders + i;
        //                    ColliderEx* pColliderEx = pColliderExs + i;
        //
        //                    if (pColliderEx->Enabled == 0)
        //                        continue;
        //
        //                    var Point0 = pColliderEx->WorldToLocal.MultiplyPoint3x4(pRW->OriginalOldPosition);
        //                    var Point1 = pColliderEx->WorldToLocal.MultiplyPoint3x4(pRW->Position);
        //                    var Direction = Point1 - Point0;
        //                    var SqrDistance = Direction.sqrMagnitude;
        //
        //                    bool NeedsCheck = false;
        //                    if (SqrDistance < EPSILON)
        //                    {
        //                        NeedsCheck = pColliderEx->LocalBounds.Contains(Point0);
        //                    }
        //                    else
        //                    {
        //                        ray.origin = Point0;
        //                        ray.direction = Direction;
        //                        if (pColliderEx->LocalBounds.IntersectRay(ray, out float IntersectDistance))
        //                        {
        //                            NeedsCheck = (IntersectDistance * IntersectDistance < SqrDistance);
        //                        }
        //                    }
        //
        //                    if (NeedsCheck)
        //                    {
        //                        if (pCollider->Height <= EPSILON)
        //                        {
        //                            ResolveSphereCollision(pCollider, pColliderEx, pRW);
        //                        }
        //                        else
        //                        {
        //                            ResolveCapsuleCollision(pCollider, pColliderEx, pRW);
        //                        }
        //                    }
        //                }
        //            }
        //
        //            bool CheckSphereCollisionOffset(Vector3 Point, Vector3 PrevPoint, Vector3 SphereCenter, float Radius, Vector3 ColliderMove, Vector3 PointMove, ref bool IsCatch, ref Vector3 Offset)
        //            {
        //                var CenterToPoint = Point - SphereCenter;
        //                float Distance = CenterToPoint.magnitude;
        //                if (Distance < Radius)
        //                {
        //                    if (ColliderMove.sqrMagnitude <= EPSILON)
        //                    {
        //                        IsCatch = false;
        //                    }
        //                    else if (PointMove.sqrMagnitude <= EPSILON)
        //                    {
        //                        IsCatch = true;
        //                    }
        //                    else
        //                    {
        //                        IsCatch = Vector3.Dot(ColliderMove, PrevPoint - SphereCenter) > 0.0f;
        //                    }
        //
        //                    // Push out for direction
        //                    var Direction = IsCatch ? ColliderMove : (-1.0f * PointMove);
        //                    float a = Vector3.Dot(Direction, Direction);
        //                    float b = Vector3.Dot(CenterToPoint, Direction) * 2.0f;
        //                    float c = Vector3.Dot(CenterToPoint, CenterToPoint) - Radius * Radius;
        //                    float D = b * b - 4.0f * a * c;
        //                    if (D > 0.0f)
        //                    {
        //                        float x = (-b + Mathf.Sqrt(D)) / (2.0f * a);
        //                        Offset = Direction * x + CenterToPoint;
        //                        return true;
        //                    }
        //                }
        //                Offset = Vector3.zero;
        //                return false;
        //            }
        //
        //            void ResolveSphereCollision(Collider* pCollider, ColliderEx* pColliderEx, PointReadWrite* pRW)
        //            {
        //                var Point0 = pRW->OriginalOldPosition;
        //                var Point1 = pRW->Position;
        //                var PointMove = Point1 - Point0;
        //
        //                var ColliderPoint0 = pColliderEx->OldPosition;
        //                var ColliderPoint1 = pColliderEx->Position;
        //                var ColliderMove = ColliderPoint1 - ColliderPoint0;
        //
        //                var Radius = pCollider->Radius;
        //
        //                var PrevPoint = Point0;
        //                var Offset = Vector3.zero;
        //
        //                int Iteration = System.Math.Max(DivideMax, (int)Mathf.Ceil(ColliderMove.magnitude / Radius));
        //                for (int i = 1; i <= Iteration; i++)
        //                {
        //                    float t = (float)i / Iteration;
        //                    var Point = Vector3.Lerp(Point0, Point1, t);
        //                    var Sphere = Vector3.Lerp(ColliderPoint0, ColliderPoint1, t);
        //
        //                    bool IsCatch = false;
        //                    if (CheckSphereCollisionOffset(Point, PrevPoint, Sphere, Radius, ColliderMove, PointMove, ref IsCatch, ref Offset))
        //                    {
        //                        pRW->Position = ColliderPoint1 + Offset;
        //                        break;
        //                    }
        //
        //                    PrevPoint = Point;
        //                }
        //            }
        //
        //            void ResolveCapsuleCollision(Collider* pCollider, ColliderEx* pColliderEx, PointReadWrite* pRW)
        //            {
        //                var Point0 = pRW->OriginalOldPosition;
        //                var Point1 = pRW->Position;
        //                var PointMove = Point1 - Point0;
        //
        //                var ColliderHead0 = pColliderEx->OldPosition;
        //                var ColliderHead1 = pColliderEx->Position;
        //                var ColliderTail0 = pColliderEx->OldPosition + pColliderEx->OldDirection;
        //                var ColliderTail1 = pColliderEx->Position + pColliderEx->Direction;
        //                var ColliderDir0 = pColliderEx->OldDirection;
        //                var ColliderDir1 = pColliderEx->Direction;
        //
        //                var HeadMove = ColliderHead1 - ColliderHead0;
        //                var TailMove = ColliderTail1 - ColliderTail0;
        //
        //                var Radius = pCollider->Radius;
        //
        //                var PrevPoint = Point0;
        //                var Offset = Vector3.zero;
        //
        //                int HeadSteps = (int)Mathf.Ceil(HeadMove.magnitude / Radius);
        //                int TailSteps = (int)Mathf.Ceil(TailMove.magnitude / Radius);
        //
        //                int Iteration = System.Math.Max(DivideMax, System.Math.Max(HeadSteps, TailSteps));
        //                for (int i = 1; i <= Iteration; i++)
        //                {
        //                    float t = (float)i / Iteration;
        //                    var Point = Vector3.Lerp(Point0, Point1, t);
        //                    var Head = Vector3.Lerp(ColliderHead0, ColliderHead1, t);
        //                    var Dir = Vector3.Lerp(ColliderDir0, ColliderDir1, t);
        //                    var HeadToPoint = Point - Head;
        //
        //                    float w = Mathf.Clamp01(Vector3.Dot(Dir, HeadToPoint) / HeadToPoint.sqrMagnitude);
        //                    var ColliderPoint = Head + Dir * w;
        //                    var ColliderMove = Vector3.Lerp(HeadMove, TailMove, w);
        //
        //                    bool IsCatch = false;
        //                    if (CheckSphereCollisionOffset(Point, PrevPoint, ColliderPoint, Radius, ColliderMove, PointMove, ref IsCatch, ref Offset))
        //                    {
        //                        ColliderPoint = ColliderHead1 + ColliderDir1 * w;
        //                        pRW->Position = ColliderPoint + Offset;
        //                        break;
        //                    }
        //
        //                    PrevPoint = Point;
        //                }
        //            }
        //        }

        //#if ENABLE_JOBSYSTEM
        //#if ENABLE_BURST
        //        [Unity.Burst.BurstCompile]
        //#endif//ENABLE_BURST
        //        struct JobCollisionPoint : IJobParallelFor
        //#else//ENABLE_JOBSYSTEM
        //        struct JobCollisionPoint : JobParallelFor
        //#endif//ENABLE_JOBSYSTEM
        //        {
        //#if ENABLE_JOBSYSTEM
        //            [NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public PointReadWrite* pRWPoints;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly, NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public Collider* pColliders;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly, NativeDisableUnsafePtrRestriction]
        //#endif//ENABLE_JOBSYSTEM
        //            public ColliderEx* pColliderExs;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly]
        //#endif//ENABLE_JOBSYSTEM
        //            public int ColliderCount;
        //#if ENABLE_JOBSYSTEM
        //            [ReadOnly]
        //#endif//ENABLE_JOBSYSTEM
        //            public bool IsEnableCollider;
        //
        //#if ENABLE_JOBSYSTEM
        //            void IJobParallelFor.Execute(int index)
        //#else//ENABLE_JOBSYSTEM
        //            public void Execute(int index)
        //#endif//ENABLE_JOBSYSTEM
        //            {
        //                var pRW = pRWPoints + index;
        //
        //                if (IsEnableCollider)
        //                {
        //                    for (int i = 0; i < ColliderCount; ++i)
        //                    {
        //                        Collider* pCollider = pColliders + i;
        //                        ColliderEx* pColliderEx = pColliderExs + i;
        //
        //                        if (pColliderEx->Enabled == 0)
        //                            continue;
        //
        //                        Vector3 point = pRW->Position;
        //
        //                        if (pCollider->Height <= 0.0f)
        //                        {
        //                            if (PushoutFromSphere(pCollider, pColliderEx, ref point))
        //                            {
        //                                pRW->Friction = Mathf.Max(pRW->Friction, pCollider->Friction * 0.5f);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (PushoutFromCapsule(pCollider, pColliderEx, ref point))
        //                            {
        //                                pRW->Friction = Mathf.Max(pRW->Friction, pCollider->Friction * 0.5f);
        //                            }
        //                        }
        //
        //                        pRW->Position += point - pRW->Position;
        //                    }
        //                }
        //            }
        //
        //            bool PushoutFromSphere(Vector3 Center, float Radius, ref Vector3 point)
        //            {
        //                var direction = point - Center;
        //                var sqrDirectionLength = direction.sqrMagnitude;
        //                var radius = Radius;
        //                if (sqrDirectionLength > EPSILON)
        //                {
        //                    if (sqrDirectionLength < radius * radius)
        //                    {
        //                        var directionLength = Mathf.Sqrt(sqrDirectionLength);
        //                        point = Center + direction * radius / directionLength;
        //                        return true;
        //                    }
        //                }
        //                return false;
        //            }
        //
        //            bool PushoutFromSphere(Collider* pCollider, ColliderEx* pColliderEx, ref Vector3 point)
        //            {
        //                return PushoutFromSphere(pColliderEx->Position, pCollider->Radius, ref point);
        //            }
        //
        //            bool PushoutFromCapsule(Collider* pCollider, ColliderEx* pColliderEx, ref Vector3 point)
        //            {
        //                var capsuleVec = pColliderEx->Direction;
        //                var capsuleVecNormal = capsuleVec.normalized;
        //                var capsulePos = pColliderEx->Position;
        //                var targetVec = point - capsulePos;
        //                var distanceOnVec = Vector3.Dot(capsuleVecNormal, targetVec);
        //                if (distanceOnVec <= EPSILON)
        //                {
        //                    return PushoutFromSphere(capsulePos, pCollider->Radius, ref point);
        //                }
        //                else if (distanceOnVec >= pCollider->Height)
        //                {
        //                    return PushoutFromSphere(capsulePos + capsuleVec, pCollider->Radius, ref point);
        //                }
        //                else
        //                {
        //                    var positionOnVec = capsulePos + (capsuleVecNormal * distanceOnVec);
        //                    var pushoutVec = point - positionOnVec;
        //                    var sqrPushoutDistance = pushoutVec.sqrMagnitude;
        //                    if (sqrPushoutDistance > EPSILON)
        //                    {
        //                        var Radius = pCollider->Radius;
        //                        if (sqrPushoutDistance < Radius * Radius)
        //                        {
        //                            var pushoutDistance = Mathf.Sqrt(sqrPushoutDistance);
        //                            point = positionOnVec + pushoutVec * Radius / pushoutDistance;
        //                            return true;
        //                        }
        //                    }
        //                    return false;
        //                }
        //            }
        //        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobTransformInitialize : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
        struct JobTransformInitialize : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointRead* pRPoints;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var pR = pRPoints + index;

                transform.localScale = pR->InitialLocalScale;
                transform.localRotation = pR->InitialLocalRotation;
                transform.localPosition = pR->InitialLocalPosition;
            }
        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobCaptureCurrentTransformPositionFromTransform : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
        struct JobCaptureCurrentTransformPositionFromTransform : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointReadWrite* pRWPoints;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                pRWPoints[index].CurrentTransformPosition = transform.position;
            }
        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobCaptureCurrentTransformPosition : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
        struct JobCaptureCurrentTransformPosition : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointReadWrite* pRWPoints;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                pRWPoints[index].CurrentTransformPosition = pRWPoints[index].Position;
            }
        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobCaptureTransformPosition : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
        struct JobCaptureTransformPosition : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public Vector3* pPositions;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                pPositions[index] = transform.position;
            }
        }

#if ENABLE_JOBSYSTEM
#if ENABLE_BURST
        [Unity.Burst.BurstCompile]
#endif//ENABLE_BURST
        struct JobApplySimlationResultToTransform : IJobParallelForTransform
#else//ENABLE_JOBSYSTEM
        struct JobApplySimlationResultToTransform : JobParallelForTransform
#endif//ENABLE_JOBSYSTEM
        {
#if ENABLE_JOBSYSTEM
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointRead* pRPoints;
#if ENABLE_JOBSYSTEM
            [NativeDisableUnsafePtrRestriction]
#endif//ENABLE_JOBSYSTEM
            public PointReadWrite* pRWPoints;
#if ENABLE_JOBSYSTEM
            [ReadOnly]
#endif//ENABLE_JOBSYSTEM
            public AngleLimitConfig angleLockConfig;

#if ENABLE_JOBSYSTEM
            void IJobParallelForTransform.Execute(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            public void Execute(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var pRW = pRWPoints + index;
                var pR = pRPoints + index;

                if (pR->Weight >= EPSILON)
                {
                    var pRWP = pRWPoints + pR->Parent;
                    var Direction = pRW->FinalPosition - pRWP->FinalPosition;
                    var RealLength = Direction.magnitude;
                    if (RealLength > EPSILON)
                    {
                        pRW->PreviousDirection = Direction;
                        transform.position = pRW->FinalPosition;
                        SetRotation(index, transform);
                    }
                    else
                    {
                        pRW->FinalPosition = pRWP->FinalPosition + pRW->PreviousDirection;
                    }
                }
                else
                {
                    pRW->FinalPosition = transform.position;
                    SetRotation(index, transform);
                }

                LockAngle(pR, pRW);
            }

#if ENABLE_JOBSYSTEM
            void SetRotation(int index, TransformAccess transform)
#else//ENABLE_JOBSYSTEM
            void SetRotation(int index, Transform transform)
#endif//ENABLE_JOBSYSTEM
            {
                var pR = pRPoints + index;
                var pRW = pRWPoints + index;

                transform.localRotation = pR->LocalRotation;
                if (pR->Child != -1)
                {
                    var pRWC = pRWPoints + pR->Child;
                    var Direction = pRWC->FinalPosition - pRW->FinalPosition;
                    if (Direction.sqrMagnitude > EPSILON)
                    {
                        Matrix4x4 mRotate = Matrix4x4.Rotate(transform.rotation);
                        Vector3 AimVector = mRotate * pR->BoneAxis;
                        Quaternion AimRotation = Quaternion.FromToRotation(AimVector, Direction);
                        transform.rotation = AimRotation * transform.rotation;
                    }
                }
            }

            void LockAngle(PointRead* pR, PointReadWrite* pRW)
            {
                if (pR->Parent == -1 || angleLockConfig.angleLimit == -1)
                    return;

                var pRp = pRPoints + pR->Parent;
                var pRWP = pRWPoints + pR->Parent;
                Vector3 superParentpos = (pRp->Parent != -1 ? (pRWPoints + pRp->Parent)->Position : pRWP->Position);

                Vector3 boneDir = pRW->Position - pRWP->Position;
                Vector3 parentBoneDir = pRWP->Position - superParentpos;

                if (parentBoneDir.magnitude == 0 || angleLockConfig.limitFromRoot)
                {
                    parentBoneDir = pRW->CurrentTransformPosition - pRWP->CurrentTransformPosition;
                }

                float angle = Vector3.Angle(parentBoneDir, boneDir);
                float remainingAngle = angle - angleLockConfig.angleLimit;

                if (remainingAngle > 0.0f)
                {
                    Vector3 axis = Vector3.Cross(parentBoneDir, boneDir);
                    pRW->Position = pRWP->Position + Quaternion.AngleAxis(-remainingAngle, axis) * boneDir;
                }
            }
        }
    }
}
