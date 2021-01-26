using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SPCRJointSettingLocalSave
{
    public static readonly string INVALID_ID = "-1";

    [System.Serializable]
    public class SPCRvec3
    {
        public float x, y, z;
        public SPCRvec3(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
        public Vector3 ToUnityVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [System.Serializable]
    public class SPCRJointDynamicsPointSave
    {
        public string RefUniqueID { get; set; }
        public float mass { get; set; }
        public string refChildID { get; set; }
        public bool IsFixed { get; set; }
        public SPCRvec3 BoneAxis { get; set; }
        public float Depth { get; set; }
        public int Index { get; set; }

        public SPCRJointDynamicsPointSave(SPCRJointDynamicsPoint spcrJointDynamicsPoint)
        {
            if (spcrJointDynamicsPoint != null)
            {
                if (string.IsNullOrEmpty(spcrJointDynamicsPoint.UniqueGUIID))
                    spcrJointDynamicsPoint.Reset();
                RefUniqueID = spcrJointDynamicsPoint.UniqueGUIID;

                mass = spcrJointDynamicsPoint._Mass;
                if (spcrJointDynamicsPoint._RefChildPoint != null)
                    refChildID = spcrJointDynamicsPoint._RefChildPoint.UniqueGUIID;
                IsFixed = spcrJointDynamicsPoint._IsFixed;
                BoneAxis = new SPCRvec3(spcrJointDynamicsPoint._BoneAxis);
                Depth = spcrJointDynamicsPoint._Depth;
                Index = spcrJointDynamicsPoint._Index;
            }
            else
            {
                RefUniqueID = INVALID_ID;
            }
        }
    }
    [System.Serializable]
    public class SPCRJointDynamicsColliderSave
    {
        public string RefUniqueId { get; set; }
        public float Radius { get; set; }
        public float HeadRadiusScale { get; set; }
        public float TailRadiusScale { get; set; }
        public float Height { get; set; }
        public float Friction { get; set; }
        public float PushOutRate { get; set; }

        public SPCRJointDynamicsColliderSave(SPCRJointDynamicsCollider spcrJoinDynamicsCollider)
        {
            if (spcrJoinDynamicsCollider != null)
            {
                if (string.IsNullOrEmpty(spcrJoinDynamicsCollider.UniqueGUIID))
                    spcrJoinDynamicsCollider.Reset();
                RefUniqueId = spcrJoinDynamicsCollider.UniqueGUIID;
                Radius = spcrJoinDynamicsCollider.Radius;
                HeadRadiusScale = spcrJoinDynamicsCollider.HeadRadiusScale;
                TailRadiusScale = spcrJoinDynamicsCollider.TailRadiusScale;
                Height = spcrJoinDynamicsCollider.Height;
                Friction = spcrJoinDynamicsCollider.Friction;
                PushOutRate = spcrJoinDynamicsCollider.PushOutRate;
            }else
            {
                RefUniqueId = INVALID_ID;
            }
        }
    }
    [System.Serializable]
    public class SPCRJointDynamicsPointGrabberSave
    {
        public string RefUniqueGUIID { get; set; }
        public bool IsEnabled { get; set; }
        public float Radius { get; set; }
        public float Force { get; set; }

        public SPCRJointDynamicsPointGrabberSave(SPCRJointDynamicsPointGrabber spcrJointDynamicsPointGrabber)
        {
            if (spcrJointDynamicsPointGrabber != null)
            {
                if (string.IsNullOrEmpty(spcrJointDynamicsPointGrabber.UniqueGUIID))
                    spcrJointDynamicsPointGrabber.Reset();
                RefUniqueGUIID = spcrJointDynamicsPointGrabber.UniqueGUIID;
                IsEnabled = spcrJointDynamicsPointGrabber.IsEnabled;
                Radius = spcrJointDynamicsPointGrabber.Radius;
                Force = spcrJointDynamicsPointGrabber.Force;
            }else
            {
                RefUniqueGUIID = INVALID_ID;
            }
        }
    }
    [System.Serializable]
    public class SPCRAnimCurveKeyFrameSave
    {
        public float time { get; set; }
        public float value { get; set; }
        public float inTangent { get; set; }
        public float outTangent { get; set; }
        public float inWeight { get; set; }
        public float outWeight { get; set; }
        public WeightedMode weightedMode { get; set; }

        public SPCRAnimCurveKeyFrameSave(Keyframe keyframe)
        {
            time = keyframe.time;
            value = keyframe.value;
            inTangent = keyframe.inTangent;
            outTangent = keyframe.outTangent;
            inWeight = keyframe.inWeight;
            outWeight = keyframe.outWeight;
            weightedMode = keyframe.weightedMode;
        }
    }

    [System.Serializable]
    public class SPCRConstraintSave
    {
        public int IsCollision;
        public int Type;
        public int IndexA;
        public int IndexB;
        public float Length;
        public float StretchLength;
        public float Shrink;
        public float Stretch;

        public SPCRConstraintSave(SPCRJointDynamicsJob.Constraint constraint)
        {
            IsCollision = constraint.IsCollision;
            Type = (int)constraint.Type;
            IndexA = constraint.IndexA;
            IndexB = constraint.IndexB;
            Length = constraint.Length;
            StretchLength = constraint.StretchLength;
            Shrink = constraint.Shrink;
            Stretch = constraint.Stretch;
        }
        public SPCRJointDynamicsJob.Constraint ConvertToJobConstraint()
        {
            SPCRJointDynamicsJob.Constraint spcrConstraint = new SPCRJointDynamicsJob.Constraint();
            spcrConstraint.IsCollision = IsCollision;
            spcrConstraint.Type = (SPCRJointDynamicsController.ConstraintType)Type;
            spcrConstraint.IndexA = IndexA;
            spcrConstraint.IndexB = IndexB;
            spcrConstraint.Length = Length;
            spcrConstraint.StretchLength = StretchLength;
            spcrConstraint.Shrink = Shrink;
            spcrConstraint.Stretch = Stretch;
            return spcrConstraint;
        }
    }

    [System.Serializable]
    public class SPCRJointDynamicsConstraintSave
    {
        public int Type;
        public string PointA_ID;
        public string PointB_ID;
        public string PointC_ID;

        public SPCRJointDynamicsConstraintSave(SPCRJointDynamicsController.SPCRJointDynamicsConstraint spcrConstraint)
        {
            Type = (int)spcrConstraint._Type;

            if(spcrConstraint._PointA != null)
            {
                if (string.IsNullOrEmpty(spcrConstraint._PointA.UniqueGUIID))
                    spcrConstraint._PointA.Reset();
                PointA_ID = spcrConstraint._PointA.UniqueGUIID;
            }else
            {
                PointA_ID = INVALID_ID;
            }

            if (spcrConstraint._PointB != null)
            {
                if (string.IsNullOrEmpty(spcrConstraint._PointB.UniqueGUIID))
                    spcrConstraint._PointB.Reset();
                PointB_ID = spcrConstraint._PointB.UniqueGUIID;
            }
            else
            {
                PointB_ID = INVALID_ID;
            }

            if (spcrConstraint._PointC != null)
            {
                if (string.IsNullOrEmpty(spcrConstraint._PointC.UniqueGUIID))
                    spcrConstraint._PointC.Reset();
                PointC_ID = spcrConstraint._PointC.UniqueGUIID;
            }
            else
            {
                PointC_ID = INVALID_ID;
            }
        }

        public static SPCRJointDynamicsController.SPCRJointDynamicsConstraint AssignReference(SPCRJointDynamicsConstraintSave spcrJointConstraintSave)
        {
            SPCRJointDynamicsPoint pointA = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointConstraintSave.PointA_ID));
            SPCRJointDynamicsPoint pointB = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointConstraintSave.PointB_ID));
            SPCRJointDynamicsPoint pointC = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointConstraintSave.PointC_ID));

            SPCRJointDynamicsController.SPCRJointDynamicsConstraint spcrJointDynamicsConstrint = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint(
                (SPCRJointDynamicsController.ConstraintType)spcrJointConstraintSave.Type,
                pointA, pointB, pointC);
            return spcrJointDynamicsConstrint;
        }
    }

    [System.Serializable]
    public class SPCRJointDynamicsControllerSave
    {
        public SPCRJointDynamicsPointSave[] spcrChildJointDynamicsPointList { get; set; }
        public SPCRJointDynamicsPointGrabberSave[] spcrChildJointDynamicsPointGtabberList { get; set; }
        public SPCRJointDynamicsColliderSave[] spcrChildJointDynamicsColliderList { get; set; }


        public string uniqueGUIID { get; set; }
        public string name { get; set; }
        public int rootTransformChildIndex { get; set; }
        public string[] RootPointTbl { get; set; }
        public string[] ColliderTbl { get; set; }
        public string[] PointGrabberTbl { get; set; }
        public SPCRJointDynamicsController.UpdateTiming UpdateTiming { get; set; }
        public int Relaxation { get; set; }
        public int SubSteps { get; set; }

        public bool IsEnableFloorCollision { get; set; }
        public float FloorHeight { get; set; }
        public bool IsEnableColliderCollision { get; set; }
        public bool IsCancelResetPhysics { get; set; }

        public SPCRAnimCurveKeyFrameSave[] MassScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] GravityScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] ResistanceCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] HardnessCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] FrictionCurve { get; set; }

        public SPCRAnimCurveKeyFrameSave[] AllShrinkScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] AllStretchScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] StructuralShrinkVerticalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] StructuralStretchVerticalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] StructuralShrinkHorizontalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] StructuralStretchHorizontalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] ShearShrinkScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] ShearStretchScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] BendingShrinkVerticalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] BendingStretchVerticalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] BendingShrinkHorizontalScaleCurve { get; set; }
        public SPCRAnimCurveKeyFrameSave[] BendingStretchHorizontalScaleCurve { get; set; }

        public SPCRvec3 Gravity { get; set; }
        public SPCRvec3 WindForce { get; set; }

        public float SpringK { get; set; }

        public float RootSlideLimit { get; set; }
        public float RootRotateLimit { get; set; }

        public int DetailHitDivideMax { get; set; }

        public float StructuralShrinkVertical { get; set; }
        public float StructuralStretchVertical { get; set; }
        public float StructuralShrinkHorizontal { get; set; }
        public float StructuralStretchHorizontal { get; set; }
        public float ShearShrink { get; set; }
        public float ShearStretch { get; set; }
        public float BendingingShrinkVertical { get; set; }
        public float BendingingStretchVertical { get; set; }
        public float BendingingShrinkHorizontal { get; set; }
        public float BendingingStretchHorizontal { get; set; }

        public bool IsAllStructuralShrinkVertical { get; set; }
        public bool IsAllStructuralStretchVertical { get; set; }
        public bool IsAllStructuralShrinkHorizontal { get; set; }
        public bool IsAllStructuralStretchHorizontal { get; set; }
        public bool IsAllShearShrink { get; set; }
        public bool IsAllShearStretch { get; set; }
        public bool IsAllBendingingShrinkVertical { get; set; }
        public bool IsAllBendingingStretchVertical { get; set; }
        public bool IsAllBendingingShrinkHorizontal { get; set; }
        public bool IsAllBendingingStretchHorizontal { get; set; }

        public bool IsCollideStructuralVertical { get; set; }
        public bool IsCollideStructuralHorizontal { get; set; }
        public bool IsCollideShear { get; set; }
        public bool IsCollideBendingVertical { get; set; }
        public bool IsCollideBendingHorizontal { get; set; }

        public bool UseLockAngles { get; set; }
        public int LockAngle { get; set; }
        public bool UseSeperateLockAxis { get; set; }
        public int LockAngleX { get; set; }
        public int LockAngleY { get; set; }
        public int LockAngleZ { get; set; }
        public string[] PointTblIDs { get; set; }

        public SPCRJointDynamicsConstraintSave[] ConstraintsStructuralVertical { get; set; }
        public SPCRJointDynamicsConstraintSave[] ConstraintsStructuralHorizontal { get; set; }
        public SPCRJointDynamicsConstraintSave[] ConstraintsShear { get; set; }
        public SPCRJointDynamicsConstraintSave[] ConstraintsBendingVertical { get; set; }
        public SPCRJointDynamicsConstraintSave[] ConstraintsBendingHorizontal { get; set; }

        public bool IsLoopRootPoints { get; set; }
        public bool IsComputeStructuralVertical { get; set; }
        public bool IsComputeStructuralHorizontal { get; set; }
        public bool IsComputeShear { get; set; }
        public bool IsComputeBendingVertical { get; set; }
        public bool IsComputeBendingHorizontal { get; set; }

        public bool IsDebugDraw_StructuralVertical { get; set; }
        public bool IsDebugDraw_StructuralHorizontal { get; set; }
        public bool IsDebugDraw_Shear { get; set; }
        public bool IsDebugDraw_BendingVertical { get; set; }
        public bool IsDebugDraw_BendingHorizontal { get; set; }
        public bool IsDebugDraw_RuntimeColliderBounds { get; set; }

        public SPCRConstraintSave[][] ConstraintTable { get; set; }
        public int MaxPointDepth { get; set; }

        public bool IsPaused { get; set; }

        public System.Collections.Generic.List<string> SubDivInsertedPoints { get; set; }

        public System.Collections.Generic.List<string> SubDivOriginalPoints { get; set; }

        public float Accel { get; set; }
        public float Delay { get; set; }
    }

    public static void Save(SPCRJointDynamicsController SPCRJointDynamicsContoller)
    {
        SPCRJointDynamicsControllerSave spcrJointDynamicsSave = new SPCRJointDynamicsControllerSave();
        if (string.IsNullOrEmpty(SPCRJointDynamicsContoller.UniqueGUIID))
            SPCRJointDynamicsContoller.Reset();

        spcrJointDynamicsSave.uniqueGUIID = SPCRJointDynamicsContoller.UniqueGUIID;
        spcrJointDynamicsSave.name = SPCRJointDynamicsContoller.Name;
        spcrJointDynamicsSave.rootTransformChildIndex = SPCRJointDynamicsContoller._RootTransform.GetSiblingIndex();

        SPCRJointDynamicsPoint[] spcrJointDynamicsPoints = SPCRJointDynamicsContoller.GetComponentsInChildren<SPCRJointDynamicsPoint>();
        spcrJointDynamicsSave.spcrChildJointDynamicsPointList = new SPCRJointDynamicsPointSave[spcrJointDynamicsPoints.Length];
        for (int i = 0; i < spcrJointDynamicsPoints.Length; i++)
        {
            spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i] = new SPCRJointDynamicsPointSave(spcrJointDynamicsPoints[i]);
        }

        SPCRJointDynamicsCollider[] spcrJointDynamicsCollider = SPCRJointDynamicsContoller.GetComponentsInChildren<SPCRJointDynamicsCollider>();
        spcrJointDynamicsSave.spcrChildJointDynamicsColliderList = new SPCRJointDynamicsColliderSave[spcrJointDynamicsCollider.Length];
        for (int i = 0; i < spcrJointDynamicsCollider.Length; i++)
        {
            spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i] = new SPCRJointDynamicsColliderSave(spcrJointDynamicsCollider[i]);
        }

        SPCRJointDynamicsPointGrabber[] spcrJointDynamicsGrabber = SPCRJointDynamicsContoller.GetComponentsInChildren<SPCRJointDynamicsPointGrabber>();
        spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList = new SPCRJointDynamicsPointGrabberSave[spcrJointDynamicsCollider.Length];
        for (int i = 0; i < spcrJointDynamicsGrabber.Length; i++)
        {
            spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i] = new SPCRJointDynamicsPointGrabberSave(spcrJointDynamicsGrabber[i]);
        }

        if (SPCRJointDynamicsContoller._RootPointTbl != null && SPCRJointDynamicsContoller._RootPointTbl.Length > 0)
        {
            spcrJointDynamicsSave.RootPointTbl = new string[SPCRJointDynamicsContoller._RootPointTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._RootPointTbl.Length; i++)
            {
                spcrJointDynamicsSave.RootPointTbl[i] = SPCRJointDynamicsContoller._RootPointTbl[i].UniqueGUIID;
            }
        }

        if (SPCRJointDynamicsContoller._ColliderTbl != null && SPCRJointDynamicsContoller._ColliderTbl.Length > 0)
        {
            spcrJointDynamicsSave.ColliderTbl = new string[SPCRJointDynamicsContoller._ColliderTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._ColliderTbl.Length; i++)
            {
                spcrJointDynamicsSave.ColliderTbl[i] = SPCRJointDynamicsContoller._ColliderTbl[i].UniqueGUIID;
            }
        }

        if (SPCRJointDynamicsContoller._PointGrabberTbl != null && SPCRJointDynamicsContoller._PointGrabberTbl.Length > 0)
        {
            spcrJointDynamicsSave.PointGrabberTbl = new string[SPCRJointDynamicsContoller._PointGrabberTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._PointGrabberTbl.Length; i++)
            {
                spcrJointDynamicsSave.PointGrabberTbl[i] = SPCRJointDynamicsContoller._PointGrabberTbl[i].UniqueGUIID;
            }
        }

        spcrJointDynamicsSave.UpdateTiming = SPCRJointDynamicsContoller._UpdateTiming;

        spcrJointDynamicsSave.Relaxation = SPCRJointDynamicsContoller._Relaxation;
        spcrJointDynamicsSave.SubSteps = SPCRJointDynamicsContoller._SubSteps;

        spcrJointDynamicsSave.IsEnableFloorCollision = SPCRJointDynamicsContoller._IsEnableFloorCollision;
        spcrJointDynamicsSave.FloorHeight = SPCRJointDynamicsContoller._FloorHeight;

        spcrJointDynamicsSave.IsEnableColliderCollision = SPCRJointDynamicsContoller._IsEnableColliderCollision;

        spcrJointDynamicsSave.IsCancelResetPhysics = SPCRJointDynamicsContoller._IsCancelResetPhysics;

        spcrJointDynamicsSave.MassScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._MassScaleCurve);
        spcrJointDynamicsSave.GravityScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._GravityScaleCurve);
        spcrJointDynamicsSave.ResistanceCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._ResistanceCurve);
        spcrJointDynamicsSave.HardnessCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._HardnessCurve);
        spcrJointDynamicsSave.FrictionCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._FrictionCurve);

        spcrJointDynamicsSave.AllShrinkScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._AllShrinkScaleCurve);
        spcrJointDynamicsSave.AllStretchScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._AllStretchScaleCurve);
        spcrJointDynamicsSave.StructuralShrinkVerticalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._StructuralShrinkVerticalScaleCurve);
        spcrJointDynamicsSave.StructuralStretchVerticalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._StructuralStretchVerticalScaleCurve);
        spcrJointDynamicsSave.StructuralShrinkHorizontalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._StructuralShrinkHorizontalScaleCurve);
        spcrJointDynamicsSave.StructuralStretchHorizontalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._StructuralStretchHorizontalScaleCurve);
        spcrJointDynamicsSave.ShearShrinkScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._ShearShrinkScaleCurve);
        spcrJointDynamicsSave.ShearStretchScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._ShearStretchScaleCurve);
        spcrJointDynamicsSave.BendingShrinkVerticalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._BendingShrinkVerticalScaleCurve);
        spcrJointDynamicsSave.BendingStretchVerticalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._BendingStretchVerticalScaleCurve);
        spcrJointDynamicsSave.BendingShrinkHorizontalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._BendingShrinkHorizontalScaleCurve);
        spcrJointDynamicsSave.BendingStretchHorizontalScaleCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._BendingStretchHorizontalScaleCurve);

        spcrJointDynamicsSave.Gravity = new SPCRvec3(SPCRJointDynamicsContoller._Gravity);
        spcrJointDynamicsSave.WindForce = new SPCRvec3(SPCRJointDynamicsContoller._WindForce);

        spcrJointDynamicsSave.SpringK = SPCRJointDynamicsContoller._SpringK;

        spcrJointDynamicsSave.RootSlideLimit = SPCRJointDynamicsContoller._RootSlideLimit;
        spcrJointDynamicsSave.RootRotateLimit = SPCRJointDynamicsContoller._RootRotateLimit;

        spcrJointDynamicsSave.DetailHitDivideMax = SPCRJointDynamicsContoller._DetailHitDivideMax;

        spcrJointDynamicsSave.StructuralShrinkVertical = SPCRJointDynamicsContoller._StructuralShrinkVertical;
        spcrJointDynamicsSave.StructuralStretchVertical = SPCRJointDynamicsContoller._StructuralStretchVertical;
        spcrJointDynamicsSave.StructuralShrinkHorizontal = SPCRJointDynamicsContoller._StructuralShrinkHorizontal;
        spcrJointDynamicsSave.StructuralStretchHorizontal = SPCRJointDynamicsContoller._StructuralStretchHorizontal;
        spcrJointDynamicsSave.ShearShrink = SPCRJointDynamicsContoller._ShearShrink;
        spcrJointDynamicsSave.ShearStretch = SPCRJointDynamicsContoller._ShearStretch;
        spcrJointDynamicsSave.BendingingShrinkVertical = SPCRJointDynamicsContoller._BendingingShrinkVertical;
        spcrJointDynamicsSave.BendingingStretchVertical = SPCRJointDynamicsContoller._BendingingStretchVertical;
        spcrJointDynamicsSave.BendingingShrinkHorizontal = SPCRJointDynamicsContoller._BendingingShrinkHorizontal;
        spcrJointDynamicsSave.BendingingStretchHorizontal = SPCRJointDynamicsContoller._BendingingStretchHorizontal;

        spcrJointDynamicsSave.IsAllStructuralShrinkVertical = SPCRJointDynamicsContoller._IsAllStructuralShrinkVertical;
        spcrJointDynamicsSave.IsAllStructuralStretchVertical = SPCRJointDynamicsContoller._IsAllStructuralStretchVertical;
        spcrJointDynamicsSave.IsAllStructuralShrinkHorizontal = SPCRJointDynamicsContoller._IsAllStructuralShrinkHorizontal;
        spcrJointDynamicsSave.IsAllStructuralStretchHorizontal = SPCRJointDynamicsContoller._IsAllStructuralStretchHorizontal;
        spcrJointDynamicsSave.IsAllShearShrink = SPCRJointDynamicsContoller._IsAllShearShrink;
        spcrJointDynamicsSave.IsAllShearStretch = SPCRJointDynamicsContoller._IsAllShearStretch;
        spcrJointDynamicsSave.IsAllBendingingShrinkVertical = SPCRJointDynamicsContoller._IsAllBendingingShrinkVertical;
        spcrJointDynamicsSave.IsAllBendingingStretchVertical = SPCRJointDynamicsContoller._IsAllBendingingStretchVertical;
        spcrJointDynamicsSave.IsAllBendingingShrinkHorizontal = SPCRJointDynamicsContoller._IsAllBendingingShrinkHorizontal;
        spcrJointDynamicsSave.IsAllBendingingStretchHorizontal = SPCRJointDynamicsContoller._IsAllBendingingStretchHorizontal;

        spcrJointDynamicsSave.IsCollideStructuralVertical = SPCRJointDynamicsContoller._IsCollideStructuralVertical;
        spcrJointDynamicsSave.IsCollideStructuralHorizontal = SPCRJointDynamicsContoller._IsCollideStructuralHorizontal;
        spcrJointDynamicsSave.IsCollideShear = SPCRJointDynamicsContoller._IsCollideShear;
        spcrJointDynamicsSave.IsCollideBendingVertical = SPCRJointDynamicsContoller._IsCollideBendingVertical;
        spcrJointDynamicsSave.IsCollideBendingHorizontal = SPCRJointDynamicsContoller._IsCollideBendingHorizontal;

        spcrJointDynamicsSave.UseLockAngles = SPCRJointDynamicsContoller._UseLockAngles;
        spcrJointDynamicsSave.LockAngle = SPCRJointDynamicsContoller._LockAngle;
        spcrJointDynamicsSave.UseSeperateLockAxis = SPCRJointDynamicsContoller._UseSeperateLockAxis;
        spcrJointDynamicsSave.LockAngleX = SPCRJointDynamicsContoller._LockAngleX;
        spcrJointDynamicsSave.LockAngleY = SPCRJointDynamicsContoller._LockAngleY;
        spcrJointDynamicsSave.LockAngleZ = SPCRJointDynamicsContoller._LockAngleZ;

        spcrJointDynamicsSave.PointTblIDs = new string[SPCRJointDynamicsContoller.PointTbl.Length];
        for(int i = 0; i < SPCRJointDynamicsContoller.PointTbl.Length; i++)
        {
            if (string.IsNullOrEmpty(SPCRJointDynamicsContoller.PointTbl[i].UniqueGUIID))
                SPCRJointDynamicsContoller.PointTbl[i].Reset();
            spcrJointDynamicsSave.PointTblIDs[i] = SPCRJointDynamicsContoller.PointTbl[i].UniqueGUIID;
        }

        spcrJointDynamicsSave.ConstraintsStructuralVertical = new SPCRJointDynamicsConstraintSave[SPCRJointDynamicsContoller.ConstraintsStructuralVertical.Length];
        for(int i = 0; i < SPCRJointDynamicsContoller.ConstraintsStructuralVertical.Length; i++)
        {
            spcrJointDynamicsSave.ConstraintsStructuralVertical[i] = new SPCRJointDynamicsConstraintSave(SPCRJointDynamicsContoller.ConstraintsStructuralVertical[i]);
        }

        spcrJointDynamicsSave.ConstraintsStructuralHorizontal = new SPCRJointDynamicsConstraintSave[SPCRJointDynamicsContoller.ConstraintsStructuralHorizontal.Length];
        for (int i = 0; i < SPCRJointDynamicsContoller.ConstraintsStructuralHorizontal.Length; i++)
        {
            spcrJointDynamicsSave.ConstraintsStructuralHorizontal[i] = new SPCRJointDynamicsConstraintSave(SPCRJointDynamicsContoller.ConstraintsStructuralHorizontal[i]);
        }

        spcrJointDynamicsSave.ConstraintsShear = new SPCRJointDynamicsConstraintSave[SPCRJointDynamicsContoller.ConstraintsShear.Length];
        for (int i = 0; i < SPCRJointDynamicsContoller.ConstraintsShear.Length; i++)
        {
            spcrJointDynamicsSave.ConstraintsShear[i] = new SPCRJointDynamicsConstraintSave(SPCRJointDynamicsContoller.ConstraintsShear[i]);
        }

        spcrJointDynamicsSave.ConstraintsBendingVertical = new SPCRJointDynamicsConstraintSave[SPCRJointDynamicsContoller.ConstraintsBendingVertical.Length];
        for (int i = 0; i < SPCRJointDynamicsContoller.ConstraintsBendingVertical.Length; i++)
        {
            spcrJointDynamicsSave.ConstraintsBendingVertical[i] = new SPCRJointDynamicsConstraintSave(SPCRJointDynamicsContoller.ConstraintsBendingVertical[i]);
        }

        spcrJointDynamicsSave.ConstraintsBendingHorizontal = new SPCRJointDynamicsConstraintSave[SPCRJointDynamicsContoller.ConstraintsBendingHorizontal.Length];
        for (int i = 0; i < SPCRJointDynamicsContoller.ConstraintsBendingHorizontal.Length; i++)
        {
            spcrJointDynamicsSave.ConstraintsBendingHorizontal[i] = new SPCRJointDynamicsConstraintSave(SPCRJointDynamicsContoller.ConstraintsBendingHorizontal[i]);
        }

        spcrJointDynamicsSave.IsLoopRootPoints = SPCRJointDynamicsContoller._IsLoopRootPoints;
        spcrJointDynamicsSave.IsComputeStructuralVertical = SPCRJointDynamicsContoller._IsComputeStructuralVertical;
        spcrJointDynamicsSave.IsComputeStructuralHorizontal = SPCRJointDynamicsContoller._IsComputeStructuralHorizontal;
        spcrJointDynamicsSave.IsComputeShear = SPCRJointDynamicsContoller._IsComputeShear;
        spcrJointDynamicsSave.IsComputeBendingVertical = SPCRJointDynamicsContoller._IsComputeBendingVertical;
        spcrJointDynamicsSave.IsComputeBendingHorizontal = SPCRJointDynamicsContoller._IsComputeBendingHorizontal;

        spcrJointDynamicsSave.IsDebugDraw_StructuralVertical = SPCRJointDynamicsContoller._IsDebugDraw_StructuralVertical;
        spcrJointDynamicsSave.IsDebugDraw_StructuralHorizontal = SPCRJointDynamicsContoller._IsDebugDraw_StructuralHorizontal;
        spcrJointDynamicsSave.IsDebugDraw_Shear = SPCRJointDynamicsContoller._IsDebugDraw_Shear;
        spcrJointDynamicsSave.IsDebugDraw_BendingVertical = SPCRJointDynamicsContoller._IsDebugDraw_BendingVertical;
        spcrJointDynamicsSave.IsDebugDraw_BendingHorizontal = SPCRJointDynamicsContoller._IsDebugDraw_BendingHorizontal;
        spcrJointDynamicsSave.IsDebugDraw_RuntimeColliderBounds = SPCRJointDynamicsContoller._IsDebugDraw_RuntimeColliderBounds;

        if (SPCRJointDynamicsContoller.ConstraintTable != null)
        {
            spcrJointDynamicsSave.ConstraintTable = new SPCRConstraintSave[SPCRJointDynamicsContoller.ConstraintTable.Length][];
            for (int i = 0; i < SPCRJointDynamicsContoller.ConstraintTable.Length; i++)
            {
                spcrJointDynamicsSave.ConstraintTable[i] = new SPCRConstraintSave[SPCRJointDynamicsContoller.ConstraintTable[i].Length];
                for (int j = 0; j < SPCRJointDynamicsContoller.ConstraintTable[i].Length; j++)
                {
                    spcrJointDynamicsSave.ConstraintTable[i][j] = new SPCRConstraintSave(SPCRJointDynamicsContoller.ConstraintTable[i][j]);
                }
            }
        }

        spcrJointDynamicsSave.MaxPointDepth = SPCRJointDynamicsContoller.MaxPointDepth;
        spcrJointDynamicsSave.IsPaused = SPCRJointDynamicsContoller._IsPaused;

        spcrJointDynamicsSave.SubDivInsertedPoints = new System.Collections.Generic.List<string>();
        for(int i = 0; i < SPCRJointDynamicsContoller._SubDivInsertedPoints.Count; i++)
        {
            spcrJointDynamicsSave.SubDivInsertedPoints.Add(SPCRJointDynamicsContoller._SubDivInsertedPoints[i].UniqueGUIID);
        }

        spcrJointDynamicsSave.SubDivOriginalPoints = new System.Collections.Generic.List<string>();
        for (int i = 0; i < SPCRJointDynamicsContoller._SubDivOriginalPoints.Count; i++)
        {
            spcrJointDynamicsSave.SubDivOriginalPoints.Add(SPCRJointDynamicsContoller._SubDivOriginalPoints[i].UniqueGUIID);
        }

        spcrJointDynamicsSave.Accel = SPCRJointDynamicsContoller.Accel;
        spcrJointDynamicsSave.Delay = SPCRJointDynamicsContoller.Delay;

        SaveIntoBinary(spcrJointDynamicsSave, GetFileName(SPCRJointDynamicsContoller));
    }

    static SPCRAnimCurveKeyFrameSave[] GetSPCRAnimaCurveKeyFrames(AnimationCurve animCurve)
    {
        SPCRAnimCurveKeyFrameSave[] animCurveData = new SPCRAnimCurveKeyFrameSave[animCurve.keys.Length];
        for (int i = 0; i < animCurve.keys.Length; i++)
        {
            animCurveData[i] = new SPCRAnimCurveKeyFrameSave(animCurve.keys[i]);
        }
        return animCurveData;
    }

    static AnimationCurve GetAnimCurve(SPCRAnimCurveKeyFrameSave[] spcrAnimCurveKeyFrames)
    {
        AnimationCurve animCurve = new AnimationCurve();
        for (int i = 0; i < spcrAnimCurveKeyFrames.Length; i++)
        {
            UnityEngine.Keyframe keyFrame = new Keyframe(spcrAnimCurveKeyFrames[i].time, spcrAnimCurveKeyFrames[i].value, spcrAnimCurveKeyFrames[i].inTangent, spcrAnimCurveKeyFrames[i].outTangent);
            animCurve.AddKey(keyFrame);
        }
        return animCurve;
    }

    private static string GetFileName(SPCRJointDynamicsController controller)
    {
        return controller.UniqueGUIID + ".SPCR";
    }

    private static void SaveIntoBinary(SPCRJointDynamicsControllerSave spcrJointDynamicsSave, string fileName)
    {
        string filePath = Application.dataPath + "/../" + "SPCRSaveConfiguration/";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        try
        {
            Stream stream = File.Open(filePath + fileName, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, spcrJointDynamicsSave);
            stream.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("ファイル保存失敗：" + e.Message);
        }
    }

    private static SPCRJointDynamicsControllerSave LoadBinary(string fileName)
    {
        string filePath = Application.dataPath + "/../" + "SPCRSaveConfiguration/";
        if (!File.Exists(filePath + fileName))
        {
            Debug.Log("保存ファイルが見つかりません");
            return null;
        }
        SPCRJointDynamicsControllerSave spcrJointDynamicsSave = null;
        try
        {
            Stream stream = File.Open(filePath + fileName, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            spcrJointDynamicsSave = (SPCRJointDynamicsControllerSave)bformatter.Deserialize(stream);
            stream.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("ファイルの読み込み失敗：" + e.Message);
        }
        return spcrJointDynamicsSave;
    }

    static System.Collections.Generic.List<Component> globalUniqueIdList;

    public static void Load(SPCRJointDynamicsController SPCRJointDynamicsContoller)
    {
        SPCRJointDynamicsControllerSave spcrJointDynamicsSave = LoadBinary(GetFileName(SPCRJointDynamicsContoller));
        if (spcrJointDynamicsSave == null)
            return;
        globalUniqueIdList = GetGlobalUniqueIdComponentList(SPCRJointDynamicsContoller);

        SPCRJointDynamicsContoller.Name = spcrJointDynamicsSave.name;
        Object RootTransform = SPCRJointDynamicsContoller.transform.GetChild(spcrJointDynamicsSave.rootTransformChildIndex);
        if (RootTransform != null)
            SPCRJointDynamicsContoller._RootTransform = (Transform)RootTransform;


        if(spcrJointDynamicsSave.spcrChildJointDynamicsPointList != null)
        {
            for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsPointList.Length; i++)
            {
                SPCRJointDynamicsPoint point = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].RefUniqueID));
                SPCRJointDynamicsPoint ChildPoint = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].refChildID));
                if (point != null)
                {
                    point._Mass = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].mass;
                    point._RefChildPoint = ChildPoint;
                    point._IsFixed = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].IsFixed;
                    point._BoneAxis = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].BoneAxis.ToUnityVector3();
                    point._Depth = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].Depth;
                    point._Index = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].Index;
                }
            }
        }

        if (spcrJointDynamicsSave.spcrChildJointDynamicsColliderList != null)
        {
            for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsColliderList.Length; i++)
            {
                SPCRJointDynamicsCollider point = (SPCRJointDynamicsCollider)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsCollider) && ((SPCRJointDynamicsCollider)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].RefUniqueId));
                point.Radius = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].Radius;
                point.HeadRadiusScale = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].HeadRadiusScale;
                point.TailRadiusScale = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].TailRadiusScale;
                point.Height = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].Height;
                point.Friction = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].Friction;
                point.PushOutRate = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].PushOutRate;
            }
        }

        if (spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList != null)
        {
            for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList.Length; i++)
            {
                SPCRJointDynamicsPointGrabber point = (SPCRJointDynamicsPointGrabber)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPointGrabber) && ((SPCRJointDynamicsPointGrabber)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].RefUniqueGUIID));
                point.IsEnabled = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].IsEnabled;
                point.Radius = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].Radius;
                point.Force = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].Force;
            }
        }

        if (spcrJointDynamicsSave.RootPointTbl != null)
        {
            SPCRJointDynamicsContoller._RootPointTbl = new SPCRJointDynamicsPoint[spcrJointDynamicsSave.RootPointTbl.Length];
            for (int i = 0; i < spcrJointDynamicsSave.RootPointTbl.Length; i++)
            {
                SPCRJointDynamicsContoller._RootPointTbl[i] = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.RootPointTbl[i]));
            }
        }else
        {
            SPCRJointDynamicsContoller._RootPointTbl = new SPCRJointDynamicsPoint[0];
        }

        if (spcrJointDynamicsSave.ColliderTbl != null)
        {
            SPCRJointDynamicsContoller._ColliderTbl = new SPCRJointDynamicsCollider[spcrJointDynamicsSave.ColliderTbl.Length];
            for (int i = 0; i < spcrJointDynamicsSave.ColliderTbl.Length; i++)
            {
                SPCRJointDynamicsContoller._ColliderTbl[i] = (SPCRJointDynamicsCollider)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsCollider) && ((SPCRJointDynamicsCollider)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.ColliderTbl[i]));
            }
        }else
        {
            SPCRJointDynamicsContoller._ColliderTbl = new SPCRJointDynamicsCollider[0];
        }

        if(spcrJointDynamicsSave.PointGrabberTbl != null)
        {
            SPCRJointDynamicsContoller._PointGrabberTbl = new SPCRJointDynamicsPointGrabber[spcrJointDynamicsSave.PointGrabberTbl.Length];
            for (int i = 0; i < spcrJointDynamicsSave.PointGrabberTbl.Length; i++)
            {
                SPCRJointDynamicsContoller._PointGrabberTbl[i] = (SPCRJointDynamicsPointGrabber)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPointGrabber) && ((SPCRJointDynamicsPointGrabber)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.PointGrabberTbl[i]));
            }
        }else
        {
            SPCRJointDynamicsContoller._PointGrabberTbl = new SPCRJointDynamicsPointGrabber[0];
        }

        SPCRJointDynamicsContoller._UpdateTiming = spcrJointDynamicsSave.UpdateTiming;

        SPCRJointDynamicsContoller._Relaxation = spcrJointDynamicsSave.Relaxation;
        SPCRJointDynamicsContoller._SubSteps = spcrJointDynamicsSave.SubSteps;

        SPCRJointDynamicsContoller._IsEnableFloorCollision = spcrJointDynamicsSave.IsEnableFloorCollision;
        SPCRJointDynamicsContoller._FloorHeight = spcrJointDynamicsSave.FloorHeight;

        SPCRJointDynamicsContoller._IsEnableColliderCollision = spcrJointDynamicsSave.IsEnableColliderCollision;

        SPCRJointDynamicsContoller._IsCancelResetPhysics = spcrJointDynamicsSave.IsCancelResetPhysics;

        SPCRJointDynamicsContoller._MassScaleCurve = GetAnimCurve(spcrJointDynamicsSave.MassScaleCurve);
        SPCRJointDynamicsContoller._GravityScaleCurve = GetAnimCurve(spcrJointDynamicsSave.GravityScaleCurve);
        SPCRJointDynamicsContoller._ResistanceCurve = GetAnimCurve(spcrJointDynamicsSave.ResistanceCurve);
        SPCRJointDynamicsContoller._HardnessCurve = GetAnimCurve(spcrJointDynamicsSave.HardnessCurve);
        SPCRJointDynamicsContoller._FrictionCurve = GetAnimCurve(spcrJointDynamicsSave.FrictionCurve);

        SPCRJointDynamicsContoller._AllShrinkScaleCurve = GetAnimCurve(spcrJointDynamicsSave.AllShrinkScaleCurve);
        SPCRJointDynamicsContoller._AllStretchScaleCurve = GetAnimCurve(spcrJointDynamicsSave.AllStretchScaleCurve);
        SPCRJointDynamicsContoller._StructuralShrinkVerticalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.StructuralShrinkVerticalScaleCurve);
        SPCRJointDynamicsContoller._StructuralStretchVerticalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.StructuralStretchVerticalScaleCurve);
        SPCRJointDynamicsContoller._StructuralShrinkHorizontalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.StructuralShrinkHorizontalScaleCurve);
        SPCRJointDynamicsContoller._StructuralStretchHorizontalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.StructuralStretchHorizontalScaleCurve);
        SPCRJointDynamicsContoller._ShearShrinkScaleCurve = GetAnimCurve(spcrJointDynamicsSave.ShearShrinkScaleCurve);
        SPCRJointDynamicsContoller._ShearStretchScaleCurve = GetAnimCurve(spcrJointDynamicsSave.ShearStretchScaleCurve);
        SPCRJointDynamicsContoller._BendingShrinkVerticalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.BendingShrinkVerticalScaleCurve);
        SPCRJointDynamicsContoller._BendingStretchVerticalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.BendingStretchVerticalScaleCurve);
        SPCRJointDynamicsContoller._BendingShrinkHorizontalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.BendingShrinkHorizontalScaleCurve);
        SPCRJointDynamicsContoller._BendingStretchHorizontalScaleCurve = GetAnimCurve(spcrJointDynamicsSave.BendingStretchHorizontalScaleCurve);

        SPCRJointDynamicsContoller._Gravity = spcrJointDynamicsSave.Gravity.ToUnityVector3();
        SPCRJointDynamicsContoller._WindForce = spcrJointDynamicsSave.WindForce.ToUnityVector3();

        SPCRJointDynamicsContoller._SpringK = spcrJointDynamicsSave.SpringK;

        SPCRJointDynamicsContoller._RootSlideLimit = spcrJointDynamicsSave.RootSlideLimit;
        SPCRJointDynamicsContoller._RootRotateLimit = spcrJointDynamicsSave.RootRotateLimit;

        SPCRJointDynamicsContoller._DetailHitDivideMax = spcrJointDynamicsSave.DetailHitDivideMax;

        SPCRJointDynamicsContoller._StructuralShrinkVertical = spcrJointDynamicsSave.StructuralShrinkVertical;
        SPCRJointDynamicsContoller._StructuralStretchVertical = spcrJointDynamicsSave.StructuralStretchVertical;
        SPCRJointDynamicsContoller._StructuralShrinkHorizontal = spcrJointDynamicsSave.StructuralShrinkHorizontal;
        SPCRJointDynamicsContoller._StructuralStretchHorizontal = spcrJointDynamicsSave.StructuralStretchHorizontal;
        SPCRJointDynamicsContoller._ShearShrink = spcrJointDynamicsSave.ShearShrink;
        SPCRJointDynamicsContoller._ShearStretch = spcrJointDynamicsSave.ShearStretch;
        SPCRJointDynamicsContoller._BendingingShrinkVertical = spcrJointDynamicsSave.BendingingShrinkVertical;
        SPCRJointDynamicsContoller._BendingingStretchVertical = spcrJointDynamicsSave.BendingingStretchVertical;
        SPCRJointDynamicsContoller._BendingingShrinkHorizontal = spcrJointDynamicsSave.BendingingShrinkHorizontal;
        SPCRJointDynamicsContoller._BendingingStretchHorizontal = spcrJointDynamicsSave.BendingingStretchHorizontal;

        SPCRJointDynamicsContoller._IsAllStructuralShrinkVertical = spcrJointDynamicsSave.IsAllStructuralShrinkVertical;
        SPCRJointDynamicsContoller._IsAllStructuralStretchVertical = spcrJointDynamicsSave.IsAllStructuralStretchVertical;
        SPCRJointDynamicsContoller._IsAllStructuralShrinkHorizontal = spcrJointDynamicsSave.IsAllStructuralShrinkHorizontal;
        SPCRJointDynamicsContoller._IsAllStructuralStretchHorizontal = spcrJointDynamicsSave.IsAllStructuralStretchHorizontal;
        SPCRJointDynamicsContoller._IsAllShearShrink = spcrJointDynamicsSave.IsAllShearShrink;
        SPCRJointDynamicsContoller._IsAllShearStretch = spcrJointDynamicsSave.IsAllShearStretch;
        SPCRJointDynamicsContoller._IsAllBendingingShrinkVertical = spcrJointDynamicsSave.IsAllBendingingShrinkVertical;
        SPCRJointDynamicsContoller._IsAllBendingingStretchVertical = spcrJointDynamicsSave.IsAllBendingingStretchVertical;
        SPCRJointDynamicsContoller._IsAllBendingingShrinkHorizontal = spcrJointDynamicsSave.IsAllBendingingShrinkHorizontal;
        SPCRJointDynamicsContoller._IsAllBendingingStretchHorizontal = spcrJointDynamicsSave.IsAllBendingingStretchHorizontal;

        SPCRJointDynamicsContoller._IsCollideStructuralVertical = spcrJointDynamicsSave.IsCollideStructuralVertical;
        SPCRJointDynamicsContoller._IsCollideStructuralHorizontal = spcrJointDynamicsSave.IsCollideStructuralHorizontal;
        SPCRJointDynamicsContoller._IsCollideShear = spcrJointDynamicsSave.IsCollideShear;
        SPCRJointDynamicsContoller._IsCollideBendingVertical = spcrJointDynamicsSave.IsCollideBendingVertical;
        SPCRJointDynamicsContoller._IsCollideBendingHorizontal = spcrJointDynamicsSave.IsCollideBendingHorizontal;

        SPCRJointDynamicsContoller._UseLockAngles = spcrJointDynamicsSave.UseLockAngles;
        SPCRJointDynamicsContoller._LockAngle = spcrJointDynamicsSave.LockAngle;
        SPCRJointDynamicsContoller._UseSeperateLockAxis = spcrJointDynamicsSave.UseSeperateLockAxis;
        SPCRJointDynamicsContoller._LockAngleX = spcrJointDynamicsSave.LockAngleX;
        SPCRJointDynamicsContoller._LockAngleY = spcrJointDynamicsSave.LockAngleY;
        SPCRJointDynamicsContoller._LockAngleZ = spcrJointDynamicsSave.LockAngleZ;

        if (spcrJointDynamicsSave.PointTblIDs != null)
        {
            SPCRJointDynamicsContoller.PointTbl = new SPCRJointDynamicsPoint[spcrJointDynamicsSave.PointTblIDs.Length];
            for (int i = 0; i < spcrJointDynamicsSave.PointTblIDs.Length; i++)
            {
                SPCRJointDynamicsContoller.PointTbl[i] = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.PointTblIDs[i]));
            }
        }else
        {
            SPCRJointDynamicsContoller.PointTbl = new SPCRJointDynamicsPoint[0];
        }

        if (spcrJointDynamicsSave.ConstraintsStructuralVertical != null)
        {
            SPCRJointDynamicsContoller.ConstraintsStructuralVertical = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[spcrJointDynamicsSave.ConstraintsStructuralVertical.Length];
            for (int i = 0; i < spcrJointDynamicsSave.ConstraintsStructuralVertical.Length; i++)
            {
                SPCRJointDynamicsContoller.ConstraintsStructuralVertical[i] = SPCRJointDynamicsConstraintSave.AssignReference(spcrJointDynamicsSave.ConstraintsStructuralVertical[i]);
            }
        }
        else
        {
            SPCRJointDynamicsContoller.ConstraintsStructuralVertical = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[0];
        }
        if (spcrJointDynamicsSave.ConstraintsStructuralHorizontal != null)
        {
            SPCRJointDynamicsContoller.ConstraintsStructuralHorizontal = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[spcrJointDynamicsSave.ConstraintsStructuralHorizontal.Length];
            for (int i = 0; i < spcrJointDynamicsSave.ConstraintsStructuralHorizontal.Length; i++)
            {
                SPCRJointDynamicsContoller.ConstraintsStructuralHorizontal[i] = SPCRJointDynamicsConstraintSave.AssignReference(spcrJointDynamicsSave.ConstraintsStructuralHorizontal[i]);
            }
        }
        else
        {
            SPCRJointDynamicsContoller.ConstraintsStructuralHorizontal = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[0];
        }
        if (spcrJointDynamicsSave.ConstraintsShear != null)
        {
            SPCRJointDynamicsContoller.ConstraintsShear = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[spcrJointDynamicsSave.ConstraintsShear.Length];
            for (int i = 0; i < spcrJointDynamicsSave.ConstraintsShear.Length; i++)
            {
                SPCRJointDynamicsContoller.ConstraintsShear[i] = SPCRJointDynamicsConstraintSave.AssignReference(spcrJointDynamicsSave.ConstraintsShear[i]);
            }
        }
        else
        {
            SPCRJointDynamicsContoller.ConstraintsShear = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[0];
        }
        if (spcrJointDynamicsSave.ConstraintsBendingVertical != null)
        {
            SPCRJointDynamicsContoller.ConstraintsBendingVertical = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[spcrJointDynamicsSave.ConstraintsBendingVertical.Length];
            for (int i = 0; i < spcrJointDynamicsSave.ConstraintsBendingVertical.Length; i++)
            {
                SPCRJointDynamicsContoller.ConstraintsBendingVertical[i] = SPCRJointDynamicsConstraintSave.AssignReference(spcrJointDynamicsSave.ConstraintsBendingVertical[i]);
            }
        }
        else
        {
            SPCRJointDynamicsContoller.ConstraintsBendingVertical = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[0];
        }
        if (spcrJointDynamicsSave.ConstraintsBendingHorizontal != null)
        {
            SPCRJointDynamicsContoller.ConstraintsBendingHorizontal = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[spcrJointDynamicsSave.ConstraintsBendingHorizontal.Length];
            for (int i = 0; i < spcrJointDynamicsSave.ConstraintsBendingHorizontal.Length; i++)
            {
                SPCRJointDynamicsContoller.ConstraintsBendingHorizontal[i] = SPCRJointDynamicsConstraintSave.AssignReference(spcrJointDynamicsSave.ConstraintsBendingHorizontal[i]);
            }
        }
        else
        {
            SPCRJointDynamicsContoller.ConstraintsBendingHorizontal = new SPCRJointDynamicsController.SPCRJointDynamicsConstraint[0];
        }

        SPCRJointDynamicsContoller._IsLoopRootPoints = spcrJointDynamicsSave.IsLoopRootPoints;
        SPCRJointDynamicsContoller._IsComputeStructuralVertical = spcrJointDynamicsSave.IsComputeStructuralVertical;
        SPCRJointDynamicsContoller._IsComputeStructuralHorizontal = spcrJointDynamicsSave.IsComputeStructuralHorizontal;
        SPCRJointDynamicsContoller._IsComputeShear = spcrJointDynamicsSave.IsComputeShear;
        SPCRJointDynamicsContoller._IsComputeBendingVertical = spcrJointDynamicsSave.IsComputeBendingVertical;
        SPCRJointDynamicsContoller._IsComputeBendingHorizontal = spcrJointDynamicsSave.IsComputeBendingHorizontal;

        SPCRJointDynamicsContoller._IsDebugDraw_StructuralVertical = spcrJointDynamicsSave.IsDebugDraw_StructuralVertical;
        SPCRJointDynamicsContoller._IsDebugDraw_StructuralHorizontal = spcrJointDynamicsSave.IsDebugDraw_StructuralHorizontal;
        SPCRJointDynamicsContoller._IsDebugDraw_Shear = spcrJointDynamicsSave.IsDebugDraw_Shear;
        SPCRJointDynamicsContoller._IsDebugDraw_BendingVertical = spcrJointDynamicsSave.IsDebugDraw_BendingVertical;
        SPCRJointDynamicsContoller._IsDebugDraw_BendingHorizontal = spcrJointDynamicsSave.IsDebugDraw_BendingHorizontal;
        SPCRJointDynamicsContoller._IsDebugDraw_RuntimeColliderBounds = spcrJointDynamicsSave.IsDebugDraw_RuntimeColliderBounds;

        if (spcrJointDynamicsSave.ConstraintTable != null)
        {
            SPCRJointDynamicsContoller.ConstraintTable = new SPCRJointDynamicsJob.Constraint[spcrJointDynamicsSave.ConstraintTable.Length][];
            for (int i = 0; i < spcrJointDynamicsSave.ConstraintTable.Length; i++)
            {
                SPCRJointDynamicsContoller.ConstraintTable[i] = new SPCRJointDynamicsJob.Constraint[spcrJointDynamicsSave.ConstraintTable[i].Length];
                for (int j = 0; j < spcrJointDynamicsSave.ConstraintTable[i].Length; j++)
                {
                    SPCRJointDynamicsContoller.ConstraintTable[i][j] = spcrJointDynamicsSave.ConstraintTable[i][j].ConvertToJobConstraint();
                }
            }
        }

        SPCRJointDynamicsContoller.MaxPointDepth = spcrJointDynamicsSave.MaxPointDepth;
        SPCRJointDynamicsContoller._IsPaused = spcrJointDynamicsSave.IsPaused;

        SPCRJointDynamicsContoller._SubDivInsertedPoints.Clear();
        for (int i = 0; i < spcrJointDynamicsSave.SubDivInsertedPoints.Count; i++)
        {
            SPCRJointDynamicsContoller._SubDivInsertedPoints.Add((SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.SubDivInsertedPoints[i])));
        }
        SPCRJointDynamicsContoller._SubDivOriginalPoints.Clear();
        for (int i = 0; i < spcrJointDynamicsSave.SubDivOriginalPoints.Count; i++)
        {
            SPCRJointDynamicsContoller._SubDivOriginalPoints.Add((SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.SubDivOriginalPoints[i])));
        }

        SPCRJointDynamicsContoller.Accel = spcrJointDynamicsSave.Accel;
        SPCRJointDynamicsContoller.Delay = spcrJointDynamicsSave.Delay;

        globalUniqueIdList.Clear();
    }

    static System.Collections.Generic.List<Component> GetGlobalUniqueIdComponentList(SPCRJointDynamicsController SPCRJointDynamicsContoller)
    {
        System.Collections.Generic.List<Component> globalUniqueIdList = new System.Collections.Generic.List<Component>();

        globalUniqueIdList.AddRange(SPCRJointDynamicsContoller.GetComponentsInChildren<SPCRJointDynamicsPoint>());
        globalUniqueIdList.AddRange(SPCRJointDynamicsContoller.GetComponentsInChildren<SPCRJointDynamicsCollider>());
        globalUniqueIdList.AddRange(SPCRJointDynamicsContoller.GetComponentsInChildren<SPCRJointDynamicsPointGrabber>());

        return globalUniqueIdList;
    }
}
