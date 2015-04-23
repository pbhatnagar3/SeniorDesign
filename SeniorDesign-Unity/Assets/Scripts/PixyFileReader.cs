using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;

namespace PixyFileReader
{
	class Program
	{
		public Vector2[] LeftLedTracking = new Vector2[4];
		public Vector2[] RightLedTracking = new Vector2[4];

		//pixyDataAvailiable
		bool pixyDataAvailiable = false;

		public Program()
		{
			for (int i=0; i<4; i++) 
			{
				LeftLedTracking[i] = new Vector3(0,0,0);
				RightLedTracking[i] = new Vector3(0,0,0);
			}
		}

		public void readFile()
		{
			String leftFile = "C:/Users/shurjobanerjee/Documents/SeniorDesign-master/JUSTIN_YOU_SUCK/0.txt";
			String rightFile = "C:/Users/shurjobanerjee/Documents/SeniorDesign-master/JUSTIN_YOU_SUCK/1.txt";
			string[] leftDat ={"",""}, rightDat={"",""};
			int index, l_n, r_n;

			Dictionary<string, int> dictionary = new Dictionary<string, int>();

			dictionary.Add ("s=1", 0);
			dictionary.Add ("s=2", 1);
			dictionary.Add ("s=3", 2);
			dictionary.Add ("s=4", 3);
			int i = 0;

			try
			{
				leftDat = File.ReadAllLines(leftFile);
				rightDat = File.ReadAllLines(rightFile);
				
				leftDat = leftDat[0].Split(',');
				rightDat = rightDat[0].Split(',');
				
				//Deal with left string
				l_n = System.Convert.ToInt32(leftDat[0]);
				for (i=1; i<3*l_n; i+=3)
				{
					index = dictionary[leftDat[i]];
					LeftLedTracking[index].x = System.Convert.ToInt32(leftDat[i+1]);
					LeftLedTracking[index].y = System.Convert.ToInt32(leftDat[i+2]);
				}

				//Deal with right string
				//Deal with left string
				r_n = System.Convert.ToInt32(rightDat[0]);
				for (i=1; i<3*r_n; i+=3)
				{
					index = dictionary[rightDat[i]];
					RightLedTracking[index].x = System.Convert.ToInt32(rightDat[i+1]);
					RightLedTracking[index].y = System.Convert.ToInt32(rightDat[i+2]);
				}

				pixyDataAvailiable = true;
			}
			catch (Exception ex)
			{
				//UnityEngine.Debug.Log(ex.ToString() + ": " + leftDat[i]+","+rightDat[i]);
				pixyDataAvailiable = false;
			}
		}

		public bool successfulRead()
		{
			return pixyDataAvailiable;
		}
	}
}