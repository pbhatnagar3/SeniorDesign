﻿/**
 * @author: Qisi Wang
 * Class facilitating pixy integration with Unity.
 *
 * for any questions, please don't hestitate to contact qisi.wang@gmail.com
 * 
 * May the force of compiler be with you. - Pujun
 */

using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;

namespace AirVisual{
	public class PixyCam {
		
		//public Text info;
		//public Vector3 originPosition;
		//public Quaternion originRotation;
		public float zoff = 0;
		public float speed = 0;
		private float rotationSpeed;
		
		private const float xfactor = -0.001f;
		private const float yfactor = -0.001f;
		public float zfactor = -.001F;

		bool successfulPixyRead = false;
		
		//private const float xfactor = -1;
		//private const float yfactor = -1;
		//public float zfactor = -1F;
		
		//private Text info;
		
		//private int count = 0;
		//private int portAccessInterval = 2;
		public const int ledN = 4;
		public Vector3[] locations = new Vector3[ledN]; //{tipL,endL,tipR,endR}
		public bool[] detected = new bool[ledN];
		public Vector3[] moves = new Vector3[ledN];
		
		public SerialPort myPort = new SerialPort("COM4",115200);
		//SerialPort myPort = new SerialPort("COM3",115200);
		
		// Use this for initialization
		public PixyCam()
		{

			try{
				myPort.Open ();
				Debug.Log ("PIXY opened correctly");
			}
			catch (Exception e)
			{
				Debug.Log ("PIXY couldn't be opened");
			}
			//originPosition = new Vector3(0,0,0);
			//originRotation = Quaternion.identity;
			// initialise the detected and location arrays
			for (int i = 0; i<ledN; i++) {
				locations[i] = new Vector3(0,0,0);
				detected[i] = false;
				moves[i] = new Vector3(0,0,0);
			}
			//move = new Vector3 (0, 0, 0);
			myPort.ReadTimeout = 40;
		}
		/*public Pixy (Vector3 orgPos, Quaternion orgRot) {
			try{
				myPort.Open ();
			}
			catch (Exception e)
			{
				
			}
			originPosition = orgPos;
			originRotation = orgRot;
			// initialise the detected and location arrays
			for (int i = 0; i<ledN; i++) {
				locations[i] = new Vector3(0,0,0);
				detected[i] = false;
				moves[i] = new Vector3(0,0,0);
			}
			//move = new Vector3 (0, 0, 0);
			myPort.ReadTimeout = 100;
		}*/
		
		// Update is called once per frame
		void Update () {
			
		}
		
		public void pixyThread()
		{
			//		while (true) {
			int objNumber = getFrame ();
			// clear dected data
			for (int i =0; i < ledN; i++) {
				detected[i] = false;
			}
			// update data from pixy
			for (int i = 0; i < objNumber; i++) {
				getObject();
			}
			//		}
		}
		
		
		String readPort(){
			successfulPixyRead = false;
			String data = "";
			if (myPort.IsOpen) {
				try {
					//data = myPort.ReadLine ();
					data = myPort.ReadTo("\n\r");
					successfulPixyRead = true;
					Debug.Log("Read success");
				} catch
				{					
				}
			}
			return data;
		}
		
		int getFrame()
		{
			String line = readPort ();
			if (line.Length > 5) // if not reading the frame line, discard the incomplete frame
			{
				line = readPort ();
				while (line.Length > 5)
				{
					line = readPort ();
				}
			}
			if (line.Length > 0) {
				char[] delimiterChars = {'f'};
				String[] words = line.Split (delimiterChars);
				return Convert.ToInt32 (words [1]);
			} else {
				return -1;
			}
		}
		
		int getObject()
		{
			String line = readPort ();
			char[] delimiterChars = {'s','x','y','z'};
			string[] words = line.Split (delimiterChars);
			int sig = Convert.ToInt32 (words [1]);
			Vector3 newLoc = new Vector3 (Convert.ToSingle (words [2]) * xfactor, Convert.ToSingle (words [3]) * yfactor, Convert.ToSingle (words [4]) * zfactor);
			moves[sig] = newLoc - locations [sig];
			locations [sig] = newLoc;
			detected [sig] = true;
			return sig;
			
		}

		public void closeSerialPort()
		{
			myPort.Close();
		}

		public bool successfulRead()
		{
			return successfulPixyRead;
		}
	}
}