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
                var _UpdateTiming = (SPCRJointDynamicsController.UpdateTiming)EditorGUILayout.EnumPopup(new string[] { "更新タイミング", "Update timing" }[Lang], controller._UpdateTiming);
                if (controller._UpdateTiming != _UpdateTiming)
                {
                    controller._UpdateTiming = _UpdateTiming;
                    EditorUtility.SetDirty(controller);
                }

                if (controller._UpdateTiming == SPCRJointDynamicsController.UpdateTiming.LateUpdate)
                    UpdateToggle(new string[] { "LateUpdate安定化", "LateUpdate stabilization" }[Lang], controller, ref controller._IsLateUpdateStabilization);

                UpdateIntSlider(new string[] { "演算安定化回数", "Number of Relaxation" }[Lang], controller, ref controller._Relaxation, 1, 16);
                UpdateIntSlider(new string[] { "演算分割数", "Number of calculation steps" }[Lang], controller, ref controller._SubSteps, 1, 16);

                GUILayout.Space(8);
                UpdateToggle(new string[] { "物理リセットを拒否", "Reject physics reset" }[Lang], controller, ref controller._IsCancelResetPhysics);
                GUILayout.Space(8);
                UpdateToggle(new string[] { "質点とコライダーの衝突判定をする", "Point and collider collide" }[Lang], controller, ref controller._IsEnableColliderCollision);
                GUILayout.Space(8);
                UpdateToggle(new string[] { "質点と床の衝突判定をする", "Point and floor collide" }[Lang], controller, ref controller._IsEnableFloorCollision);
                if (controller._IsEnableFloorCollision)
                {
                    UpdateFloat(new string[] { "床の高さ", "Floor height" }[Lang], controller, ref controller._FloorHeight);
                }
                GUILayout.Space(8);
                UpdateIntSlider(new string[] { "詳細な衝突判定の最大分割数", "Max divisions for collision detection" }[Lang], controller, ref controller._DetailHitDivideMax, 0, 16);

                GUILayout.Space(8);
                UpdateToggle(new string[] { "表面衝突", "Surface collision" }[Lang], controller, ref controller._IsEnableSurfaceCollision);
                if (controller._IsEnableSurfaceCollision)
                {
                    UpdateIntSlider(new string[] { "表面衝突分割数", "Surface collision division" }[Lang], controller, ref controller._SurfaceCollisionDivision, 1, 16);
                }

                GUILayout.Space(8);
                UpdateFloat(new string[] { "ルートの最大移動距離", "Maximum move distance of root" }[Lang], controller, ref controller._RootSlideLimit);
                UpdateFloat(new string[] { "ルートの最大回転角", "Maximum rotate angle of root" }[Lang], controller, ref controller._RootRotateLimit);

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

                GUILayout.Space(8);
                UpdateToggle(new string[] { "アニメーションを参照する", "Refer to animation" }[Lang], controller, ref controller._IsReferToAnimation);
            }

            controller.Opened_ConstraintSettings = Foldout(controller.Opened_ConstraintSettings, new string[] { "拘束設定", "Constraint settings" }[Lang], new Color(0.7f, 1.0f, 1.0f));
            if (controller.Opened_ConstraintSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                UpdateToggle(new string[] { "水平拘束のループ", "Horizontal Point Loop" }[Lang], controller, ref controller._IsLoopRootPoints);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "水平方向スライダージョイント", "Horizontal Slider Joint" }[Lang], new Color(96, 96, 96));
                UpdateCurve(new string[] { "伸び", "Expand Limit" }[Lang], controller, ref controller._SliderJointLengthCurve);
                UpdateCurve(new string[] { "バネ", "Spring" }[Lang], controller, ref controller._SliderJointSpringCurve);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "拘束力の一括設定", "Common Parameter" }[Lang], new Color(96, 96, 96));
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
                
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "垂直構造", "Structural (Vertical)" }[Lang], new Color(96, 96, 96));
                if (controller._IsComputeStructuralVertical)
                {
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
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "水平構造", "Structural (Horizontal)" }[Lang], new Color(96, 96, 96));
                if (controller._IsComputeStructuralHorizontal)
                {
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
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "せん断", "Shear" }[Lang], new Color(96, 96, 96));
                if (controller._IsComputeShear)
                {
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
                }
                EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "垂直曲げ", "Bending (Vertical)" }[Lang], new Color(96, 96, 96));
                if (controller._IsComputeBendingVertical)
                {
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
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "水平曲げ", "Bending (Horizontal)" }[Lang], new Color(96, 96, 96));
                if (controller._IsComputeBendingHorizontal)
                {
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
                }
                EditorGUILayout.EndVertical();
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

            controller.Opened_OptionSettings = Foldout(controller.Opened_OptionSettings, new string[] { "オプション", "Option" }[Lang], new Color(0.7f, 1.0f, 0.7f));
            if (controller.Opened_OptionSettings)
            {
                if (GUILayout.Button(new string[] { "物理初期化", "Physics reset" }[Lang]))
                {
                    controller.ResetPhysics(0.3f);
                }

                GUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_IsPaused"), new GUIContent(new string[] { "一時停止", "Pause" }[Lang]), true);

                Titlebar(new string[] { "デバッグ表示", "Show debug information" }[Lang], new Color(0.7f, 1.0f, 1.0f));
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

            controller.Opened_PreSettings = Foldout(controller.Opened_PreSettings, new string[] { "拘束情報事前計算", "Constraint information pre-calculation" }[Lang], new Color(1.0f, 0.7f, 0.7f));
            if (controller.Opened_PreSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Titlebar(new string[] { "拘束条件", "Constraint rules" }[Lang], new Color(96, 96, 96));

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
                if (controller._IsComputeBendingVertical)
                {
                    UpdateToggle(new string[] { "コリジョン", "Collision" }[Lang], controller, ref controller._IsCollideBendingVertical);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                UpdateToggle(new string[] { "水平曲げ", "Bending (Horizontal)" }[Lang], controller, ref controller._IsComputeBendingHorizontal);
                if (controller._IsComputeBendingHorizontal)
                {
                    UpdateToggle(new string[] { "コリジョン", "Collision" }[Lang], controller, ref controller._IsCollideBendingHorizontal);
                }
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
                if (GUILayout.Button(new string[] { "拘束情報を事前計算", "Near point automatic search XYZ (Fixed tip)" }[Lang]))
                {
                    SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXYZ_FixedBeginEnd);
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束情報を事前計算（近ポイント自動検索XYZ：先端終端固定）", "Near point automatic search XZ (Fixed tip)" }[Lang]))
                {
                    SortConstraintsHorizontalRoot(controller, UpdateJointConnectionType.SortNearPointXZ_FixedBeginEnd);
                    controller.UpdateJointConnection();
                    EditorUtility.SetDirty(controller);
                }
                if (GUILayout.Button(new string[] { "拘束情報を事前計算（近ポイント自動検索XZ：先端終端固定）", "Constraint length recalculation" }[Lang]))
                {
                    controller.UpdateJointDistance();
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

                GUILayout.Space(5);

                Titlebar(new string[] { "細分化", "Subdivision" }[Lang], new Color(0.7f, 1.0f, 0.7f));
                if (PrefabUtility.IsPartOfAnyPrefab(controller.gameObject))
                {
                    EditorGUILayout.HelpBox(new string[] { "UnpackされていないPrefabは細分化できません", "Unpacked Prefabs cannot be subdivided" }[Lang], MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button(new string[] { "垂直の拘束を挿入", "Insert vertical constraint" }[Lang]))
                    {
                        SubdivideVerticalChain(controller, 1);
                        EditorUtility.SetDirty(controller);
                    }
                    if (GUILayout.Button(new string[] { "水平の拘束を挿入", "Insert horizontal constraint" }[Lang]))
                    {
                        SubdivideHorizontalChain(controller, 1);
                        EditorUtility.SetDirty(controller);
                    }
                    if (controller._SubDivInsertedPoints.Count > 0)
                    {
                        if (GUILayout.Button(new string[] { "細分化を元に戻す", "Undo subdivision" }[Lang]))
                        {
                            RemoveInsertedPoints(controller);
                            EditorUtility.SetDirty(controller);
                        }
                        {
                            var bgColor = GUI.backgroundColor;
                            var contentColor = GUI.contentColor;
                            GUI.contentColor = Color.yellow;
                            GUI.backgroundColor = new Color(0.6f, 0.0f, 0.0f);
                            if (GUILayout.Button(new string[] { "細分化の確定", "Confirmation of subdivision" }[Lang]))
                            {
                                PurgeSubdivideOriginalInfo(controller);
                                EditorUtility.SetDirty(controller);
                            }
                            GUI.backgroundColor = bgColor;
                            GUI.contentColor = contentColor;
                        }

                        {
                            var message = string.Format(
                                new string[] {
                                    "分割後には自動設定を行ってください\nオリジナルの点:{0}個\n追加された点:{1}個",
                                    "Please set automatically after splitting\nOriginal points: {0}\nAdded points: {1}" }[Lang],
                                controller._SubDivOriginalPoints.Count,
                                controller._SubDivInsertedPoints.Count);
                            EditorGUILayout.HelpBox(message, MessageType.Warning);
                        }
                    }
                }
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
            GUI.backgroundColor = color;

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

        void SubdivideVerticalChain(SPCRJointDynamicsController controller, int NumInsert)
        {
            var rnd = new System.Random();
            var RootList = new List<SPCRJointDynamicsPoint>(controller._RootPointTbl);
            var OriginalPoints = controller._SubDivOriginalPoints;
            var InsertedPoints = controller._SubDivInsertedPoints;
            var IsFirstSubdivide = (OriginalPoints.Count == 0);

            foreach (var rootPoint in RootList)
            {
                if (IsFirstSubdivide)
                    OriginalPoints.Add(rootPoint);

                var parentPoint = rootPoint;

                while (parentPoint.transform.childCount > 0)
                {
                    var parentTransform = parentPoint.transform;

                    var points = parentTransform.GetComponentsInChildren<SPCRJointDynamicsPoint>();
                    if (points.Length < 2)
                    {
                        break;
                    }

                    var childPoint = points[1];

                    if (parentPoint == childPoint)
                    {
                        Debug.LogWarning("Infinite Loop!:" + parentPoint.name);
                        break;
                    }

                    if (IsFirstSubdivide)
                        OriginalPoints.Add(childPoint);

                    var childTransform = childPoint.transform;

                    SPCRJointDynamicsPoint newPoint = null;
                    for (int i = 1; i <= NumInsert; i++)
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

            for (int iroot = Start; iroot > 0; iroot--)
            {
                var root0 = RootList[iroot % Count];
                var root1 = RootList[iroot - 1];

                for (int iin = 1; iin <= NumInsert; iin++)
                {
                    var point0 = root0;
                    var point1 = root1;
                    var parentTransform = root0.transform.parent;

                    float weight = iin / (NumInsert + 1.0f);
                    SPCRJointDynamicsPoint newRoot = null;

                    while (point0 != null && point1 != null)
                    {
                        if (IsFirstSubdivide && iin == 1)
                        {
                            if (!controller._IsLoopRootPoints && iroot == Start)
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

                        if (newRoot == null)
                        {
                            newRoot = newPoint;
                            RootList.Insert(iroot, newRoot);
                        }
                    }
                }
            }
            controller._RootPointTbl = RootList.ToArray();
        }

        SPCRJointDynamicsPoint CreateInterpolatedPoint(SPCRJointDynamicsPoint point0, SPCRJointDynamicsPoint point1, float weight0, string newName = "SubDivPoint")
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

            if (OriginalPoints.Count == 0)
            {
                return;
            }

            controller.DeleteJointConnection();

            var originalPoints = new Dictionary<int, SPCRJointDynamicsPoint>(OriginalPoints.Count);
            foreach (var op in OriginalPoints)
            {
                int key = op.GetInstanceID();
                if (!originalPoints.ContainsKey(key))
                {
                    originalPoints.Add(key, op);
                }
            }

            var rootList = new List<SPCRJointDynamicsPoint>();
            foreach (var root in controller._RootPointTbl)
            {
                if (!originalPoints.ContainsKey(root.GetInstanceID()))
                {
                    continue;
                }

                rootList.Add(root);

                var parentPoint = root;
                var chainPoint = root;
                while (chainPoint != null)
                {
                    var children = chainPoint.GetComponentsInChildren<SPCRJointDynamicsPoint>();
                    if (children.Length < 2)
                    {
                        break;
                    }
                    var childPoint = children[1];
                    if (originalPoints.ContainsKey(childPoint.GetInstanceID()))
                    {
                        childPoint.transform.SetParent(parentPoint.transform);
                        parentPoint = childPoint;
                    }
                    chainPoint = childPoint;
                }
            }

            foreach (var point in InsertedPoints)
            {
                point._RefChildPoint = null;
                point.transform.SetParent(null);
            }

            foreach (var point in InsertedPoints)
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

            for (int i = 0; i < RootTbl.Length; ++i)
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
