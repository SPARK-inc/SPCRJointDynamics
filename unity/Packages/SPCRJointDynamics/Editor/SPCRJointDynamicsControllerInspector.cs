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

namespace SPCR
{
    [CanEditMultipleObjects]
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

        public static bool Foldout(bool display, string title, Color color)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            GUI.backgroundColor = backgroundColor;

            return display;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var controller = target as SPCRJointDynamicsController;

            GUILayout.Space(8);
            controller.InspectorLang = (SPCRJointDynamicsController.eInspectorLang)EditorGUILayout.EnumPopup("言語(Language)", controller.InspectorLang);
            var Lang = (int)controller.InspectorLang;
            var TextShrink = new string[] { "伸びた時縮む力", "Shrink when stretched" }[Lang];
            var TextStretch = new string[] { "縮む時伸びる力", "Stretch when shrinking" }[Lang];
            var TextIndividual = new string[] { "個別設定", "Individual setting" }[Lang];
            var TextConstraintForce = new string[] { "拘束力", "Constraint Force" }[Lang];
            var TextCurve = new string[] { "カーブ", "Curve" }[Lang];

            controller.Name = EditorGUILayout.TextField(new string[] { "名称", "Name" }[Lang], controller.Name);

            controller.Opened_BaseSettings = Foldout(controller.Opened_BaseSettings, new string[] { "基本設定", "Basic settings" }[Lang], new Color(1.0f, 0.7f, 1.0f));
            if (controller.Opened_BaseSettings)
            {
                var _RootTransform = (Transform)EditorGUILayout.ObjectField(new GUIContent(new string[] { "親 Transform", "Parent Transform" }[Lang]), controller._RootTransform, typeof(Transform), true);
                if (controller._RootTransform != _RootTransform)
                {
                    controller._RootTransform = _RootTransform;
                    EditorUtility.SetDirty(controller);
                }

                if (GUILayout.Button(new GUIContent(new string[] { "ルートの点群自動検出", "Automatically detect the root points" }[Lang]), GUILayout.Height(22.0f)))
                {
                    SearchRootPoints(controller);
                }

                if (EditorGUILayout.PropertyField(serializedObject.FindProperty("_RootPointTbl"), new GUIContent(new string[] { "ルートの点群", "Root points" }[Lang]), true))
                {
                    EditorUtility.SetDirty(controller);
                }
                GUILayout.Space(5);

                if (EditorGUILayout.PropertyField(serializedObject.FindProperty("_PlaneLimitterTbl"), new GUIContent(new string[] { "無限平面コライダー", "Flat place colliders" }[Lang]), true))
                {
                    EditorUtility.SetDirty(controller);
                }
                if (EditorGUILayout.PropertyField(serializedObject.FindProperty("_ColliderTbl"), new GUIContent(new string[] { "コライダー", "Colliders" }[Lang]), true))
                {
                    EditorUtility.SetDirty(controller);
                }
                if (EditorGUILayout.PropertyField(serializedObject.FindProperty("_PointGrabberTbl"), new GUIContent(new string[] { "グラバー", "Grabbers" }[Lang]), true))
                {
                    EditorUtility.SetDirty(controller);
                }
            }

            controller.Opened_PhysicsSettings = Foldout(controller.Opened_PhysicsSettings, new string[] { "物理設定", "Physics settings" }[Lang], new Color(1.0f, 1.0f, 0.7f));
            if (controller.Opened_PhysicsSettings)
            {
                UpdateIntSlider(new string[] { "演算フレームレート", "Simulation Frame rate" }[Lang], controller, ref controller._StabilizationFrameRate, 15, 240);
                GUILayout.Space(8);

                UpdateIntSlider(new string[] { "演算安定化回数", "Number of Relaxation" }[Lang], controller, ref controller._Relaxation, 1, 16);
                UpdateIntSlider(new string[] { "演算細分化数", "Number of calculation steps" }[Lang], controller, ref controller._SubSteps, 1, 8);

                GUILayout.Space(8);
                UpdateToggle(new string[] { "アニメーションを参照する", "Refer to animation information" }[Lang], controller, ref controller._EnableCaptureAnimationTransform);
                if (controller._EnableCaptureAnimationTransform)
                {
                    GUILayout.Space(8);
                    UpdateSlider(new string[] { "ブレンド率", "Blend rate" }[Lang], controller, ref controller._BlendRatio, 0.0f, 1.0f);
                    UpdateToggle(new string[] { "風力でブレンド率を下げる", "Linking wind force and animation blend rate" }[Lang], controller, ref controller._EnableWindForcePowerToAnimationBlendRatio);
                    if (controller._EnableWindForcePowerToAnimationBlendRatio)
                    {
                        UpdateCurve(new string[] { "x:風力 y:ブレンド率", "x:Wind force y:Ratio" }[Lang], controller, ref controller._WindForcePowerToAnimationBlendRatioCurve);
                    }
                }

                GUILayout.Space(8);
                UpdateToggle(new string[] { "物理リセットを拒否", "Reject physics reset" }[Lang], controller, ref controller._IsCancelResetPhysics);

                //@GUILayout.Space(8);
                //@UpdateToggle(new string[] { "質点とコライダーの衝突判定をする", "Point and collider collide" }[Lang], controller, ref controller._IsEnablePointCollision);
                //@UpdateIntSlider(new string[] { "詳細な衝突判定の最大分割数", "Max divisions for collision detection" }[Lang], controller, ref controller._DetailHitDivideMax, 0, 16);

                GUILayout.Space(8);
                UpdateFloat(new string[] { "ルートの最大移動距離(秒)", "Maximum move distance of root(sec.)" }[Lang], controller, ref controller._RootSlideLimit);
                UpdateFloat(new string[] { "ルートの最大回転角(秒)", "Maximum rotate angle of root(sec.)" }[Lang], controller, ref controller._RootRotateLimit);

                GUILayout.Space(8);
                UpdateVector3(new string[] { "重力", "" }[Lang], controller, ref controller._Gravity);
                UpdateVector3(new string[] { "風力", "" }[Lang], controller, ref controller._WindForce);

                GUILayout.Space(8);
                UpdateCurve(new string[] { "質量", "Mass" }[Lang], controller, ref controller._MassScaleCurve);
                UpdateCurve(new string[] { "重力", "Gravity scale" }[Lang], controller, ref controller._GravityScaleCurve);
                UpdateCurve(new string[] { "風力", "Wind force scale" }[Lang], controller, ref controller._WindForceScaleCurve);
                UpdateCurve(new string[] { "空気抵抗", "Resistance" }[Lang], controller, ref controller._ResistanceCurve);
                UpdateCurve(new string[] { "硬さ", "Hardness" }[Lang], controller, ref controller._HardnessCurve);
                UpdateCurve(new string[] { "摩擦", "Friction" }[Lang], controller, ref controller._FrictionCurve);
            }

            controller.Opened_ConstraintSettings = Foldout(controller.Opened_ConstraintSettings, new string[] { "拘束設定", "Constraint settings" }[Lang], new Color(0.7f, 1.0f, 1.0f));
            if (controller.Opened_ConstraintSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "水平方向スライダージョイント", "Horizontal Slider Joint" }[Lang], new Color(96, 96, 96));
                UpdateCurve(new string[] { "伸び", "Expand Limit" }[Lang], controller, ref controller._SliderJointLengthCurve);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "拘束力の一括設定", "Common Parameter" }[Lang], new Color(32, 96, 96));
                EditorGUILayout.BeginVertical(GUI.skin.box);
                UpdateSlider(TextShrink, controller, ref controller._AllShrink, 0.0f, 1.0f);
                UpdateCurve(TextCurve, controller, ref controller._AllShrinkScaleCurve);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                UpdateSlider(TextStretch, controller, ref controller._AllStretch, 0.0f, 1.0f);
                UpdateCurve(TextCurve, controller, ref controller._AllStretchScaleCurve);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                if (controller._IsComputeStructuralVertical)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Titlebar(new string[] { "垂直構造", "Structural (Vertical)" }[Lang], new Color(96, 96, 96));
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextShrink, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllStructuralShrinkVertical, true);
                    if (!controller._IsAllStructuralShrinkVertical)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._StructuralShrinkVertical, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._StructuralShrinkVerticalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextStretch, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllStructuralStretchVertical, true);
                    if (!controller._IsAllStructuralStretchVertical)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateCurve(TextConstraintForce, controller, ref controller._StructuralStretchVerticalScaleCurve);
                        UpdateSlider(TextCurve, controller, ref controller._StructuralStretchVertical, 0.0f, 1.0f);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                }

                if (controller._IsComputeStructuralHorizontal)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Titlebar(new string[] { "水平構造", "Structural (Horizontal)" }[Lang], new Color(96, 96, 96));
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextShrink, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllStructuralShrinkHorizontal, true);
                    if (!controller._IsAllStructuralShrinkHorizontal)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._StructuralShrinkHorizontal, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._StructuralShrinkHorizontalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextStretch, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllStructuralStretchHorizontal, true);
                    if (!controller._IsAllStructuralStretchHorizontal)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._StructuralStretchHorizontal, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._StructuralStretchHorizontalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                }

                if (controller._IsComputeShear)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Titlebar(new string[] { "せん断", "Shear" }[Lang], new Color(96, 96, 96));
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextShrink, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllShearShrink, true);
                    if (!controller._IsAllShearShrink)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._ShearShrink, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._ShearShrinkScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextStretch, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllShearStretch, true);
                    if (!controller._IsAllShearStretch)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._ShearStretch, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._ShearStretchScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                }

                if (controller._IsComputeBendingVertical)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Titlebar(new string[] { "垂直曲げ", "Bending (Vertical)" }[Lang], new Color(96, 96, 96));
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextShrink, new Color(255, 255, 255));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllBendingShrinkVertical, true);
                    if (!controller._IsAllBendingShrinkVertical)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._BendingShrinkVertical, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._BendingShrinkVerticalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextStretch, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllBendingStretchVertical, true);
                    if (!controller._IsAllBendingStretchVertical)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._BendingStretchVertical, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._BendingStretchVerticalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                }

                if (controller._IsComputeBendingHorizontal)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Titlebar(new string[] { "水平曲げ", "Bending (Horizontal)" }[Lang], new Color(96, 96, 96));
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextShrink, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllBendingShrinkHorizontal, true);
                    if (!controller._IsAllBendingShrinkHorizontal)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._BendingShrinkHorizontal, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._BendingShrinkHorizontalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TitlebarSub(TextStretch, new Color(192, 192, 192));
                    UpdateToggle(TextIndividual, controller, ref controller._IsAllBendingStretchHorizontal, true);
                    if (!controller._IsAllBendingStretchHorizontal)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        UpdateSlider(TextConstraintForce, controller, ref controller._BendingStretchHorizontal, 0.0f, 1.0f);
                        UpdateCurve(TextCurve, controller, ref controller._BendingStretchHorizontalScaleCurve);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                }
            }

            controller.Opened_AngleLockSettings = Foldout(controller.Opened_AngleLockSettings, new string[] { "角度制限設定", "Angle lock settings" }[Lang], new Color(0.7f, 0.7f, 1.0f));
            if (controller.Opened_AngleLockSettings)
            {
                controller._UseLimitAngles = EditorGUILayout.Toggle(new string[] { "角度制限", "Angle limit" }[Lang], controller._UseLimitAngles);
                if (controller._UseLimitAngles)
                {
                    controller._LimitAngle = EditorGUILayout.IntSlider(new string[] { "制限角度", "Limit angle" }[Lang], controller._LimitAngle, 0, 180);
                    controller._LimitPowerCurve = EditorGUILayout.CurveField(new string[] { "制限力", "Limit power curve" }[Lang], controller._LimitPowerCurve);
                    controller._LimitFromRoot = EditorGUILayout.Toggle(new string[] { "ルートから角度制限", "Limit from root" }[Lang], controller._LimitFromRoot);
                }
            }

            controller.Opened_PreSettings = Foldout(controller.Opened_PreSettings, new string[] { "拘束情報事前計算", "Constraint information pre-calculation" }[Lang], new Color(1.0f, 0.7f, 0.7f));
            if (controller.Opened_PreSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                UpdateToggle(new string[] { "表面衝突", "Surface collision" }[Lang], controller, ref controller._IsEnableSurfaceCollision);
                if (controller._IsEnableSurfaceCollision)
                {
                    UpdateIntSlider(new string[] { "表面衝突分割数", "Surface collision division" }[Lang], controller, ref controller._SurfaceCollisionDivision, 1, 16);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "拘束条件", "Constraint rules" }[Lang], new Color(96, 96, 96));

                EditorGUILayout.BeginVertical(GUI.skin.box);
                UpdateToggle(new string[] { "水平拘束のループ", "Horizontal Point Loop" }[Lang], controller, ref controller._IsLoopRootPoints);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                UpdateToggle(new string[] { "垂直構造", "Structural (Vertical)" }[Lang], controller, ref controller._IsComputeStructuralVertical);
                GUILayout.Space(5);
                if (controller._IsComputeStructuralVertical)
                {
                    UpdateToggle(new string[] { "コリジョン", "Collision" }[Lang], controller, ref controller._IsCollideStructuralVertical);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                UpdateToggle(new string[] { "水平構造", "Structural (Horizontal)" }[Lang], controller, ref controller._IsComputeStructuralHorizontal);
                if (controller._IsComputeStructuralHorizontal)
                {
                    UpdateToggle(new string[] { "コリジョン", "Collision" }[Lang], controller, ref controller._IsCollideStructuralHorizontal);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                UpdateToggle(new string[] { "せん断", "Shear" }[Lang], controller, ref controller._IsComputeShear);
                if (controller._IsComputeShear)
                {
                    UpdateToggle(new string[] { "コリジョン", "Collision" }[Lang], controller, ref controller._IsCollideShear);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                UpdateToggle(new string[] { "垂直曲げ", "Bending (Vertical)" }[Lang], controller, ref controller._IsComputeBendingVertical);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                UpdateToggle(new string[] { "水平曲げ", "Bending (Horizontal)" }[Lang], controller, ref controller._IsComputeBendingHorizontal);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();

                Titlebar(new string[] { "拘束情報を計算", "Calculate constraint information" }[Lang], new Color(96, 96, 96));
                if (GUILayout.Button(new string[] { "拘束情報を事前計算", "From Root points" }[Lang]))
                {
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束情報を事前計算（近ポイント自動検索XYZ）", "Near point automatic search XYZ" }[Lang]))
                {
                    SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ);
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束情報を事前計算（近ポイント自動検索XZ）", "Near point automatic search XZ" }[Lang]))
                {
                    SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ);
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束情報を事前計算（近ポイント自動検索XYZ：先端終端固定）", "Near point automatic search XYZ (Fixed tip)" }[Lang]))
                {
                    SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ_FixedBeginEnd);
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束情報を事前計算（近ポイント自動検索XZ：先端終端固定）", "Near point automatic search XZ (Fixed tip)" }[Lang]))
                {
                    SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ_FixedBeginEnd);
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束間の長さを再計算", "Constraint length recalculation" }[Lang]))
                {
                    controller.UpdateJointDistance();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "再生中即時パラメーター反映", "Immediate parameter reflection when playing" }[Lang]))
                {
                    controller.ImmediateParameterReflection();
                    EditorUtility.SetDirty(controller);
                }
                {
                    var bgColor = GUI.backgroundColor;
                    var contentColor = GUI.contentColor;
                    GUI.contentColor = Color.yellow;
                    GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f);
                    if (GUILayout.Button(new string[] { "拘束の設定を破棄", "Discard constraint settings" }[Lang]))
                    {
                        controller.DeleteJointConnection();
                        EditorUtility.SetDirty(controller);
                    }
                    GUI.backgroundColor = bgColor;
                    GUI.contentColor = contentColor;
                }

                Titlebar(new string[] { "設定保存", "Save settings" }[Lang], new Color(1.0f, 0.7f, 0.7f));
                if (GUILayout.Button(new string[] { "設定を保存する", "Save settings" }[Lang]))
                {
                    SPCRJointSettingLocalSave.Save(controller);
                }
                if (GUILayout.Button(new string[] { "設定をロードする", "Load settings" }[Lang]))
                {
                    SPCRJointSettingLocalSave.Load(controller);
                }
            }

            controller.Opened_OptionSettings = Foldout(controller.Opened_OptionSettings, new string[] { "デバッグ", "Debug" }[Lang], new Color(0.7f, 1.0f, 0.7f));
            if (controller.Opened_OptionSettings)
            {
                if (GUILayout.Button(new string[] { "物理初期化", "Physics reset" }[Lang]))
                {
                    controller.ResetPhysics(0.3f);
                }

                GUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_IsPaused"), new GUIContent(new string[] { "一時停止", "Pause" }[Lang]), true);

                Titlebar(new string[] { "デバッグ表示", "Show debug information" }[Lang], new Color(0.7f, 1.0f, 1.0f));
                UpdateToggle(new string[] { "コライダー", "Collider" }[Lang], controller, ref controller._IsDebugDraw_Collider);
                UpdateToggle(new string[] { "ポイントギズモ", "3D point gizmo" }[Lang], controller, ref controller._IsDebugDrawPointGizmo);
                UpdateToggle(new string[] { "垂直構造", "Structural (Vertical)" }[Lang], controller, ref controller._IsDebugDraw_StructuralVertical);
                UpdateToggle(new string[] { "水平構造", "Structural (Horizontal)" }[Lang], controller, ref controller._IsDebugDraw_StructuralHorizontal);
                UpdateToggle(new string[] { "せん断", "Shear" }[Lang], controller, ref controller._IsDebugDraw_Shear);
                UpdateToggle(new string[] { "垂直曲げ", "Bending (Vertical);" }[Lang], controller, ref controller._IsDebugDraw_BendingVertical);
                UpdateToggle(new string[] { "水平曲げ", "Bending (Horizontal);" }[Lang], controller, ref controller._IsDebugDraw_BendingHorizontal);
                UpdateToggle(new string[] { "ポリゴン面", "Surface Face" }[Lang], controller, ref controller._IsDebugDraw_SurfaceFace);
                if (controller._IsDebugDraw_SurfaceFace)
                {
                    UpdateSlider(new string[] { "ポリゴン法線の長さ", "Surface normal length" }[Lang], controller, ref controller._Debug_SurfaceNormalLength, 0, 1);
                }
                UpdateToggle(new string[] { "実行中のコリジョン情報", "Runtime collider bounds" }[Lang], controller, ref controller._IsDebugDraw_RuntimeColliderBounds);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(controller);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateToggle(string Label, SPCRJointDynamicsController Source, ref bool Value, bool Reverse = false)
        {
            if (Reverse)
                Value = !EditorGUILayout.Toggle(Label, !Value);
            else
                Value = EditorGUILayout.Toggle(Label, Value);
        }

        void UpdateIntSlider(string Label, SPCRJointDynamicsController Source, ref int Value, int Min, int Max)
        {
            Value = EditorGUILayout.IntSlider(Label, Value, Min, Max);
        }

        void UpdateSlider(string Label, SPCRJointDynamicsController Source, ref float Value, float Min, float Max)
        {
            Value = EditorGUILayout.Slider(Label, Value, Min, Max);
        }

        void UpdateFloat(string Label, SPCRJointDynamicsController Source, ref float Value)
        {
            Value = EditorGUILayout.FloatField(Label, Value);
        }

        void UpdateVector3(string Label, SPCRJointDynamicsController Source, ref Vector3 Value)
        {
            Value = EditorGUILayout.Vector3Field(Label, Value);
        }

        void UpdateCurve(string Label, SPCRJointDynamicsController Source, ref AnimationCurve Value)
        {
            Value = EditorGUILayout.CurveField(Label, Value);
        }

        void Titlebar(string text, Color color)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;// color;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(text);
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = backgroundColor;

            GUILayout.Space(3);
        }

        void TitlebarSub(string text, Color color)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
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
                if (!PlayerSettings.allowUnsafeCode)
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
                "PlayerSettings.allowUnsafeCode = true\n",
                MessageType.Info);
            if (GUILayout.Button("fix Settings"))
            {
                if (!PlayerSettings.allowUnsafeCode)
                {
                    PlayerSettings.allowUnsafeCode = true;
                }

                Close();
            }
        }
    }
}
