using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

public class SpeechRecognition : MonoBehaviour
{
	Thread receiveThread;
	UdpClient client;
	public int port = 26000; // DEFAULT UDP PORT !!!!! THE QUAKE ONE ;)
	string strReceiveUDP = "";
	string LocalIP = String.Empty;
	string hostname;
	bool isChangeSkyBox = false;

	public Material initial;
	public Material[] Skyboxes = new Material[7];
	public System.Diagnostics.Process speechRecognitionProcess;

	void OnDestroy() {
		speechRecognitionProcess.CloseMainWindow ();
		speechRecognitionProcess.Close ();
		print("Script was destroyed");
	}

	public void Update(){
		if (isChangeSkyBox) {
			//Camera.main.GetComponent<Skybox>().material = Skyboxes[Random.Range(0,Skyboxes.Length)];
			//					strReceiveUDP
			isChangeSkyBox = false;
			int index = UnityEngine.Random.Range(0,Skyboxes.Length);
			Debug.Log("generated index: " + index);
			RenderSettings.skybox = Skyboxes[index];

		}

		if (Input.GetKeyDown (KeyCode.Z)) {
			//Camera.main.GetComponent<Skybox>().material = Skyboxes[Random.Range(0,Skyboxes.Length)];
			RenderSettings.skybox = Skyboxes[UnityEngine.Random.Range(0,Skyboxes.Length)];
		}
	}

	public void Start()
	{
		string exePath = "Assets\\SpeechRecognition\\SpeechRecognition_64.exe";
		string exeFullPath = System.IO.Path.GetFullPath(exePath);
		Debug.Log (exeFullPath);
		System.Diagnostics.ProcessStartInfo theProcess = new System.Diagnostics.ProcessStartInfo(exeFullPath);
		
		theProcess.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
		speechRecognitionProcess = System.Diagnostics.Process.Start(theProcess);
		


		Application.runInBackground = true;
		init();  
		RenderSettings.skybox = initial;
	}
	// init
	private void init()
	{
		receiveThread = new Thread( new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
		hostname = Dns.GetHostName();
		IPAddress[] ips = Dns.GetHostAddresses(hostname);
		if (ips.Length > 0)
		{
			LocalIP = ips[0].ToString();
			Debug.Log(" MY IP : "+LocalIP);
		}
	}
	
	private  void ReceiveData()
	{
		client = new UdpClient(port);
		while (true)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Broadcast, port);
				byte[] data = client.Receive(ref anyIP);
				strReceiveUDP = Encoding.UTF8.GetString(data);

				isChangeSkyBox = (strReceiveUDP.CompareTo("change") == 0);
				
					// ***********************************************************************
				// Simple Debug. Must be replaced with SendMessage for example.
				// ***********************************************************************
				Debug.Log(strReceiveUDP);

				// ***********************************************************************
			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}
	
	public string UDPGetPacket()
	{
		return strReceiveUDP;
	}
	
	void OnDisable()
	{
		if ( receiveThread != null) receiveThread.Abort();
		client.Close();
	}
}