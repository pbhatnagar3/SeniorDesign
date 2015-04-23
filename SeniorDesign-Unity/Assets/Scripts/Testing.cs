using System;
using UnityEngine;
using Frankfort.Threading;
using System.Threading;
using System.Collections;



public class Testing : MonoBehaviour {
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
		notDestroyed = false;
	}
	
	
	void Start()
	{
		pixyData = MatrixLibrary.Matrix.ZeroMatrix(1,AirVisual.PixyCam.ledN * 4);
		// just for testing purpose
		for (int i = 0; i<AirVisual.PixyCam.ledN; i++) {
			locations[i] = new Vector3(0,0,0);
			detected[i] = false;
			moves[i] = new Vector3(0,0,0);
		}
		//pixy = new Cheese.Pixy ();
		//originTrans
		//originTrans [0] = GameObject.Find ("Sphere").transform.position;
		//originTrans [1] = GameObject.Find ("Shpere").transform.rotation;
		Application.targetFrameRate = 25;
		//myThreadScheduler = Loom.CreateThreadPoolScheduler();
		
		//--------------- Ending Single threaded routine --------------------
		//pixyThread = Loom.StartSingleThread(pixyRoutine, System.Threading.ThreadPriority.Normal, true);
		pixyThread = Loom.StartSingleThread(pixyRoutine, System.Threading.ThreadPriority.Normal, true);
		kalmanThread = Loom.StartSingleThread (kalmanRoutine, System.Threading.ThreadPriority.Normal, true);
		//--------------- Ending Single threaded routine --------------------
		
		//--------------- Continues Single threaded routine --------------------
		// threadB = Loom.StartSingleThread(ContinuesSingleThreadCoroutine, System.Threading.ThreadPriority.Normal, true);
		//--------------- Continues Single threaded routine --------------------
		
		//--------------- Start Multithreaded packages --------------------
		//int i = TestWorkerObjects;
		//IThreadWorkerObject[] workerObjects = new IThreadWorkerObject[TestWorkerObjects];
		
		// while (--i > -1)
		//   workerObjects[i] = new LotsOfNumbers(UnityEngine.Random.Range(minCalculations, maxCalculations));
		
		//myThreadScheduler.StartASyncThreads(workerObjects, OnThreadWorkComplete, OnWorkerObjectDone, maxThreads);
		//StartCoroutine(AbortAllThreadsAfterDelay());
		//--------------- Start Multithreaded packages --------------------	
		//		sphere = GameObject.Find ("Sphere");
	}
	//void OnGUI() {
		//zScale = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), zScale, 0.0F, 0.01F);
	//}
	void Update(){
		// move the gameobject
		/*if (pixy.detected [0] && pixy.detected [1]) {
			Quaternion rotation = Quaternion.LookRotation(pixy.locations[1] - pixy.locations[0]);
			GameObject.Find("Sphere").rigidbody.MoveRotation(rotation);
			GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
		} else if (pixy.detected [0]) {
			GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
			pixy.locations[1] = pixy.locations[1] + pixy.moves[0];
		} else if (pixy.detected [1]) {
			pixy.locations[0] = pixy.locations[0] + pixy.moves[1];
			GameObject.Find("Sphere").rigidbody.MovePosition(pixy.locations[0] + pixy.originPosition);
		}*/
		
	}

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
		Vector3 originPosition = (Vector3)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.position);
		Quaternion originRotation = (Quaternion)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.rotation);
		
		Vector3 updateLoc;
		Quaternion rotation;

		while (notDestroyed) {
			
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
		}
	}

	// Update is called once per frame
	//	void Update () {
	//		Debug.Log ("hello there");
	//	}
}
