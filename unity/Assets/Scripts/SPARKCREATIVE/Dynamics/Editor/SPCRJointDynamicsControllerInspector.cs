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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(SPCRJointDynamicsController))]
public class SPCRJointDynamicsControllerInspector : Editor
{
    public enum UpdateJointConnectionType
    {
        Default,
        SortNearPointXYZ,
        SortNearPointXZ,
        SortNearPointXYZ_FixedBeginEnd,
        SortNearPointXZ_FixedBeginEnd,
    }

    int _SubdivisionJointCountH = 12;
    int _SubdivisionJointCountV = 6;
    float _BoneStretchScale = 1.0f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var controller = target as SPCRJointDynamicsController;

        GUILayout.Space(8);
        controller.Name = EditorGUILayout.TextField("名称", controller.Name);

        Titlebar("基本設定", new Color(0.7f, 1.0f, 0.7f));
        controller._RootTransform = (Transform)EditorGUILayout.ObjectField(new GUIContent("親Transform"), controller._RootTransform, typeof(Transform), true);

        if (GUILayout.Button("ルートの点群自動検出", GUILayout.Height(22.0f)))
        {
            SearchRootPoints(controller);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_RootPointTbl"), new GUIContent("ルートの点群"), true);
        GUILayout.Space(5);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_ColliderTbl"), new GUIContent("コライダー"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_PointGrabberTbl"), new GUIContent("グラバー"), true);

        Titlebar("物理設定", new Color(0.7f, 1.0f, 0.7f));

        controller._UpdateTiming = (SPCRJointDynamicsController.UpdateTiming)EditorGUILayout.EnumPopup("更新タイミング", controller._UpdateTiming);
        controller._Relaxation = EditorGUILayout.IntSlider("演算繰り返し回数", controller._Relaxation, 1, 16);
        controller._SubSteps = EditorGUILayout.IntSlider("演算分割数", controller._SubSteps, 1, 16);
        
        GUILayout.Space(8);
        controller._IsCancelResetPhysics = EditorGUILayout.Toggle("物理リセットを拒否", controller._IsCancelResetPhysics);
        GUILayout.Space(8);
        controller._IsEnableColliderCollision = EditorGUILayout.Toggle("質点とコライダーの衝突判定をする", controller._IsEnableColliderCollision);
        GUILayout.Space(8);
        controller._IsEnableFloorCollision = EditorGUILayout.Toggle("質点と床の衝突判定をする", controller._IsEnableFloorCollision);
        if (controller._IsEnableFloorCollision)
        {
            controller._FloorHeight = EditorGUILayout.FloatField("床の高さ", controller._FloorHeight);
        }
        GUILayout.Space(8);
        controller._DetailHitDivideMax = EditorGUILayout.IntSlider("詳細な衝突判定の最大分割数", controller._DetailHitDivideMax, 0, 16);

        GUILayout.Space(8);
        controller._RootSlideLimit = EditorGUILayout.FloatField("ルートの最大移動距離", controller._RootSlideLimit);
        controller._RootRotateLimit = EditorGUILayout.FloatField("ルートの最大回転角", controller._RootRotateLimit);

        GUILayout.Space(8);
        controller._SpringK = EditorGUILayout.Slider("バネ係数", controller._SpringK, 0.0f, 1.0f);

        GUILayout.Space(8);
        controller._Gravity = EditorGUILayout.Vector3Field("重力", controller._Gravity);
        controller._WindForce = EditorGUILayout.Vector3Field("風力", controller._WindForce);

        GUILayout.Space(8);
        controller._MassScaleCurve = EditorGUILayout.CurveField("質量", controller._MassScaleCurve);
        controller._GravityScaleCurve = EditorGUILayout.CurveField("重力", controller._GravityScaleCurve);
        controller._ResistanceCurve = EditorGUILayout.CurveField("空気抵抗", controller._ResistanceCurve);
        controller._HardnessCurve = EditorGUILayout.CurveField("硬さ", controller._HardnessCurve);
        controller._FrictionCurve = EditorGUILayout.CurveField("摩擦", controller._FrictionCurve);

        Titlebar("拘束設定", new Color(0.7f, 1.0f, 0.7f));
        EditorGUILayout.LabelField("=============== 拘束（一括）");
        controller._AllShrinkScaleCurve = EditorGUILayout.CurveField("伸びた時縮む力", controller._AllShrinkScaleCurve);
        controller._AllStretchScaleCurve = EditorGUILayout.CurveField("縮む時伸びる力", controller._AllStretchScaleCurve);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("=============== 構成拘束（垂直）");
        if (controller._IsComputeStructuralVertical)
        {
            controller._StructuralShrinkVertical = EditorGUILayout.Slider("伸びた時縮む力", controller._StructuralShrinkVertical, 0.0f, 1.0f);
            controller._StructuralStretchVertical = EditorGUILayout.Slider("縮む時伸びる力", controller._StructuralStretchVertical, 0.0f, 1.0f);
            GUILayout.Space(5);
            controller._StructuralShrinkVerticalScaleCurve = EditorGUILayout.CurveField("伸びた時縮む力", controller._StructuralShrinkVerticalScaleCurve);
            controller._StructuralStretchVerticalScaleCurve = EditorGUILayout.CurveField("縮む時伸びる力", controller._StructuralStretchVerticalScaleCurve);
            GUILayout.Space(5);
            controller._IsAllStructuralShrinkVertical = EditorGUILayout.Toggle("伸びた時縮む力（一括設定）", controller._IsAllStructuralShrinkVertical);
            controller._IsAllStructuralStretchVertical = EditorGUILayout.Toggle("縮む時伸びる力（一括設定）", controller._IsAllStructuralStretchVertical);
        }
        else
        {
            EditorGUILayout.LabelField("※ 無効 ※");
        }

        EditorGUILayout.LabelField("=============== 構成拘束（水平）");
        if (controller._IsComputeStructuralHorizontal)
        {
            controller._StructuralShrinkHorizontal = EditorGUILayout.Slider("伸びた時縮む力", controller._StructuralShrinkHorizontal, 0.0f, 1.0f);
            controller._StructuralStretchHorizontal = EditorGUILayout.Slider("縮む時伸びる力", controller._StructuralStretchHorizontal, 0.0f, 1.0f);
            GUILayout.Space(5);
            controller._StructuralShrinkHorizontalScaleCurve = EditorGUILayout.CurveField("伸びた時縮む力", controller._StructuralShrinkHorizontalScaleCurve);
            controller._StructuralStretchHorizontalScaleCurve = EditorGUILayout.CurveField("縮む時伸びる力", controller._StructuralStretchHorizontalScaleCurve);
            GUILayout.Space(5);
            controller._IsAllStructuralShrinkHorizontal = EditorGUILayout.Toggle("伸びた時縮む力（一括設定）", controller._IsAllStructuralShrinkHorizontal);
            controller._IsAllStructuralStretchHorizontal = EditorGUILayout.Toggle("縮む時伸びる力（一括設定）", controller._IsAllStructuralStretchHorizontal);
        }
        else
        {
            EditorGUILayout.LabelField("※ 無効 ※");
        }

        EditorGUILayout.LabelField("=============== せん断拘束");
        if (controller._IsComputeShear)
        {
            controller._ShearShrink = EditorGUILayout.Slider("伸びた時縮む力", controller._ShearShrink, 0.0f, 1.0f);
            controller._ShearStretch = EditorGUILayout.Slider("縮む時伸びる力", controller._ShearStretch, 0.0f, 1.0f);
            GUILayout.Space(5);
            controller._ShearShrinkScaleCurve = EditorGUILayout.CurveField("伸びた時縮む力", controller._ShearShrinkScaleCurve);
            controller._ShearStretchScaleCurve = EditorGUILayout.CurveField("縮む時伸びる力", controller._ShearStretchScaleCurve);
            GUILayout.Space(5);
            controller._IsAllShearShrink = EditorGUILayout.Toggle("伸びた時縮む力（一括設定）", controller._IsAllShearShrink);
            controller._IsAllShearStretch = EditorGUILayout.Toggle("縮む時伸びる力（一括設定）", controller._IsAllShearStretch);
        }
        else
        {
            EditorGUILayout.LabelField("※ 無効 ※");
        }

        EditorGUILayout.LabelField("=============== 曲げ拘束（垂直）");
        if (controller._IsComputeBendingVertical)
        {
            controller._BendingingShrinkVertical = EditorGUILayout.Slider("伸びた時縮む力", controller._BendingingShrinkVertical, 0.0f, 1.0f);
            controller._BendingingStretchVertical = EditorGUILayout.Slider("縮む時伸びる力", controller._BendingingStretchVertical, 0.0f, 1.0f);
            GUILayout.Space(5);
            controller._BendingShrinkVerticalScaleCurve = EditorGUILayout.CurveField("伸びた時縮む力", controller._BendingShrinkVerticalScaleCurve);
            controller._BendingStretchVerticalScaleCurve = EditorGUILayout.CurveField("縮む時伸びる力", controller._BendingStretchVerticalScaleCurve);
            GUILayout.Space(5);
            controller._IsAllBendingingShrinkVertical = EditorGUILayout.Toggle("伸びた時縮む力（一括設定）", controller._IsAllBendingingShrinkVertical);
            controller._IsAllBendingingStretchVertical = EditorGUILayout.Toggle("縮む時伸びる力（一括設定）", controller._IsAllBendingingStretchVertical);
        }
        else
        {
            EditorGUILayout.LabelField("※ 無効 ※");
        }

        EditorGUILayout.LabelField("=============== 曲げ拘束（水平）");
        if (controller._IsComputeBendingHorizontal)
        {
            controller._BendingingShrinkHorizontal = EditorGUILayout.Slider("伸びた時縮む力", controller._BendingingShrinkHorizontal, 0.0f, 1.0f);
            controller._BendingingStretchHorizontal = EditorGUILayout.Slider("縮む時伸びる力", controller._BendingingStretchHorizontal, 0.0f, 1.0f);
            GUILayout.Space(5);
            controller._BendingShrinkHorizontalScaleCurve = EditorGUILayout.CurveField("伸びた時縮む力", controller._BendingShrinkHorizontalScaleCurve);
            controller._BendingStretchHorizontalScaleCurve = EditorGUILayout.CurveField("縮む時伸びる力", controller._BendingStretchHorizontalScaleCurve);
            GUILayout.Space(5);
            controller._IsAllBendingingShrinkHorizontal = EditorGUILayout.Toggle("伸びた時縮む力（一括設定）", controller._IsAllBendingingShrinkHorizontal);
            controller._IsAllBendingingStretchHorizontal = EditorGUILayout.Toggle("縮む時伸びる力（一括設定）", controller._IsAllBendingingStretchHorizontal);
        }
        else
        {
            EditorGUILayout.LabelField("※ 無効 ※");
        }

        Titlebar("オプション", new Color(0.7f, 1.0f, 0.7f));
        if (GUILayout.Button("物理初期化"))
        {
            controller.ResetPhysics(0.3f);
        }

        Titlebar("デバッグ表示", new Color(0.7f, 1.0f, 1.0f));
        controller._IsDebugDraw_StructuralVertical = EditorGUILayout.Toggle("垂直構造", controller._IsDebugDraw_StructuralVertical);
        controller._IsDebugDraw_StructuralHorizontal = EditorGUILayout.Toggle("水平構造", controller._IsDebugDraw_StructuralHorizontal);
        controller._IsDebugDraw_Shear = EditorGUILayout.Toggle("せん断", controller._IsDebugDraw_Shear);
        controller._IsDebugDraw_BendingVertical = EditorGUILayout.Toggle("垂直曲げ", controller._IsDebugDraw_BendingVertical);
        controller._IsDebugDraw_BendingHorizontal = EditorGUILayout.Toggle("水平曲げ", controller._IsDebugDraw_BendingHorizontal);
        controller._IsDebugDraw_RuntimeColliderBounds = EditorGUILayout.Toggle("実行中のコリジョン情報", controller._IsDebugDraw_RuntimeColliderBounds);

        Titlebar("事前設定", new Color(1.0f, 1.0f, 0.7f));
        controller._IsLoopRootPoints = EditorGUILayout.Toggle("拘束のループ", controller._IsLoopRootPoints);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("=============== 拘束の有無");
        controller._IsComputeStructuralVertical = EditorGUILayout.Toggle("拘束：垂直構造", controller._IsComputeStructuralVertical);
        controller._IsComputeStructuralHorizontal = EditorGUILayout.Toggle("拘束：水平構造", controller._IsComputeStructuralHorizontal);
        controller._IsComputeShear = EditorGUILayout.Toggle("拘束：せん断", controller._IsComputeShear);
        controller._IsComputeBendingVertical = EditorGUILayout.Toggle("拘束：垂直曲げ", controller._IsComputeBendingVertical);
        controller._IsComputeBendingHorizontal = EditorGUILayout.Toggle("拘束：水平曲げ", controller._IsComputeBendingHorizontal);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("=============== コリジョン");
        controller._IsCollideStructuralVertical = EditorGUILayout.Toggle("衝突：垂直構造", controller._IsCollideStructuralVertical);
        controller._IsCollideStructuralHorizontal = EditorGUILayout.Toggle("衝突：水平構造", controller._IsCollideStructuralHorizontal);
        controller._IsCollideShear = EditorGUILayout.Toggle("衝突：せん断", controller._IsCollideShear);
        controller._IsCollideBendingVertical = EditorGUILayout.Toggle("衝突：垂直曲げ", controller._IsCollideBendingVertical);
        controller._IsCollideBendingHorizontal = EditorGUILayout.Toggle("衝突：水平曲げ", controller._IsCollideBendingHorizontal);
        GUILayout.Space(10);

        if (GUILayout.Button("自動設定"))
        {
            controller.UpdateJointConnection();
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XYZ）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ);
            controller.UpdateJointConnection();
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XZ）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ);
            controller.UpdateJointConnection();
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XYZ：先端終端固定）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ_FixedBeginEnd);
            controller.UpdateJointConnection();
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XZ：先端終端固定）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ_FixedBeginEnd);
            controller.UpdateJointConnection();
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("拘束長さ再計算"))
        {
            controller.UpdateJointDistance();
            EditorUtility.SetDirty(controller);
        }
        {
            var bgColor = GUI.backgroundColor;
            var contentColor = GUI.contentColor;
            GUI.contentColor = Color.yellow;
            GUI.backgroundColor = new Color(0.6f, 0.0f, 0.0f);
            if (GUILayout.Button("拘束の設定を破棄"))
            {
                controller.DeleteJointConnection();
                EditorUtility.SetDirty(controller);
            }
            GUI.backgroundColor = bgColor;
            GUI.contentColor = contentColor;
        }

        Titlebar("拡張設定", new Color(1.0f, 0.7f, 0.7f));
        GUILayout.Space(3);
        _SubdivisionJointCountH = EditorGUILayout.IntSlider("水平骨分割数", _SubdivisionJointCountH, 2, 256);
        _SubdivisionJointCountV = EditorGUILayout.IntSlider("垂直骨分割数", _SubdivisionJointCountV, 2, 256);
        GUILayout.Space(3);
        if (GUILayout.Button("骨構造からポリゴンを生成する"))
        {
            CreationSubdivisionJoint(controller, _SubdivisionJointCountH, _SubdivisionJointCountV);
        }
        GUILayout.Space(3);
        _BoneStretchScale = EditorGUILayout.Slider("伸縮比率", _BoneStretchScale, -5.0f, +5.0f);
        if (GUILayout.Button("垂直方向にボーンを伸縮する"))
        {
            controller.StretchBoneLength(_BoneStretchScale);
        }

        GUILayout.Space(5);
        EditorGUILayout.LabelField("=============== 細分化");
        if (GUILayout.Button("水平の拘束を挿入"))
        {
            SubdivideVerticalChain(controller, 1);
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("垂直の拘束を挿入"))
        {
            SubdivideHorizontalChain(controller, 1);
            EditorUtility.SetDirty(controller);
        }
        if(controller._SubDivInsertedPoints.Count > 0)
        {
            if (GUILayout.Button("細分化を元に戻す"))
            {
                RemoveInsertedPoints(controller);
                EditorUtility.SetDirty(controller);
            }
            {
                var bgColor = GUI.backgroundColor;
                var contentColor = GUI.contentColor;
                GUI.contentColor = Color.yellow;
                GUI.backgroundColor = new Color(0.6f, 0.0f, 0.0f);
                if (GUILayout.Button("細分化の確定"))
                {
                    PurgeSubdivideOriginalInfo(controller);
                    EditorUtility.SetDirty(controller);
                }
                GUI.backgroundColor = bgColor;
                GUI.contentColor = contentColor;
            }
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("_SubDivInsertedPoints"), new GUIContent("追加された点群"), true);
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("_SubDivOriginalPoints"), new GUIContent("オリジナルの点群"), true);
            
            {
                var message = string.Format(
                    "分割後には自動設定を行ってください\nオリジナルの点:{0}個\n追加された点:{1}個",
                    controller._SubDivOriginalPoints.Count,
                    controller._SubDivInsertedPoints.Count);
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void Titlebar(string text, Color color)
    {
        GUILayout.Space(12);

        var backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(text);
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = backgroundColor;

        GUILayout.Space(3);
    }

    void SearchRootPoints(SPCRJointDynamicsController controller)
    {
        if (controller._RootTransform != null)
        {
            var PointList = new List<SPCRJointDynamicsPoint>();
            for (int i = 0; i < controller._RootTransform.transform.childCount; ++i)
            {
                var child = controller._RootTransform.transform.GetChild(i);
                var point = child.GetComponent<SPCRJointDynamicsPoint>();
                if (point != null)
                {
                    PointList.Add(point);
                }
            }
            controller._RootPointTbl = PointList.ToArray();
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

    void SortConstraintsHorizontalRoot(SPCRJointDynamicsController controller, UpdateJointConnectionType Type)
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
                var EdgeA = controller._RootPointTbl[0];
                for (int i = 1; i < controller._RootPointTbl.Length; ++i)
                {
                    SourcePoints.Add(controller._RootPointTbl[i]);
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
                controller._RootPointTbl = SortedPoints.ToArray();
            }
            break;
        case UpdateJointConnectionType.SortNearPointXZ:
            {
                var SourcePoints = new List<SPCRJointDynamicsPoint>();
                var EdgeA = controller._RootPointTbl[0];
                for (int i = 1; i < controller._RootPointTbl.Length; ++i)
                {
                    SourcePoints.Add(controller._RootPointTbl[i]);
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
                controller._RootPointTbl = SortedPoints.ToArray();
            }
            break;
        case UpdateJointConnectionType.SortNearPointXYZ_FixedBeginEnd:
            {
                var SourcePoints = new List<SPCRJointDynamicsPoint>();
                var EdgeA = controller._RootPointTbl[0];
                var EdgeB = controller._RootPointTbl[controller._RootPointTbl.Length - 1];
                for (int i = 1; i < controller._RootPointTbl.Length - 1; ++i)
                {
                    SourcePoints.Add(controller._RootPointTbl[i]);
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
                controller._RootPointTbl = SortedPoints.ToArray();
            }
            break;
        case UpdateJointConnectionType.SortNearPointXZ_FixedBeginEnd:
            {
                var SourcePoints = new List<SPCRJointDynamicsPoint>();
                var EdgeA = controller._RootPointTbl[0];
                var EdgeB = controller._RootPointTbl[controller._RootPointTbl.Length - 1];
                for (int i = 1; i < controller._RootPointTbl.Length - 1; ++i)
                {
                    SourcePoints.Add(controller._RootPointTbl[i]);
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
                controller._RootPointTbl = SortedPoints.ToArray();
            }
            break;
        }
    }

    void SubdivideVerticalChain(SPCRJointDynamicsController controller, int NumInsert)
    {
        var rnd = new System.Random();
        var RootList = new List<SPCRJointDynamicsPoint>(controller._RootPointTbl);
        var OriginalPoints = controller._SubDivOriginalPoints;
        var InsertedPoints = controller._SubDivInsertedPoints;
        var IsFirstSubdivide = (OriginalPoints.Count == 0);

        foreach(var rootPoint in RootList)
        {
            if(IsFirstSubdivide)
                OriginalPoints.Add(rootPoint);
            
            var parentPoint = rootPoint;

            while(parentPoint.transform.childCount > 0)
            {
                var parentTransform = parentPoint.transform;
                
                var points = parentTransform.GetComponentsInChildren<SPCRJointDynamicsPoint>();
                if(points.Length < 2)
                {
                    break;
                }

                var childPoint = points[1];
                
                if(parentPoint == childPoint)
                {
                    Debug.LogWarning("Infinite Loop!:" + parentPoint.name);
                    break;
                }

                if(IsFirstSubdivide)
                    OriginalPoints.Add(childPoint);

                var childTransform = childPoint.transform;

                SPCRJointDynamicsPoint newPoint = null;
                for(int i = 1; i <= NumInsert; i++)
                {
                    float weight = i / (NumInsert + 1.0f);

                    newPoint = CreateInterpolatedPoint(parentPoint, childPoint, weight, "VSubdiv_" + rnd.Next());
                    InsertedPoints.Add(newPoint);

                    newPoint.transform.SetParent(parentTransform);
                    parentTransform = newPoint.transform;
                }
                childTransform.SetParent(newPoint.transform);

                parentPoint = childPoint;
            }
        }
    }

    void SubdivideHorizontalChain(SPCRJointDynamicsController controller, int NumInsert)
    {
        var rnd = new System.Random();
        var RootList = new List<SPCRJointDynamicsPoint>(controller._RootPointTbl);
        var OriginalPoints = controller._SubDivOriginalPoints;
        var InsertedPoints = controller._SubDivInsertedPoints;
        var IsFirstSubdivide = (OriginalPoints.Count == 0);

        int Count = RootList.Count;
        int Start = controller._IsLoopRootPoints ? Count : (Count - 1);

        for(int iroot = Start; iroot > 0; iroot--)
        {
            var root0 = RootList[iroot % Count];
            var root1 = RootList[iroot - 1];

            for(int iin = 1; iin <= NumInsert; iin++)
            {
                var point0 = root0;
                var point1 = root1;
                var parentTransform = root0.transform.parent;

                float weight = iin / (NumInsert + 1.0f);
                SPCRJointDynamicsPoint newRoot = null;

                while(point0 != null && point1 != null)
                {
                    if(IsFirstSubdivide && iin == 1)
                    {
                        if(!controller._IsLoopRootPoints && iroot == Start)
                        {
                            OriginalPoints.Add(point0);
                        }
                        OriginalPoints.Add(point1);
                    }

                    var newPoint = CreateInterpolatedPoint(point0, point1, weight, "HSubdiv_" + rnd.Next());
                    InsertedPoints.Add(newPoint);

                    var newTransform = newPoint.transform;
                    newTransform.SetParent(parentTransform);
                    parentTransform = newTransform;
                    
                    SPCRJointDynamicsPoint[] points;

                    points = point0.transform.GetComponentsInChildren<SPCRJointDynamicsPoint>();
                    point0 = (points.Length > 1) ? points[1] : null;

                    points = point1.transform.GetComponentsInChildren<SPCRJointDynamicsPoint>();
                    point1 = (points.Length > 1) ? points[1] : null;
                    
                    if(newRoot == null)
                    {
                        newRoot = newPoint;
                        RootList.Insert(iroot, newRoot);
                    }
                }
            }
        }
        controller._RootPointTbl = RootList.ToArray();
    }

    SPCRJointDynamicsPoint CreateInterpolatedPoint(SPCRJointDynamicsPoint point0, SPCRJointDynamicsPoint point1, float weight0, string newName="SubDivPoint")
    {
        var Transform0 = point0.transform;
        var Transform1 = point1.transform;
        var pos = Vector3.Lerp(Transform0.position, Transform1.position, weight0);
        var rot = Quaternion.Slerp(Transform0.rotation, Transform1.rotation, weight0);
        var obj = new GameObject(newName);
        var newPoint = obj.AddComponent<SPCRJointDynamicsPoint>();
        var objTransform = obj.transform;
        objTransform.position = pos;
        objTransform.rotation = rot;
        return newPoint;
    }

    void RemoveInsertedPoints(SPCRJointDynamicsController controller)
    {
        var OriginalPoints = controller._SubDivOriginalPoints;
        var InsertedPoints = controller._SubDivInsertedPoints;

        if(OriginalPoints.Count == 0)
        {
            return;
        }

        controller.DeleteJointConnection();

        var originalPoints = new Dictionary<int, SPCRJointDynamicsPoint>(OriginalPoints.Count);
        foreach(var op in OriginalPoints)
        {
            int key = op.GetInstanceID();
            if(!originalPoints.ContainsKey(key))
            {
                originalPoints.Add(key, op);
            }
        }

        var rootList = new List<SPCRJointDynamicsPoint>();
        foreach(var root in controller._RootPointTbl)
        {
            if(!originalPoints.ContainsKey(root.GetInstanceID()))
            {
                continue;
            }
            
            rootList.Add(root);

            var parentPoint = root;
            var chainPoint = root;
            while(chainPoint != null)
            {
                var children = chainPoint.GetComponentsInChildren<SPCRJointDynamicsPoint>();
                if(children.Length < 2)
                {
                    break;
                }
                var childPoint = children[1];
                if(originalPoints.ContainsKey(childPoint.GetInstanceID()))
                {
                    childPoint.transform.SetParent(parentPoint.transform);
                    parentPoint = childPoint;
                }
                chainPoint = childPoint;
            }
        }

        foreach(var point in InsertedPoints)
        {
            point._RefChildPoint = null;
            point.transform.SetParent(null);
        }

        foreach(var point in InsertedPoints)
        {
            DestroyImmediate(point.gameObject);
        }

        controller._RootPointTbl = rootList.ToArray();
        controller._SubDivOriginalPoints.Clear();
        controller._SubDivInsertedPoints.Clear();
    }

    void PurgeSubdivideOriginalInfo(SPCRJointDynamicsController controller)
    {
        controller._SubDivOriginalPoints.Clear();
        controller._SubDivInsertedPoints.Clear();
    }
    
    void CreationSubdivisionJoint(SPCRJointDynamicsController Controller, int HDivCount, int VDivCount)
    {
        var VCurve = new List<CurveData>();
        var HCurve = new List<CurveData>();

        var RootTbl = Controller._RootPointTbl;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(Controller.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#else//UNITY_2018_3_OR_NEWER
        PrefabUtility.DisconnectPrefabInstance(Controller.gameObject);
#endif//UNITY_2018_3_OR_NEWER

        for (int i = 0; i < RootTbl.Length; ++i)
        {
            float Length = 0.0f;
            var Curve = new CurveData();
            var Point = RootTbl[i];
            while (Point != null)
            {
                Curve.Keys.Add(new CurveKey()
                {
                    Length = Length,
                    Value = Point.transform.position,
                });
                var NextPoint = Controller.GetChildJointDynamicsPoint(Point);
                if (NextPoint != null)
                {
                    Length += (NextPoint.transform.position - Point.transform.position).magnitude;
                }
                Point = NextPoint;
            }
            Curve.CreateSpileData(Length, false);
            VCurve.Add(Curve);
        }

        for (int v = 0; v <= VDivCount; ++v)
        {
            float Rate = (float)v / (float)VDivCount;
            var Curve = new CurveData();
            var OldPoint = VCurve[0].ComputeLinear(Rate);
            float Length = 0.0f;
            for (int i = 0; i < VCurve.Count; ++i)
            {
                var NewPoint = VCurve[i].ComputeLinear(Rate);
                Length += (NewPoint - OldPoint).magnitude;
                Curve.Keys.Add(new CurveKey()
                {
                    Length = Length,
                    Value = NewPoint,
                });
                OldPoint = NewPoint;
            }
            Curve.CreateSpileData(Length, true);
            HCurve.Add(Curve);
        }

        var RootPointList = new List<SPCRJointDynamicsPoint>();
        for (int h = 0; h < HDivCount; ++h)
        {
            float Rate = (float)h / (float)HDivCount;
            var parent = Controller._RootTransform;
            for (int i = 0; i < HCurve.Count; ++i)
            {
                var Position = HCurve[i].ComputeSpline(Rate);
                var go = new GameObject(h.ToString("D3") + "_" + i.ToString("D3"));
                var pt = go.AddComponent<SPCRJointDynamicsPoint>();
                go.transform.SetParent(parent);
                go.transform.position = Position;
                if (i == 0)
                {
                    RootPointList.Add(pt);
                }
                parent = go.transform;
            }
        }

        for (int i = 0; i < RootTbl.Length;++i)
        {
            SetTransformScaleZero(RootTbl[i].transform);
        }

        Controller._RootPointTbl = RootPointList.ToArray();
        Controller.UpdateJointConnection();

        GenerateMeshFromBoneConstraints(Controller);
    }

    void SetTransformScaleZero(Transform t)
    {
        t.localScale = Vector3.zero;
        for (int i = 0; i < t.childCount; ++i)
        {
            SetTransformScaleZero(t.GetChild(i));
        }
    }

    void GenerateMeshFromBoneConstraints(SPCRJointDynamicsController Controller)
    {
        var vertices = new List<Vector3>();
        var boneWeights = new List<BoneWeight>();
        var bindposes = new List<Matrix4x4>();

        var AllBones = new List<Transform>();
        var HorizontalBones = new List<List<Transform>>();
        for (int i = 0; i <= Controller._RootPointTbl.Length; ++i)
        {
            var verticalBones = new List<Transform>();
            var child = Controller._RootPointTbl[i % Controller._RootPointTbl.Length];
            while (child != null)
            {
                vertices.Add(child.transform.position);
                boneWeights.Add(new BoneWeight()
                {
                    boneIndex0 = AllBones.Count,
                    weight0 = 1.0f,
                });
                bindposes.Add(child.transform.worldToLocalMatrix);
                AllBones.Add(child.transform);
                verticalBones.Add(child.transform);
                child = Controller.GetChildJointDynamicsPoint(child);
            }
            HorizontalBones.Add(verticalBones);
        }

        var uvs = new List<Vector2>();
        for (int h = 0; h < HorizontalBones.Count; ++h)
        {
            for (int v = 0; v < HorizontalBones[h].Count; ++v)
            {
                uvs.Add(new Vector2(
                    (float)h / (float)(HorizontalBones.Count - 1),
                    (float)v / (float)(HorizontalBones[h].Count - 1)));
            }
        }

        int index = 0;
        var triangles = new List<int>();
        var HorizontalCount = HorizontalBones.Count;
        for (int h = 0; h < HorizontalCount - 1; ++h)
        {
            var Vertical0Count = HorizontalBones[h].Count;
            var Vertical1Count = HorizontalBones[h + 1].Count;
            if (Vertical0Count == Vertical1Count)
            {
                for (int v = 0; v < Vertical0Count - 1; ++v)
                {
                    var top0 = index;
                    var top1 = index + Vertical0Count;
                    var x0y0 = top0 + v;
                    var x1y0 = top0 + v + 1;
                    var x0y1 = top1 + v;
                    var x1y1 = top1 + v + 1;
                    triangles.Add(x0y0);
                    triangles.Add(x1y0);
                    triangles.Add(x1y1);
                    triangles.Add(x1y1);
                    triangles.Add(x0y1);
                    triangles.Add(x0y0);
                }
            }
            index += Vertical0Count;
        }

        var mesh = new Mesh();
        mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.boneWeights = boneWeights.ToArray();
        mesh.bindposes = bindposes.ToArray();
        mesh.triangles = triangles.ToArray();
        RecalculateNormals(mesh);

        var renderers = Controller.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length != 0)
        {
            var Path = AssetDatabase.GetAssetPath(renderers[renderers.Length - 1].sharedMesh);
            var DirName = System.IO.Path.GetDirectoryName(Path);
            AssetDatabase.CreateAsset(mesh, DirName + "/" + Controller.gameObject.name + "_" + Controller.Name + ".asset");
            AssetDatabase.Refresh();
        }

        var skinMesh = Controller.gameObject.GetComponent<SkinnedMeshRenderer>();
        if (skinMesh == null) skinMesh = Controller.gameObject.AddComponent<SkinnedMeshRenderer>();
        skinMesh.bones = AllBones.ToArray();
        skinMesh.sharedMesh = mesh;
        skinMesh.rootBone = Controller._RootTransform;
        var meshFilter = Controller.gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = Controller.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void RecalculateNormals(Mesh mesh)
    {
        var Normals = new Dictionary<Vector3, Vector3>();

        var triangles = mesh.triangles;

        var vertices = mesh.vertices;
        var normals = new Vector3[vertices.Length];

        for (var i = 0; i < triangles.Length; i += 3)
        {
            var i1 = triangles[i];
            var i2 = triangles[i + 1];
            var i3 = triangles[i + 2];

            var p1 = vertices[i2] - vertices[i1];
            var p2 = vertices[i3] - vertices[i1];
            var normal = Vector3.Cross(p1, p2).normalized;
            Normals[vertices[i1]] = normal;
            Normals[vertices[i2]] = normal;
            Normals[vertices[i3]] = normal;
        }

        for (int i = 0; i < vertices.Length; ++i)
        {
            normals[i] = Normals[vertices[i]].normalized;
        }

        mesh.normals = normals;
    }

    struct CurveKey
    {
        public float Length;
        public Vector3 Value;
    }

    class CurveData
    {
        public List<CurveKey> Keys = new List<CurveKey>();
        public float TotalLength = 0.0f;

        public void CreateSpileData(float Length, bool IsLoop)
        {
            TotalLength = Length;
            if (!IsLoop) return;

            var Key_p1 = new CurveKey()
            {
                Length = -(Keys[Keys.Count - 1].Value - Keys[0].Value).magnitude,
                Value = Keys[Keys.Count - 1].Value,
            };
            var Key_p2 = new CurveKey()
            {
                Length = Key_p1.Length - (Keys[Keys.Count - 2].Value - Keys[Keys.Count - 1].Value).magnitude,
                Value = Keys[Keys.Count - 2].Value,
            };
            var Key_n1 = new CurveKey()
            {
                Length = TotalLength + (Keys[Keys.Count - 1].Value - Keys[0].Value).magnitude,
                Value = Keys[0].Value,
            };
            TotalLength = Key_n1.Length;
            var Key_n2 = new CurveKey()
            {
                Length = TotalLength + (Keys[0].Value - Keys[1].Value).magnitude,
                Value = Keys[1].Value,
            };
            Keys.Insert(0, Key_p1);
            Keys.Insert(0, Key_p2);
            Keys.Add(Key_n1);
            Keys.Add(Key_n2);
        }

        void Hermite(float t, out float H1, out float H2, out float H3, out float H4)
        {
            float t2 = t * t;
            float t3 = t * t2;

            H1 = +2.0f * t3 - 3.0f * t2 + 1.0f;
            H2 = -2.0f * t3 + 3.0f * t2;
            H3 = t3 - 2.0f * t2 + t;
            H4 = t3 - t2;
        }

        Vector3 GetOut(int No)
        {
            var Key1 = Keys[No];
            var Key2 = Keys[No + 1];

            var a = 1.0f;
            var b = 1.0f;
            var d = Key2.Value - Key1.Value;

            var Key0 = Keys[No - 1];
            var t = (Key2.Length - Key1.Length) / (Key2.Length - Key0.Length);
            return t * (b * (Key1.Value - Key0.Value) + (a * d));
        }

        Vector3 GetIn(int No)
        {
            var Key0 = Keys[No];
            var Key1 = Keys[No + 1];

            var a = 1.0f;
            var b = 1.0f;
            var d = Key1.Value - Key0.Value;

            var Key2 = Keys[No + 2];
            var t = (Key1.Length - Key0.Length) / (Key2.Length - Key0.Length);
            return t * (a * (Key2.Value - Key1.Value) + (b * d));
        }

        public Vector3 ComputeLinear(float Length)
        {
            Length *= TotalLength;

            int No = 0;
            while (!((Keys[No].Length <= Length) && (Length <= Keys[No + 1].Length))) No++;

            var Key1 = Keys[No];
            var Key2 = Keys[(No + 1) % Keys.Count];

            var ValueRate = (Length - Key1.Length) / (Key2.Length - Key1.Length);
            return Vector3.Lerp(Key1.Value, Key2.Value, ValueRate);
        }

        public Vector3 ComputeSpline(float Length)
        {
            Length *= TotalLength;

            int No = 0;
            while (!((Keys[No].Length <= Length) && (Length <= Keys[No + 1].Length))) No++;

            var Key1 = Keys[No];
            var Key2 = Keys[(No + 1) % Keys.Count];

            var ValueRate = (Length - Key1.Length) / (Key2.Length - Key1.Length);
            var Out = GetOut(No);
            var In = GetIn(No);
            float H1, H2, H3, H4;
            Hermite(ValueRate, out H1, out H2, out H3, out H4);
            return (H1 * Key1.Value) + (H2 * Key2.Value) + (H3 * Out) + (H4 * In);
        }
    }
}

[InitializeOnLoad]
public class SPCRJointDynamicsSettingsWindow : EditorWindow
{
    static SPCRJointDynamicsSettingsWindow _Window;

    static SPCRJointDynamicsSettingsWindow()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (!PlayerSettings.allowUnsafeCode || (PlayerSettings.scriptingRuntimeVersion != ScriptingRuntimeVersion.Latest))
            {
                _Window = GetWindow<SPCRJointDynamicsSettingsWindow>(true);
                _Window.minSize = new Vector2(450, 200);
            }
        }
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Recommended project settings for SPCRJointDynamics:\n" +
            "PlayerSettings.allowUnsafeCode = true\n" +
            "PlayerSettings.scriptingRuntimeVersion = Latest",
            MessageType.Info);
        if (GUILayout.Button("fix Settings"))
        {
            if (!PlayerSettings.allowUnsafeCode)
            {
                PlayerSettings.allowUnsafeCode = true;
            }
            if (PlayerSettings.scriptingRuntimeVersion != ScriptingRuntimeVersion.Latest)
            {
                PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
            }

            Close();
        }
    }
}
