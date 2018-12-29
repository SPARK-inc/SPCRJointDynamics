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
        public float FrictionScale;
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
        public float Friction;
        public int GrabberIndex;
        public float GrabberDistance;
        public Vector3 Gravity;
        public Vector3 BoneAxis;
        public Vector3 InitialPosition;
        public Quaternion LocalRotation;
        public Vector3 Position;
        public Vector3 OldPosition;
        public Vector3 PreviousDirection;
    }

    public struct Constraint
    {
        public bool IsCollision;
        public SPCRJointDynamicsController.ConstraintType Type;
        public int IndexA;
        public int IndexB;
        public float Length;
        public float Shrink;
        public float Stretch;
    }

    struct Collider
    {
        public float RadiusHead;
        public float RadiusTail;
        public float Height;
        public float Friction;
        public Vector3 Position;
        public Vector3 Direction;
    }

    struct Grabber
    {
        public bool IsEnabled;
        public float Radius;
        public float Force;
        public Vector3 Position;
    }

    Transform _RootBone;
    Point[] _Points;
    Constraint[][] _Constraints;
    Transform[] _PointTransforms;
    SPCRJointDynamicsCollider[] _RefColliders;
    Collider[] _Colliders;
    SPCRJointDynamicsPointGrabber[] _RefGrabbers;
    Grabber[] _Grabbers;

    public void Initialize(Transform RootBone, Point[] Points, Transform[] PointTransforms, Constraint[][] Constraints, SPCRJointDynamicsCollider[] Colliders, SPCRJointDynamicsPointGrabber[] Grabbers)
    {
        _RootBone = RootBone;
        _Points = new Point[Points.Length];

        for (int i = 0; i < Points.Length; ++i)
        {
            var src = Points[i];
            _Points[i].Parent = src.Parent;
            _Points[i].Child = src.Child;
            _Points[i].Weight = src.Weight;
            _Points[i].Mass = src.Mass;
            _Points[i].Resistance = src.Resistance;
            _Points[i].FrictionScale = src.FrictionScale;
            _Points[i].ParentLength = src.ParentLength;
            _Points[i].StructuralShrinkHorizontal = src.StructuralShrinkHorizontal * 0.5f;
            _Points[i].StructuralStretchHorizontal = src.StructuralStretchHorizontal * 0.5f;
            _Points[i].StructuralShrinkVertical = src.StructuralShrinkVertical * 0.5f;
            _Points[i].StructuralStretchVertical = src.StructuralStretchVertical * 0.5f;
            _Points[i].ShearShrink = src.ShearShrink * 0.5f;
            _Points[i].ShearStretch = src.ShearStretch * 0.5f;
            _Points[i].BendingShrinkHorizontal = src.BendingShrinkHorizontal * 0.5f;
            _Points[i].BendingStretchHorizontal = src.BendingStretchHorizontal * 0.5f;
            _Points[i].BendingShrinkVertical = src.BendingShrinkVertical * 0.5f;
            _Points[i].BendingStretchVertical = src.BendingStretchVertical * 0.5f;
            _Points[i].Gravity = src.Gravity;
            _Points[i].BoneAxis = src.BoneAxis;
            _Points[i].LocalRotation = src.LocalRotation;
            _Points[i].InitialPosition = src.InitialPosition;
            _Points[i].Position = src.Position;
            _Points[i].OldPosition = src.OldPosition;
            _Points[i].PreviousDirection = src.PreviousDirection;
            _Points[i].Friction = 0.5f;
            _Points[i].GrabberIndex = -1;
            _Points[i].GrabberDistance = 0.0f;
        }

        _PointTransforms = new Transform[_Points.Length];
        for (int i = 0; i < _Points.Length; ++i)
        {
            _PointTransforms[i] = PointTransforms[i];
        }

        _Constraints = new Constraint[Constraints.Length][];
        for (int i = 0; i < Constraints.Length; ++i)
        {
            _Constraints[i] = new Constraint[Constraints[i].Length];
            for (int j = 0; j < Constraints[i].Length; ++j)
            {
                _Constraints[i][j] = Constraints[i][j];
            }
        }

        _RefColliders = Colliders;
        _Colliders = new Collider[Colliders.Length];
        for (int i = 0; i < Colliders.Length; ++i)
        {
            var src = Colliders[i];
            if (src.IsCapsule)
            {
                _Colliders[i].RadiusHead = src.RadiusHead;
                _Colliders[i].RadiusTail = src.RadiusTail;
                _Colliders[i].Height = src.Height;
            }
            else
            {
                _Colliders[i].RadiusHead = src.RadiusHead;
                _Colliders[i].Height = 0.0f;
            }
            _Colliders[i].Friction = src.Friction;
        }

        _RefGrabbers = Grabbers;
        _Grabbers = new Grabber[Grabbers.Length];
        for (int i = 0; i < Grabbers.Length; ++i)
        {
            _Grabbers[i].Radius = Grabbers[i].Radius;
            _Grabbers[i].Force = Grabbers[i].Force;
        }
    }

    public void Uninitialize()
    {
    }

    public void Reset()
    {
        for (int i = 0; i < _Points.Length; ++i)
        {
            _Points[i].OldPosition = _Points[i].Position = _PointTransforms[i].position;
        }
    }

    public void Restore()
    {
        for (int i = 0; i < _Points.Length; ++i)
        {
            _Points[i].Position = _RootBone.TransformPoint(_Points[i].InitialPosition);
            _Points[i].OldPosition = _Points[i].Position;
            _PointTransforms[i].position = _Points[i].Position;
        }
    }

    public void Execute(
        float StepTime, Vector3 WindForce,
        int Relaxation, float SpringK,
        bool IsEnableFloorCollision, float FloorHeight)
    {
        int ColliderCount = _RefColliders.Length;
        for (int i = 0; i < ColliderCount; ++i)
        {
            var Src = _RefColliders[i];
            var SrcT = Src.RefTransform;
            if (Src.Height <= EPSILON)
            {
                _Colliders[i].Position = _RefColliders[i].RefTransform.position;
            }
            else
            {
                _Colliders[i].Direction = SrcT.rotation * Vector3.up * Src.Height;
                _Colliders[i].Position = SrcT.position - (_Colliders[i].Direction * 0.5f);
            }
        }

        int GrabberCount = _RefGrabbers.Length;
        for (int i = 0; i < GrabberCount; ++i)
        {
            _Grabbers[i].IsEnabled = _RefGrabbers[i].IsEnabled;
            _Grabbers[i].Position = _RefGrabbers[i].RefTransform.position;
        }

        UpdatePoints(
            WindForce, StepTime,
            IsEnableFloorCollision, FloorHeight);

        for (int i = 0; i < Relaxation; ++i)
        {
            ConstraintUpdate(SpringK);
        }

        PointToTransform();
    }

    void UpdatePoints(
        Vector3 WindForce, float StepTime,
        bool IsEnableFloorCollision, float FloorHeight)
    {
        var StepTime_x2_Half = StepTime * StepTime * 0.5f;

        for (int i = 0; i < _Points.Length; ++i)
        {
            UpdatePointsOne(
                WindForce, StepTime_x2_Half,
                IsEnableFloorCollision, FloorHeight,
                i, ref _Points[i]);
        }
    }

    void UpdatePointsOne(
        Vector3 WindForce, float StepTime_x2_Half,
        bool IsEnableFloorCollision, float FloorHeight,
        int Index, ref Point point)
    {
        if (point.Weight < EPSILON) return;

        Vector3 Force;
        Force = point.Gravity;
        Force += WindForce;
        Force *= StepTime_x2_Half;

        Vector3 Displacement;
        Displacement = point.Position - point.OldPosition;
        Displacement += Force / point.Mass;
        Displacement *= point.Resistance;
        Displacement *= 1.0f - (point.Friction * point.FrictionScale);

        point.OldPosition = point.Position;
        point.Position += Displacement;
        point.Friction = 0.0f;

        if (point.GrabberIndex != -1)
        {
            var grabber = _Grabbers[point.GrabberIndex];
            if (!grabber.IsEnabled)
            {
                point.GrabberIndex = -1;
            }
            else
            {
                var Vec = point.Position - grabber.Position;
                var Pos = grabber.Position + Vec.normalized * point.GrabberDistance;
                point.Position += (Pos - point.Position) * grabber.Force;
            }
        }
        else
        {
            var NearIndex = -1;
            var sqrNearRange = 1000.0f * 1000.0f;
            for (int i = 0; i < _Grabbers.Length; ++i)
            {
                var grabber = _Grabbers[i];
                if (!grabber.IsEnabled) continue;

                var Vec = grabber.Position - point.Position;
                var sqrVecLength = Vec.sqrMagnitude;
                if (sqrVecLength < grabber.Radius * grabber.Radius)
                {
                    if (sqrVecLength < sqrNearRange)
                    {
                        sqrNearRange = sqrVecLength;
                        NearIndex = Index;
                    }
                }
            }
            if (NearIndex != -1)
            {
                point.GrabberIndex = NearIndex;
                point.GrabberDistance = Mathf.Sqrt(sqrNearRange) / 2.0f;
            }
        }

        if (IsEnableFloorCollision)
        {
            if (point.Position.y <= FloorHeight)
            {
                point.Position.y = FloorHeight;
            }
        }
    }

    void ConstraintUpdate(float SpringK)
    {
        for (int i = 0; i < _Constraints.Length; ++i)
        {
            for (int j = 0; j < _Constraints[i].Length; ++j)
            {
                Constraint constraint = _Constraints[i][j];
                ConstraintUpdateOne(
                    constraint,
                    ref _Points[constraint.IndexA],
                    ref _Points[constraint.IndexB],
                    SpringK);
            }
        }
    }

    void ConstraintUpdateOne(Constraint constraint, ref Point pointA, ref Point pointB, float SpringK)
    {
        var WeightA = pointA.Weight;
        var WeightB = pointB.Weight;

        if ((WeightA <= EPSILON) && (WeightB <= EPSILON)) return;

        var Direction = pointB.Position - pointA.Position;

        var Distance = Direction.magnitude;
        var Force = (Distance - constraint.Length) * SpringK;

        var IsShrink = Force >= 0.0f;
        float ConstraintPower;
        switch (constraint.Type)
        {
        case SPCRJointDynamicsController.ConstraintType.Structural_Vertical:
            ConstraintPower = IsShrink
                ? constraint.Shrink * (pointA.StructuralShrinkVertical + pointB.StructuralShrinkVertical)
                : constraint.Stretch * (pointA.StructuralStretchVertical + pointB.StructuralStretchVertical);
            break;
        case SPCRJointDynamicsController.ConstraintType.Structural_Horizontal:
            ConstraintPower = IsShrink
                ? constraint.Shrink * (pointA.StructuralShrinkHorizontal + pointB.StructuralShrinkHorizontal)
                : constraint.Stretch * (pointA.StructuralStretchHorizontal + pointB.StructuralStretchHorizontal);
            break;
        case SPCRJointDynamicsController.ConstraintType.Shear:
            ConstraintPower = IsShrink
                ? constraint.Shrink * (pointA.ShearShrink + pointB.ShearShrink)
                : constraint.Stretch * (pointA.ShearStretch + pointB.ShearStretch);
            break;
        case SPCRJointDynamicsController.ConstraintType.Bending_Vertical:
            ConstraintPower = IsShrink
                ? constraint.Shrink * (pointA.BendingShrinkVertical + pointB.BendingShrinkVertical)
                : constraint.Stretch * (pointA.BendingStretchVertical + pointB.BendingStretchVertical);
            break;
        case SPCRJointDynamicsController.ConstraintType.Bending_Horizontal:
            ConstraintPower = IsShrink
                ? constraint.Shrink * (pointA.BendingShrinkHorizontal + pointB.BendingShrinkHorizontal)
                : constraint.Stretch * (pointA.BendingStretchHorizontal + pointB.BendingStretchHorizontal);
            break;
        default:
            ConstraintPower = 0.0f;
            break;
        }

        if (ConstraintPower > 0.0f)
        {
            var Displacement = Direction.normalized * (Force * ConstraintPower);

            float WightAB = WeightA + WeightB;
            pointA.Position += Displacement * WeightA / WightAB;
            pointB.Position -= Displacement * WeightB / WightAB;
        }

        if (!constraint.IsCollision) return;

        float Friction = 0.0f;
        for (int k = 0; k < _Colliders.Length; ++k)
        {
            Collider collider = _Colliders[k];

            float Radius;
            Vector3 pointOnLine, pointOnCollider;
            if (CollisionDetection(collider, pointA.Position, pointB.Position, out pointOnLine, out pointOnCollider, out Radius))
            {
                var Pushout = pointOnLine - pointOnCollider;
                var PushoutDistance = Pushout.magnitude;

                var pointDistance = (pointB.Position - pointA.Position).magnitude * 0.5f;
                var rateP1 = Mathf.Clamp01((pointOnLine - pointA.Position).magnitude / pointDistance);
                var rateP2 = Mathf.Clamp01((pointOnLine - pointB.Position).magnitude / pointDistance);

                Pushout /= PushoutDistance;
                Pushout *= Mathf.Max(Radius - PushoutDistance, 0.0f);
                pointA.Position += Pushout * rateP2;
                pointB.Position += Pushout * rateP1;

                var Dot = Vector3.Dot(Vector3.up, (pointOnLine - pointOnCollider).normalized);
                Friction = Mathf.Max(Friction, collider.Friction * Mathf.Clamp01(Dot));
            }
        }

        pointA.Friction = Mathf.Max(Friction, pointA.Friction);
        pointB.Friction = Mathf.Max(Friction, pointB.Friction);
    }

    bool CollisionDetection(Collider collider, Vector3 point1, Vector3 point2, out Vector3 pointOnLine, out Vector3 pointOnCollider, out float Radius)
    {
        if (collider.Height <= EPSILON)
        {
            var direction = point2 - point1;
            var directionLength = direction.magnitude;
            direction /= directionLength;

            var toCenter = collider.Position - point1;
            var dot = Vector3.Dot(direction, toCenter);
            var pointOnDirection = direction * Mathf.Clamp(dot, 0.0f, directionLength);

            pointOnCollider = collider.Position;
            pointOnLine = pointOnDirection + point1;
            Radius = collider.RadiusHead;

            if ((pointOnCollider - pointOnLine).sqrMagnitude > collider.RadiusHead * collider.RadiusHead)
            {
                return false;
            }

            return true;
        }
        else
        {
            var capsuleDir = collider.Direction;
            var capsulePos = collider.Position;
            var pointDir = point2 - point1;

            float t1, t2;
            var sqrDistance = ComputeNearestPoints(capsulePos, capsuleDir, point1, pointDir, out t1, out t2, out pointOnCollider, out pointOnLine);
            t1 = Mathf.Clamp01(t1);
            Radius = collider.RadiusHead + (collider.RadiusTail - collider.RadiusHead) * t1;

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

    void PointToTransform()
    {
        for (int i = 0; i < _Points.Length; ++i)
        {
            PointToTransformOne(_PointTransforms[i], ref _Points[i]);
        }
    }

    void PointToTransformOne(Transform transform, ref Point point)
    {
        if (point.Weight > EPSILON)
        {
            var parent = _Points[point.Parent];
            var direction = point.Position - parent.Position;
            var realLength = direction.magnitude;
            if (realLength > EPSILON)
            {
                point.PreviousDirection = direction;
                transform.position = point.Position;
                SetRotation(transform, ref point);
            }
            else
            {
                point.Position = parent.Position + point.PreviousDirection;
            }
        }
        else
        {
            point.Position = transform.position;
            SetRotation(transform, ref point);
        }
    }

    void SetRotation(Transform transform, ref Point point)
    {
        transform.localRotation = Quaternion.identity * point.LocalRotation;
        if (point.Child != -1)
        {
            var child = _Points[point.Child];
            var direction = child.Position - point.Position;
            if (direction.sqrMagnitude > EPSILON)
            {
                var aimVector = transform.TransformDirection(point.BoneAxis);
                var aimRotation = Quaternion.FromToRotation(aimVector, direction);
                transform.rotation = aimRotation * transform.rotation;
            }
        }
    }
}
