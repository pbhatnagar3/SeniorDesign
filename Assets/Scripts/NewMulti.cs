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
	//private Object[] originTrans = new Object[2];
	//private ThreadPoolScheduler myThreadScheduler;
	//private Cheese.Pixy pixy;
	
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
	void OnGUI() {
		//zScale = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), zScale, 0.0F, 0.01F);
	}
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
	
	void pixyRoutine(){
		Vector3 orgPos = (Vector3)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.position);
		Quaternion orgRot = (Quaternion)  Loom.DispatchToMainThreadReturn(()=>GameObject.Find ("Sphere").transform.rotation);
		Cheese.Pixy pixy = new Cheese.Pixy (orgPos, orgRot) ;
		Vector3 updateLoc;
		Quaternion rotation;
		int nAve = 2;
		int first = nAve;
		float offSum = 0;
		while (notDestroyed) {

			pixy.pixyThread();
			//pixy.zfactor = -1 * zScale;
			 if (pixy.detected[0] || pixy.detected[1])
			{
			if (pixy.detected [0] && pixy.detected [1]) {
					rotation = Quaternion.LookRotation(pixy.locations[1] - pixy.locations[0])*pixy.originRotation;
					Loom.DispatchToMainThread((object rot) => GameObject.Find("Sphere").rigidbody.MoveRotation((Quaternion) rot), rotation,true,true);
				//GameObject.Find("Sphere").rigidbody.MoveRotation(rotation);
				//GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
				updateLoc = pixy.locations[0] + pixy.originPosition;
			} else if (pixy.detected [0]) {
				//GameObject.Find("Sphere").rigidbody.MovePosition (pixy.locations[0] + pixy.originPosition);
				pixy.locations[1] = pixy.locations[1] + pixy.moves[0];
				updateLoc = pixy.locations[0] + pixy.originPosition;
				} else  { //(pixy.detected [1])
				pixy.locations[0] = pixy.locations[0] + pixy.moves[1];
				//GameObject.Find("Sphere").rigidbody.MovePosition(pixy.locations[0] + pixy.originPosition);
				updateLoc = pixy.locations[0] + pixy.originPosition;
			}
			//Debug.Log ("Where am I (" + pixy.locations [0].x + " " + pixy.locations [0].y + " " + pixy.locations [0].z+")");
			//Loom.DispatchToMainThread(() => Debug.Log("I waited atleast 30 frames. Whats the current frameCount? : " + Time.frameCount), true);
			//Loom.DispatchToMainThread((object cam) => Debug.Log("Where am I (" + ((Cheese.Pixy)cam).locations [0].x + " " +((Cheese.Pixy)cam).locations [0].y + " " +((Cheese.Pixy)cam).locations [0].z+")"), pixy,true,true);
			Loom.DispatchToMainThread((object loc) => Debug.Log("Where am I (" + ((Vector3)loc).x + " " +((Vector3)loc).y + " " +((Vector3)loc).z+")"), pixy.locations[0],true,true);
			Loom.DispatchToMainThread((object loc2) => GameObject.Find("Sphere").rigidbody.MovePosition((Vector3)loc2), updateLoc,true,true);
			if (first > 0)
				{
					offSum = offSum + updateLoc.z - pixy.originPosition.z;
					first--;
					if (first <= 0)
					{
						pixy.zoff = offSum/nAve;
					}
				}
			}
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
