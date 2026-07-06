using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AudioAsset
{
	[CustomPropertyDrawer(typeof(AudioAssetSwitch.AudioAssetSwitchData))]
	public class AudioAssetSwitchDataPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			AudioAssetSwitch switchTarget = (AudioAssetSwitch) property.serializedObject.targetObject;
			AudioAssetSwitchGroup switchGroup = switchTarget.SwitchGroup;

			List<string> switchGroupOptions = switchGroup != null ? switchGroup.Options : new List<string>();

			Rect optionRect = new (position.x, position.y, position.width, EditorGUIUtility.singleLineHeight); 

			property.FindPropertyRelative("optionIndex").intValue = EditorGUI.Popup
			(
				optionRect, "Option:", property.FindPropertyRelative("optionIndex").intValue,
				switchGroupOptions.ToArray()
			);

			Rect assetRect = new
			(
				position.x, position.y + EditorGUIUtility.singleLineHeight, 
				position.width, EditorGUIUtility.singleLineHeight
			);

			EditorGUI.PropertyField
				(assetRect, property.FindPropertyRelative("asset"), new GUIContent("Asset:"));
			
			EditorGUI.EndProperty();
		}
	}
}
