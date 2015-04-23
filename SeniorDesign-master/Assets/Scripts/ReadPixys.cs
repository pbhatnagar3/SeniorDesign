using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using MatrixLibrary;
using KalmanFilterImplementation;

namespace SensorReadings{
	public class ReadPixys {

		//Communicate with the PIXY
		public SerialPort myPort;
		public int numLEDs = 2;
		int expectedBytes;

		//Success read
		bool successfulPIXYRead = false;

		public Vector3[] locations;
		Vector3[] locations_cal;
		public bool[] available;
		float[] pixyData;

		float Xscale = -.001f, Yscale = -.001f, Zscale = -.001f;

		
		// Use this for initialization
		public ReadPixys () 
		{
			expectedBytes = numLEDs * 4;
			pixyData = new float[expectedBytes];

			myPort = new SerialPort ("COM4", 115200);

			Debug.Log ("Start Pixy reading");
			locations = new Vector3[numLEDs];
			locations_cal = new Vector3[numLEDs];
			available = new bool[numLEDs];

			myPort.Open ();
			
			if (myPort.IsOpen) {
				Debug.Log ("Pixy Opened");
				myPort.ReadTimeout = 50;
			} else {
				Debug.Log ("Pixy couldn't be opened");
				myPort.ReadTimeout = 10;
			}

			for (int i=0; i<10; i++)
				readPort ();
		}

		public void readPort()
		{
			successfulPIXYRead = false;
			
			try{
				string data = "";
				if (myPort.IsOpen) {
					data = myPort.ReadTo ("\n");
					string [] temp = data.Split (' ');
					if (temp.Length == expectedBytes) {
						successfulPIXYRead = true;
						for (int i=0; i<expectedBytes; i++) {
							if (!String.Equals (temp [i].Trim (), "")) {
								pixyData [i] = (float)System.Convert.ToDouble (temp [i]);
							}
						}
						saveData();
						successfulPIXYRead = true;
						Debug.Log ("Update Measurement based on pixies");
					}
				}
			}
			catch
			{

			}
		}

		public void saveData()
		{
			int temp;
			for (int i=0; i<expectedBytes; i+=4)
			{
				available[i/4]   = pixyData[i]== 1f;
				//X scaling
				locations[i/4].x = Xscale*pixyData[i+1] - locations_cal[i/4].x;
				//Y scaling
				locations[i/4].y = Yscale*pixyData[i+2] - locations_cal[i/4].y;
				//Z scaling
				locations[i/4].z = Zscale*pixyData[i+3] - locations_cal[i/4].z;
			}
		}

		public void saveCalibrationData()
		{
			int temp;
			for (int i=0; i<expectedBytes; i+=4)
			{
				//X scaling
				locations_cal[i/4].x = Xscale*pixyData[i+1];
				//Y scaling
				locations_cal[i/4].y = Yscale*pixyData[i+2];
				//Z scaling
				locations_cal[i/4].z = Zscale*pixyData[i+3];
			}
		}

//		public void saveCalibrationData

		public void printData()
		{
			string output = "";
			for (int i=0; i<expectedBytes; i++) 
			{
				output += pixyData[i].ToString()+",";
			}
			Debug.Log (output);
		}

		public void closeSerialPort()
		{
			myPort.Close();
			myPort.Dispose ();
			Debug.Log ("Pixy closed");
		}

		public bool successfulRead()
		{
			return successfulPIXYRead;
		}
		
	}
}
