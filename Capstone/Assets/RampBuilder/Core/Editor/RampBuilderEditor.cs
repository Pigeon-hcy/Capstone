using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RampCreation
{
    [CustomEditor(typeof(RampBuilder))]
    public class RampBuilderEditor : Editor
    {
        Dictionary<string, SerializedProperty> name2property;
        string[] names = { "rampType", "rampCurvature", "rampTiles", "width", "endAngle", "tableTopLength", "topCurvature", "flatTakeOffLength", "deckDepth"};

        static string[] oneSided = { "rampType", "rampCurvature", "rampTiles", "width", "endAngle", "flatTakeOffLength", "deckDepth"};
        static string[] tableTop = { "rampType", "rampCurvature", "rampTiles", "width", "endAngle", "tableTopLength" };
        static string[] hill = { "rampType", "rampCurvature", "rampTiles", "width", "endAngle", "topCurvature" };

        Dictionary<int, string[]> rampType2propertyNames = new Dictionary<int, string[]>{ {0, oneSided},
        {1, tableTop}, {2, hill} };
        RampBuilder rampBuilder;
        bool drawFlightPath = false;
        private float launchSpeed = 10, drag = 1, timeStep = 0.001f, flightDuration = 3;

        private void OnEnable()
        {
            rampBuilder = (RampBuilder)target;
            rampBuilder.BuildRampSegments();

            name2property = new Dictionary<string, SerializedProperty>();
            foreach (string name in names)
            {
                name2property[name] = serializedObject.FindProperty(name);
            }
        }

        void OnSceneGUI()
        {
            // Purple color
            Handles.color = new Color(0.5f, 0, 0.5f, 1.0f);
            // Draw the flight path
            if (drawFlightPath)
            {
                Vector3[] flightPath = rampBuilder.GetFlightPath(launchSpeed, drag, timeStep, flightDuration);
                Handles.DrawAAPolyLine(5, flightPath);
            }

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string[] propertyNames = rampType2propertyNames[name2property["rampType"].enumValueIndex];
            foreach (string name in propertyNames)
            {
                EditorGUILayout.PropertyField(name2property[name]);
            }

            // Add spacing and draw a Label for the Flight Simulator section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Flight Simulator", EditorStyles.boldLabel);
            drawFlightPath = EditorGUILayout.Toggle("Draw Flight Path", drawFlightPath);
            launchSpeed = EditorGUILayout.FloatField("Launch Speed", launchSpeed);
            drag = EditorGUILayout.FloatField("Drag", drag);
            timeStep = EditorGUILayout.Slider("Time Step", timeStep, 0.0005f, 0.1f);
            flightDuration = EditorGUILayout.Slider("Flight Duration", flightDuration, 1, 10);
            

            serializedObject.ApplyModifiedProperties();
            // when editor values are changed, update the mesh
            if (GUI.changed)
            {
                rampBuilder.BuildRampSegments();
            }
        }
    }
}
