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
using UnityEngine.Rendering;

[DefaultExecutionOrder(10000)]
public class SPCRJointDynamicsController : MonoBehaviour
{
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

    public string Name;

    public Transform _RootTransform;
    public SPCRJointDynamicsPoint[] _RootPointTbl = new SPCRJointDynamicsPoint[0];

    public SPCRJointDynamicsCollider[] _ColliderTbl = new SPCRJointDynamicsCollider[0];

    public SPCRJointDynamicsPointGrabber[] _PointGrabberTbl = new SPCRJointDynamicsPointGrabber[0];

    public UpdateTiming _UpdateTiming = UpdateTiming.LateUpdate;

    public int _Relaxation = 3;

    public bool _IsEnableFloorCollision = true;
    public float _FloorHeight = 0.02f;

    public bool _IsCancelResetPhysics = false;

    public AnimationCurve _MassScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
    public AnimationCurve _GravityScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
    public AnimationCurve _ResistanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });
    public AnimationCurve _FrictionCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.7f), new Keyframe(1.0f, 0.7f) });

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

    public Vector3 _Gravity = new Vector3(0.0f, -10.0f, 0.0f);
    public Vector3 _WindForce = new Vector3(0.0f, 0.0f, 0.0f);

    public float _SpringK = 1.0f;

    public float _StructuralShrinkVertical = 1.0f;
    public float _StructuralStretchVertical = 1.0f;
    public float _StructuralShrinkHorizontal = 1.0f;
    public float _StructuralStretchHorizontal = 1.0f;
    public float _ShearShrink = 1.0f;
    public float _ShearStretch = 1.0f;
    public float _BendingingShrinkVertical = 1.0f;
    public float _BendingingStretchVertical = 1.0f;
    public float _BendingingShrinkHorizontal = 1.0f;
    public float _BendingingStretchHorizontal = 1.0f;

    public bool _IsAllStructuralShrinkVertical = false;
    public bool _IsAllStructuralStretchVertical = true;
    public bool _IsAllStructuralShrinkHorizontal = true;
    public bool _IsAllStructuralStretchHorizontal = true;
    public bool _IsAllShearShrink = true;
    public bool _IsAllShearStretch = true;
    public bool _IsAllBendingingShrinkVertical = true;
    public bool _IsAllBendingingStretchVertical = true;
    public bool _IsAllBendingingShrinkHorizontal = true;
    public bool _IsAllBendingingStretchHorizontal = true;

    public bool _IsCollideStructuralVertical = true;
    public bool _IsCollideStructuralHorizontal = true;
    public bool _IsCollideShear = true;
    public bool _IsCollideBendingVertical = false;
    public bool _IsCollideBendingHorizontal = false;

    [SerializeField]
    SPCRJointDynamicsPoint[] _PointTbl = new SPCRJointDynamicsPoint[0];
    [SerializeField]
    SPCRJointDynamicsConstraint[] _ConstraintsStructuralVertical = new SPCRJointDynamicsConstraint[0];
    [SerializeField]
    SPCRJointDynamicsConstraint[] _ConstraintsStructuralHorizontal = new SPCRJointDynamicsConstraint[0];
    [SerializeField]
    SPCRJointDynamicsConstraint[] _ConstraintsShear = new SPCRJointDynamicsConstraint[0];
    [SerializeField]
    SPCRJointDynamicsConstraint[] _ConstraintsBendingVertical = new SPCRJointDynamicsConstraint[0];
    [SerializeField]
    SPCRJointDynamicsConstraint[] _ConstraintsBendingHorizontal = new SPCRJointDynamicsConstraint[0];

    public bool _IsLoopRootPoints = false;
    public bool _IsComputeStructuralVertical = true;
    public bool _IsComputeStructuralHorizontal = true;
    public bool _IsComputeShear = false;
    public bool _IsComputeBendingVertical = true;
    public bool _IsComputeBendingHorizontal = true;

    public bool _IsDebugDraw_StructuralVertical = false;
    public bool _IsDebugDraw_StructuralHorizontal = false;
    public bool _IsDebugDraw_Shear = false;

    [SerializeField]
    SPCRJointDynamicsJob.Constraint[][] _ConstraintTable;

    [SerializeField]
    int _MaxPointDepth = 0;

    float _Accel;
    float _Delay;

    SPCRJointDynamicsJob _Job = new SPCRJointDynamicsJob();

    void Awake()
    {
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
            Points[i].Mass = src._Mass * _MassScaleCurve.Evaluate(rate);
            Points[i].Resistance = 1.0f - _ResistanceCurve.Evaluate(rate);
            Points[i].Gravity = _Gravity * _GravityScaleCurve.Evaluate(rate);
            Points[i].FrictionScale = _FrictionCurve.Evaluate(rate);
            Points[i].BoneAxis = src._BoneAxis;
            Points[i].Position = PointTransforms[i].position;
            Points[i].OldPosition = PointTransforms[i].position;
            Points[i].InitialPosition = _RootTransform.InverseTransformPoint(PointTransforms[i].position);
            Points[i].PreviousDirection = PointTransforms[i].parent.position - PointTransforms[i].position;
            Points[i].ParentLength = Points[i].PreviousDirection.magnitude;
            Points[i].LocalRotation = PointTransforms[i].localRotation;

            Points[i].StructuralShrinkVertical = _StructuralShrinkVerticalScaleCurve.Evaluate(rate);
            Points[i].StructuralStretchVertical = _StructuralStretchVerticalScaleCurve.Evaluate(rate);
            Points[i].StructuralShrinkHorizontal = _StructuralShrinkHorizontalScaleCurve.Evaluate(rate);
            Points[i].StructuralStretchHorizontal = _StructuralStretchHorizontalScaleCurve.Evaluate(rate);
            Points[i].ShearShrink = _ShearShrinkScaleCurve.Evaluate(rate);
            Points[i].ShearStretch = _ShearStretchScaleCurve.Evaluate(rate);
            Points[i].BendingShrinkVertical = _BendingShrinkVerticalScaleCurve.Evaluate(rate);
            Points[i].BendingStretchVertical = _BendingStretchVerticalScaleCurve.Evaluate(rate);
            Points[i].BendingShrinkHorizontal = _BendingShrinkHorizontalScaleCurve.Evaluate(rate);
            Points[i].BendingStretchHorizontal = _BendingStretchHorizontalScaleCurve.Evaluate(rate);

            var AllShrinkScale = _AllShrinkScaleCurve.Evaluate(rate);
            var AllStretchScale = _AllStretchScaleCurve.Evaluate(rate);
            if (_IsAllStructuralShrinkVertical) Points[i].StructuralShrinkVertical *= AllShrinkScale;
            if (_IsAllStructuralStretchVertical) Points[i].StructuralStretchVertical *= AllStretchScale;
            if (_IsAllStructuralShrinkHorizontal) Points[i].StructuralShrinkHorizontal *= AllShrinkScale;
            if (_IsAllStructuralStretchHorizontal) Points[i].StructuralStretchHorizontal *= AllStretchScale;
            if (_IsAllShearShrink) Points[i].ShearShrink *= AllShrinkScale;
            if (_IsAllShearStretch) Points[i].ShearStretch *= AllStretchScale;
            if (_IsAllBendingingShrinkVertical) Points[i].BendingShrinkVertical *= AllShrinkScale;
            if (_IsAllBendingingStretchVertical) Points[i].BendingStretchVertical *= AllStretchScale;
            if (_IsAllBendingingShrinkHorizontal) Points[i].BendingShrinkHorizontal *= AllShrinkScale;
            if (_IsAllBendingingStretchHorizontal) Points[i].BendingStretchHorizontal *= AllStretchScale;
        }

        for (int i = 0; i < _PointTbl.Length; ++i)
        {
            if (_PointTbl[i]._RefChildPoint == null) continue;

            Points[i].Child = _PointTbl[i]._RefChildPoint._Index;
            Points[Points[i].Child].Parent = _PointTbl[i]._Index;
        }

        CreationConstraintTable();
        _Job.Initialize(_RootTransform, Points, PointTransforms, _ConstraintTable, _ColliderTbl, _PointGrabberTbl);

        _Delay = 15.0f / 60.0f;
    }

    void OnDestroy()
    {
        _Job.Uninitialize();
    }

    void FixedUpdate()
    {
        if (_UpdateTiming != UpdateTiming.FixedUpdate) return;
        UpdateImpl(Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        if (_UpdateTiming != UpdateTiming.LateUpdate) return;
        UpdateImpl(Time.deltaTime);
    }

    void UpdateImpl(float DeltaTime)
    {
        if (_Delay > 0.0f)
        {
            _Delay -= DeltaTime;
            if (_Delay > 0.0f)
            {
                return;
            }

            _Job.Reset();
        }

        float StepTime = DeltaTime;
        float WindForcePower = (Mathf.Sin(_Accel) * 0.5f + 0.5f);
        _Accel += StepTime * 3.0f;

        _Job.Execute(
            StepTime, _WindForce * WindForcePower,
            _Relaxation, _SpringK,
            _IsEnableFloorCollision, _FloorHeight);
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

        CreationConstraintTable();
    }

    public void UpdateJointDistance()
    {
        for (int i = 0; i < _ConstraintsStructuralVertical.Length; ++i)
        {
            _ConstraintsStructuralHorizontal[i].UpdateLength();
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

        var childPointB = childB.GetComponent<SPCRJointDynamicsPoint>();

        if (childPointB != null)
        {
            ConstraintList.Add(new SPCRJointDynamicsConstraint(
                ConstraintType.Bending_Vertical,
                Point,
                childPointB));
        }

        var childPointA = childA.GetComponent<SPCRJointDynamicsPoint>();
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

            CreationConstraintHorizontal(childPointA, childPointB, ref ConstraintList);
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
                    Shrink = _BendingingShrinkHorizontal,
                    Stretch = _BendingingStretchHorizontal,
                    IsCollision = !src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideBendingHorizontal,
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
                    Shrink = _StructuralShrinkHorizontal,
                    Stretch = _StructuralStretchHorizontal,
                    IsCollision = !src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideStructuralHorizontal,
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
                    Shrink = _ShearShrink,
                    Stretch = _ShearStretch,
                    IsCollision = !src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideShear,
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
                    Shrink = _BendingingShrinkVertical,
                    Stretch = _BendingingStretchVertical,
                    IsCollision = !src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideBendingVertical,
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
                    Shrink = _StructuralShrinkVertical,
                    Stretch = _StructuralStretchVertical,
                    IsCollision = !src._PointA._IsFixed && !src._PointB._IsFixed && _IsCollideStructuralVertical,
                });
            }
        }

        _ConstraintTable = new SPCRJointDynamicsJob.Constraint[ConstraintTable.Count][];
        for (int i = 0; i < ConstraintTable.Count; ++i)
        {
            _ConstraintTable[i] = ConstraintTable[i].ToArray();
        }
    }

    void OnDrawGizms_Constraint(SPCRJointDynamicsConstraint [] constraints)
    {
        for (int i = 0; i < constraints.Length; i++)
        {
            var constraint = constraints[i];
            var pointA = constraint._PointA.transform.position;
            var pointB = constraint._PointB.transform.position;
            Gizmos.DrawLine(pointA, pointB);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < _PointTbl.Length; ++i)
        {
            Gizmos.DrawSphere(_PointTbl[i].transform.position, 0.002f);
        }

        Gizmos.color = Color.magenta;
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
    }
}
