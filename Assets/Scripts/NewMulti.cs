using System;
using UnityEngine;
using Frankfort.Threading;
using System.Threading;
using System.Collections;
using KalmanFilterImplementation;
using System.IO.Ports;
using MatrixLibrary;
using SensorReadings;
using PixyFileReader;
using StereoVision;


namespace MultiThreadImplementation{
	public class NewMulti : MonoBehaviour {

		int	POSX = 0, POSY = 1 , POSZ = 2, VELX = 3, VELY = 4, VELZ = 5,
		ACCX = 6, ACCY = 7, ACCZ = 8, ANG_X = 9, ANG_Y = 10, ANG_Z = 11,
		VEL_ANGX = 12, VEL_ANGY = 13, VEL_ANGZ = 14, ANG_DDX = 15, ANG_DDY = 16, ANG_DDZ = 17;

		int MPOSX = 0, MPOSY = 1, MPOSZ = 2, MACCX = 3, MACCY = 4, MACCZ = 5, MVEL_ANGX = 6, MVEL_ANGY = 7, MVEL_ANGZ = 8;

		//Store Pixy positions
		Vector3 PixyPosition1_1 = new Vector3 (0, 0, 0);
		Vector3 PixyPosition1_2 = new Vector3 (0, 0, 0);
		Vector3 PixyPosition2_1 = new Vector3 (0, 0, 0);
		Vector3 PixyPosition2_2 = new Vector3 (0, 0, 0);

		//Update final locations of sticks
		public Vector3 stick1Location1;
		public Vector3 stick1Location2;

		//Pixy 
		public Vector3 rotPixy1;
		public Vector3 rotPixy2;

		bool usePixy, useAccelerometer, useGyroScopes;

		bool led1, led2, led3, led4;

		// Exit threads when class ends
		private bool notDestroyed;

		//Actual threads used
		public Thread pixyThread;
		public Thread kalmanThread;
		public Thread readSensorsThread;
		public Thread triangulationThread;

		//Mutexes that will need to be set - ignoring this for now = bad programming practice
		private object pixyLocker;
		private object IMULocker;

		//Available data
		public bool IMUAvailable = false;
		public bool pixyAvailable = false;

		//Matrices for Kalman
		public Matrix X, M;
		public double dt;
		//Separting in position, acceleration and gyroscope matrices
		public Matrix Xp, Mp;
		public Matrix Xa, Ma;
		public Matrix Xg, Mg;

		//Store Variance Matrices
		Matrix iniVariancePos;		
		Matrix iniVarianceBig;
		Matrix iniVarianceAcc;

		//Store initial position of rigiddbody
		Vector3 iniPosition;

		//Update R and Q
		double r,q;

		// Create objects that will read pixy, sensors and calculate triangulation
		PixyFileReader.Program readPixies;
		StereoVision.StereoCamera cameraTriangulation;
		SensorReadings.ReadSensors rs;

		public NewMulti(Matrix X, Matrix M, double dt, Matrix iniVariancePos, Matrix iniVarianceBig,
		                bool usePixy, bool useAccelerometer, bool useGyroScopes,
		                bool led1, bool led2, bool led3, bool led4, double r, double q,
		                string com)
		{
			//Set objects for reading sensors
			readPixies = new PixyFileReader.Program ();			
			cameraTriangulation = new StereoVision.StereoCamera ();
			rs = new ReadSensors (com);

			//Update initial positon vector
			iniPosition = new Vector3 ((float)X [0], (float)X [1], (float)X [2]);

			//Which LEDs do we track - comes from the Main Thread
			this.led1 = led1;
			this.led2 = led2;
			this.led3 = led3;
			this.led4 = led4;

			//What data should we update? i.e. Positional, rotation or both

			//Position
			this.usePixy = usePixy;
			//Not really used right now
			this.useAccelerometer = useAccelerometer; 
			//Rotation
			this.useGyroScopes = useGyroScopes;

			//Set initial params
			this.X = X;
			this.M = M;
			this.dt = dt;

			// Set Variance Matrices
			this.iniVariancePos = iniVariancePos;
			this.iniVarianceBig = iniVarianceBig;

			this.r = r;
			this.q = q;


			//Setup state and measurement vectors for smaller Kalman filters

			//Acceleration based kalman filter
			Xa = Matrix.ZeroMatrix (9, 1);
			//Overwrite Xa
			for (int i=0; i<9; i++) {
				Xa [i] = X [i];
			}
			Ma = Matrix.ZeroMatrix (3, 1);


			//Gyroscope based kalman filter
			Xg = Matrix.ZeroMatrix (9, 1);
			for (int i=9; i<18; i++) {
				Xg[i-9] = X [i];
			}
			Mg = Matrix.ZeroMatrix (3, 1);


			//Positon based kalman filter
			Xp = Matrix.ZeroMatrix (9, 1);
			for (int i=0; i<9; i++) {
				Xp[i] = X [i];
			}
			Mp = Matrix.ZeroMatrix (3, 1);

			//No data is available at the beginning
			IMUAvailable = false;
			pixyAvailable = false;
			notDestroyed = true;

			//Start the threads
			Start ();
		}
		
		public void OnDestroy()
		{
			// to terminate the worker threads
			notDestroyed = false;
		}
		
		
		void Start()
		{
			// start worker threads
			if (usePixy)
				pixyThread = Loom.StartSingleThread(pixyRoutine, System.Threading.ThreadPriority.Normal, true);
			if (useGyroScopes || useAccelerometer)
				readSensorsThread = Loom.StartSingleThread (readSensorsRoutine, System.Threading.ThreadPriority.Normal, true);

			//Runs kalman thread
			kalmanThread = Loom.StartSingleThread (kalmanRoutine, System.Threading.ThreadPriority.Normal, true);

			//Runs triangulation thread
			triangulationThread = Loom.StartSingleThread (triangulationRoutine, System.Threading.ThreadPriority.Normal, true);
		}


		void readSensorsRoutine()
		{
			Matrix temp;
			while (notDestroyed) {

				temp = rs.getSensorData();

				//Update acceleration and gyroscope readings of large Kalman Filter
				for (int i=3; i<9; i++)
				{
					M[i] = temp[i];
				}

				//Update an acceleration measurement vector so to speak
				if (useAccelerometer){
					Ma[0] = M[MACCX];
					Ma[1] = M[MACCY];
					Ma[2] = M[MACCZ];
				}

				//Update gyroscope values
				if (useGyroScopes){
					Mg[0] = M[MVEL_ANGX];
					Mg[1] = M[MVEL_ANGY];
					Mg[2] = M[MVEL_ANGZ];
				}

				IMUAvailable = rs.successfulRead();
			}

			//This is stupid. Oh well
			rs.closeSerialPort ();
			UnityEngine.Debug.Log ("Serial port closed");
		}

		
		void pixyRoutine(){
						
			while (notDestroyed) {

				readPixies.readFile ();
//
				//Update measurement vector
				M[POSX] = stick1Location2.x/-1000;
				M[POSY] = stick1Location2.y/-1000;
				M[POSZ] = stick1Location2.z/-1000;

				//update positional kalman filter
				Mp[POSX] = M[POSX];
				Mp[POSY] = M[POSY];
				Mp[POSZ] = M[POSZ];

				//Keep track of successful read
				pixyAvailable = readPixies.successfulRead();
			}
		}



		void kalmanRoutine()
		{
			//Position or acceleration based kalman filter
			Kalman kalman_pos = new Kalman(iniVariancePos, dt, dt, r, q);
			//Orientation based kalman filter
			KalmanOrientation kalman_or = new KalmanOrientation (iniVariancePos, dt, r, q);

			//Update Kalman filter for all params
			Kalman_Pos_Or finalKalman = new Kalman_Pos_Or (iniVarianceBig, dt, r, q);

			// the main loop of the thread

			while (notDestroyed) {

				//Update Complete Kalman filter
				X = finalKalman.predictionStep(X);
				X = finalKalman.updateStep(X, M);

				//Update position Kalman filter
				Xp = kalman_pos.updateStepPosition(Xp, Mp);
			    Xp = kalman_pos.predictionStepPosition(Xp);

				//Update acceleration Kalman fitler
				Xa = kalman_pos.predictionStepAcceleration(Xa);
				Xa = kalman_pos.updateStepAcceleration(Xa,Ma);

				//Update Gyroscope Kalman filter
				if (useGyroScopes)
				{
					Xg = kalman_or.predictionStepOrientation(Xg);
					Xg = kalman_or.updateStepOrientation(Xg, Mg);
					Xg[0] = angleMod(Xg[0]);
					Xg[1] = angleMod(Xg[1]);
					Xg[2] = angleMod(Xg[2]);
				}

			}
		}

		public void triangulationRoutine()
		{
			while (notDestroyed) {

				try {
					if (led1)
					{
						PixyPosition1_1 = (cameraTriangulation.triangulation ((int)readPixies.LeftLedTracking [0].x,
					                                                        (int)readPixies.LeftLedTracking   [0].y,
					                                                        (int)readPixies.RightLedTracking  [0].x,
					                                                        (int)readPixies.RightLedTracking  [0].y));
					}
					if (led2)
					{
						PixyPosition1_2 = (cameraTriangulation.triangulation ((int)readPixies.LeftLedTracking [1].x,
						                                                    (int)readPixies.LeftLedTracking   [1].y,
						                                                    (int)readPixies.RightLedTracking  [1].x,
						                                                    (int)readPixies.RightLedTracking  [1].y));
					}
					if (led3)
					{
						PixyPosition2_1 = (cameraTriangulation.triangulation ((int)readPixies.LeftLedTracking [2].x,
						                                                    (int)readPixies.LeftLedTracking   [2].y,
						                                                    (int)readPixies.RightLedTracking  [2].x,
						                                                    (int)readPixies.RightLedTracking  [2].y));

					}
					if (led4)
					{
						PixyPosition2_2 = (cameraTriangulation.triangulation ((int)readPixies.LeftLedTracking [3].x,
						                                                    (int)readPixies.LeftLedTracking   [3].y,
						                                                    (int)readPixies.RightLedTracking  [3].x,
						                                                    (int)readPixies.RightLedTracking  [3].y));

					}


					// Update position estimates
					if (led1 && led2)
					{
						stick1Location1 = (PixyPosition1_1 + PixyPosition1_2)/2.0f;
						rotPixy1 = PixyPosition1_1 - PixyPosition1_2;
					}
					else if (led1)
					{
						stick1Location1 = PixyPosition1_1;
					}

					if (led3 && led4)
					{
						stick1Location2 = (PixyPosition2_1 + PixyPosition2_2)/2.0f;
						rotPixy2 = PixyPosition2_1 - PixyPosition2_2;
					}
					else if (led3)
					{
						stick1Location2 = PixyPosition2_1;
					}

				}
				catch{
				}
			}

		}

		public Matrix getStateMatrix()
		{
			return X;
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
}