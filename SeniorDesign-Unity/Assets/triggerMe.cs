using UnityEngine;
using System.Collections;

public class triggerMe : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	void OnTriggerEnter(Collider other) {
//		Destroy(other.gameObject);
		Debug.Log ("hi there");
		Debug.Log (other.tag);

	}
	// Update is called once per frame
	void Update () {
	
	}
}
