using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
		public float maxWaitTime = 15f;
		public TrackInfoClass[] trackList;
		//Uses the class below to take in information about the track

		private float waitTime;
		//Time to wait between songs
		private float timeCounter = 0;
		//Used for the downtime between songs
		private int currentTrack;
		//Current track number

		#region Start

		void Start ()
		{
				currentTrack = Random.Range (0, trackList.Length);
				if (trackList.Length > 0)
						trackList [currentTrack] = trackList [currentTrack];
				waitTime = Random.Range (5, maxWaitTime);
				timeCounter = 0;
		}

		#endregion

		#region Update

		void Update ()
		{
				if (trackList == null || trackList.Length < 1)
						return;
				if (GetComponent<AudioSource> ().isPlaying && !NPCScript.inCombat)
						return;
				if (NPCScript.inCombat == true)
						PlayCombatTracks ();

				if (WaitComplete ())
						GoToNextTrack ();
		}

		#endregion

		#region Next Music

		void PlayCombatTracks ()
		{
				List<int> combatTracks = new List<int> ();

				for (int i = 0; i < trackList.Length; i++) {
						if (trackList [currentTrack].musicToggle.isOn) {
								GetComponent<AudioSource> ().enabled = true;
								if (trackList [currentTrack].songType.Equals ("Combat")) {
										currentTrack = combatTracks [i];
								}
						} else
								GetComponent<AudioSource> ().enabled = false;
				}

				GetComponent<AudioSource> ().Stop ();
				GetComponent<AudioSource> ().clip = trackList [currentTrack].audioList;
				//audio.Play ();
				//Debug.Log("Current Track: " + currentTrack);
		
				waitTime = Random.Range (0, maxWaitTime);
				timeCounter = 0;
		}

		void GoToNextTrack ()	//Selects next track to play and plays it
		{
				float sumOfProbability = 0f;
				List<int> possibleTracks = new List<int> ();

				for (int i = 0; i < trackList.Length; i++) {
						if (trackList [currentTrack].musicToggle.isOn) {
								GetComponent<AudioSource> ().enabled = true;
								if (trackList [currentTrack].songType.Equals (SceneManager.GetActiveScene ().name)) {
										trackList [i].probability = GetSongProbability (trackList [i].probability);
										sumOfProbability += trackList [i].probability;
										possibleTracks.Add (i);
								}
						} else
								GetComponent<AudioSource> ().enabled = false;
				}

				for (int t = 0; t < possibleTracks.Count; t++) {
						if (Random.Range (0f, 1f) < (trackList [t].probability / sumOfProbability) && trackList [t].probability != 0) {
								currentTrack = possibleTracks [t];	
								break;
						}
				}

				if (GetComponent<AudioSource> ().enabled == true) {
						//audio.Stop();
						GetComponent<AudioSource> ().clip = trackList [currentTrack].audioList;
						GetComponent<AudioSource> ().Play ();
				}

				waitTime = Random.Range (5, maxWaitTime);
				timeCounter = 0;
		}

		#endregion

		private float GetSongProbability (float prob)
		{
				for (int i = 0; i < trackList.Length; i++) {
						trackList [i].probability = trackList [i].musicSlider.value;
				}
				return prob;
		}

		#region Wait

		private bool WaitComplete ()	//Should allow a delay between songs
		{
				timeCounter += Time.deltaTime;

				//Debug.Log (timeCounter + "/" + waitTime);

				if (timeCounter >= waitTime) {
						return true;
				}
				return false;
		}

		#endregion

		//	void SwitchToCombatMusic()
		//	{
		//		if(npc.inCombat && trackList[currentTrack].songType.Equals("Combat"))
		//		{
		//			Debug.Log("In Combat");
		//		}
		//	}
}

[System.Serializable]
public class TrackInfoClass
{
		public AudioClip audioList;
		//Clips
		public string songType;
		//Used to select when the song should be played
		public Toggle musicToggle;
		//Used to access the ability to turn music on and off
		public Slider musicSlider;
		//Used to access the slider and determine the probability of songs to play
		[HideInInspector]
		public float probability;
		//Probability that the player has selected for the music to play
}
