using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AudioAsset
{
	[CustomPropertyDrawer(typeof(AudioAssetStateTransition))]
	public class AudioAssetStateTransitionPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2.0f;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			AudioAssetStateGroup stateGroupTarget = (AudioAssetStateGroup) property.serializedObject.targetObject;
			List<string> states = stateGroupTarget.States;

			List<string> stateOptions = states ?? new List<string>();

			Rect state1Rect = new 
			(
				position.x, position.y, 
				position.width * 0.5f, EditorGUIUtility.singleLineHeight
			);
			
			int state1Index = property.FindPropertyRelative("state1Index").intValue;
			property.FindPropertyRelative("state1Index").intValue = 
				EditorGUI.Popup(state1Rect, state1Index, stateOptions.ToArray());

			Rect state2Rect = new
			(
				position.x + position.width * 0.5f, position.y, 
				position.width * 0.5f, EditorGUIUtility.singleLineHeight
			);
			
			int state2Index = property.FindPropertyRelative("state2Index").intValue;
			property.FindPropertyRelative("state2Index").intValue = 
				EditorGUI.Popup(state2Rect, state2Index, stateOptions.ToArray());
			
			Rect transitionTimeRect = new
			(
				position.x, position.y + EditorGUIUtility.singleLineHeight + 2, 
				position.width, EditorGUIUtility.singleLineHeight
			);

			EditorGUI.PropertyField
			(
				transitionTimeRect, property.FindPropertyRelative("transitionTime"), 
				new GUIContent("Transition Time")
			);
			
			EditorGUI.EndProperty();
		}
	}
}