using UnityEngine;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorHexField))]
    public class HexfieldInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty displayAlpha = serializedObject.FindProperty("displayAlpha");
            SerializedProperty displayHashtag = serializedObject.FindProperty("displayHashtag");
            SerializedProperty acceptedInput = serializedObject.FindProperty("acceptedInput");

            EditorGUILayout.PropertyField(displayAlpha);
            EditorGUILayout.PropertyField(displayHashtag);

            acceptedInput.intValue = (int)(ColorHexField.HexfieldType)EditorGUILayout.EnumFlagsField(new GUIContent(acceptedInput.displayName, ""), (ColorHexField.HexfieldType)acceptedInput.intValue);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
