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
        controller._SpringK = EditorGUILayout.Slider("バネ係数", controller._SpringK, 0.0f, 1.0f);

        GUILayout.Space(8);
        controller._Gravity = EditorGUILayout.Vector3Field("重力", controller._Gravity);
        controller._WindForce = EditorGUILayout.Vector3Field("風力", controller._WindForce);

        GUILayout.Space(8);
        controller._MassScaleCurve = EditorGUILayout.CurveField("質量", controller._MassScaleCurve);
        controller._GravityScaleCurve = EditorGUILayout.CurveField("重力", controller._GravityScaleCurve);
        controller._ResistanceCurve = EditorGUILayout.CurveField("空気抵抗", controller._ResistanceCurve);
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
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XYZ）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ);
            controller.UpdateJointConnection();
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XZ）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ);
            controller.UpdateJointConnection();
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XYZ：先端終端固定）"))
        {
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ_FixedBeginEnd);
            controller.UpdateJointConnection();
        }
        if (GUILayout.Button("自動設定（近ポイント自動検索XZ：先端終端固定）"))
        {
            controller.UpdateJointConnection();
            SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ_FixedBeginEnd);
        }
        if (GUILayout.Button("拘束長さ再計算"))
        {
            controller.UpdateJointDistance();
        }

        Titlebar("拡張設定", new Color(1.0f, 0.7f, 0.7f));
        GUILayout.Space(3);
        _BoneStretchScale = EditorGUILayout.Slider("伸縮比率", _BoneStretchScale, -5.0f, +5.0f);
        if (GUILayout.Button("垂直方向にボーンを伸縮する"))
        {
            controller.StretchBoneLength(_BoneStretchScale);
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
