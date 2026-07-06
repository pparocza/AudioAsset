using UnityEngine;

namespace AudioAsset
{
	public class AudioAssetPlaylistTrigger : MonoBehaviour
	{
		[SerializeField]
		private AudioAssetPlaylist playlist;

		[SerializeField] 
		private AudioAssetPlaylist.PlaylistCommand command;

		[SerializeField] 
		private int sectionIndex;
		
		public void TriggerAudioPlaylistCommand()
		{
			if (playlist == null)
			{
				return;
			}
			
			playlist.HandlePlaylistCommand(command, sectionIndex);
		}
	}
}