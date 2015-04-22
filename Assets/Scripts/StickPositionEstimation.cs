//using UnityEngine;
//using System.Collections;
//using System.IO.Ports;
//using System;
//using MatrixLibrary;
//using KalmanFilterImplementation;
//
//
//public class StickPositionEstimation : MonoBehaviour {
//
//	SerialPort myPort = new SerialPort("COM3", 115200);
//	string text;
//	
//	float scaleGyro = 14.375f;
//	float scaleAcc = 1/(.732f*1e-3f*9.81f);
//	float scaleMag = 1 / (.16f*1e-3f*9.81f);
//	
//	float [] sensorData = new float[15];
//	
//	//Calibration Values
//	Vector3 gyrdoData_cal = new Vector3 (0,0,0);
//	Vector3 accUp_cal  = new Vector3 (0,0,0);
//	Vector3 accDown_cal  = new Vector3 (0,0,0);
//	Vector3 compassUp_cal = new Vector3 (0, 0, 0);
//	Vector3 compassDown_cal = new Vector3 (0, 0, 0);	
//	
//	//Live Update
//	Vector3 gyrdoData = new Vector3 (0,0,0);
//	Vector3 accUp  = new Vector3 (0,0,0);
//	Vector3 accDown  = new Vector3 (0,0,0);
//	Vector3 compassUp = new Vector3 (0, 0, 0);
//	Vector3 compassDown = new Vector3 (0, 0, 0);
//	
//	//Properties of the body
//	Vector3 u = new Vector3(0,0,0);
//	Vector3 v = new Vector3(0,0,0);
//	Vector3 a = new Vector3 (0, 0, 0);
//	Vector3 pos = new Vector3 (0, 0, 0);
//	
//	//Setup Inertial frame
//	float yaw;
//	float pitch;
//	float roll;
//	
//	//Rigid Body
//	Rigidbody rb;
//	
//	//Kalman Filter
//	Kalman k;	
//	
//	//Matrix updates
//
//	//State Vector
//	Matrix X;
//
//	//Measurement Vector
//	Matrix M;
//
//	//How often should we update?
//	double dt;
//
//	
//	// Use this for initialization
//	void Start () 
//	{
//		//Get component realting to 
//		rb = GetComponent<Rigidbody>();
//		pos = rb.position;
//		Debug.Log (rb.position);
//		//Initialize measurement matrix
//		M = Matrix.ZeroMatrix (3, 1);
//
//		//Update Local Euler angles
//		yaw = rb.rotation.eulerAngles.x;
//		pitch = rb.rotation.eulerAngles.y;
//		roll = rb.rotation.eulerAngles.z;
//
//		//Initialize kalman filter
//		Vector3 iniPos = pos;
//		Vector3 iniVel = new Vector3 (0, 0, 0);
//		Vector3 iniAcc = new Vector3 (0, 0, 0);
//		double  iniVar = .001;
//
//		//Update dt
//		dt = Time.deltaTime;
//
//		k = new Kalman (iniPos, iniVel, iniAcc, iniVar, dt);
//		
//		try 
//		{
//			myPort.Open ();
//		}
//		catch (Exception e)
//		{
//			
//		}
//		
//		if (myPort.IsOpen) 
//		{
//			Debug.Log ("Port opened successfully");
//			myPort.ReadTimeout = 50;
//			
//			//Calibration
//			for (int i=0; i<10; i++){
//				try 
//				{
//					readPort();
//					saveCalibrationData();
//					break;
//				}
//				catch(Exception e)
//				{
//					
//				}
//			}
//			Debug.Log ("Calibration completed");
//		}
//		else
//			myPort.ReadTimeout = 10;
//		
//	}
//	
//	// Update is called once per frame
//	void Update () 
//	{
//
//		try
//		{
//			readPort();
//			rotateBody();
//			//translateBody();
//		}
//		catch
//		{
//			Debug.Log ("Stuff ain't working yo");
//		}
//
//		//rb.position = k.Update (M, Time.deltaTime);		
//		//rb.MovePosition (k.Update (M, Time.deltaTime));
//	}
//	
//	void readPort()
//	{
//		myPort.Write ("0");
//		string data = "";
//		if (myPort.IsOpen) {
//			data = myPort.ReadTo ("\n");
//			string [] temp = data.Split (' ');
//			if (temp.Length == 16) {
//				for (int i=0; i<15; i++) {
//					if (!String.Equals (temp [i].Trim (), "")) {
//						sensorData [i] = (float)System.Convert.ToDouble (temp [i + 1]);
//					}
//				}
//				saveData ();
//			}
//		}
//	}
//	
//	void printData()
//	{		
//		for (int i=0; i<15; i++)
//		{
//			Debug.Log (System.Convert.ToString(i) + ": "  + sensorData[i]);
//		}
//	}
//	
//	void saveCalibrationData()
//	{
//		//Save Gyro Data
//		gyrdoData_cal [0] = sensorData [0]/scaleGyro;
//		gyrdoData_cal [1] = sensorData [1]/scaleGyro;
//		gyrdoData_cal [2] = sensorData [2]/scaleGyro;
//		
//		//Save Upper Accerleration
//		accUp_cal [0] = sensorData [3]/scaleAcc;
//		accUp_cal [1] = sensorData [4]/scaleAcc;
//		accUp_cal [2] = sensorData [5]/scaleAcc;
//		
//		//Save Lower Accerleration
//		accDown_cal [0] = sensorData [6]/scaleAcc;
//		accDown_cal [1] = sensorData [7]/scaleAcc;
//		accDown_cal [2] = sensorData [8]/scaleAcc;
//		
//		//Save Upper Compass
//		compassUp_cal [0] = sensorData [9]/scaleMag;
//		compassUp_cal [1] = sensorData [10]/scaleMag;
//		compassUp_cal [2] = sensorData [11]/scaleMag;
//		
//		//Save Down Compass
//		compassDown_cal [0] = sensorData [12]/scaleMag;
//		compassDown_cal [1] = sensorData [13]/scaleMag;
//		compassDown_cal [2] = sensorData [14]/scaleMag;
//	}
//	
//	void saveData()
//	{
//		//Save Gyro Data
//		gyrdoData [0] = sensorData [0]/scaleGyro - gyrdoData_cal [0];
//		gyrdoData [1] = sensorData [1]/scaleGyro - gyrdoData_cal [1];
//		gyrdoData [2] = sensorData [2]/scaleGyro - gyrdoData_cal [2];
//		
//		//Save Upper Accerleration
//		accUp [0] = sensorData [3] / scaleAcc - accUp_cal[0];
//		accUp [1] = sensorData [4]/scaleAcc - accUp_cal[1];
//		accUp [2] = sensorData [5]/scaleAcc - accUp_cal[2];
//		
//		//Save Lower Accerleration
//		accDown [0] = sensorData [6]/scaleAcc - accDown_cal[0];
//		accDown [1] = sensorData [7]/scaleAcc - accDown_cal[1];
//		accDown [2] = sensorData [8]/scaleAcc - accDown_cal[2];
//		
//		//Save Upper Compass
//		compassUp [0] = sensorData [9]/scaleMag;
//		compassUp [1] = sensorData [10]/scaleMag;
//		compassUp [2] = sensorData [11]/scaleMag;
//		
//		//Save Down Compass
//		compassDown [0] = sensorData [12]/scaleMag;
//		compassDown [1] = sensorData [13]/scaleMag;
//		compassDown [2] = sensorData [14]/scaleMag;
//
//		//Set Measurement vector (measure acceleration from accelerometers)
//		M [0, 0] = -(accUp [1] + accDown [1]) / 2.0f;
//		M [1, 0] =  (accUp [2] + accDown [2]) / 2.0f;
//		M [2, 0] =  (accUp [0] + accDown [0]) / 2.0f;
//	}
//	
//	void rotateBody()
//	{ 
//		float a = (-gyrdoData [1]* Time.deltaTime);
//		float b = (gyrdoData [0]* Time.deltaTime);
//		float c = (gyrdoData [2]* Time.deltaTime);
//
//		yaw += a;
//		pitch += b;
//		roll -= c;
//
//		//Keep range between 0 and 360
//		yaw = (yaw < 0) ? 360 + yaw : ((yaw > 360) ? yaw - 360 : yaw);
//		pitch = (pitch < 0) ? 360 + pitch : ((pitch > 360) ? pitch - 360 : pitch);
//		roll = (roll < 0) ? 360 + roll : ((roll > 360) ? roll - 360 : roll);
//
//
//		Quaternion rt = Quaternion.Euler(a,b,c);
//		rb.MoveRotation (rb.rotation * rt);
////
////		rb.rotation.eulerAngles.x = yaw;
////		rb.rotation.eulerAngles.y = roll;
////		rb.rotation.eulerAngles.z = pitch;
//
//
//
//		Debug.Log("Expected Specs: " +
//		          "X - " + Math.Round (yaw).ToString() + ";" +
//		          "Y- " + Math.Round (roll).ToString() + ";" +
//		          "Z - " + Math.Round (pitch).ToString() + ";" +
//			      "\n" +
//		          "Rigidbidy Specs: " +
//			      "X - " + Math.Round (rb.rotation.eulerAngles.x).ToString() + ";" +
//		          "Y - " + Math.Round (rb.rotation.eulerAngles.y).ToString() + ";" +
//		          "Z - " + Math.Round (rb.rotation.eulerAngles.z).ToString() + ";");
//	}
//	
//	void translateBody()
//	{
//		u = v;
//		a = ((accUp + accDown) / 2.0f);
//
//		v = u + a * Time.deltaTime;
//		//pos = pos + v * Time.deltaTime;
//
////		Vector3 t = new Vector3 (-v [1], v[2], v [0]);
////		rb.MovePosition(t*Time.deltaTime);
//
//
//		
//		//Account for body frame
//		//pos [0] = pos [0] - v [1] * Time.deltaTime;
//		//pos [1] = 0;//pos [1] + v [2] * Time.deltaTime;
//		//pos [2] = 0;//pos [2] + v [0] * Time.deltaTime;
//		//Debug.Log (Time.deltaTime);
//		
//		//rb.position = pos;
//	}
//}
