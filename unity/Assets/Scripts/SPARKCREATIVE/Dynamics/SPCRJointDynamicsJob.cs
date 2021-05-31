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
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe class SPCRJointDynamicsJob
{
    const float EPSILON = 0.001f;

    public struct Point
    {
        public int Parent;
        public int Child;
        public float Weight;
        public float Mass;
        public float Resistance;
        public float Hardness;
        public float FrictionScale;
        public float SliderJointLength;
        public float SliderJointSpring;
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
        public float LimitPower;
        public Vector3 Gravity;
        public Vector3 BoneAxis;
        public Vector3 InitialPosition;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 Position;
        public Vector3 OldPosition;
        public Vector3 PreviousDirection;
    }

    struct PointRead
    {
        public int Parent;
        public int Child;
        public float Weight;
        public float Mass;
        public float Resistance;
        public float Hardness;
        public float FrictionScale;
        public float SliderJointLength;
        public float SliderJointSpring;
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
        public float LimitPower;
        public Vector3 Gravity;
        public Vector3 BoneAxis;
        public Vector3 InitialPosition;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 InitialWorldPosition;
    }

    struct PointReadWrite
    {
        public Vector3 Position;
        public Vector3 TargetDisplacement;
        public Vector3 OldPosition;
        public Vector3 OriginalOldPosition;
        public Vector3 PreviousDirection;
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
        public float StretchLength;
        public float Shrink;
        public float Stretch;
    }

    struct Collider
    {
        public float RadiusHead;
        public float RadiusTail;
        public float Height;
        public float Friction;
        public float PushOutRate;
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
    }

    struct Grabber
    {
        public float Radius;
        public float Force;
    }

    struct GrabberEx
    {
        public int IsEnabled;
        public Vector3 Position;
    }

    public struct AngleLimitConfig
    {
        public float angleLimit;
        public bool limitFromRoot;
    }

    Transform _RootBone;
    Vector3 _OldRootPosition;
    Vector3 _OldRootScale;
    Quaternion _OldRootRotation;
    int _PointCount;
    NativeArray<PointRead> _PointsR;
    NativeArray<PointReadWrite> _PointsRW;
    NativeArray<Constraint>[] _Constraints;
    Transform[] _PointTransforms;
    TransformAccessArray _TransformArray;
    SPCRJointDynamicsCollider[] _RefColliders;
    NativeArray<Collider> _Colliders;
    NativeArray<ColliderEx> _ColliderExs;
    SPCRJointDynamicsPointGrabber[] _RefGrabbers;
    NativeArray<Grabber> _Grabbers;
    NativeArray<GrabberEx> _GrabberExs;
    JobHandle _hJob = default(JobHandle);

    public void Initialize(Transform RootBone, Point[] Points, Transform[] PointTransforms, Constraint[][] Constraints, SPCRJointDynamicsCollider[] Colliders, SPCRJointDynamicsPointGrabber[] Grabbers)
    {
        _RootBone = RootBone;
        _PointCount = Points.Length;
        _OldRootPosition = _RootBone.position;
        _OldRootRotation = _RootBone.rotation;
        _OldRootScale = _RootBone.lossyScale;

        _PointTransforms = new Transform[_PointCount];
        for (int i = 0; i < _PointCount; ++i)
        {
            _PointTransforms[i] = PointTransforms[i];
        }

        var PointsR = new PointRead[_PointCount];
        var PointsRW = new PointReadWrite[_PointCount];
        for (int i = 0; i < Points.Length; ++i)
        {
            var src = Points[i];
            PointsR[i].Parent = src.Parent;
            PointsR[i].Child = src.Child;
            PointsR[i].Weight = src.Weight;
            PointsR[i].Mass = src.Mass;
            PointsR[i].Resistance = src.Resistance;
            PointsR[i].Hardness = src.Hardness;
            PointsR[i].FrictionScale = src.FrictionScale;
            PointsR[i].SliderJointLength = src.SliderJointLength;
            PointsR[i].SliderJointSpring = src.SliderJointSpring;
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
            PointsR[i].LimitPower = src.LimitPower;
            PointsR[i].Gravity = src.Gravity;
            PointsR[i].BoneAxis = src.BoneAxis;
            PointsR[i].LocalPosition = src.LocalPosition;
            PointsR[i].LocalRotation = src.LocalRotation;
            PointsR[i].InitialPosition = src.InitialPosition;
            PointsR[i].InitialWorldPosition = src.Position;
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

        _TransformArray = new TransformAccessArray(_PointTransforms);

        _Constraints = new NativeArray<Constraint>[Constraints.Length];
        for (int i = 0; i < Constraints.Length; ++i)
        {
            var src = Constraints[i];
            _Constraints[i] = new NativeArray<Constraint>(src.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _Constraints[i].CopyFrom(src);
        }

        _RefColliders = Colliders;
        var ColliderR = new Collider[Colliders.Length];
        var ColliderExR = new ColliderEx[Colliders.Length];
        for (int i = 0; i < Colliders.Length; ++i)
        {
            var src = Colliders[i];
            if (src.IsCapsule)
            {
                ColliderR[i].RadiusHead = src.RadiusHead;
                ColliderR[i].RadiusTail = src.RadiusTail;
                ColliderR[i].Height = src.Height;
            }
            else
            {
                ColliderR[i].RadiusHead = src.RadiusHead;
                ColliderR[i].Height = 0.0f;
            }
            ColliderR[i].Friction = src.Friction;
            ColliderR[i].PushOutRate = src.PushOutRate;
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
            GrabberR[i].Radius = Grabbers[i].Radius;
            GrabberR[i].Force = Grabbers[i].Force;
        }
        _Grabbers = new NativeArray<Grabber>(Grabbers.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        _Grabbers.CopyFrom(GrabberR);
        _GrabberExs = new NativeArray<GrabberEx>(Grabbers.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        _hJob = default(JobHandle);
    }

    public void Uninitialize()
    {
        WaitForComplete();
        _hJob = default(JobHandle);

        _GrabberExs.Dispose();
        _Grabbers.Dispose();
        _ColliderExs.Dispose();
        _Colliders.Dispose();
        for (int i = 0; i < _Constraints.Length; ++i)
        {
            _Constraints[i].Dispose();
        }
        _TransformArray.Dispose();
        _PointsR.Dispose();
        _PointsRW.Dispose();
    }

    public void Reset()
    {
        var pPointRW = (PointReadWrite*)_PointsRW.GetUnsafePtr();
        for (int i = 0; i < _PointCount; ++i)
        {
            pPointRW[i].OldPosition = pPointRW[i].Position = _PointTransforms[i].position;
        }

        var pColloderExs = (ColliderEx*)_ColliderExs.GetUnsafePtr();
        for (int i = 0; i < _RefColliders.Length; ++i)
        {
            var Src = _RefColliders[i];
            pColloderExs[i].Position = pColloderExs[i].OldPosition = Src.transform.position;
            pColloderExs[i].Direction = pColloderExs[i].OldDirection = Src.transform.rotation * Vector3.up * Src.Height;
        }

        _OldRootPosition = _RootBone.position;
        _OldRootRotation = _RootBone.rotation;
        _OldRootScale = _RootBone.lossyScale;
    }

    public void Restore()
    {
        var pPointR = (PointRead*)_PointsR.GetUnsafePtr();
        var pPointRW = (PointReadWrite*)_PointsRW.GetUnsafePtr();
        for (int i = 0; i < _PointCount; ++i)
        {
            pPointRW[i].Position = _RootBone.TransformPoint(pPointR[i].InitialPosition);
            pPointRW[i].OldPosition = pPointRW[i].Position;
            _PointTransforms[i].position = pPointRW[i].Position;
        }

        var pColloderExs = (ColliderEx*)_ColliderExs.GetUnsafePtr();
        for (int i = 0; i < _RefColliders.Length; ++i)
        {
            var Src = _RefColliders[i];
            pColloderExs[i].Position = pColloderExs[i].OldPosition = Src.transform.position;
            pColloderExs[i].Direction = pColloderExs[i].OldDirection = Src.transform.rotation * Vector3.up * Src.Height;
        }

        _OldRootPosition = _RootBone.position;
        _OldRootRotation = _RootBone.rotation;
        _OldRootScale = _RootBone.lossyScale;
    }

    public void Execute(
        Transform RootTransform, float RootSlideLimit, float RootRotateLimit,
        float StepTime, int SubSteps,
        Vector3 WindForce,
        int Relaxation, float SpringK,
        bool IsEnableFloorCollision, float FloorHeight,
        int DetailHitDivideMax,
        bool IsEnableColliderCollision,
        AngleLimitConfig angleLockConfig)
    {
        bool IsPaused = StepTime <= 0.0f;
        if (IsPaused)
        {
            SubSteps = 1;
        }

        WaitForComplete();

        var RootPosition = RootTransform.position;
        var RootRotation = RootTransform.rotation;
        var RootScale = RootTransform.lossyScale;

        var RootSlide = RootPosition - _OldRootPosition;
        var SystemOffset = Vector3.zero;
        float SlideLength = RootSlide.magnitude;
        if (RootSlideLimit >= 0.0f && SlideLength > RootSlideLimit)
        {
            SystemOffset = RootSlide * (1.0f - RootSlideLimit / SlideLength);
            SystemOffset /= SubSteps;
        }

        var RootDeltaRotation = RootRotation * Quaternion.Inverse(_OldRootRotation);
        float RotateAngle = Mathf.Acos(RootDeltaRotation.w) * 2.0f * Mathf.Rad2Deg;
        Quaternion SystemRotation = Quaternion.identity;
        if (RootRotateLimit >= 0.0f && Mathf.Abs(RotateAngle) > RootRotateLimit)
        {
            Vector3 RotateAxis = Vector3.zero;
            RootDeltaRotation.ToAngleAxis(out RotateAngle, out RotateAxis);
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
        var pColliders = (Collider*)_Colliders.GetUnsafePtr();
        var pColliderExs = (ColliderEx*)_ColliderExs.GetUnsafePtr();
        var pGrabbers = (Grabber*)_Grabbers.GetUnsafePtr();
        var pGrabberExs = (GrabberEx*)_GrabberExs.GetUnsafePtr();

        int ColliderCount = _RefColliders.Length;
        Bounds TempBounds = new Bounds();

        int GrabberCount = _RefGrabbers.Length;
        for (int i = 0; i < GrabberCount; ++i)
        {
            var pDst = pGrabberExs + i;

            pDst->IsEnabled = _RefGrabbers[i].IsEnabled ? 1 : 0;
            pDst->Position = _RefGrabbers[i].RefTransform.position;
        }

        var CalculateDisplacement = new JobCalculateDisplacement();
        CalculateDisplacement.pRWPoints = pRWPoints;
        _hJob = CalculateDisplacement.Schedule(_PointCount, 8, _hJob);

        float DeltaStepMulDeltaRelax = (1.0f / SubSteps) * (1.0f / Relaxation);
        StepTime /= SubSteps;

        for (int iSubStep = 1; iSubStep <= SubSteps; iSubStep++)
        {
            float SubDelta = (float)iSubStep / SubSteps;

            float ColliderDelta = 1.0f / (SubSteps - iSubStep + 1.0f);
            for (int i = 0; i < ColliderCount; ++i)
            {
                var pDst = pColliderExs + i;
                var Src = _RefColliders[i];

                if (iSubStep == 1)
                {
                    var SrcT = Src.RefTransform;
                    if (Src.Height <= EPSILON)
                    {
                        pDst->SourcePosition = SrcT.position;
                    }
                    else
                    {
                        pDst->SourceDirection = SrcT.rotation * Vector3.up * Src.Height;
                        pDst->SourcePosition = SrcT.position - (pDst->Direction * 0.5f);
                    }

                    pDst->WorldToLocal = SrcT.worldToLocalMatrix;
                }

                pDst->OldPosition = pDst->Position;
                pDst->OldDirection = pDst->Direction;

                pDst->Position = Vector3.Lerp(pDst->OldPosition, pDst->SourcePosition, ColliderDelta);
                pDst->Direction = Vector3.Lerp(pDst->OldDirection, pDst->SourceDirection, ColliderDelta);

                Vector3 Center;
                Vector3 Corner;
                // Head
                Corner = Vector3.one * Src.RadiusHead;
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
                    Corner = Vector3.one * Src.RadiusTail;
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
                Vector3.Lerp(_OldRootScale, RootScale, SubDelta)
            );

            var PointUpdate = new JobPointUpdate();
            PointUpdate.RootMatrix = RootMatrix;
            PointUpdate.OldRootPosition = _OldRootPosition;
            PointUpdate.GrabberCount = _RefGrabbers.Length;
            PointUpdate.pGrabbers = pGrabbers;
            PointUpdate.pGrabberExs = pGrabberExs;
            PointUpdate.pRPoints = pRPoints;
            PointUpdate.pRWPoints = pRWPoints;
            PointUpdate.WindForce = WindForce;
            PointUpdate.StepTime_x2_Half = StepTime * StepTime * 0.5f * SubSteps;
            PointUpdate.SystemOffset = SystemOffset;
            PointUpdate.SystemRotation = SystemRotation;
            PointUpdate.IsPaused = IsPaused;
            _hJob = PointUpdate.Schedule(_PointCount, 8, _hJob);

            if (!IsPaused)
            {
                if (IsEnableColliderCollision && DetailHitDivideMax > 0)
                {
                    var MovingCollisionPoint = new JobMovingCollisionPoint();
                    MovingCollisionPoint.pRPoints = pRPoints;
                    MovingCollisionPoint.pRWPoints = pRWPoints;
                    MovingCollisionPoint.pColliders = pColliders;
                    MovingCollisionPoint.pColliderExs = pColliderExs;
                    MovingCollisionPoint.ColliderCount = ColliderCount;
                    MovingCollisionPoint.DivideMax = DetailHitDivideMax;
                    _hJob = MovingCollisionPoint.Schedule(_PointCount, 8, _hJob);
                }

                for (int i = 0; i < Relaxation; ++i)
                {
                    foreach (var constraint in _Constraints)
                    {
                        var ConstraintUpdate = new JobConstraintUpdate();
                        ConstraintUpdate.pConstraints = (Constraint*)constraint.GetUnsafePtr();
                        ConstraintUpdate.pRPoints = pRPoints;
                        ConstraintUpdate.pRWPoints = pRWPoints;
                        ConstraintUpdate.pColliders = pColliders;
                        ConstraintUpdate.pColliderExs = pColliderExs;
                        ConstraintUpdate.ColliderCount = ColliderCount;
                        ConstraintUpdate.DeltaSubstepMulDeltaRelax = DeltaStepMulDeltaRelax;
                        ConstraintUpdate.SpringK = SpringK;
                        _hJob = ConstraintUpdate.Schedule(constraint.Length, 8, _hJob);
                    }
                }

                if (IsEnableFloorCollision || IsEnableColliderCollision)
                {
                    var CollisionPoint = new JobCollisionPoint();
                    CollisionPoint.pRWPoints = pRWPoints;
                    CollisionPoint.pColliders = pColliders;
                    CollisionPoint.pColliderExs = pColliderExs;
                    CollisionPoint.ColliderCount = ColliderCount;
                    CollisionPoint.FloorHeight = FloorHeight;
                    CollisionPoint.IsEnableFloor = IsEnableFloorCollision;
                    CollisionPoint.IsEnableCollider = IsEnableColliderCollision;
                    _hJob = CollisionPoint.Schedule(_PointCount, 8, _hJob);
                }
            }

            var PointToTransform = new JobPointToTransform();
            PointToTransform.pRPoints = pRPoints;
            PointToTransform.pRWPoints = pRWPoints;
            PointToTransform.UpdateTransform = iSubStep == SubSteps;
            PointToTransform.angleLockConfig = angleLockConfig;
            _hJob = PointToTransform.Schedule(_TransformArray, _hJob);
        }

        _OldRootPosition = RootPosition;
        _OldRootRotation = RootRotation;
        _OldRootScale = RootScale;
    }

    public void WaitForComplete()
    {
        _hJob.Complete();
        _hJob = default(JobHandle);
    }

    public void DrawGizmos_Points()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < _PointCount; ++i)
        {
            Gizmos.DrawSphere(_PointsRW[i].Position, 0.005f);
        }
    }

    public void DrawGizmos_Constraints(int A, int B)
    {
        Gizmos.DrawLine(_PointTransforms[A].position, _PointTransforms[B].position);
    }

    public void DrawGizmos_ColliderEx()
    {
        var BoundsColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
        for (int i = 0; i < _ColliderExs.Length; i++)
        {
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

#if ENABLE_BURST
    [Unity.Burst.BurstCompile]
#endif
    struct JobCalculateDisplacement : IJobParallelFor
    {
        [NativeDisableUnsafePtrRestriction]
        public PointReadWrite* pRWPoints;
        public void Execute(int index)
        {
            var pRW = pRWPoints + index;
            pRW->TargetDisplacement = pRW->Position - pRW->OldPosition;
        }
    }

#if ENABLE_BURST
    [Unity.Burst.BurstCompile]
#endif
    struct JobPointUpdate : IJobParallelFor
    {
        [ReadOnly]
        public int GrabberCount;
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public Grabber* pGrabbers;
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public GrabberEx* pGrabberExs;
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public PointRead* pRPoints;
        [NativeDisableUnsafePtrRestriction]
        public PointReadWrite* pRWPoints;

        [ReadOnly]
        public Matrix4x4 RootMatrix;
        [ReadOnly]
        public Vector3 OldRootPosition;
        [ReadOnly]
        public Vector3 WindForce;
        [ReadOnly]
        public float StepTime_x2_Half;

        [ReadOnly]
        public Vector3 SystemOffset;
        [ReadOnly]
        public Quaternion SystemRotation;
        [ReadOnly]
        public bool IsPaused;

        private Vector3 ApplySystemTransform(Vector3 Point, Vector3 Pivot)
        {
            return SystemRotation * (Point - Pivot) + Pivot + SystemOffset;
        }

        void IJobParallelFor.Execute(int index)
        {
            var pR = pRPoints + index;
            var pRW = pRWPoints + index;

            if (pR->Weight <= EPSILON)
            {
                pRW->OriginalOldPosition = pRW->Position;
                pRW->OldPosition = ApplySystemTransform(pRW->Position, OldRootPosition);
                pRW->Position = RootMatrix.MultiplyPoint3x4(pR->InitialPosition);
                pRW->Friction = 0.0f;

                return;
            }

            pRW->OriginalOldPosition = pRW->Position;
            pRW->OldPosition = ApplySystemTransform(pRW->OldPosition, OldRootPosition);
            pRW->Position = ApplySystemTransform(pRW->Position, OldRootPosition);

            Vector3 Displacement = Vector3.zero;
            if (!IsPaused)
            {
                Vector3 Force = Vector3.zero;
                Force += pR->Gravity;
                Force += WindForce;
                Force *= StepTime_x2_Half;

                Displacement = pRW->TargetDisplacement;
                Displacement += Force / pR->Mass;
                Displacement *= pR->Resistance;
                Displacement *= 1.0f - (pRW->Friction * pR->FrictionScale);
            }

            pRW->OldPosition = pRW->Position;
            pRW->Position += Displacement;
            pRW->Friction = 0.0f;

            if (pR->Hardness > 0.0f)
            {
                var Target = RootMatrix.MultiplyPoint3x4(pR->InitialPosition);
                pRW->Position += (Target - pRW->Position) * pR->Hardness;
            }

            if (pRW->GrabberIndex != -1)
            {
                Grabber* pGR = pGrabbers + pRW->GrabberIndex;
                GrabberEx* pGRW = pGrabberExs + pRW->GrabberIndex;
                if (pGRW->IsEnabled == 0)
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

                    if (pGRW->IsEnabled == 0) continue;

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
    }

#if ENABLE_BURST
    [Unity.Burst.BurstCompile]
#endif
    struct JobConstraintUpdate : IJobParallelFor
    {
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public Constraint* pConstraints;

        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public PointRead* pRPoints;
        [NativeDisableUnsafePtrRestriction]
        public PointReadWrite* pRWPoints;

        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public Collider* pColliders;
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public ColliderEx* pColliderExs;
        [ReadOnly]
        public int ColliderCount;
        [ReadOnly]
        public float DeltaSubstepMulDeltaRelax;

        [ReadOnly]
        public float SpringK;

        void IJobParallelFor.Execute(int index)
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
            float Force = 0.0f;

            float LimitLength = constraint->StretchLength;
            switch (constraint->Type)
            {
            case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
            case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
            case SPCRJointDynamicsController.ConstraintType.Shear:
                LimitLength += (RptA->SliderJointLength + RptB->SliderJointLength) * 0.5f;
                break;
            }
            if (Distance <= constraint->Length)
            {
                Force = Distance - constraint->Length;
            }
            else
            {
                if (Distance < LimitLength)
                {
                    Force = LimitLength - Distance;
                    Force *= (RptA->SliderJointSpring + RptB->SliderJointSpring) * 0.5f;
                }
                else
                {
                    Force = Distance - LimitLength;
                }
            }

            Force *= SpringK * DeltaSubstepMulDeltaRelax;

            bool IsShrink = Force >= 0.0f;
            float ConstraintPower;
            switch (constraint->Type)
            {
            case SPCRJointDynamicsController.ConstraintType.Structural_Vertical:
                ConstraintPower = IsShrink
                    ? constraint->Shrink * (RptA->StructuralShrinkVertical + RptB->StructuralShrinkVertical)
                    : constraint->Stretch * (RptA->StructuralStretchVertical + RptB->StructuralStretchVertical);
                break;
            case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
                ConstraintPower = IsShrink
                    ? constraint->Shrink * (RptA->StructuralShrinkHorizontal + RptB->StructuralShrinkHorizontal)
                    : constraint->Stretch * (RptA->StructuralStretchHorizontal + RptB->StructuralStretchHorizontal);
                break;
            case SPCRJointDynamicsController.ConstraintType.Shear:
                ConstraintPower = IsShrink
                    ? constraint->Shrink * (RptA->ShearShrink + RptB->ShearShrink)
                    : constraint->Stretch * (RptA->ShearStretch + RptB->ShearStretch);
                break;
            case SPCRJointDynamicsController.ConstraintType.Bending_Vertical:
                ConstraintPower = IsShrink
                    ? constraint->Shrink * (RptA->BendingShrinkVertical + RptB->BendingShrinkVertical)
                    : constraint->Stretch * (RptA->BendingStretchVertical + RptB->BendingStretchVertical);
                break;
            case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
                ConstraintPower = IsShrink
                    ? constraint->Shrink * (RptA->BendingShrinkHorizontal + RptB->BendingShrinkHorizontal)
                    : constraint->Stretch * (RptA->BendingStretchHorizontal + RptB->BendingStretchHorizontal);
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

            float Friction = 0.0f;
            for (int i = 0; i < ColliderCount; ++i)
            {
                Collider* pCollider = pColliders + i;
                ColliderEx* pColliderEx = pColliderExs + i;

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
                    Pushout *= Mathf.Max(Radius - PushoutDistance, 0.0f);
                    RWptA->Position += Pushout * rateP2;
                    RWptB->Position += Pushout * rateP1;

                    var Dot = Vector3.Dot(Vector3.up, (pointOnLine - pointOnCollider).normalized);
                    Friction = Mathf.Max(Friction, pCollider->Friction * Mathf.Clamp01(Dot));
                }
            }

            RWptA->Friction = Mathf.Max(Friction, RWptA->Friction);
            RWptB->Friction = Mathf.Max(Friction, RWptB->Friction);
        }

        bool CollisionDetection(Collider* pCollider, ColliderEx* pColliderEx, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
        {
            if (pCollider->Height <= EPSILON)
            {
                var direction = point2 - point1;
                var directionLength = direction.magnitude;
                direction /= directionLength;

                var toCenter = pColliderEx->Position - point1;
                var dot = Vector3.Dot(direction, toCenter);
                var pointOnDirection = direction * Mathf.Clamp(dot, 0.0f, directionLength);

                pointOnCollider = pColliderEx->Position;
                pointOnLine = pointOnDirection + point1;
                Radius = pCollider->RadiusHead;

                if ((pointOnCollider - pointOnLine).sqrMagnitude > pCollider->RadiusHead * pCollider->RadiusHead)
                {
                    return false;
                }

                return true;
            }
            else
            {
                var capsuleDir = pColliderEx->Direction;
                var capsulePos = pColliderEx->Position;
                var pointDir = point2 - point1;

                float t1, t2;
                var sqrDistance = ComputeNearestPoints(capsulePos, capsuleDir, point1, pointDir, out t1, out t2, out pointOnCollider, out pointOnLine);
                t1 = Mathf.Clamp01(t1);
                Radius = pCollider->RadiusHead + (pCollider->RadiusTail - pCollider->RadiusHead) * t1;

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

#if ENABLE_BURST
    [Unity.Burst.BurstCompile]
#endif
    struct JobMovingCollisionPoint : IJobParallelFor
    {
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public PointRead* pRPoints;
        [NativeDisableUnsafePtrRestriction]
        public PointReadWrite* pRWPoints;

        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public Collider* pColliders;
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public ColliderEx* pColliderExs;
        [ReadOnly]
        public int ColliderCount;

        [ReadOnly]
        public int DivideMax;

        void IJobParallelFor.Execute(int index)
        {
            var pR = pRPoints + index;
            var pRW = pRWPoints + index;

            if (pR->Weight <= EPSILON)
            {
                return;
            }

            var ray = new Ray();

            for (int i = 0; i < ColliderCount; ++i)
            {
                Collider* pCollider = pColliders + i;
                ColliderEx* pColliderEx = pColliderExs + i;

                if (pCollider->PushOutRate < 1.0f)
                {
                    continue;
                }

                var Point0 = pColliderEx->WorldToLocal.MultiplyPoint3x4(pRW->OriginalOldPosition);
                var Point1 = pColliderEx->WorldToLocal.MultiplyPoint3x4(pRW->Position);
                var Direction = Point1 - Point0;
                var SqrDistance = Direction.sqrMagnitude;

                bool NeedsCheck = false;
                if (SqrDistance < EPSILON)
                {
                    NeedsCheck = pColliderEx->LocalBounds.Contains(Point0);
                }
                else
                {
                    ray.origin = Point0;
                    ray.direction = Direction;
                    if (pColliderEx->LocalBounds.IntersectRay(ray, out float IntersectDistance))
                    {
                        NeedsCheck = (IntersectDistance * IntersectDistance < SqrDistance);
                    }
                }

                if (NeedsCheck)
                {
                    if (pCollider->Height <= EPSILON)
                    {
                        ResolveSphereCollision(pCollider, pColliderEx, pRW);
                    }
                    else
                    {
                        ResolveCapsuleCollision(pCollider, pColliderEx, pRW);
                    }
                }
            }
        }

        bool CheckSphereCollisionOffset(Vector3 Point, Vector3 PrevPoint, Vector3 SphereCenter, float Radius, Vector3 ColliderMove, Vector3 PointMove, ref bool IsCatch, ref Vector3 Offset)
        {
            var CenterToPoint = Point - SphereCenter;
            float Distance = CenterToPoint.magnitude;
            if (Distance < Radius)
            {
                if (ColliderMove.sqrMagnitude <= EPSILON)
                {
                    IsCatch = false;
                }
                else if (PointMove.sqrMagnitude <= EPSILON)
                {
                    IsCatch = true;
                }
                else
                {
                    IsCatch = Vector3.Dot(ColliderMove, PrevPoint - SphereCenter) > 0.0f;
                }

                // Push out for direction
                var Direction = IsCatch ? ColliderMove : (-1.0f * PointMove);
                float a = Vector3.Dot(Direction, Direction);
                float b = Vector3.Dot(CenterToPoint, Direction) * 2.0f;
                float c = Vector3.Dot(CenterToPoint, CenterToPoint) - Radius * Radius;
                float D = b * b - 4.0f * a * c;
                if (D > 0.0f)
                {
                    float x = (-b + Mathf.Sqrt(D)) / (2.0f * a);
                    Offset = Direction * x + CenterToPoint;
                    return true;
                }
            }
            Offset = Vector3.zero;
            return false;
        }

        void ResolveSphereCollision(Collider* pCollider, ColliderEx* pColliderEx, PointReadWrite* pRW)
        {
            var Point0 = pRW->OriginalOldPosition;
            var Point1 = pRW->Position;
            var PointMove = Point1 - Point0;

            var ColliderPoint0 = pColliderEx->OldPosition;
            var ColliderPoint1 = pColliderEx->Position;
            var ColliderMove = ColliderPoint1 - ColliderPoint0;

            var Radius = pCollider->RadiusHead;

            var PrevPoint = Point0;
            var Offset = Vector3.zero;

            int Iteration = System.Math.Max(DivideMax, (int)Mathf.Ceil(ColliderMove.magnitude / Radius));
            for (int i = 1; i <= Iteration; i++)
            {
                float t = (float)i / Iteration;
                var Point = Vector3.Lerp(Point0, Point1, t);
                var Sphere = Vector3.Lerp(ColliderPoint0, ColliderPoint1, t);

                bool IsCatch = false;
                if (CheckSphereCollisionOffset(Point, PrevPoint, Sphere, Radius, ColliderMove, PointMove, ref IsCatch, ref Offset))
                {
                    pRW->Position = ColliderPoint1 + Offset;
                    break;
                }

                PrevPoint = Point;
            }
        }

        void ResolveCapsuleCollision(Collider* pCollider, ColliderEx* pColliderEx, PointReadWrite* pRW)
        {
            var Point0 = pRW->OriginalOldPosition;
            var Point1 = pRW->Position;
            var PointMove = Point1 - Point0;

            var ColliderHead0 = pColliderEx->OldPosition;
            var ColliderHead1 = pColliderEx->Position;
            var ColliderTail0 = pColliderEx->OldPosition + pColliderEx->OldDirection;
            var ColliderTail1 = pColliderEx->Position + pColliderEx->Direction;
            var ColliderDir0 = pColliderEx->OldDirection;
            var ColliderDir1 = pColliderEx->Direction;

            var HeadMove = ColliderHead1 - ColliderHead0;
            var TailMove = ColliderTail1 - ColliderTail0;

            var HeadRadius = pCollider->RadiusHead;
            var TailRadius = pCollider->RadiusTail;

            var PrevPoint = Point0;
            var Offset = Vector3.zero;

            int HeadSteps = (int)Mathf.Ceil(HeadMove.magnitude / HeadRadius);
            int TailSteps = (int)Mathf.Ceil(TailMove.magnitude / TailRadius);

            int Iteration = System.Math.Max(DivideMax, System.Math.Max(HeadSteps, TailSteps));
            for (int i = 1; i <= Iteration; i++)
            {
                float t = (float)i / Iteration;
                var Point = Vector3.Lerp(Point0, Point1, t);
                var Head = Vector3.Lerp(ColliderHead0, ColliderHead1, t);
                var Dir = Vector3.Lerp(ColliderDir0, ColliderDir1, t);
                var HeadToPoint = Point - Head;

                float w = Mathf.Clamp01(Vector3.Dot(Dir, HeadToPoint) / HeadToPoint.sqrMagnitude);
                var ColliderPoint = Head + Dir * w;
                var ColliderMove = Vector3.Lerp(HeadMove, TailMove, w);
                var Radius = Mathf.Lerp(HeadRadius, TailRadius, w);

                bool IsCatch = false;
                if (CheckSphereCollisionOffset(Point, PrevPoint, ColliderPoint, Radius, ColliderMove, PointMove, ref IsCatch, ref Offset))
                {
                    ColliderPoint = ColliderHead1 + ColliderDir1 * w;
                    pRW->Position = ColliderPoint + Offset;
                    break;
                }

                PrevPoint = Point;
            }
        }
    }

#if ENABLE_BURST
    [Unity.Burst.BurstCompile]
#endif
    struct JobCollisionPoint : IJobParallelFor
    {
        [NativeDisableUnsafePtrRestriction]
        public PointReadWrite* pRWPoints;

        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public Collider* pColliders;
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public ColliderEx* pColliderExs;
        [ReadOnly]
        public int ColliderCount;

        [ReadOnly]
        public float FloorHeight;

        [ReadOnly]
        public bool IsEnableFloor;
        public bool IsEnableCollider;

        void IJobParallelFor.Execute(int index)
        {
            var pRW = pRWPoints + index;

            if (IsEnableFloor)
            {
                if (pRW->Position.y <= FloorHeight)
                {
                    pRW->Position.y = FloorHeight;
                }
            }

            if (IsEnableCollider)
            {
                for (int i = 0; i < ColliderCount; ++i)
                {
                    Collider* pCollider = pColliders + i;
                    ColliderEx* pColliderEx = pColliderExs + i;

                    Vector3 point = pRW->Position;

                    if (pCollider->Height <= 0.0f)
                    {
                        PushoutFromSphere(pCollider, pColliderEx, ref point);
                    }
                    else
                    {
                        PushoutFromCapsule(pCollider, pColliderEx, ref point);
                    }

                    pRW->Position += (point - pRW->Position) * pCollider->PushOutRate;
                }
            }
        }

        void PushoutFromSphere(Vector3 Center, float Radius, ref Vector3 point)
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
                    return;
                }
            }
        }

        void PushoutFromSphere(Collider* pCollider, ColliderEx* pColliderEx, ref Vector3 point)
        {
            PushoutFromSphere(pColliderEx->Position, pCollider->RadiusHead, ref point);
        }

        void PushoutFromCapsule(Collider* pCollider, ColliderEx* pColliderEx, ref Vector3 point)
        {
            var capsuleVec = pColliderEx->Direction;
            var capsuleVecNormal = capsuleVec.normalized;
            var capsulePos = pColliderEx->Position;
            var targetVec = point - capsulePos;
            var distanceOnVec = Vector3.Dot(capsuleVecNormal, targetVec);
            if (distanceOnVec <= EPSILON)
            {
                PushoutFromSphere(capsulePos, pCollider->RadiusHead, ref point);
                return;
            }
            else if (distanceOnVec >= pCollider->Height)
            {
                PushoutFromSphere(capsulePos + capsuleVec, pCollider->RadiusTail, ref point);
                return;
            }
            else
            {
                var positionOnVec = capsulePos + (capsuleVecNormal * distanceOnVec);
                var pushoutVec = point - positionOnVec;
                var sqrPushoutDistance = pushoutVec.sqrMagnitude;
                if (sqrPushoutDistance > EPSILON)
                {
                    var Radius = pCollider->RadiusHead + (pCollider->RadiusTail - pCollider->RadiusHead) * distanceOnVec;
                    if (sqrPushoutDistance < Radius * Radius)
                    {
                        var pushoutDistance = Mathf.Sqrt(sqrPushoutDistance);
                        point = positionOnVec + pushoutVec * Radius / pushoutDistance;
                        return;
                    }
                }
            }
        }
    }

#if ENABLE_BURST
    [Unity.Burst.BurstCompile]
#endif
    struct JobPointToTransform : IJobParallelForTransform
    {
        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public PointRead* pRPoints;
        [NativeDisableUnsafePtrRestriction]
        public PointReadWrite* pRWPoints;
        [ReadOnly]
        public bool UpdateTransform;
        [ReadOnly]
        public AngleLimitConfig angleLockConfig;

        void IJobParallelForTransform.Execute(int index, TransformAccess transform)
        {
            var pRW = pRWPoints + index;
            var pR = pRPoints + index;

            if (pR->Weight >= EPSILON)
            {
                var pRWP = pRWPoints + pR->Parent;
                var Direction = pRW->Position - pRWP->Position;
                var RealLength = Direction.magnitude;
                if (RealLength > EPSILON)
                {
                    pRW->PreviousDirection = Direction;
                    if (UpdateTransform)
                    {
                        transform.position = pRW->Position;
                        SetRotation(index, transform);
                    }
                }
                else
                {
                    pRW->Position = pRWP->Position + pRW->PreviousDirection;
                }
            }
            else
            {
                pRW->Position = transform.position;
                if (UpdateTransform)
                {
                    SetRotation(index, transform);
                }
            }

            LockAngle(pR, pRW);
        }

        void SetRotation(int index, TransformAccess transform)
        {
            var pR = pRPoints + index;
            var pRW = pRWPoints + index;

            transform.localRotation = pR->LocalRotation;
            if (pR->Child != -1)
            {
                var pRWC = pRWPoints + pR->Child;
                var Direction = pRWC->Position - pRW->Position;
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

            Vector3 initialDeltaPos = pR->InitialWorldPosition - pRp->InitialWorldPosition;
            Vector3 superParentpos = (pRp->Parent != -1 ? (pRWPoints + pRp->Parent)->Position : pRWP->Position);
            Vector3 deltaPositionFromParent = pRWP->Position - superParentpos;

            if (deltaPositionFromParent.magnitude == 0 || angleLockConfig.limitFromRoot)
            {
                deltaPositionFromParent = initialDeltaPos;
            }

            Vector3 finalPostion = pRW->Position;
            //X軸の制限
            {
                Vector3 currDeltaPosition = finalPostion - pRWP->Position;
                float currAngleX = Mathf.Acos(currDeltaPosition.x / currDeltaPosition.magnitude) * Mathf.Rad2Deg;
                float parentAngleX = Mathf.Acos(deltaPositionFromParent.x / deltaPositionFromParent.magnitude) * Mathf.Rad2Deg;
                float minAngleX = parentAngleX - angleLockConfig.angleLimit;
                float maxAngleX = parentAngleX + angleLockConfig.angleLimit;
                if (currAngleX <= minAngleX || currAngleX >= maxAngleX)
                {
                    float angle = currAngleX <= minAngleX ? minAngleX : maxAngleX;
                    float X = pRWP->Position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * initialDeltaPos.magnitude;
                    finalPostion.x = Mathf.Lerp(finalPostion.x, X, pR->LimitPower);
                }
            }
            //ｙ軸の制限
            {
                Vector3 currDeltaPosition = finalPostion - pRWP->Position;
                float currAngleY = Mathf.Acos(currDeltaPosition.y / currDeltaPosition.magnitude) * Mathf.Rad2Deg;
                float parentAngleY = Mathf.Acos(deltaPositionFromParent.y / deltaPositionFromParent.magnitude) * Mathf.Rad2Deg;
                float minAngleY = parentAngleY - angleLockConfig.angleLimit;
                float maxAngleY = parentAngleY + angleLockConfig.angleLimit;

                if (currAngleY <= minAngleY || currAngleY >= maxAngleY)
                {
                    float angle = currAngleY <= minAngleY ? minAngleY : maxAngleY;
                    float Y = pRWP->Position.y + Mathf.Cos(angle * Mathf.Deg2Rad) * initialDeltaPos.magnitude;
                    finalPostion.y = Mathf.Lerp(finalPostion.y, Y, pR->LimitPower);
                }
            }
            //ｚ軸の制限
            {
                Vector3 currDeltaPosition = finalPostion - pRWP->Position;
                float currAngleZ = Mathf.Acos(currDeltaPosition.z / currDeltaPosition.magnitude) * Mathf.Rad2Deg;
                float parentAngleZ = Mathf.Acos(deltaPositionFromParent.z / deltaPositionFromParent.magnitude) * Mathf.Rad2Deg;
                float minAngleZ = parentAngleZ - angleLockConfig.angleLimit;
                float maxAngleZ = parentAngleZ + angleLockConfig.angleLimit;
                if (currAngleZ <= minAngleZ || currAngleZ >= maxAngleZ)
                {
                    float angle = currAngleZ <= minAngleZ ? minAngleZ : maxAngleZ;
                    float Z = pRWP->Position.z + Mathf.Cos(angle * Mathf.Deg2Rad) * initialDeltaPos.magnitude;
                    finalPostion.z = Mathf.Lerp(finalPostion.z, Z, pR->LimitPower);
                }
            }
            pRW->Position = finalPostion;
        }


    }
}
