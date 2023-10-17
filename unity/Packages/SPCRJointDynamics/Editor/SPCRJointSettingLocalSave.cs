/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 *  @author Hiromoto Noriyuki <hrmtnryk@sparkfx.jp>
 *          Piyush Nitnaware <nitnaware.piyush@spark-creative.co.jp>
*/

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

        public static List<TransformData> GetParentNameList(Transform currChild, SPCRJointDynamicsController controller)
        {
            List<TransformData> parentList = new List<TransformData>();

            if (controller.transform.IsChildOf(currChild))
            {
                Transform currTrans = currChild;
                while(currTrans != controller.transform)
                {
                    TransformData transformData = new TransformData();
                    transformData.SaveTransformData(currTrans, controller);
                    parentList.Add(transformData);
                    currTrans = currTrans.parent;
                }
            }else
            {

                //Even though the transfrom is child of the current transform but it's returning a false
                Transform currTrans = currChild;
                while (currTrans != null)
                {
                    TransformData transformData = new TransformData();
                    transformData.SaveTransformData(currTrans, controller);
                    parentList.Add(transformData);

                    if (currTrans == controller.transform)
                        break;

                    currTrans = currTrans.parent;
                }
            }

            return parentList;
        }

        [System.Serializable]
        public class TransformData
        {
            public string GOName { get; set; }
            public SPCRvec3 GOPosition { get; set; }
            public SPCRvec3 GOEular { get; set; }
            public SPCRvec3 GOScaler { get; set; }

            public bool IsChildOfController = false;

            //This variable will be implemented by the inheriting class
            public List<TransformData> ParentList { get; set; }

            public void SaveTransformData(Transform transform, SPCRJointDynamicsController controller)
            {
                GOName = transform.name;
                GOPosition = new SPCRvec3(transform.position);
                GOEular = new SPCRvec3(transform.eulerAngles);
                GOScaler = new SPCRvec3(transform.localScale);

                IsChildOfController = controller.transform.IsChildOf(transform);
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
        public class SPCRJointDynamicsPointSave : TransformData
        {
            public string RefUniqueID { get; set; }
            public bool IsFixed { get; set; }
            public float mass { get; set; }
            public float pointRadius { get; set; }
            public float MovableLimitRadius { get; set; }
            public bool UseForSurfaceCollision { get; set; }
            public bool ApplyInvertCollision { get; set; }
            public string ForceChildPointRefID { get; set; }
            public string refChildID { get; set; }
            public SPCRvec3 BoneAxis { get; set; }
            public float Depth { get; set; }
            public int Index { get; set; }

            public SPCRJointDynamicsPointSave(SPCRJointDynamicsPoint spcrJointDynamicsPoint, SPCRJointDynamicsController controller)
            {
                if (spcrJointDynamicsPoint != null)
                {
                    if (string.IsNullOrEmpty(spcrJointDynamicsPoint.UniqueGUIID))
                        spcrJointDynamicsPoint.Reset();
                    RefUniqueID = spcrJointDynamicsPoint.UniqueGUIID;

                    IsFixed = spcrJointDynamicsPoint._IsFixed;
                    mass = spcrJointDynamicsPoint._Mass;
                    MovableLimitRadius = spcrJointDynamicsPoint._MovableLimitRadius;
                    pointRadius = spcrJointDynamicsPoint._PointRadius;
                    UseForSurfaceCollision = spcrJointDynamicsPoint._UseForSurfaceCollision;
                    ApplyInvertCollision = spcrJointDynamicsPoint._ApplyInvertCollision;
                    if(spcrJointDynamicsPoint._ForceChildPoint != null)
                        ForceChildPointRefID = spcrJointDynamicsPoint._ForceChildPoint.UniqueGUIID;
                    if (spcrJointDynamicsPoint._RefChildPoint != null)
                        refChildID = spcrJointDynamicsPoint._RefChildPoint.UniqueGUIID;
                    BoneAxis = new SPCRvec3(spcrJointDynamicsPoint._BoneAxis);
                    Depth = spcrJointDynamicsPoint._Depth;
                    Index = spcrJointDynamicsPoint._Index;

                    SaveTransformData(spcrJointDynamicsPoint.transform, controller);
                    ParentList = GetParentNameList(spcrJointDynamicsPoint.transform, controller);
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
            public float RadiusTailScale { get; set; }
            public float Height { get; set; }
            public float Friction { get; set; }
            public bool IsInvertCollider { get; set; }

            public SPCRJointDynamicsCollider.ColliderForce ForceType { get; set; }

            public bool ShowColliderGizmo { get; set; }

            public SPCRJointDynamicsColliderSave(SPCRJointDynamicsCollider spcrJoinDynamicsCollider, SPCRJointDynamicsController controller)
            {
                if (spcrJoinDynamicsCollider != null)
                {
                    if (string.IsNullOrEmpty(spcrJoinDynamicsCollider.UniqueGUIID))
                        spcrJoinDynamicsCollider.Reset();
                    RefUniqueId = spcrJoinDynamicsCollider.UniqueGUIID;
                    Radius = spcrJoinDynamicsCollider.RadiusRaw;
                    RadiusTailScale = spcrJoinDynamicsCollider.RadiusTailScale;
                    Height = spcrJoinDynamicsCollider.HeightRaw;
                    Friction = spcrJoinDynamicsCollider.FrictionRaw;
                    IsInvertCollider = spcrJoinDynamicsCollider._IsInverseCollider;
                    ForceType = spcrJoinDynamicsCollider._SurfaceColliderForce;

                    ShowColliderGizmo = spcrJoinDynamicsCollider._ShowColiiderGizmo;

                    SaveTransformData(spcrJoinDynamicsCollider.transform, controller);

                    ParentList = GetParentNameList(spcrJoinDynamicsCollider.transform, controller);
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

            public SPCRJointDynamicsPointGrabberSave(SPCRJointDynamicsPointGrabber spcrJointDynamicsPointGrabber, SPCRJointDynamicsController controller)
            {
                if (spcrJointDynamicsPointGrabber != null)
                {
                    if (string.IsNullOrEmpty(spcrJointDynamicsPointGrabber.UniqueGUIID))
                        spcrJointDynamicsPointGrabber.Reset();
                    RefUniqueGUIID = spcrJointDynamicsPointGrabber.UniqueGUIID;
                    IsEnabled = spcrJointDynamicsPointGrabber.IsEnabled;
                    Radius = spcrJointDynamicsPointGrabber.RadiusRaw;
                    Force = spcrJointDynamicsPointGrabber.Force;

                    SaveTransformData(spcrJointDynamicsPointGrabber.transform, controller);
                    ParentList = GetParentNameList(spcrJointDynamicsPointGrabber.transform, controller);
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
            public float Shrink;
            public float Stretch;

            public SPCRConstraintSave(SPCRJointDynamicsJob.Constraint constraint)
            {
                IsCollision = constraint.IsCollision;
                Type = (int)constraint.Type;
                IndexA = constraint.IndexA;
                IndexB = constraint.IndexB;
                Length = constraint.Length;
            }
            public SPCRJointDynamicsJob.Constraint ConvertToJobConstraint()
            {
                SPCRJointDynamicsJob.Constraint spcrConstraint = new SPCRJointDynamicsJob.Constraint();
                spcrConstraint.IsCollision = IsCollision;
                spcrConstraint.Type = (SPCRJointDynamicsController.ConstraintType)Type;
                spcrConstraint.IndexA = IndexA;
                spcrConstraint.IndexB = IndexB;
                spcrConstraint.Length = Length;
                return spcrConstraint;
            }
        }

        [System.Serializable]
        public class SPCRJointDynamicsControllerSave
        {
            public int SaveVersion = 0;

            public SPCRJointDynamicsPointSave[] spcrChildJointDynamicsPointList { get; set; }
            public SPCRJointDynamicsPointGrabberSave[] spcrChildJointDynamicsPointGtabberList;
            public SPCRJointDynamicsColliderSave[] spcrChildJointDynamicsColliderList;

            public string name { get; set; }
            public string rootTransformName { get; set; }
            public string[] RootPointTbl { get; set; }

            public int Relaxation { get; set; }
            public int SubSteps { get; set; }

            public bool IsEnableFloorCollision { get; set; }
            public float FloorHeight { get; set; }
            public bool IsEnablePointCollision { get; set; }
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

            public bool IsReferAnimation = false;

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

            public bool UseLimitAngles { get; set; }
            public int LimitAngle { get; set; }
            public bool LimitFromRoot { get; set; }
            public SPCRAnimCurveKeyFrameSave[] LimitPowerCurve;
        }

        //The version index as of 20231010
        public static int SAVE_VERSION = 2;

        public static void Save(SPCRJointDynamicsController SPCRJointDynamicsContoller)
        {
            SPCRJointDynamicsControllerSave spcrJointDynamicsSave = new SPCRJointDynamicsControllerSave();
            //The version index as of 20231010
            spcrJointDynamicsSave.SaveVersion = SAVE_VERSION;

            spcrJointDynamicsSave.name = SPCRJointDynamicsContoller.Name;
            spcrJointDynamicsSave.rootTransformName = SPCRJointDynamicsContoller._RootTransform.name;

            spcrJointDynamicsSave.spcrChildJointDynamicsPointList = new SPCRJointDynamicsPointSave[SPCRJointDynamicsContoller.PointTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller.PointTbl.Length; i++)
            {
                if (SPCRJointDynamicsContoller.PointTbl[i] == null)
                    continue;
                spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i] = new SPCRJointDynamicsPointSave(SPCRJointDynamicsContoller.PointTbl[i], SPCRJointDynamicsContoller);
            }

            spcrJointDynamicsSave.spcrChildJointDynamicsColliderList = new SPCRJointDynamicsColliderSave[SPCRJointDynamicsContoller._ColliderTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._ColliderTbl.Length; i++)
            {
                if (SPCRJointDynamicsContoller._ColliderTbl[i] == null)
                    continue;
                spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i] = new SPCRJointDynamicsColliderSave(SPCRJointDynamicsContoller._ColliderTbl[i], SPCRJointDynamicsContoller);
            }
            UpdateIDIfSameUniqueId(ref spcrJointDynamicsSave.spcrChildJointDynamicsColliderList);

            spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList = new SPCRJointDynamicsPointGrabberSave[SPCRJointDynamicsContoller._PointGrabberTbl.Length];
            for (int i = 0; i < SPCRJointDynamicsContoller._PointGrabberTbl.Length; i++)
            {
                if (SPCRJointDynamicsContoller._PointGrabberTbl[i] == null)
                    continue;
                spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i] = new SPCRJointDynamicsPointGrabberSave(SPCRJointDynamicsContoller._PointGrabberTbl[i], SPCRJointDynamicsContoller);
            }
            UpdateIDIfSameUniqueId(ref spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList);

            if (SPCRJointDynamicsContoller._RootPointTbl != null && SPCRJointDynamicsContoller._RootPointTbl.Length > 0)
            {
                spcrJointDynamicsSave.RootPointTbl = new string[SPCRJointDynamicsContoller._RootPointTbl.Length];
                for (int i = 0; i < SPCRJointDynamicsContoller._RootPointTbl.Length; i++)
                {
                    if (SPCRJointDynamicsContoller._RootPointTbl[i] == null)
                        continue;
                    spcrJointDynamicsSave.RootPointTbl[i] = SPCRJointDynamicsContoller._RootPointTbl[i].UniqueGUIID;
                }
            }

            spcrJointDynamicsSave.SubSteps = SPCRJointDynamicsContoller._SubSteps;

            //@spcrJointDynamicsSave.IsEnablePointCollision = SPCRJointDynamicsContoller._IsEnablePointCollision;
            //@spcrJointDynamicsSave.DetailHitDivideMax = SPCRJointDynamicsContoller._DetailHitDivideMax;

            spcrJointDynamicsSave.IsCancelResetPhysics = SPCRJointDynamicsContoller._IsCancelResetPhysics;

            spcrJointDynamicsSave.IsEnableSurfaceCollision = SPCRJointDynamicsContoller._IsEnableSurfaceCollision;

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

            spcrJointDynamicsSave.RootSlideLimit = SPCRJointDynamicsContoller._RootSlideLimit;
            spcrJointDynamicsSave.RootRotateLimit = SPCRJointDynamicsContoller._RootRotateLimit;

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

            spcrJointDynamicsSave.UseLimitAngles = SPCRJointDynamicsContoller._UseLimitAngles;
            spcrJointDynamicsSave.LimitAngle = SPCRJointDynamicsContoller._LimitAngle;
            spcrJointDynamicsSave.LimitFromRoot = SPCRJointDynamicsContoller._LimitFromRoot;
            spcrJointDynamicsSave.LimitPowerCurve = GetSPCRAnimaCurveKeyFrames(SPCRJointDynamicsContoller._LimitPowerCurve);


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
        public static System.Collections.Generic.List<Object> globalUniqueIdList;
        public static void Load(SPCRJointDynamicsController SPCRJointDynamicsContoller)
        {
            SPCRJointDynamicsControllerSave spcrJointDynamicsSave = LoadBinary();
            if (spcrJointDynamicsSave == null)
                return;

            globalUniqueIdList = GetGlobalUniqueIdComponentList();

            SPCRLoadOptionsWindow.ShowWindow(SPCRJointDynamicsContoller, spcrJointDynamicsSave, 
                (bool canLoad, bool loadWithOldVer, SPCRLoadOptionsWindow.HierarchyObject hierarchyRoot) =>
            {
                Debug.Log("Load settings");
                if (canLoad)
                {
                    if (loadWithOldVer)
                        //Loading with old format
                        LoadWithConfig(SPCRJointDynamicsContoller, spcrJointDynamicsSave);
                    else
                        LoadWithHierarchy(SPCRJointDynamicsContoller, spcrJointDynamicsSave, hierarchyRoot);
                    
                }
                
                globalUniqueIdList.Clear();
            });
             
        }

        //This is a new system from ver >= 2
        public static void LoadWithHierarchy(SPCRJointDynamicsController controller, SPCRJointDynamicsControllerSave spcrJointDynamicsSave, SPCRLoadOptionsWindow.HierarchyObject hierarchyRoot)
        {
            CreateHierarchy(controller.transform, hierarchyRoot);

            LoadControllerSettings(controller, spcrJointDynamicsSave);

            SPCRJointDynamicsCollider[] colliderArray = controller.gameObject.GetComponentsInChildren<SPCRJointDynamicsCollider>();
            controller._ColliderTbl = colliderArray;

            SPCRJointDynamicsPointGrabber[] grabberArray = controller.gameObject.GetComponentsInChildren<SPCRJointDynamicsPointGrabber>();
            controller._PointGrabberTbl = grabberArray;

            EditorUtility.SetDirty(controller);
        }

        static void CreateHierarchy(Transform parent, SPCRLoadOptionsWindow.HierarchyObject current)
        {

            for (int i = 0; i < current.directChild.Count; i++)
            {
                SPCRLoadOptionsWindow.HierarchyObject child = current.directChild[i];

                if (!child.isEnabled)
                    continue;

                Transform currTransform = null;
                switch (child.type)
                {
                    case SPCRLoadOptionsWindow.ItemType.Controller:
                        //if this case is true, 
                        //Means we have more than one controller
                        //And there some problem with the hierarchy creation
                        Debug.LogError("Cannout create multiple SPCRJointDynamicsController inside a SPCRJointDynamicsController");
                        break;
                    case SPCRLoadOptionsWindow.ItemType.Point:
                        {
                            SPCRJointDynamicsPointSave pointSave = (SPCRJointDynamicsPointSave)child.savedData;

                            GameObject pointGO = CreateNewOrReturnExistingGameObject(parent, child);

                            SPCRJointDynamicsPoint point = pointGO.GetComponent<SPCRJointDynamicsPoint>();
                            if (point == null)
                            {
                                point = pointGO.AddComponent<SPCRJointDynamicsPoint>();
                                pointSave.AssignTransformData(point.transform);
                                point.SetUniqueID(pointSave.RefUniqueID);
                            }

                            SPCRJointDynamicsPoint ChildPoint = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(
                            obj =>
                            obj.GetType() == typeof(SPCRJointDynamicsPoint)
                                            && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(pointSave.refChildID)
                            );
                            SPCRJointDynamicsPoint forceChildPoint = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(
                                obj =>
                                obj.GetType() == typeof(SPCRJointDynamicsPoint)
                                                && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(pointSave.ForceChildPointRefID)
                                );

                            point._RefChildPoint = ChildPoint;
                            point._ForceChildPoint = forceChildPoint;
                            point._IsFixed = pointSave.IsFixed;
                            point._Mass = pointSave.mass;
                            point._MovableLimitRadius = pointSave.MovableLimitRadius;
                            point._PointRadius = pointSave.pointRadius;
                            point._UseForSurfaceCollision = pointSave.UseForSurfaceCollision;
                            point._ApplyInvertCollision = pointSave.ApplyInvertCollision;
                            point._BoneAxis = pointSave.BoneAxis.ToUnityVector3();
                            point._Depth = pointSave.Depth;
                            point._Index = pointSave.Index;

                            currTransform = point.transform;
                        }
                        //This is a point
                        break;
                    case SPCRLoadOptionsWindow.ItemType.Collider:
                        {
                            SPCRJointDynamicsColliderSave colliderSave = (SPCRJointDynamicsColliderSave)child.savedData;
                            GameObject pointGO = CreateNewOrReturnExistingGameObject(parent, child);

                            SPCRJointDynamicsCollider collider = pointGO.GetComponent<SPCRJointDynamicsCollider>();
                            if (collider == null)
                            {
                                collider = pointGO.AddComponent<SPCRJointDynamicsCollider>();
                                colliderSave.AssignTransformData(collider.transform);
                                collider.SetUniqueID(colliderSave.RefUniqueId);
                            }

                            collider.RadiusRaw = colliderSave.Radius;
                            collider.RadiusTailScaleRaw = colliderSave.RadiusTailScale;
                            collider.HeightRaw = colliderSave.Height;
                            collider.FrictionRaw = colliderSave.Friction;
                            collider._IsInverseCollider = colliderSave.IsInvertCollider;
                            collider._SurfaceColliderForce = colliderSave.ForceType;

                            currTransform = collider.transform;
                        }
                        break;
                    case SPCRLoadOptionsWindow.ItemType.Grabber:
                        {
                            SPCRJointDynamicsPointGrabberSave grabberSave = (SPCRJointDynamicsPointGrabberSave)child.savedData;
                            GameObject pointGO = CreateNewOrReturnExistingGameObject(parent, child);

                            SPCRJointDynamicsPointGrabber grabber = pointGO.GetComponent<SPCRJointDynamicsPointGrabber>();
                            if (grabber == null)
                            {
                                grabber = pointGO.AddComponent<SPCRJointDynamicsPointGrabber>();
                                grabberSave.AssignTransformData(grabber.transform);
                                grabber.SetUniqueId(grabberSave.RefUniqueGUIID);
                            }

                            grabber.IsEnabled = grabberSave.IsEnabled;
                            grabber.RadiusRaw = grabberSave.Radius;
                            grabber.Force = grabberSave.Force;

                            currTransform = grabber.transform;
                        }
                        break;
                    case SPCRLoadOptionsWindow.ItemType.UNDEFINED:
                        //This might be an empty game without any component
                        Transform transform = parent.Find(child.name);
                        if(transform == null)
                        {
                            GameObject go = CreateNewOrReturnExistingGameObject(parent, child);
                            transform = go.transform;
                            if(child.transformData != null)
                            {
                                child.transformData.AssignTransformData(transform);
                            }

                        }
                        currTransform = transform;
                        break;
                }

                CreateHierarchy(currTransform, child);
            }
        }

        static GameObject CreateNewOrReturnExistingGameObject(Transform parent, SPCRLoadOptionsWindow.HierarchyObject current)
        {
            if(current.sceneObject == null)
            {
                //Create a new game object here and return
                GameObject newGO = new GameObject(current.name);
                newGO.transform.SetParent(parent);
                return newGO;
            }

            return (GameObject)current.sceneObject;
        }

        //This is a load system for old saved files
        public static void LoadWithConfig(SPCRJointDynamicsController SPCRJointDynamicsContoller, SPCRJointDynamicsControllerSave spcrJointDynamicsSave)
        {
            if (string.IsNullOrEmpty(SPCRJointDynamicsContoller.Name))
                SPCRJointDynamicsContoller.Name = spcrJointDynamicsSave.name;

            Transform rootGameTrans = null;

            if (spcrJointDynamicsSave.rootTransformName.Equals(SPCRJointDynamicsContoller.transform.name))
            {
                rootGameTrans = SPCRJointDynamicsContoller.transform;
            }
            else
            {
                rootGameTrans = SPCRJointDynamicsContoller.transform.Find(spcrJointDynamicsSave.rootTransformName);
            }

            if (rootGameTrans == null)
            {
                GameObject rootGameGO = GameObject.Find(spcrJointDynamicsSave.rootTransformName);
                if (rootGameGO == null)
                    rootGameTrans = rootGameGO.transform;
            }

            if (rootGameTrans != null)
                SPCRJointDynamicsContoller._RootTransform = rootGameTrans;

            if (spcrJointDynamicsSave.spcrChildJointDynamicsPointList != null)
            {
                for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsPointList.Length; i++)
                {
                    SPCRJointDynamicsPointSave pointSave = spcrJointDynamicsSave.spcrChildJointDynamicsPointList[i];

                    SPCRJointDynamicsPoint point = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(
                        obj =>
                        obj.GetType() == typeof(SPCRJointDynamicsPoint)
                                        && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(pointSave.RefUniqueID)
                        );
                    SPCRJointDynamicsPoint ChildPoint = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(
                        obj =>
                        obj.GetType() == typeof(SPCRJointDynamicsPoint)
                                        && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(pointSave.refChildID)
                        );
                    SPCRJointDynamicsPoint forceChildPoint = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(
                        obj =>
                        obj.GetType() == typeof(SPCRJointDynamicsPoint)
                                        && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(pointSave.ForceChildPointRefID)
                        );

                    if (point != null)
                    {
                        point._RefChildPoint = ChildPoint;
                        point._ForceChildPoint = forceChildPoint;
                        point._IsFixed = pointSave.IsFixed;
                        point._Mass = pointSave.mass;
                        point._MovableLimitRadius = pointSave.MovableLimitRadius;
                        point._PointRadius = pointSave.pointRadius;
                        point._UseForSurfaceCollision = pointSave.UseForSurfaceCollision;
                        point._ApplyInvertCollision = pointSave.ApplyInvertCollision;
                        point._BoneAxis = pointSave.BoneAxis.ToUnityVector3();
                        point._Depth = pointSave.Depth;
                        point._Index = pointSave.Index;
                    }
                }
            }

            if (spcrJointDynamicsSave.spcrChildJointDynamicsColliderList != null)
            {
                List<SPCRJointDynamicsCollider> colliderTable = new List<SPCRJointDynamicsCollider>();
                for (int i = 0; i < spcrJointDynamicsSave.spcrChildJointDynamicsColliderList.Length; i++)
                {
                    SPCRJointDynamicsColliderSave colliderSave = spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i];
                    SPCRJointDynamicsCollider point = (SPCRJointDynamicsCollider)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsCollider) && ((SPCRJointDynamicsCollider)obj).UniqueGUIID.Equals(spcrJointDynamicsSave.spcrChildJointDynamicsColliderList[i].RefUniqueId));
                    if (point == null)
                        point = CreateNewCollider(colliderSave);
                    point.RadiusRaw = colliderSave.Radius;
                    point.RadiusTailScaleRaw = colliderSave.RadiusTailScale;
                    point.HeightRaw = colliderSave.Height;
                    point.FrictionRaw = colliderSave.Friction;
                    point._IsInverseCollider = colliderSave.IsInvertCollider;
                    point._SurfaceColliderForce = colliderSave.ForceType;

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
                    SPCRJointDynamicsPointGrabberSave grabberSave = spcrJointDynamicsSave.spcrChildJointDynamicsPointGtabberList[i];

                    SPCRJointDynamicsPointGrabber point = (SPCRJointDynamicsPointGrabber)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPointGrabber) && ((SPCRJointDynamicsPointGrabber)obj).UniqueGUIID.Equals(grabberSave.RefUniqueGUIID));
                    if (point == null)
                        point = CreateNewGrabber(grabberSave);
                    point.IsEnabled = grabberSave.IsEnabled;
                    point.RadiusRaw = grabberSave.Radius;
                    point.Force = grabberSave.Force;

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

            //@SPCRJointDynamicsContoller._IsEnablePointCollision = spcrJointDynamicsSave.IsEnablePointCollision;
            //@SPCRJointDynamicsContoller._DetailHitDivideMax = spcrJointDynamicsSave.DetailHitDivideMax;

            LoadControllerSettings(SPCRJointDynamicsContoller, spcrJointDynamicsSave);

            globalUniqueIdList.Clear();
        }

        static void LoadControllerSettings(SPCRJointDynamicsController SPCRJointDynamicsContoller, SPCRJointDynamicsControllerSave spcrJointDynamicsSave)
        {
            SPCRJointDynamicsContoller._IsCancelResetPhysics = spcrJointDynamicsSave.IsCancelResetPhysics;
            SPCRJointDynamicsContoller._IsEnableSurfaceCollision = spcrJointDynamicsSave.IsEnableSurfaceCollision;

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

            SPCRJointDynamicsContoller._RootSlideLimit = spcrJointDynamicsSave.RootSlideLimit;
            SPCRJointDynamicsContoller._RootRotateLimit = spcrJointDynamicsSave.RootRotateLimit;

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

            SPCRJointDynamicsContoller._UseLimitAngles = spcrJointDynamicsSave.UseLimitAngles;
            SPCRJointDynamicsContoller._LimitAngle = spcrJointDynamicsSave.LimitAngle;
            SPCRJointDynamicsContoller._LimitFromRoot = spcrJointDynamicsSave.LimitFromRoot;
            SPCRJointDynamicsContoller._LimitPowerCurve = GetAnimCurve(spcrJointDynamicsSave.LimitPowerCurve);

            SPCRJointDynamicsContoller.SearchRootPoints();
        }

        static SPCRJointDynamicsCollider CreateNewCollider(SPCRJointDynamicsColliderSave colliderData)
        {
            GameObject colliderObject = new GameObject();
            SPCRJointDynamicsCollider collider = colliderObject.AddComponent<SPCRJointDynamicsCollider>();
            colliderData.AssignTransformData(collider.transform);
            collider.SetUniqueID(colliderData.RefUniqueId);
            return collider;
        }

        static SPCRJointDynamicsPointGrabber CreateNewGrabber(SPCRJointDynamicsPointGrabberSave grabberData)
        {
            GameObject colliderObject = new GameObject();
            SPCRJointDynamicsPointGrabber grabber = colliderObject.AddComponent<SPCRJointDynamicsPointGrabber>();
            grabberData.AssignTransformData(grabber.transform);
            grabber.SetUniqueId(grabberData.RefUniqueGUIID);
            return grabber;
        }

        public static List<Object> GetGlobalUniqueIdComponentList()
        {
            List<Object> globalUniqueIdList = new List<Object>();
            globalUniqueIdList.AddRange(GameObject.FindObjectsOfType(typeof(SPCRJointDynamicsPoint)));
            globalUniqueIdList.AddRange(GameObject.FindObjectsOfType(typeof(SPCRJointDynamicsCollider)));
            globalUniqueIdList.AddRange(GameObject.FindObjectsOfType(typeof(SPCRJointDynamicsPointGrabber)));
            return globalUniqueIdList;
        }
    }
}
