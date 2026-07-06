using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	[CreateAssetMenu(fileName = "AudioSwitchGroup", menuName = "Hololabs/Audio/Control/Switch Group")]
	public class AudioAssetSwitchGroup : ScriptableObject
	{
		[SerializeField] 
		private List<string> options = new();
		public List<string> Options => options;
		
		[SerializeField, HideInInspector]
		private int currentOptionIndex;
		public int CurrentOptionIndex => currentOptionIndex;
		public string CurrentOption => currentOptionIndex < options.Count ? options[currentOptionIndex] : "";

		public void SetOptionName(string optionName)
		{
			if (!options.Contains(optionName))
			{
				Debug.LogWarning($"{name} does not contain an option {optionName}");
				return;
			}

			currentOptionIndex = options.IndexOf(optionName);
		}

		public void SetOptionIndex(int optionIndex)
		{
			if (optionIndex < 0 || optionIndex > options.Count)
			{
				Debug.LogWarning($"{name} does not contain an option for index {optionIndex}");
				return;
			}

			currentOptionIndex = optionIndex;
		}
	}
}