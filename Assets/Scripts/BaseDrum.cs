using UnityEngine;
using System.Collections;
using System;

public class BaseDrum : MonoBehaviour {
	//sound source
	public AudioClip DrumBeat;
	float volumeScale = 0.7f;
	// Use this for initialization
	void Start () {
	}
	
	
 	void  play(){
		audio.PlayOneShot (DrumBeat, volumeScale);
	}

	void OnMouseEnter()
	{
		play ();
		Debug.Log("YOLO");
	}

	
	// Update is called once per frame
	void Update () {

	}
}
