using UnityEngine;
using System.Collections;

public class SkyboxSelect : MonoBehaviour {
	public Material initial;
	public Material[] Skyboxes = new Material[7];
	// Use this for initialization
	void Start () {
		//Camera.main.GetComponent<Skybox>().material = initial;
		RenderSettings.skybox = initial;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Z)) {
			//Camera.main.GetComponent<Skybox>().material = Skyboxes[Random.Range(0,Skyboxes.Length)];
			RenderSettings.skybox = Skyboxes[Random.Range(0,Skyboxes.Length)];
		}
	}
}
