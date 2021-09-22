using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

namespace SPCR
{
    public static class SPCRJointSettingLocalSave
    {
        public static readonly string INVALID_ID = "INV";

        [System.Serializable]
        public class TransformData
        {
            public string GOName { get; set; }
            public SPCRvec3 GOPosition { get; set; }
            public SPCRvec3 GOEular { get; set; }
            public SPCRvec3 GOScaler { get; set; }

            public void SaveTransformData(Transform transform)
            {
                GOName = transform.name;
                GOPosition = new SPCRvec3(transform.position);
                GOEular = new SPCRvec3(transform.eulerAngles);
                GOScaler = new SPCRvec3(transform.localScale);
            }

            public void AssignTransformData(Transform transform)
            {
                transform.name = GOName;
                transform.position = GOPosition.ToUnityVector3();
                transform.eulerAngles = GOEular.ToUnityVector3();
                transform.localScale = GOScaler.ToUnityVector3();
            }
        }

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
            public bool UseForSurfaceCollision { get; set; }

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
                    UseForSurfaceCollision = spcrJointDynamicsPoint._UseForSurfaceCollision;
                }
                else
                {
                    RefUniqueID = INVALID_ID;
                }
            }
        }
        [System.Serializable]
        public class SPCRJointDynamicsColliderSave : TransformData
        {
            public string RefUniqueId { get; set; }
            public float Radius { get; set; }
            public float HeadRadiusScale { get; set; }
            public float TailRadiusScale { get; set; }
            public float Height { get; set; }
            public float Friction { get; set; }
            public float PushOutRate { get; set; }
            public SPCRJointDynamicsCollider.ColliderForce ForceType { get; set; }

            public SPCRJointDynamicsColliderSave(SPCRJointDynamicsCollider spcrJoinDynamicsCollider)
            {
                if (spcrJoinDynamicsCollider != null)
                {
                    if (string.IsNullOrEmpty(spcrJoinDynamicsCollider.UniqueGUIID))
                        spcrJoinDynamicsCollider.Reset();
                    RefUniqueId = spcrJoinDynamicsCollider.UniqueGUIID;
                    Radius = spcrJoinDynamicsCollider.RadiusRaw;
                    HeadRadiusScale = spcrJoinDynamicsCollider.HeadRadiusScale;
                    TailRadiusScale = spcrJoinDynamicsCollider.TailRadiusScale;
                    Height = spcrJoinDynamicsCollider.HeightRaw;
                    Friction = spcrJoinDynamicsCollider.Friction;
                    PushOutRate = spcrJoinDynamicsCollider.PushOutRate;
                    ForceType = spcrJoinDynamicsCollider._SurfaceColliderForce;

                    SaveTransformData(spcrJoinDynamicsCollider.transform);

                }
                else
                {
                    RefUniqueId = INVALID_ID;
                }
            }
        }
        [System.Serializable]
        public class SPCRJointDynamicsPointGrabberSave : TransformData
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
                    Radius = spcrJointDynamicsPointGrabber.RadiusRaw;
                    Force = spcrJointDynamicsPointGrabber.Force;

                    SaveTransformData(spcrJointDynamicsPointGrabber.transform);
                }
                else
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
                return spcrConstraint;
            }
        }

        [System.Serializable]
        public class SPCRJointDynamicsControllerSave
        {
            public SPCRJointDynamicsPointSave[] spcrChildJointDynamicsPointList { get; set; }
            public SPCRJointDynamicsPointGrabberSave[] spcrChildJointDynamicsPointGtabberList;
            public SPCRJointDynamicsColliderSave[] spcrChildJointDynamicsColliderList;

            public string name { get; set; }
            public string rootTransformName { get; set; }
            public string[] RootPointTbl { get; set; }

            public SPCRJointDynamicsController.UpdateTiming UpdateTiming { get; set; }
            public int Relaxation { get; set; }
            public int SubSteps { get; set; }

            public bool IsEnableFloorCollision { get; set; }
            public float FloorHeight { get; set; }
            public bool IsEnableColliderCollision { get; set; }
            public bool IsCancelResetPhysics { get; set; }

            public bool IsEnableSurfaceCollision { get; set; }
            public int SurfaceCollisionDivision { get; set; }

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
            public float BendingShrinkVertical { get; set; }
            public float BendingStretchVertical { get; set; }
            public float BendingShrinkHorizontal { get; set; }
            public float BendingStretchHorizontal { get; set; }

            public bool IsAllStructuralShrinkVertical { get; set; }
            public bool IsAllStructuralStretchVertical { get; set; }
            public bool IsAllStructuralShrinkHorizontal { get; set; }
            public bool IsAllStructuralStretchHorizontal { get; set; }
            public bool IsAllShearShrink { get; set; }
            public bool IsAllShearStretch { get; set; }
            public bool IsAllBendingShrinkVertical { get; set; }
            public bool IsAllBendingStretchVertical { get; set; }
            public bool IsAllBendingShrinkHorizontal { get; set; }
            public bool IsAllBendingStretchHorizontal { get; set; }

            public bool IsCollideStructuralVertical { get; set; }
            public bool IsCollideStructuralHorizontal { get; set; }
            public bool IsCollideShear { get; set; }
            public bool IsCollideBendingVertical { get; set; }
            public bool IsCollideBendingHorizontal { get; set; }

            public bool UseLimitAngles { get; set; }
            public int LimitAngle { get; set; }
            public SPCRAnimCurveKeyFrameSave[] LimitPowerCurve { get; set; }
            public bool LimitFromRoot { get; set; }

            public string[] PointTblIDs { get; set; }

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
            public bool IsDebugDraw_SurfaceFace { get; set; }
            public float Debug_SurfaceNoramlLength { get; set; }
            public bool IsDebugDraw_RuntimeColliderBounds { get; set; }

            public SPCRConstraintSave[][] ConstraintTable { get; set; }
            public int MaxPointDepth { get; set; }
        }

        public static void Save(SPCRJointDynamicsController SPCRJointDynamicsContoller)
        {
            SPCRJointDynamicsControllerSave spcrJointDynamicsSave = new SPCRJointDynamicsControllerSave();

            spcrJointDynamicsSave.name = SPCRJointDynamicsContoller.Name;
            spcrJointDynamicsSave.rootTransformName = SPCRJointDynamicsContoller._RootTransform.name;

            spcrJointDynamicsSave.spcrChildJointDynamicsPointList = new SPCRJointDynamicsPointSave[SPCRJointDynamicsContoller.PointTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller.PointTbl.Length; i++)
            {
                spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i] = new SPCRJointDynamicsPointSave(SPCRJointDynamicsContoller.PointTbl[i]);
            }

            spcrJointDynamicsSave.spcrChildJointDynamicsColliderList = new SPCRJointDynamicsColliderSave[SPCRJointDynamicsContoller._ColliderTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._ColliderTbl.Length; i++)
            {
                spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i] = new SPCRJointDynamicsColliderSave(SPCRJointDynamicsContoller._ColliderTbl[i]);
            }
            UpdateIDIfSameUniqueId(ref spcrJointDynamicsSave.spcrChildJointDynamicsColliderList);

            spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList = new SPCRJointDynamicsPointGrabberSave[SPCRJointDynamicsContoller._PointGrabberTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._PointGrabberTbl.Length; i++)
            {
                spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i] = new SPCRJointDynamicsPointGrabberSave(SPCRJointDynamicsContoller._PointGrabberTbl[i]);
            }
            UpdateIDIfSameUniqueId(ref spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList);

            if (SPCRJointDynamicsContoller._RootPointTbl != null && SPCRJointDynamicsContoller._RootPointTbl.Length > 0)
            {
                spcrJointDynamicsSave.RootPointTbl = new string[SPCRJointDynamicsContoller._RootPointTbl.Length];
                for (int i = 0; i < SPCRJointDynamicsContoller._RootPointTbl.Length; i++)
                {
                    spcrJointDynamicsSave.RootPointTbl[i] = SPCRJointDynamicsContoller._RootPointTbl[i].UniqueGUIID;
                }
            }

            spcrJointDynamicsSave.UpdateTiming = SPCRJointDynamicsContoller._UpdateTiming;

            spcrJointDynamicsSave.Relaxation = SPCRJointDynamicsContoller._Relaxation;
            spcrJointDynamicsSave.SubSteps = SPCRJointDynamicsContoller._SubSteps;

            spcrJointDynamicsSave.IsEnableFloorCollision = SPCRJointDynamicsContoller._IsEnableFloorCollision;
            spcrJointDynamicsSave.FloorHeight = SPCRJointDynamicsContoller._FloorHeight;

            spcrJointDynamicsSave.IsEnableColliderCollision = SPCRJointDynamicsContoller._IsEnableColliderCollision;

            spcrJointDynamicsSave.IsCancelResetPhysics = SPCRJointDynamicsContoller._IsCancelResetPhysics;

            spcrJointDynamicsSave.IsEnableSurfaceCollision = SPCRJointDynamicsContoller._IsEnableSurfaceCollision;
            spcrJointDynamicsSave.SurfaceCollisionDivision = SPCRJointDynamicsContoller._SurfaceCollisionDivision;

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
            spcrJointDynamicsSave.BendingShrinkVertical = SPCRJointDynamicsContoller._BendingShrinkVertical;
            spcrJointDynamicsSave.BendingStretchVertical = SPCRJointDynamicsContoller._BendingStretchVertical;
            spcrJointDynamicsSave.BendingShrinkHorizontal = SPCRJointDynamicsContoller._BendingShrinkHorizontal;
            spcrJointDynamicsSave.BendingStretchHorizontal = SPCRJointDynamicsContoller._BendingStretchHorizontal;

            spcrJointDynamicsSave.IsAllStructuralShrinkVertical = SPCRJointDynamicsContoller._IsAllStructuralShrinkVertical;
            spcrJointDynamicsSave.IsAllStructuralStretchVertical = SPCRJointDynamicsContoller._IsAllStructuralStretchVertical;
            spcrJointDynamicsSave.IsAllStructuralShrinkHorizontal = SPCRJointDynamicsContoller._IsAllStructuralShrinkHorizontal;
            spcrJointDynamicsSave.IsAllStructuralStretchHorizontal = SPCRJointDynamicsContoller._IsAllStructuralStretchHorizontal;
            spcrJointDynamicsSave.IsAllShearShrink = SPCRJointDynamicsContoller._IsAllShearShrink;
            spcrJointDynamicsSave.IsAllShearStretch = SPCRJointDynamicsContoller._IsAllShearStretch;
            spcrJointDynamicsSave.IsAllBendingShrinkVertical = SPCRJointDynamicsContoller._IsAllBendingShrinkVertical;
            spcrJointDynamicsSave.IsAllBendingStretchVertical = SPCRJointDynamicsContoller._IsAllBendingStretchVertical;
            spcrJointDynamicsSave.IsAllBendingShrinkHorizontal = SPCRJointDynamicsContoller._IsAllBendingShrinkHorizontal;
            spcrJointDynamicsSave.IsAllBendingStretchHorizontal = SPCRJointDynamicsContoller._IsAllBendingStretchHorizontal;

            spcrJointDynamicsSave.IsCollideStructuralVertical = SPCRJointDynamicsContoller._IsCollideStructuralVertical;
            spcrJointDynamicsSave.IsCollideStructuralHorizontal = SPCRJointDynamicsContoller._IsCollideStructuralHorizontal;
            spcrJointDynamicsSave.IsCollideShear = SPCRJointDynamicsContoller._IsCollideShear;
            spcrJointDynamicsSave.IsCollideBendingVertical = SPCRJointDynamicsContoller._IsCollideBendingVertical;
            spcrJointDynamicsSave.IsCollideBendingHorizontal = SPCRJointDynamicsContoller._IsCollideBendingHorizontal;

            spcrJointDynamicsSave.UseLimitAngles = SPCRJointDynamicsContoller._UseLimitAngles;
            spcrJointDynamicsSave.LimitAngle = SPCRJointDynamicsContoller._LimitAngle;
            spcrJointDynamicsSave.LimitPowerCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._LimitPowerCurve);
            spcrJointDynamicsSave.LimitFromRoot = SPCRJointDynamicsContoller._LimitFromRoot;

            spcrJointDynamicsSave.PointTblIDs = new string[SPCRJointDynamicsContoller.PointTbl.Length];

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
            spcrJointDynamicsSave.IsDebugDraw_SurfaceFace = SPCRJointDynamicsContoller._IsDebugDraw_SurfaceFace;
            spcrJointDynamicsSave.Debug_SurfaceNoramlLength = SPCRJointDynamicsContoller._Debug_SurfaceNormalLength;
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

            SaveIntoBinary(spcrJointDynamicsSave);
        }

        static void UpdateIDIfSameUniqueId(ref SPCRJointDynamicsColliderSave[] spcrColliderlist)
        {
            for (int i = 0; i < spcrColliderlist.Length - 1; i++)
            {
                for (int j = i + 1; j < spcrColliderlist.Length; j++)
                {
                    if (spcrColliderlist[i].RefUniqueId.Equals(spcrColliderlist[j].RefUniqueId))
                    {
                        spcrColliderlist[i].RefUniqueId = System.Guid.NewGuid().ToString();
                    }
                }
            }
        }

        static void UpdateIDIfSameUniqueId(ref SPCRJointDynamicsPointGrabberSave[] spcrGrabberList)
        {
            for (int i = 0; i < spcrGrabberList.Length - 1; i++)
            {
                for (int j = i + 1; j < spcrGrabberList.Length; j++)
                {
                    if (spcrGrabberList[i].RefUniqueGUIID.Equals(spcrGrabberList[j].RefUniqueGUIID))
                    {
                        spcrGrabberList[i].RefUniqueGUIID = System.Guid.NewGuid().ToString();
                    }
                }
            }
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

        private static void SaveIntoBinary(SPCRJointDynamicsControllerSave spcrJointDynamicsSave)
        {
#if UNITY_EDITOR
            var filePath = EditorUtility.SaveFilePanel("Save Joint Dynamics Configuration", Application.dataPath,
                                    "SPCRJointConfigutation",
                                    "SPCR");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            try
            {
                Stream stream = File.Open(filePath, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, spcrJointDynamicsSave);
                stream.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError("ファイル保存失敗：" + e.Message);
            }
#endif//UNITY_EDITOR
        }

        private static SPCRJointDynamicsControllerSave LoadBinary()
        {
            SPCRJointDynamicsControllerSave spcrJointDynamicsSave = null;
#if UNITY_EDITOR
            string filePath = EditorUtility.OpenFilePanel("Open Joint Dynamics Configuration", Application.dataPath, "SPCR");
            if (!File.Exists(filePath))
            {
                return null;
            }
            try
            {
                Stream stream = File.Open(filePath, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();
                spcrJointDynamicsSave = (SPCRJointDynamicsControllerSave)bformatter.Deserialize(stream);
                stream.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError("ファイルの読み込み失敗：" + e.Message);
            }
#endif//UNITY_EDITOR
            return spcrJointDynamicsSave;
        }
        static System.Collections.Generic.List<Object> globalUniqueIdList;
        public static void Load(SPCRJointDynamicsController SPCRJointDynamicsContoller)
        {
            SPCRJointDynamicsControllerSave spcrJointDynamicsSave = LoadBinary();
            if (spcrJointDynamicsSave == null)
                return;

            if (string.IsNullOrEmpty(SPCRJointDynamicsContoller.Name))
                SPCRJointDynamicsContoller.Name = spcrJointDynamicsSave.name;

            GameObject RootGameObject = GameObject.Find(spcrJointDynamicsSave.rootTransformName);
            if (RootGameObject != null)
                SPCRJointDynamicsContoller._RootTransform = RootGameObject.transform;

            globalUniqueIdList = GetGlobalUniqueIdComponentList();

            if (spcrJointDynamicsSave.spcrChildJointDynamicsPointList != null)
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
                        point._UseForSurfaceCollision = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i].UseForSurfaceCollision;
                    }
                }
            }

            if (spcrJointDynamicsSave.spcrChildJointDynamicsColliderList != null)
            {
                List<SPCRJointDynamicsCollider> colliderTable = new List<SPCRJointDynamicsCollider>();
                for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsColliderList.Length; i++)
                {
                    SPCRJointDynamicsCollider point = (SPCRJointDynamicsCollider)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsCollider) && ((SPCRJointDynamicsCollider)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].RefUniqueId));
                    if (point == null)
                        point = CreateNewCollider(spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i]);
                    point.RadiusRaw = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].Radius;
                    point.HeadRadiusScale = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].HeadRadiusScale;
                    point.TailRadiusScale = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].TailRadiusScale;
                    point.HeightRaw = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].Height;
                    point.Friction = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].Friction;
                    point.PushOutRate = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].PushOutRate;
                    point._SurfaceColliderForce = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].ForceType;

                    if (!colliderTable.Contains(point))
                    {
                        colliderTable.Add(point);
                    }
                }
                if (colliderTable.Count > 0)
                    SPCRJointDynamicsContoller._ColliderTbl = colliderTable.ToArray();
            }

            if (spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList != null)
            {
                List<SPCRJointDynamicsPointGrabber> grabberList = new List<SPCRJointDynamicsPointGrabber>();
                for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList.Length; i++)
                {
                    SPCRJointDynamicsPointGrabber point = (SPCRJointDynamicsPointGrabber)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPointGrabber) && ((SPCRJointDynamicsPointGrabber)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].RefUniqueGUIID));
                    if (point == null)
                        point = CreateNewGrabber(spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i]);
                    point.IsEnabled = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].IsEnabled;
                    point.RadiusRaw = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].Radius;
                    point.Force = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i].Force;

                    grabberList.Add(point);
                }
                if (grabberList.Count > 0)
                    SPCRJointDynamicsContoller._PointGrabberTbl = grabberList.ToArray();
            }

            if (spcrJointDynamicsSave.RootPointTbl != null)
            {
                List<SPCRJointDynamicsPoint> pointList = new List<SPCRJointDynamicsPoint>();
                for (int i = 0; i < spcrJointDynamicsSave.RootPointTbl.Length; i++)
                {
                    SPCRJointDynamicsPoint point = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPoint) && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.RootPointTbl[i]));
                    if (point == null)
                        continue;
                    pointList.Add(point);
                }
                if (pointList.Count > 0)
                    SPCRJointDynamicsContoller._RootPointTbl = pointList.ToArray();
            }
            else
            {
                SPCRJointDynamicsContoller._RootPointTbl = new SPCRJointDynamicsPoint[0];
            }

            SPCRJointDynamicsContoller._UpdateTiming = spcrJointDynamicsSave.UpdateTiming;

            SPCRJointDynamicsContoller._Relaxation = spcrJointDynamicsSave.Relaxation;
            SPCRJointDynamicsContoller._SubSteps = spcrJointDynamicsSave.SubSteps;

            SPCRJointDynamicsContoller._IsEnableFloorCollision = spcrJointDynamicsSave.IsEnableFloorCollision;
            SPCRJointDynamicsContoller._FloorHeight = spcrJointDynamicsSave.FloorHeight;

            SPCRJointDynamicsContoller._IsEnableColliderCollision = spcrJointDynamicsSave.IsEnableColliderCollision;

            SPCRJointDynamicsContoller._IsCancelResetPhysics = spcrJointDynamicsSave.IsCancelResetPhysics;
            SPCRJointDynamicsContoller._IsEnableSurfaceCollision = spcrJointDynamicsSave.IsEnableSurfaceCollision;
            SPCRJointDynamicsContoller._SurfaceCollisionDivision = spcrJointDynamicsSave.SurfaceCollisionDivision;

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
            SPCRJointDynamicsContoller._BendingShrinkVertical = spcrJointDynamicsSave.BendingShrinkVertical;
            SPCRJointDynamicsContoller._BendingStretchVertical = spcrJointDynamicsSave.BendingStretchVertical;
            SPCRJointDynamicsContoller._BendingShrinkHorizontal = spcrJointDynamicsSave.BendingShrinkHorizontal;
            SPCRJointDynamicsContoller._BendingStretchHorizontal = spcrJointDynamicsSave.BendingStretchHorizontal;

            SPCRJointDynamicsContoller._IsAllStructuralShrinkVertical = spcrJointDynamicsSave.IsAllStructuralShrinkVertical;
            SPCRJointDynamicsContoller._IsAllStructuralStretchVertical = spcrJointDynamicsSave.IsAllStructuralStretchVertical;
            SPCRJointDynamicsContoller._IsAllStructuralShrinkHorizontal = spcrJointDynamicsSave.IsAllStructuralShrinkHorizontal;
            SPCRJointDynamicsContoller._IsAllStructuralStretchHorizontal = spcrJointDynamicsSave.IsAllStructuralStretchHorizontal;
            SPCRJointDynamicsContoller._IsAllShearShrink = spcrJointDynamicsSave.IsAllShearShrink;
            SPCRJointDynamicsContoller._IsAllShearStretch = spcrJointDynamicsSave.IsAllShearStretch;
            SPCRJointDynamicsContoller._IsAllBendingShrinkVertical = spcrJointDynamicsSave.IsAllBendingShrinkVertical;
            SPCRJointDynamicsContoller._IsAllBendingStretchVertical = spcrJointDynamicsSave.IsAllBendingStretchVertical;
            SPCRJointDynamicsContoller._IsAllBendingShrinkHorizontal = spcrJointDynamicsSave.IsAllBendingShrinkHorizontal;
            SPCRJointDynamicsContoller._IsAllBendingStretchHorizontal = spcrJointDynamicsSave.IsAllBendingStretchHorizontal;

            SPCRJointDynamicsContoller._IsCollideStructuralVertical = spcrJointDynamicsSave.IsCollideStructuralVertical;
            SPCRJointDynamicsContoller._IsCollideStructuralHorizontal = spcrJointDynamicsSave.IsCollideStructuralHorizontal;
            SPCRJointDynamicsContoller._IsCollideShear = spcrJointDynamicsSave.IsCollideShear;
            SPCRJointDynamicsContoller._IsCollideBendingVertical = spcrJointDynamicsSave.IsCollideBendingVertical;
            SPCRJointDynamicsContoller._IsCollideBendingHorizontal = spcrJointDynamicsSave.IsCollideBendingHorizontal;

            SPCRJointDynamicsContoller._UseLimitAngles = spcrJointDynamicsSave.UseLimitAngles;
            SPCRJointDynamicsContoller._LimitAngle = spcrJointDynamicsSave.LimitAngle;
            SPCRJointDynamicsContoller._LimitPowerCurve = GetAnimCurve(spcrJointDynamicsSave.LimitPowerCurve);
            SPCRJointDynamicsContoller._LimitFromRoot = spcrJointDynamicsSave.LimitFromRoot;

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
            SPCRJointDynamicsContoller._IsDebugDraw_SurfaceFace = spcrJointDynamicsSave.IsDebugDraw_SurfaceFace;
            SPCRJointDynamicsContoller._Debug_SurfaceNormalLength = spcrJointDynamicsSave.Debug_SurfaceNoramlLength;
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

            globalUniqueIdList.Clear();
        }

        static SPCRJointDynamicsCollider CreateNewCollider(SPCRJointDynamicsColliderSave colliderData)
        {
            GameObject colliderObject = new GameObject();
            SPCRJointDynamicsCollider collider = colliderObject.AddComponent<SPCRJointDynamicsCollider>();
            colliderData.AssignTransformData(collider.transform);
            collider.SetGUIIIde(colliderData.RefUniqueId);
            return collider;
        }

        static SPCRJointDynamicsPointGrabber CreateNewGrabber(SPCRJointDynamicsPointGrabberSave grabberData)
        {
            GameObject colliderObject = new GameObject();
            SPCRJointDynamicsPointGrabber grabber = colliderObject.AddComponent<SPCRJointDynamicsPointGrabber>();
            grabberData.AssignTransformData(grabber.transform);
            grabber.SetGUIIIde(grabberData.RefUniqueGUIID);
            return grabber;
        }

        private static List<Object> GetGlobalUniqueIdComponentList()
        {
            List<Object> globalUniqueIdList = new List<Object>();
            globalUniqueIdList.AddRange(GameObject.FindObjectsOfType(typeof(SPCRJointDynamicsPoint)));
            globalUniqueIdList.AddRange(GameObject.FindObjectsOfType(typeof(SPCRJointDynamicsCollider)));
            globalUniqueIdList.AddRange(GameObject.FindObjectsOfType(typeof(SPCRJointDynamicsPointGrabber)));
            return globalUniqueIdList;
        }
    }
}
