using System;
using UnityEngine;
using Frankfort.Threading;
using System.Threading;
using System.Collections;



public class NewMulti : MonoBehaviour {
	//public float zScale = 0;
	private bool notDestroyed = true;
	//public float zoff = 2;
	//public int maxThreads = 2;
	//public int TestWorkerObjects = 4;
	//public int minCalculations = 10;
	//public int maxCalculations = 50;a
	//public float abortAfterSeconds = 3f;	
	public Thread pixyThread;
	public Thread kalmanThread;
	//just for testing purpose
	public Vector3[] locations = new Vector3[AirVisual.PixyCam.ledN]; //{tipL,endL,tipR,endR}
	public bool[] detected = new bool[AirVisual.PixyCam.ledN];
	public Vector3[] moves = new Vector3[AirVisual.PixyCam.ledN];
	private object locker = new object();
	//private Object[] originTrans = new Object[2];
	//private ThreadPoolScheduler myThreadScheduler;
	//private Cheese.Pixy pixy;
	
	// Use this for initialization
	//	void Start () {
	//	
	//	}

	MatrixLibrary.Matrix pixyData; // shared data between worker threads

	
	void OnDestroy()
	{
		// to terminate the worker threads
		notDestroyed = false;
	}
	
	
	void Start()
	{
		pixyData = MatrixLibrary.Matrix.ZeroMatrix(1,AirVisual.PixyCam.ledN * 4);
		// just for testing purpose
		/*
		for (int i = 0; i<AirVisual.PixyCam.ledN; i++) {
			locations[i] = new Vector3(0,0,0);
			detected[i] = false;
			moves[i] = new Vector3(0,0,0);
		}*/
		
		//Application.targetFrameRate = 25;
		
		// start worker threads
		pixyThread = Loom.StartSingleThread(pixyRoutine, System.Threading.ThreadPriority.Normal, true);
		kalmanThread = Loom.StartSingleThread (kalmanRoutine, System.Threading.ThreadPriority.Normal, true);
	}
	
	void Update(){
		
	}

	
	void pixyRoutine(){
		AirVisual.PixyCam pixy = new AirVisual.PixyCam () ;
		int nAve = 2;
		int first = nAve;
		float offSum = 0;
		while (notDestroyed) {
			pixy.pixyThread();

			// modifying the matrix
			lock(locker)
			{
				for (int i =0; i< AirVisual.PixyCam.ledN;i++)
				{
					if (pixy.detected[i])
					{
						// if the corresponding led is detected
						pixyData[0,4*i] = 1;
						// changed the x,y,z data
						pixyData[0,4*i+1] = pixy.locations[i].x;
						pixyData[0,4*i+2] = pixy.locations[i].y;
						pixyData[0,4*i+3] = pixy.locations[i].z - pixy.zoff;
						Loom.DispatchToMainThread((object loc) => Debug.Log("Where am I (" + ((Vector3)loc).x + " " +((Vector3)loc).y + " " +((Vector3)loc).z+")"), pixy.locations[0],true,true);

					} else{
						// if not detected
						pixyData[0,4*i] = -1;
					}
				}
			}

			if (first > 0)
			{
				offSum = offSum + pixy.locations[0].z;
				first--;
				if (first <= 0)
				{
					pixy.zoff = offSum/nAve;
				}
			}
		}

	}



	void kalmanRoutine()
	{
		// set up local variables below
		// local variable holding pixy location data
		MatrixLibrary.Matrix locMatrix = MatrixLibrary.Matrix.ZeroMatrix(1,AirVisual.PixyCam.ledN * 4); // AirVisual.PixyCam.ledN is a constant whose value is 4

		// The following 2 lines get the original status (position and rotation) of the drumstick in unity
		/*
		Vector3 originPosition = (Vector3)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.position);
		Quaternion originRotation = (Quaternion)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.rotation);
		
		Vector3 updateLoc;
		Quaternion rotation;
		*/

		// the main loop of the thread
		while (notDestroyed) {

			lock(locker)
			{
				// copy the pixy location data from the shared matrix to a local matrix
				for (int i = 0; i<AirVisual.PixyCam.ledN * 4; i++) {
					locMatrix[0,i] = pixyData[0,i];
					/**
					 * locMatrix is the 1x16 local matrix containing the pixy location data {tipL,endL,tipR,endR}
					 * data format:
					 * 	[tipL.detected, tipL.x, tipL.y,tipL.z,
					 *	 endL.detected, endL.x, endL.y,endL.z,
					 *	 tipR.detected, tipR.x, tipR.y,tipR.z,
					 *	 endR.detected, endR.x, endR.y,endR.z]
					 * where detected >= 0 indicates corresponding LED detected
					 * detected < 0 indicates corresponding LED not detected
					*/
				}
			}

			// TODO kalman filter calculation below
			// The pixy location data can be access locally through the matrix locMatrix (documentated above)
			// You can redefine locMatrix as you like
			/* To post change to the main thread (e.g. change the location of the drum stick),
			 * use the function Loom.DispatchToMainThread();
			 * For example: (for more example see the use of the function in codes above and below)
			 * Loom.DispatchToMainThread(
			 * 		(object rot) => GameObject.Find("Sphere").rigidbody.MoveRotation((Quaternion) rot),
			 * 		rotation,
			 * 		true,
			 * 		true
			 * );
			 * Explaination:
			 * You put the function you want to call after "=>" in the first parameter
			 * rotation		the second parameter is the local variable you want to pass to the function in the first parameter
			 * rot			in the first parameter is just a new name for the parameter you pass in 
			 * 				(don't forget to cast it back to the correct type when you use it in the function call)
			 * and just keep the last 2 parameters as true
			 * 
			 * Also, you may want to keep the GameObject.Find("Sphere") part in the function call to access the drumstick object
			 * /
			










			/* Testing data exchange between 2 worker thread
			// the following lines access the pixyData variable
			lock(locker)
			{
				for (int i = 0; i<AirVisual.PixyCam.ledN; i++) {
					locations[i].x = (float) pixyData[0,4*i+1];
					locations[i].y = (float) pixyData[0,4*i+2];
					locations[i].z = (float) pixyData[0,4*i+3];
					detected[i] = (pixyData[0,4*i] >= 0);
				}
			}

			
			if (detected[0] || detected[1])
			{
				if (detected [0] && detected [1]) {
					rotation = Quaternion.LookRotation(locations[1] - locations[0])*originRotation;
					Loom.DispatchToMainThread((object rot) => GameObject.Find("Sphere").rigidbody.MoveRotation((Quaternion) rot), rotation,true,true);
					updateLoc = locations[0] + originPosition;
				} else if (detected [0]) {
					locations[1] = locations[1] +moves[0];
					updateLoc = locations[0] + originPosition;
				} else  {
					locations[0] = locations[0] + moves[1];
					updateLoc = locations[0] + originPosition;
				}
				Loom.DispatchToMainThread((object loc) => Debug.Log("Where am I (" + ((Vector3)loc).x + " " +((Vector3)loc).y + " " +((Vector3)loc).z+")"), locations[0],true,true);
				Loom.DispatchToMainThread((object loc2) => GameObject.Find("Sphere").rigidbody.MovePosition((Vector3)loc2), updateLoc,true,true);

			}*/
		}
	}

	// one worker thread version
	/*void pixyRoutine(){
		Vector3 originPosition = (Vector3)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.position);
		Quaternion originRotation = (Quaternion)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.rotation);
		
		Vector3 updateLoc;
		Quaternion rotation;
		AirVisual.PixyCam pixy = new AirVisual.PixyCam () ;
		int nAve = 2;
		int first = nAve;
		float offSum = 0;
		while (notDestroyed) {
			pixy.pixyThread();
			
			// modifying the matrix
			for (int i =0; i< AirVisual.PixyCam.ledN;i++)
				{
					if (pixy.detected[i])
					{
						// if the corresponding led is detected
						pixyData[0,4*i] = 1;
						// changed the x,y,z data
						pixyData[0,4*i+1] = pixy.locations[i].x;
						pixyData[0,4*i+2] = pixy.locations[i].y;
						pixyData[0,4*i+3] = pixy.locations[i].z - pixy.zoff;
						//Loom.DispatchToMainThread((object loc) => Debug.Log("Where am I (" + ((Vector3)loc).x + " " +((Vector3)loc).y + " " +((Vector3)loc).z+")"), pixy.locations[0],true,true);
						
					} else{
						// if not detected
						pixyData[0,4*i] = -1;
					}
				}
			
			// the following lines access the pixyData variable

				for (int i = 0; i<AirVisual.PixyCam.ledN; i++) {
					locations[i].x = (float) pixyData[0,4*i+1];
					locations[i].y = (float) pixyData[0,4*i+2];
					locations[i].z = (float) pixyData[0,4*i+3];
					detected[i] = (pixyData[0,4*i] >= 0);
				}
			
			
			//pixy.zfactor = -1 * zScale;
			if (detected[0] || detected[1])
			{
				if (detected [0] && detected [1]) {
					rotation = Quaternion.LookRotation(locations[1] - locations[0])*originRotation;
					Loom.DispatchToMainThread((object rot) => GameObject.Find("Sphere").rigidbody.MoveRotation((Quaternion) rot), rotation,true,true);
					//GameObject.Find("Sphere").rigidbody.MoveRotation(rotation);
					//GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
					updateLoc = locations[0] + originPosition;
				} else if (detected [0]) {
					//GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
					locations[1] = locations[1] +moves[0];
					updateLoc = locations[0] + originPosition;
				} else  { //(pixy.detected [1])
					locations[0] = locations[0] + moves[1];
					//GameObject.Find("Sphere").rigidbody.MovePosition(pixy.locations[0] + pixy.originPosition);
					updateLoc = locations[0] + originPosition;
				}
				//Debug.Log ("Where am I (" + pixy.locations [0].x + " " + pixy.locations [0].y + " " + pixy.locations [0].z+")");
				//Loom.DispatchToMainThread(() => Debug.Log("I waited atleast 30 frames. Whats the current frameCount? : " + Time.frameCount), true);
				//Loom.DispatchToMainThread((object cam) => Debug.Log("Where am I (" + ((Cheese.Pixy)cam).locations [0].x + " " +((Cheese.Pixy)cam).locations [0].y + " " +((Cheese.Pixy)cam).locations [0].z+")"), pixy,true,true);
				Loom.DispatchToMainThread((object loc) => Debug.Log("Where am I (" + ((Vector3)loc).x + " " +((Vector3)loc).y + " " +((Vector3)loc).z+")"), locations[0],true,true);
				Loom.DispatchToMainThread((object loc2) => GameObject.Find("Sphere").rigidbody.MovePosition((Vector3)loc2), updateLoc,true,true);
				
			}
			if (first > 0)
			{
				offSum = offSum + pixy.locations[0].z;
				first--;
				if (first <= 0)
				{
					pixy.zoff = offSum/nAve;
				}
			}
		}

	}

	void kalmanRoutine()
	{

	}
*/
}
