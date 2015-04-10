using UnityEngine;
using System.Collections;

public class Skybox : MonoBehaviour {

	public Material otherSkybox;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.A)) {
			RenderSettings.skybox = otherSkybox;
		}
	}
}
