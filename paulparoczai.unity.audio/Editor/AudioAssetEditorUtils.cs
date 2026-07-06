using System.IO;
using UnityEditor;
using UnityEngine;

namespace AudioAsset
{
	public class AudioAssetEditorUtils
	{
		#region AssetCreation
		[MenuItem("Assets/Create/AudioAsset/Asset/Clip")]
		static void CreateAudioAsset()
		{
			Object[] selectedAudioClips = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);

			if (selectedAudioClips.Length <= 0)
			{
				AudioAssetClip assetClip = ScriptableObject.CreateInstance<AudioAssetClip>();
				string path = $"{AssetDatabase.GetAssetPath(Selection.activeObject)}/AC.asset";

				CreateAsset(path, assetClip);
			}

			foreach (AudioClip audioClip in selectedAudioClips)
			{
				CreateAudioAssetClipFromUnityAudioClip(audioClip);
			}
		}

		private static AudioAssetClip CreateAudioAssetClipFromUnityAudioClip(AudioClip audioClip)
		{
			string selectionPath = AssetDatabase.GetAssetPath(audioClip);
			string selectionDirectory = Path.GetDirectoryName(selectionPath);
			string selectionFilename = Path.GetFileName(selectionPath);
			
			string path = $"{selectionDirectory}/AC-{selectionFilename}.asset";
			
			AudioAssetClip audioAssetClip = ScriptableObject.CreateInstance<AudioAssetClip>();
			audioAssetClip.AudioClip = audioClip;

			CreateAsset(path, audioAssetClip);
			return audioAssetClip;
		}
		
		[MenuItem("Assets/Create/AudioAsset/Asset/Group")]
		static void CreateAudioAssetGroup()
		{
			Object[] selectedAssets = Selection.GetFiltered(typeof(AudioAssetPlayable), SelectionMode.Assets);

			if (selectedAssets.Length <= 0)
			{
				selectedAssets = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);
			}
			
			AudioAssetGroup assetGroup = ScriptableObject.CreateInstance<AudioAssetGroup>();;

			string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			string selectionDirectory = Path.GetDirectoryName(selectionPath);
			string selectionFilename = Path.GetFileName(selectionPath);

			string path;
			
			if (selectedAssets.Length <= 0)
			{
				path = $"{selectionPath}/AAG.asset";

				CreateAsset(path, assetGroup);
				return;
			}

			path = $"{selectionDirectory}/AAG-{selectionFilename}.asset";

			foreach (Object selectedAsset in selectedAssets)
			{
				switch (selectedAsset)
				{
					case AudioClip clip:
					{
						AudioAssetClip audioAssetClipAsset = CreateAudioAssetClipFromUnityAudioClip(clip);
						assetGroup.Assets.Add(audioAssetClipAsset);
						break;
					}
					case AudioAssetPlayable asset:
						assetGroup.Assets.Add(asset);
						break;
				}
			}
			
			CreateAsset(path, assetGroup);
		}
		
		[MenuItem("Assets/Create/AudioAsset/Asset/Switch")]
		static void CreateAudioAssetSwitch()
		{
			Object[] selectedAssets = Selection.GetFiltered(typeof(AudioAssetPlayable), SelectionMode.Assets);
			
			if (selectedAssets.Length <= 0)
			{
				selectedAssets = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);
			}
			
			AudioAssetSwitch audioAssetSwitch = ScriptableObject.CreateInstance<AudioAssetSwitch>();
			
			string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			string selectionDirectory = Path.GetDirectoryName(selectionPath);
			string selectionFilename = Path.GetFileName(selectionPath);

			string path;
			
			if (selectedAssets.Length <= 0)
			{
				path = $"{selectionPath}/ASw.asset";

				CreateAsset(path, audioAssetSwitch);
				return;
			}

			path = $"{selectionDirectory}/ASw-{selectionFilename}.asset";
			
			foreach (Object selectedAsset in selectedAssets)
			{
				switch (selectedAsset)
				{
					case AudioClip clip:
					{
						AudioAssetClip audioAssetClipAsset = CreateAudioAssetClipFromUnityAudioClip(clip);
						AudioAssetSwitch.AudioAssetSwitchData assetSwitchData = new ()
						{
							AssetPlayable = audioAssetClipAsset
						};
						audioAssetSwitch.SwitchData.Add(assetSwitchData);
						break;
					}
					case AudioAssetPlayable asset:
					{
						AudioAssetSwitch.AudioAssetSwitchData assetSwitchData = new ()
						{
							AssetPlayable = asset
						};
						audioAssetSwitch.SwitchData.Add(assetSwitchData);
						break;
					}
				}
			}
			
			CreateAsset(path, audioAssetSwitch);
		}

		private static bool PromptCreationMethod()
		{
			return EditorUtility.DisplayDialog
			(
				"Select Creation Method", "Select Creation Method", 
				"Single Asset", "Individual Assets"
			);
		}

		private static void CreateAsset(string path, Object asset)
		{
			AssetDatabase.CreateAsset(asset, path);
				
			EditorGUIUtility.PingObject(asset);
			Selection.activeObject = asset;
		}
		#endregion
	}
}