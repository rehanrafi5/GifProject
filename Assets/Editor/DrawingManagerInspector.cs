using UnityEditor;
using UnityEngine;

namespace XDPaint.Editor
{
	[CustomEditor(typeof(DrawingManager))]
	public class DrawingManagerInspector : PaintManagerInspector
	{
		private SerializedProperty _defaultToolProperty;
		private SerializedProperty _defaultColorProperty;
		private SerializedProperty _defaultBrushProperty;
		private SerializedProperty _defaultSizeProperty;
		private SerializedProperty _defaultHardnessProperty;

		protected override void OnEnable()
		{
			_defaultToolProperty = serializedObject.FindProperty("DefaultTool");
			_defaultColorProperty = serializedObject.FindProperty("DefaultColor");
			_defaultBrushProperty = serializedObject.FindProperty("DefaultBrush");
			_defaultSizeProperty = serializedObject.FindProperty("DefaultSize");
			_defaultHardnessProperty = serializedObject.FindProperty("DefaultHardness");

			base.OnEnable();
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(_defaultToolProperty, new GUIContent("Default Tool"));
			EditorGUILayout.PropertyField(_defaultColorProperty, new GUIContent("Default Color"));
			EditorGUILayout.PropertyField(_defaultBrushProperty, new GUIContent("Default Brush"));
			EditorGUILayout.PropertyField(_defaultSizeProperty, new GUIContent("Default Size"));
			EditorGUILayout.PropertyField(_defaultHardnessProperty, new GUIContent("Default Hardness"));

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}