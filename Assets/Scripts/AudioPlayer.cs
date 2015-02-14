using UnityEngine;
using System.Collections;

public class AudioPlayer : MonoBehaviour {

	public AudioClip DrumBeat1;
	public AudioClip DrumBeat2;
	public AudioClip DrumBeat3;
	public AudioClip DrumBeat4;
	public AudioClip DrumBeat5;
	public AudioClip DrumBeat6;
	float volumeScale = 0.7f;
	// Use this for initialization
	void Start () {
	
	}


	void play(){
		if (Input.GetKeyDown (KeyCode.U)) {
			audio.PlayOneShot(DrumBeat1, volumeScale);
				}
		if (Input.GetKeyDown (KeyCode.J)) {
			audio.PlayOneShot(DrumBeat2, volumeScale);
		}
		if (Input.GetKeyDown (KeyCode.H)) {
			
			audio.PlayOneShot(DrumBeat3, volumeScale);
		}
		if (Input.GetKeyDown (KeyCode.K)) {
			
			audio.PlayOneShot(DrumBeat4, volumeScale);
		}
		if (Input.GetKeyDown (KeyCode.I)) {
			audio.PlayOneShot(DrumBeat5, volumeScale);
		}
		if (Input.GetKeyDown (KeyCode.Y)) {
			
			audio.PlayOneShot(DrumBeat6, volumeScale);
		}

	}
	// Update is called once per frame
	void Update () {
		play ();
	}
}
