using UnityEngine;
using UnityEditor;

namespace SPCR
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SPCRJointDynamicsPoint))]
    public class SPCRJointDynamicsPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var point = target as SPCRJointDynamicsPoint;

            Titlebar("物理設定項目", new Color(1.5f, 0.7f, 0.5f));
            UpdateToggle("IsFixed", point, ref point._IsFixed);
            UpdateFloat("Mass", point, ref point._Mass);
            UpdateFloat("Movable limit radius", point, ref point._MovableLimitRadius);
            UpdateFloat("Radius", point, ref point._PointRadius);

            Titlebar("コライダー用", new Color(0.5f, 1f, 0.5f));
            UpdateToggle("Apply Surface Collision", point, ref point._UseForSurfaceCollision);
            UpdateToggle("Apply Invert Collision", point, ref point._ApplyInvertCollision);

            Titlebar("子ポイント", new Color(0.7f, 1.0f, 1.0f));
            var childPoint = (SPCRJointDynamicsPoint)EditorGUILayout.ObjectField("Force child point", point._ForceChildPoint, typeof(SPCRJointDynamicsPoint), true);

            if (childPoint != point._ForceChildPoint)
            {
                point._ForceChildPoint = childPoint;
                EditorUtility.SetDirty(point);
            }

            Titlebar("デバッグ用", new Color(0.7f, 1.0f, 1.0f));

            UpdateToggle("Point Radius", point, ref point._DebugDrawPointRadius);
            if (GUI.changed)
            {
                EditorUtility.SetDirty(point);
            }
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

        void UpdateSlider(string Label, SPCRJointDynamicsPoint Source, ref float Value, float Min, float Max)
        {
            EditorGUI.BeginChangeCheck();
            float Newvalue = EditorGUILayout.Slider(Label, Value, Min, Max);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "SPCRColliderUndo");
                Value = Newvalue;
            }
        }

        void UpdateToggle(string Label, SPCRJointDynamicsPoint Source, ref bool Value, bool Reverse = false)
        {
            if (Reverse)
                Value = !EditorGUILayout.Toggle(Label, !Value);
            else
                Value = EditorGUILayout.Toggle(Label, Value);
        }

        void UpdateFloat(string Label, SPCRJointDynamicsPoint Source, ref float Value)
        {
            Value = EditorGUILayout.FloatField(Label, Value);
        }
    }
}
