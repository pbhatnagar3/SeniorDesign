using UnityEngine;
using System.Collections;
using System;

public class PlaySound : MonoBehaviour {
	//sound source
	public AudioClip DrumBeat;
	float volumeScale = 0.7f;
	// Use this for initialization
	void Start () 
	{
	}
	
	
 	void  play(){
		audio.PlayOneShot (DrumBeat, volumeScale);
	}

	void OnTriggerEnter(Collider other) {
//		Destroy(other.gameObject);
		play ();
	}

	void OnCollisionEnter(Collision col){
	}
	
	// Update is called once per frame
	void Update () {

	}
}
