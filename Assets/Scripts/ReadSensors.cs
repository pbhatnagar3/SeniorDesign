using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using MatrixLibrary;
using KalmanFilterImplementation;

namespace SensorReadings{
	public class ReadSensors {

		//Short cut to look at measurement vectors values
		int MPOSX = 0, MPOSY = 1, MPOSZ = 2, MACCX = 3, MACCY = 4, MACCZ = 5, MVEL_ANGX = 6, MVEL_ANGY = 7, MVEL_ANGZ = 8;		
		public SerialPort myPort;
		string text;
		
		float scaleGyro = 14.375f;
		float scaleAccUp = 1/(.732f*1e-3f*9.81f);
		float scaleAccDown = 1/(.061f*1e-3f*9.81f);
		float scaleMag = 1 / (.16f*1e-3f*9.81f);
		
		float [] sensorData = new float[15];
		
		//Calibration Values
		Vector3 gyrdoData_cal = new Vector3 (0,0,0);
		Vector3 accUp_cal  = new Vector3 (0,0,0);
		Vector3 accDown_cal  = new Vector3 (0,0,0);
		Vector3 compassUp_cal = new Vector3 (0, 0, 0);
		Vector3 compassDown_cal = new Vector3 (0, 0, 0);	
		
		//Live Update
		Vector3 gyrdoData = new Vector3 (0,0,0);
		Vector3 accUp  = new Vector3 (0,0,0);
		Vector3 accDown  = new Vector3 (0,0,0);
		Vector3 compassUp = new Vector3 (0, 0, 0);
		Vector3 compassDown = new Vector3 (0, 0, 0);
		
		//Properties of the body
		Vector3 u = new Vector3(0,0,0);
		Vector3 v = new Vector3(0,0,0);
		Vector3 a = new Vector3 (0, 0, 0);
		
		//Setup Inertial frame
		float yaw;
		float pitch;
		float roll;

		bool ignoreValues = false;
		
		//Rigid Body
		Rigidbody rb;
		
		//Kalman Filter
		Kalman_Pos_Or kalman;
		
		//State Vector
		public Matrix X;
		
		//Measurement Vector
		public Matrix M;
		
		//How often should we update?
		double dt;
		bool flag = true;
		
		//Store Information pertaining to 
		Quaternion rt;	
		Vector3 pos = new Vector3 (0, 0, 0);
		Vector3 initialPos;

		//Success read
		bool successfulIMURead = false;

		//
		System.Random t = new System.Random ();
		
		// Use this for initialization
		public ReadSensors (string port) 
		{
			myPort = new SerialPort (port, 115200);
			M = Matrix.ZeroMatrix (9, 1);

			myPort.Open ();
			
			if (myPort.IsOpen) {
				myPort.ReadTimeout = 50;
				
				//Calibration
				for (int i=0; i<10; i++) {
					try {
						readPort ();
						saveCalibrationData ();
						break;
					} catch (Exception e) {
					}
				}
			} else {
				Debug.Log ("Calibration failed");
				myPort.ReadTimeout = 10;
			}
			
		}
		
		// Update is called once per frame
		public Matrix getSensorData () 
		{
			readPort();
			updateMeasurementVector();
			return M;
		}
		
		public void readPort()
		{
			successfulIMURead = false;

			try{
				myPort.Write ("0");
				string data = "";
				if (myPort.IsOpen) {
					data = myPort.ReadTo ("\n");
					string [] temp = data.Split (' ');
					if (temp.Length == 16) {
						for (int i=0; i<15; i++) {
							if (!String.Equals (temp [i].Trim (), "")) {
								sensorData [i] = (float)System.Convert.ToDouble (temp [i + 1]);
							}
						}
						saveData ();
						successfulIMURead = true;
						ignoreValues = false;
					}
				}
			}
			catch{
				ignoreValues = true;
			}
		}
		
		
		
		public void printData()
		{		
			for (int i=0; i<15; i++)
			{
				Debug.Log (System.Convert.ToString(i) + ": "  + sensorData[i]);
			}
		}
		
		public void saveCalibrationData()
		{
			//Save Gyro Data
			gyrdoData_cal [0] = sensorData [0]/scaleGyro;
			gyrdoData_cal [1] = sensorData [1]/scaleGyro;
			gyrdoData_cal [2] = sensorData [2]/scaleGyro;
			
			//Save Upper Accerleration
			accUp_cal [0] = sensorData [3]/scaleAccUp;
			accUp_cal [1] = sensorData [4]/scaleAccUp;
			accUp_cal [2] = sensorData [5]/scaleAccUp;
			
			//Save Lower Accerleration
			accDown_cal [0] = sensorData [6]/scaleAccDown;
			accDown_cal [1] = sensorData [7]/scaleAccDown;
			accDown_cal [2] = sensorData [8]/scaleAccDown;
			
			//Save Upper Compass
			compassUp_cal [0] = sensorData [9]/scaleMag;
			compassUp_cal [1] = sensorData [10]/scaleMag;
			compassUp_cal [2] = sensorData [11]/scaleMag;
			
			//Save Down Compass
			compassDown_cal [0] = sensorData [12]/scaleMag;
			compassDown_cal [1] = sensorData [13]/scaleMag;
			compassDown_cal [2] = sensorData [14]/scaleMag;
		}
		
		public void saveData()
		{
			//Save Gyro Data
			gyrdoData [0] = sensorData [0]/scaleGyro - gyrdoData_cal [0];
			gyrdoData [1] = sensorData [1]/scaleGyro - gyrdoData_cal [1];
			gyrdoData [2] = sensorData [2]/scaleGyro - gyrdoData_cal [2];
			
			//Save Upper Accerleration - 16g
			accUp [0] = sensorData [3] / scaleAccUp - accUp_cal[0];
			accUp [1] = sensorData [4]/scaleAccUp - accUp_cal[1];
			accUp [2] = sensorData [5]/scaleAccUp - accUp_cal[2];
			
			//Save Lower Accerleration - 2g
			accDown [0] = sensorData [6] / scaleAccDown - accDown_cal[0];
			accDown [1] = sensorData [7]/scaleAccDown - accDown_cal[1];
			accDown [2] = sensorData [8]/scaleAccDown - accDown_cal[2];
			
			//Save Upper Compass
			compassUp [0] = sensorData [9]/scaleMag;
			compassUp [1] = sensorData [10]/scaleMag;
			compassUp [2] = sensorData [11]/scaleMag;
			
			//Save Down Compass
			compassDown [0] = sensorData [12]/scaleMag;
			compassDown [1] = sensorData [13]/scaleMag;
			compassDown [2] = sensorData [14]/scaleMag;
		}

		public void updateMeasurementVector()
		{
			// Set Measurement vector (measure acceleration from accelerometers)

			if (!ignoreValues){

				Debug.Log ("Update Measurement based on accelerometer");


				M [MACCX] = -(accDown [1]);
				M [MACCY] =  (accDown [2]);
				M [MACCZ] =  (accDown [0]);
				
				// Set Euler Angles rates (theta/s)
				M [MVEL_ANGX] = -gyrdoData [1];
				M [MVEL_ANGY] = -gyrdoData [2];
				M [MVEL_ANGZ] =  gyrdoData [0];
			}
			else
			{
					M = Matrix.ZeroMatrix(9,1);
			}
		}	
		
		public float angleMod(float angle)
		{
			while(angle<0)
			{
				angle += 360;
			}
			while(angle>360)
			{
				angle -= 360;
			}
			
			return angle;
		}
		
		public float angleMod(double angle)
		{
			while(angle<0)
			{
				angle += 360;
			}
			while(angle>360)
			{
				angle -= 360;
			}
			
			return (float)angle;
		}

		public void closeSerialPort()
		{
			myPort.Close();
			myPort.Dispose ();
			Debug.Log ("Definitely closed");
		}

		public bool successfulRead()
		{
			return successfulIMURead;
		}
		
	}
}
