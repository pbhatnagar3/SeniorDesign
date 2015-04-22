using UnityEngine;
using System.Collections;
using MultiThreadImplementation;
using MatrixLibrary;
using PixyFileReader;

public class MainThread : MonoBehaviour {

	int	POSX = 0, POSY = 1 , POSZ = 2, VELX = 3, VELY = 4, VELZ = 5,
	ACCX = 6, ACCY = 7, ACCZ = 8, ANG_X = 9, ANG_Y = 10, ANG_Z = 11,
	VEL_ANGX = 12, VEL_ANGY = 13, VEL_ANGZ = 14, ANG_DDX = 15, ANG_DDY = 16, ANG_DDZ = 17;

	Matrix X = Matrix.ZeroMatrix(18,1);
	Matrix M = Matrix.ZeroMatrix(9,1);
	NewMulti multi;
	Rigidbody rb;
	Quaternion rt;



	Vector3 u   = new Vector3 (0, 0, 0);
	Vector3 v   = new Vector3 (0, 0, 0);
	Vector3 a   = new Vector3 (0, 0, 0);
	Vector3 pos = new Vector3(0, 0, 0);
	double time,thresholdTime;

	//Stick details
	Vector3 pos1 = new Vector3(0,0,0);
	Vector3 pos2 = new Vector3 (0, 0, 0);

	//Rotation
	Vector3 euler1 = new Vector3(0,0,0);
	Vector3 euler2 = new Vector3(0,0,0);

	Vector3 iniPosition;

	public bool usePixy, useAccelerometer, useGyroScopes;

	Matrix[] p_pos;
	Quaternion originRotation, rotation;

	//How many LEDs do you want to track?
	public bool led1, led2, led3, led4;
	public double r,q;
	public double varPos, varVel, varAcc;
	public int time_multiple;
	public double varAngle, varAngleVel, varAngleDD;
	public bool useDirectPosition, useKalmanPosition, useDirectGyroscope,useKalmanGyroscope, useKalmanAll;
	public bool pixyBasedRotation;

	// Use this for initialization
	void Start () {

		if (r == 0) 
		{
			r = 10000;
		}

		if (q == 0)
		{
			q = -827.649;
		}
		//Parameters to changes
		if (time_multiple == 0)
			time_multiple = 1;

		double dt = time_multiple*Time.fixedDeltaTime;

		if (varPos == 0 && varVel == 0 && varAcc == 0)
		{
			varPos = 10;
			varVel = 50; 
			varAcc = 50;
		}
		double varAngle = 100, varAngleVel = 100, varAngleDD = 100;

		if (varAngle == 0 && varAngleVel == 0 && varAngleDD == 0) {
			varAngle = 100;
			varAngleVel = 100;
			varAngleDD = 100;
		}

		//How many 

		rb = GetComponent<Rigidbody>();
		pos = rb.position;
		pos1 = pos;
		pos2 = pos;

		iniPosition = new Vector3 (pos.x, pos.y, pos.z);

		Matrix iniVariancePos = Matrix.ZeroMatrix(9, 9);
		Matrix iniVarianceAng = Matrix.ZeroMatrix(9, 9);
		Matrix iniVarianceBig = Matrix.ZeroMatrix (18, 18);

		// Update pixy position variances
		for (int i=0; i<3; i++) {
			iniVariancePos[i,i] = varPos;
			iniVarianceBig[i,i] = varPos;
		}
		for (int i=3; i<6; i++) {
			iniVariancePos[i,i] = varVel;
			iniVarianceBig[i,i] = varVel;
		}
		for (int i=6; i<9; i++) {
			iniVariancePos[i,i] = varAcc;
			iniVarianceBig[i,i] = varAcc;
		}

		// Update Gyroscope variances

		for (int i=9; i<12; i++) {
			iniVarianceAng[i-9,i-9] = varAngle;
			iniVarianceBig[i,i]     = varAngle;
		}
		for (int i=12; i<15; i++) {
			iniVarianceAng[i-9,i-9] = varAngleVel;
			iniVarianceBig[i,i]     = varAngleVel;
		}
		for (int i=15; i<18; i++) {
			iniVarianceAng[i-9,i-9] = varAngleDD;
			iniVarianceBig[i,i]     = varAngleDD;
		}
		//Update Angle Estimate

		setupStateVector ();
		multi = new NewMulti (X, M, dt, iniVariancePos, iniVarianceBig,
		                      usePixy, useAccelerometer,useGyroScopes,
		                      led1, led2, led3, led4, r, q,
		                      "COM5", "1");

		// Origin's original rotation  
	}
	
	// Update is called once per frame
	void Update () {
		//Update position based on Pixy Value
		if (usePixy) {

			// Update based on direct position
			if (useDirectPosition)
			{
				pos1.x = (float)multi.Mp[0];
				pos1.y = (float)multi.Mp[1];
				pos1.z = (float)multi.Mp[2];
			}
			// update position based on Kalman filter (big)
			if (useKalmanAll)
			{
				pos1.x = (float)multi.X[0];
				pos1.y = (float)multi.X[1];
				pos1.z = (float)multi.X[2];
			}
//			//Update position based on postion Kalman filter alone
			if (useKalmanPosition)
			{
				pos1.x = (float)multi.Xp[0];
				pos1.y = (float)multi.Xp[1];
				pos1.z = (float)multi.Xp[2];
			}

			rb.MovePosition(pos1 + iniPosition);
		}

		if (useGyroScopes) {			

			//Update rotation based on measurements
			if (useDirectGyroscope)			
			{
				euler1.x = (float)multi.Mg[0]*Time.deltaTime;
				euler1.y = (float)multi.Mg[1]*Time.deltaTime;
				euler1.z = (float)multi.Mg[2]*Time.deltaTime;
				rt = Quaternion.Euler(euler1.x, euler1.y, euler1.z);
				rb.MoveRotation (rb.rotation * rt);
			}
			//Update based on Big filter
			if (useKalmanAll)
			{
				euler1.x = (float)multi.X[12]*Time.deltaTime;
				euler1.y = (float)multi.X[13]*Time.deltaTime;
				euler1.z = (float)multi.X[14]*Time.deltaTime;
				rt = Quaternion.Euler(euler1.x, euler1.y, euler1.z);
				rb.MoveRotation (rb.rotation * rt);
			}
//			//Update based on local filter
			if (useKalmanGyroscope)
			{
				euler1.x = (float)multi.Xg[0]*Time.deltaTime;
				euler1.y = (float)multi.Xg[0]*Time.deltaTime;
				euler1.z = (float)multi.Xg[0]*Time.deltaTime;
				rt = Quaternion.Euler(euler1.x, euler1.y, euler1.z);
				rb.MoveRotation (rb.rotation * rt);

			}
			// Rotate based on previous values	
		}

		if(pixyBasedRotation)
		{
			Debug.Log(multi.rotPixy2);
			rb.MoveRotation(Quaternion.LookRotation(-multi.rotPixy2));
		}
	}
	
	void OnApplicationQuit() {
		// Close all threads
		multi.OnDestroy ();

	}

	void setupStateVector()
	{		
		//Position (it's all relative)
		Vector3 initialPos = rb.position;
		Vector3 initialVelocity = new Vector3 (0, 0, 0);
		Vector3 initialAcceleration = new Vector3 (0, 0, 0);
		
		
		//Orientation (initially flat)
		Vector3 EulerAng = rb.rotation.eulerAngles;
		Vector3 EulerAngDer = new Vector3 (0, 0, 0);
		Vector3 EulerAngDoubDer = new Vector3 (0, 0, 0);
		
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

}
