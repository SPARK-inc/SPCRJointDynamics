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
using UnityEditor;

namespace SPCR
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SPCRJointDynamicsCollider))]
    public class SPCRJointDynamicsColliderEditor : Editor
    {
        public bool HasFrameBounds()
        {
            return true;
        }

        public Bounds OnGetFrameBounds()
        {
            SPCRJointDynamicsCollider jointCollider = (SPCRJointDynamicsCollider)target;
            Bounds bounds = new Bounds(jointCollider.transform.position, new Vector3(jointCollider.Radius, jointCollider.Height, jointCollider.Radius));
            return bounds;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var collider = target as SPCRJointDynamicsCollider;

            Titlebar("一般設定", new Color(0.7f, 1.0f, 1.0f));
            UpdateSlider("Radius", collider, ref collider._Radius, 0.0f, 5.0f);
            UpdateSlider("Head Radius Scale", collider, ref collider._HeadRadiusScale, 0.0f, 5.0f);
            UpdateSlider("Tail Radius Scale", collider, ref collider._TailRadiusScale, 0.0f, 5.0f);
            UpdateSlider("Height", collider, ref collider._Height, 0.0f, 5.0f);
            UpdateSlider("Friction", collider, ref collider._Friction, 0.0f, 1.0f);
            UpdateSlider("Push Out Rate", collider, ref collider._PushOutRate, 0.0f, 1.0f);

            if (collider.IsCapsule)
            {
                Titlebar("表面衝突", new Color(1.0f, 0.7f, 0.7f));
                collider._SurfaceColliderForce = (SPCRJointDynamicsCollider.ColliderForce)EditorGUILayout.EnumPopup(new GUIContent("Force Type", "表面がコライダーに刺さった時のフォース"), collider._SurfaceColliderForce);
            }

            Titlebar("デバッグ", new Color(0.5f, 0.5f, 0.5f));
            UpdateToggle("Gizmo", collider, ref collider._ShowColiiderGizmo);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(collider);
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

        void UpdateSlider(string Label, SPCRJointDynamicsCollider Source, ref float Value, float Min, float Max)
        {
            EditorGUI.BeginChangeCheck();
            float Newvalue = EditorGUILayout.Slider(Label, Value, Min, Max);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "SPCRColliderUndo");
                Value = Newvalue;
            }
        }

        void UpdateToggle(string Label, SPCRJointDynamicsCollider Source, ref bool Value, bool Reverse = false)
        {
            if (Reverse)
                Value = !EditorGUILayout.Toggle(Label, !Value);
            else
                Value = EditorGUILayout.Toggle(Label, Value);
        }
    }
}
