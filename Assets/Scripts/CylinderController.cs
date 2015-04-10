using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;
using System;

public class CylinderController : MonoBehaviour {
	public Text info;
	private Vector3 originPosition;
	public float zoff = 2;
	public float speed = 0;
	private float rotationSpeed;

	private const float xfactor = 0.01f;
	private const float yfactor = 0.01f;
	private const float zfactor = 0.02f;

	//private Text info;

	//private int count = 0;
	//private int portAccessInterval = 2;
	private const int ledN = 4;
	private Vector3[] locations = new Vector3[ledN]; //{tipL,endL,tipR,endR}
	private bool[] detected = new bool[ledN];
	private Vector3 move;

	SerialPort myPort = new SerialPort("COM3", 115200);

	// Use this for initialization
	void Start () {
		try{
			myPort.Open ();
		}
		catch (Exception e)
		{

		}
		originPosition = transform.position;

		for (int i = 0; i<ledN; i++) {
			locations[i] = new Vector3(0,0,0);
			detected[i] = false;
		}
		move = new Vector3 (0, 0, 0);
		myPort.ReadTimeout = 20;
	}
	
	// Update is called once per frame
	void Update () {
		int objNumber = getFrame ();
		for (int i =0; i < ledN; i++) {
			detected[i] = false;
		}
		for (int i = 0; i < objNumber; i++) {
			getObject();
		}
		if (detected [0] && detected [1]) {
			Quaternion rotation = Quaternion.LookRotation(locations[1] - locations[0]);
			rigidbody.MoveRotation(rotation);
			rigidbody.MovePosition (locations[0] + originPosition);
		} else if (detected [0]) {
			rigidbody.MovePosition (locations[0] + originPosition);
		} else if (detected [1]) {
			locations[0] = locations[0] + move;
			rigidbody.MovePosition(locations[0] + originPosition);
		}

		info.text = "x " + locations [0].x + " y " + locations [0].y + " z " + locations [0].z + "\nx " + locations [1].x + " y " + locations [1].y + " z " + locations [1].z ;
	}

	void FixedUpdate() // for acceleration data
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		rigidbody.AddForce (movement * speed * Time.deltaTime);
		//if (count == 0) 
			//do the munipulation

		//count = (count + 1) % portAccessInterval;

	}

	String readPort(){
		String data = "";
		if (myPort.IsOpen) {
			try {
				//data = myPort.ReadLine ();
				data = myPort.ReadTo("\n\r");
				//info.text = "line read";
			} catch (Exception e)
			{
				//info.text = "port not read";
			}
		}
		return data;
	}

	int getFrame()
	{
		String line = readPort ();
		if (line.Length > 10) // if not reading the frame line, discard the incomplete frame
		{
			line = readPort ();
			while (line.Length > 10)
			{
				line = readPort ();
			}
		}
		if (line.Length > 0) {
			char[] delimiterChars = {':'};
			String[] words = line.Split (delimiterChars);
			return Convert.ToInt32 (words [1]);
		} else {
			return -1;
		}
	}

	int getObject()
	{
		String line = readPort ();
		char[] delimiterChars = {',',':'};
		string[] words = line.Split (delimiterChars);
		int sig = Convert.ToInt32 (words [1]);
		Vector3 newLoc = new Vector3 (Convert.ToSingle (words [3]) * xfactor, Convert.ToSingle (words [5]) * yfactor, Convert.ToSingle (words [7]) * zfactor+zoff);
		if (sig == 1) {
			move = newLoc - locations [sig];
		}
		locations [sig] = newLoc;
		detected [sig] = true;
		return sig;

	}
}
