using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace AudioAsset
{
	[CreateAssetMenu(fileName = "AudioStateGroup", menuName = "Hololabs/Audio/Control/State Group")]
	public class AudioAssetStateGroup : ScriptableObject
	{
		[SerializeField] 
		private float defaultTransitionTime = 1.0f;
		
		[SerializeField] 
		private List<string> states = new();
		public List<string> States => states;

		[SerializeField] 
		private List<AudioAssetStateTransition> transitions = new();

		[SerializeField, HideInInspector]
		private int currentStateIndex;
		public int CurrentStateIndex => currentStateIndex;
		public string CurrentState => currentStateIndex < states.Count ? states[currentStateIndex] : "";

		public UnityAction<AudioAssetStateGroup, string, float> OnStateChanged;

		public void SetStateName(string stateName)
		{
			if (!states.Contains(stateName))
			{
				Debug.LogWarning($"{name} does not contain a state {stateName}");
				return;
			}

			string previousState = CurrentState;
			currentStateIndex = states.IndexOf(stateName);

			float transitionTime = GetStateTransitionTime(previousState, CurrentState);
			
			OnStateChanged?.Invoke(this, CurrentState, transitionTime);
		}

		private float GetStateTransitionTime(string state1, string state2)
		{
			int state1Index = States.IndexOf(state1);
			int state2Index = States.IndexOf(state2);

			AudioAssetStateTransition transition = transitions.Find
			(
				transition =>
				transition.State1Index == state1Index && transition.State2Index == state2Index ||
				transition.State1Index == state2Index && transition.State2Index == state1Index
			);

			return transition?.TransitionTime ?? defaultTransitionTime;
		}
	}
}