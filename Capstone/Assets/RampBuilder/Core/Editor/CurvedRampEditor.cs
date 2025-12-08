using UnityEngine;
using UnityEditor;


namespace RampCreation
{
    [CustomEditor(typeof(CurvedRampBuilder))]
    public class CurvedRampEditor : Editor
    {
        SerializedProperty heightProp;
        CurvedRampBuilder rampBuilder;

        private void OnEnable()
        {
            rampBuilder = (CurvedRampBuilder)target;
            rampBuilder.BuildRamp();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            // when editor values are changed, update the mesh
            if (GUI.changed)
            {
                rampBuilder.BuildRamp();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
