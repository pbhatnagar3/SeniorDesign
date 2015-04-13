using UnityEngine;
using System.Collections;
using System;

public class PlaySound : MonoBehaviour {
	//sound source
	public AudioClip DrumBeat;
	float volumeScale = 0.7f;
	// Use this for initialization
	void Start () {
	}
	
	
 	void  play(){
		audio.PlayOneShot (DrumBeat, volumeScale);
	}

	void OnCollisionEnter(Collision col){
		Debug.Log("hey there drum");
		play ();
		Debug.Log("hey there drum");
	}
	
	// Update is called once per frame
	void Update () {

	}
}
