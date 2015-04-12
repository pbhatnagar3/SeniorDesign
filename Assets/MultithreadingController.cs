using System;
using UnityEngine;
using Frankfort.Threading;
using System.Threading;
using System.Collections;



public class MultithreadingController : MonoBehaviour {
	public float zoff = 2;
	private bool notDestroyed = true;
	//public int maxThreads = 2;
	//public int TestWorkerObjects = 4;
	//public int minCalculations = 10;
	//public int maxCalculations = 50;
	//public float abortAfterSeconds = 3f;	
	public Thread pixyThread;
	public Thread kalmanThread;
	//private ThreadPoolScheduler myThreadScheduler;
	private Cheese.Pixy pixy;

	// Use this for initialization
//	void Start () {
//	
//	}

	void OnDestroy()
	{
		notDestroyed = false;
	}


	void Start()
	{
		pixy = new Cheese.Pixy ();
		Application.targetFrameRate = 25;
		//myThreadScheduler = Loom.CreateThreadPoolScheduler();
		
		//--------------- Ending Single threaded routine --------------------
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


	void Update(){
		Debug.Log ("Where am I (" + pixy.locations [0].x + " " + pixy.locations [0].y + " " + pixy.locations [0].z+")");
		// move the gameobject
		if (pixy.detected [0] && pixy.detected [1]) {
			Quaternion rotation = Quaternion.LookRotation(pixy.locations[1] - pixy.locations[0]);
			GameObject.Find("Sphere").rigidbody.MoveRotation(rotation);
			GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
		} else if (pixy.detected [0]) {
			GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
			pixy.locations[1] = pixy.locations[1] + pixy.moves[0];
		} else if (pixy.detected [1]) {
			pixy.locations[0] = pixy.locations[0] + pixy.moves[1];
			GameObject.Find("Sphere").rigidbody.MovePosition(pixy.locations[0] + pixy.originPosition);
		}
		
	}

	void pixyRoutine(){
		while (notDestroyed) {
			pixy.pixyThread();

		}
	}

	void kalmanRoutine()
	{
		while (notDestroyed) {

		}
	}
	// Update is called once per frame
//	void Update () {
//		Debug.Log ("hello there");
//	}
}
