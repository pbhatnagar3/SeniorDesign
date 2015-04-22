using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using MatrixLibrary;
using KalmanFilterImplementation;


public class StickPositionOrientationAndEstimation : MonoBehaviour {

	//C%s enums are freaking stupid
	//Short cut to look at State vectors values
	int	POSX = 0, POSY = 1 , POSZ = 2, VELX = 3, VELY = 4, VELZ = 5,
	ACCX= 6, ACCY = 7, ACCZ = 8, ANG_X = 9, ANG_Y = 10, ANG_Z = 11,
	VEL_ANGX = 12, VEL_ANGY = 13, VEL_ANGZ = 14, ANG_DDX = 15, ANG_DDY = 16, ANG_DDZ = 17;

	//Short cut to look at measurement vectors values
	int MPOSX = 0, MPOSY = 1, MPOSZ = 2, MACCX = 3, MACCY = 4, MACCZ = 5, MVEL_ANGX = 6, MVEL_ANGY = 7, MVEL_ANGZ = 8;

	SerialPort myPort = new SerialPort("COM3", 115200);
	string text;
	
	float scaleGyro = 14.375f;
	float scaleAcc = 1/(.732f*1e-3f*9.81f);
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
	
	//Rigid Body
	Rigidbody rb;
	
	//Kalman Filter
	Kalman_Pos_Or kalman;

	//State Vector
	Matrix X;

	//Measurement Vector
	Matrix M;

	//How often should we update?
	double dt;
	bool flag = true;

	//Store Information pertaining to 
	Quaternion rt;	
	Vector3 pos = new Vector3 (0, 0, 0);
	Vector3 initialPos;
	
	// Use this for initialization
	void Start () 
	{
		//Get component realting to 
		rb = GetComponent<Rigidbody>();
		pos = rb.position;
		//Initialize measurement matrix
		M = Matrix.ZeroMatrix (9, 1);

		//Update Local Euler angles
		yaw = rb.rotation.eulerAngles.x;
		pitch = rb.rotation.eulerAngles.y;
		roll = rb.rotation.eulerAngles.z;

		// Initialize kalman filter
		setupKalmanFilter ();
		
		try 
		{
			myPort.Open ();
		}
		catch (Exception e)
		{
			
		}
		
		if (myPort.IsOpen) 
		{
			Debug.Log ("Port opened successfully");
			myPort.ReadTimeout = 50;
			
			//Calibration
			for (int i=0; i<10; i++){
				try 
				{
					readPort();
					saveCalibrationData();
					break;
				}
				catch(Exception e)
				{
					
				}
			}
			Debug.Log ("Calibration completed");
		}
		else
			myPort.ReadTimeout = 10;
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		readPort();
		updateMeasurementVector();
		updatePositionAndOrientation();
		rotateBody ();
	}
	
	void readPort()
	{
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
			}
		}
	}


	
	void printData()
	{		
		for (int i=0; i<15; i++)
		{
			Debug.Log (System.Convert.ToString(i) + ": "  + sensorData[i]);
		}
	}
	
	void saveCalibrationData()
	{
		//Save Gyro Data
		gyrdoData_cal [0] = sensorData [0]/scaleGyro;
		gyrdoData_cal [1] = sensorData [1]/scaleGyro;
		gyrdoData_cal [2] = sensorData [2]/scaleGyro;
		
		//Save Upper Accerleration
		accUp_cal [0] = sensorData [3]/scaleAcc;
		accUp_cal [1] = sensorData [4]/scaleAcc;
		accUp_cal [2] = sensorData [5]/scaleAcc;
		
		//Save Lower Accerleration
		accDown_cal [0] = sensorData [6]/scaleAcc;
		accDown_cal [1] = sensorData [7]/scaleAcc;
		accDown_cal [2] = sensorData [8]/scaleAcc;
		
		//Save Upper Compass
		compassUp_cal [0] = sensorData [9]/scaleMag;
		compassUp_cal [1] = sensorData [10]/scaleMag;
		compassUp_cal [2] = sensorData [11]/scaleMag;
		
		//Save Down Compass
		compassDown_cal [0] = sensorData [12]/scaleMag;
		compassDown_cal [1] = sensorData [13]/scaleMag;
		compassDown_cal [2] = sensorData [14]/scaleMag;
	}
	
	void saveData()
	{
		//Save Gyro Data
		gyrdoData [0] = sensorData [0]/scaleGyro - gyrdoData_cal [0];
		gyrdoData [1] = sensorData [1]/scaleGyro - gyrdoData_cal [1];
		gyrdoData [2] = sensorData [2]/scaleGyro - gyrdoData_cal [2];
		
		//Save Upper Accerleration
		accUp [0] = sensorData [3] / scaleAcc - accUp_cal[0];
		accUp [1] = sensorData [4]/scaleAcc - accUp_cal[1];
		accUp [2] = sensorData [5]/scaleAcc - accUp_cal[2];
		
		//Save Lower Accerleration
		accDown [0] = sensorData [6]/scaleAcc - accDown_cal[0];
		accDown [1] = sensorData [7]/scaleAcc - accDown_cal[1];
		accDown [2] = sensorData [8]/scaleAcc - accDown_cal[2];
		
		//Save Upper Compass
		compassUp [0] = sensorData [9]/scaleMag;
		compassUp [1] = sensorData [10]/scaleMag;
		compassUp [2] = sensorData [11]/scaleMag;
		
		//Save Down Compass
		compassDown [0] = sensorData [12]/scaleMag;
		compassDown [1] = sensorData [13]/scaleMag;
		compassDown [2] = sensorData [14]/scaleMag;
	}
	

	
	void translateBody()
	{
		u = v;
		a = ((accUp + accDown) / 2.0f);

		v = u + a * Time.deltaTime;
		//pos = pos + v * Time.deltaTime;

//		Vector3 t = new Vector3 (-v [1], v[2], v [0]);
//		rb.MovePosition(t*Time.deltaTime);


		
		//Account for body frame
		//pos [0] = pos [0] - v [1] * Time.deltaTime;
		//pos [1] = 0;//pos [1] + v [2] * Time.deltaTime;
		//pos [2] = 0;//pos [2] + v [0] * Time.deltaTime;
		//Debug.Log (Time.deltaTime);
		
		//rb.position = pos;
	}

	void setupKalmanFilter()
	{
		//Initialize kalman filter 

		double iniVariance = .01;
		
		//Position (it's all relative
				initialPos = rb.position;
		Vector3 initialVelocity = new Vector3 (0, 0, 0);
		Vector3 initialAcceleration = new Vector3 (0, 0, 0);

		
		//Orientation (initially flat)
		Vector3 EulerAng = rb.rotation.eulerAngles;
		Vector3 EulerAngDer = new Vector3 (0, 0, 0);
		Vector3 EulerAngDoubDer = new Vector3 (0, 0, 0);
		
		//Update dt
//		dt = Time.deltaTime;
		dt = Time.deltaTime;
		
		//Create initial state vector
		X = Matrix.IdentityMatrix(18, 1);
		
		//Set position
		X [POSX] = initialPos [0];
		X [POSY] = initialPos [1];
		X [POSZ] = initialPos [2];
		//Set Velocity
		X [VELX] = initialVelocity [0];
		X [VELY] = initialVelocity [1];
		X [VELZ] = initialVelocity [2];
		//Set Acceleration
		X [ACCX] = initialAcceleration [0];
		X [ACCY] = initialAcceleration [1];
		X [ACCZ] = initialAcceleration [2];
		
		//Set Euler Angles
		X [ANG_X] = EulerAng [0];
		X [ANG_Y] = EulerAng [1];
		X [ANG_Z] = EulerAng [2];
		//Set their derivative (measurement updated)
		X [VEL_ANGX]= EulerAngDer [0];
		X [VEL_ANGY] = EulerAngDer [1];
		X [VEL_ANGZ] = EulerAngDer [2];
		//Set Acceleration
		X [ANG_DDX] = EulerAngDoubDer [0];
		X [ANG_DDY] = EulerAngDoubDer [1];
		X [ANG_DDZ] = EulerAngDoubDer [2];
		
		//Initialize the Kalman Filter
//		kalman = new Kalman_Pos_Or(X, iniVariance, dt);
	}

	void updateMeasurementVector()
	{
		 //Set Measurement for position (from PIXYs - if available?)
		M [MPOSX] = initialPos[0];
		M [MPOSY] = initialPos[1];
		M [MPOSZ] = initialPos[2];

		// Set Measurement vector (measure acceleration from accelerometers)
		M [MACCX] = 0;//-(accUp [1] + accDown [1]) / 2.0f;
		M [MACCY] = 0;//(accUp [2] + accDown [0]) / 2.0f;
		M [MACCZ] = 0;//(accUp [0] + accDown [2]) / 2.0f;

		// Set Euler Angles rates (theta/s)
		M [MVEL_ANGX] =  -gyrdoData [1];
		M [MVEL_ANGY] =  -gyrdoData [2];
		M [MVEL_ANGZ] =   gyrdoData [0];
	}

	void updatePositionAndOrientation()
	{
		dt = 1;
		Debug.Log (1 / Time.deltaTime);

		//Update Kalman filter with Measurement vector
		//X = kalman.Update (X, M, dt);

		// Mod the angles to be withing 0 and 360
		X [ANG_X] = angleMod (X [ANG_X]);
		X [ANG_Y] = angleMod (X [ANG_Y]);
		X [ANG_Z] = angleMod (X [ANG_Z]);

		//Update Position
		//pos = new Vector3 ((float) X [POSX], (float) X [POSY], (float) X [POSZ]);
		//rb.MovePosition (pos);

		//Update Euler Angles
		rt = Quaternion.Euler((float)(X [VEL_ANGX]*dt),(float)(X [VEL_ANGY]*dt),(float)(X [VEL_ANGZ]*dt));
		rb.MoveRotation (rb.rotation * rt);
	}

	void rotateBody()
	{ 
		//Don't change these values
		float a = -(gyrdoData [1]* Time.deltaTime);
		float b = -(gyrdoData [2]* Time.deltaTime);
		float c =  (gyrdoData [0]* Time.deltaTime);
		
		yaw += a;
		pitch += b;
		roll += c;
		
		//Keep range between 0 and 360
		yaw = angleMod (yaw);
		pitch = angleMod (pitch);
		roll = angleMod (roll);
		
		yaw = angleMod (X [ANG_X]);
		pitch = angleMod (X [ANG_Y]);
		roll = angleMod (X [ANG_Z]);
		
		Quaternion rt = Quaternion.Euler(a,b,c);
		//rb.MoveRotation (rb.rotation * rt);
		
//		Debug.Log("Expected Specs: " +
//		          "X: " + Math.Round (yaw).ToString() + ";" +
//		          "Y: " + Math.Round (pitch).ToString() + ";" +
//		          "Z: " + Math.Round (roll).ToString() + ";" +
//		          "\n" +
//		          "Rigidbidy Specs: " +
//		          "X: " + Math.Round (rb.rotation.eulerAngles.x).ToString() + ";" +
//		          "Y: " + Math.Round (rb.rotation.eulerAngles.y).ToString() + ";" +
//		          "Z: " + Math.Round (rb.rotation.eulerAngles.z).ToString() + ";");
	}


	float angleMod(float angle)
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

	float angleMod(double angle)
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

}
