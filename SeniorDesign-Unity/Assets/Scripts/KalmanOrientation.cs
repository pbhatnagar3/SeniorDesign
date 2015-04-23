///////////////////////////////////////////////////////////////////////////////
//
//  Kalman2D.cs
//
//  By Philip R. Braica (HoshiKata@aol.com, VeryMadSci@gmail.com)
//
//  Distributed under the The Code Project Open License (CPOL)
//  http://www.codeproject.com/info/cpol10.aspx
///////////////////////////////////////////////////////////////////////////////

// Using.
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixLibrary;

/// <summary>
/// Kalman 
/// </summary>

namespace KalmanFilterImplementation
{
	public class KalmanOrientation
	{
		int	POSX = 0, POSY = 1 , POSZ = 2, VELX = 3, VELY = 4, VELZ = 5, ACCX = 6, ACCY = 7, ACCZ = 8;

		Matrix X;
		//Something about U
		Matrix U;
		//Something about Y
		Matrix Y;

		/// <summary>
		/// Covariance.
		/// </summary>
		Matrix P;
		/// <summary>
		/// Minimal covariance.
		/// </summary>
		Matrix Q;

		//Something about M
		Matrix S;

		Matrix K;
		
		/// <summary>
		/// Minimal innovative covariance, keeps filter from locking in to a solution.
		/// </summary>
		Matrix R;

		Matrix Fo;
		Matrix Ho, Ha;
		Matrix I;
		double LastGain;

		Vector3 pos;

		double dt;
		double dtSq;

		public KalmanOrientation(Matrix iniVariance, double dt, double r, double q)
		{
			//Use an initial guess for covariance matrices, P. Diagonal elements = initial variance guess
			P = iniVariance * Matrix.IdentityMatrix (9, 9);
			//Q - added to P
			if (q == -827.649)
				Q = iniVariance * Matrix.IdentityMatrix (9, 9);		
			else
				Q = q * Matrix.IdentityMatrix (9, 9);		
			//Add values every iteration (not needed for now)
			U = Matrix.ZeroMatrix(9, 1);
			//Y - innovation, state - measurement. Used to measure new state. Initial value doesn't matter
			Y = Matrix.ZeroMatrix(3, 1);
			//Reinitialized in code. Initial value don't matter
			S = Matrix.ZeroMatrix(3, 3);
			//Reinitialize R to a small value to prevent locking
			R = r*Matrix.IdentityMatrix (3, 3);

			//K - Kalman gain
			K = Matrix.IdentityMatrix (9, 9);

			//Process model - relate new state to old (based on http://campar.in.tum.de/Chair/KalmanFilter, Lin with pos tracking only)
			//Note - the string for parsing is generated in MATLAB

			this.dt = dt;
			dtSq = (dt * dt) / 2;

			//String created in matlab (much easier)
			string f_input = "1 0 0 a 0 0 b 0 0   \r\n   " +
				             "0 1 0 0 a 0 0 b 0   \r\n   " +
				             "0 0 1 0 0 a 0 0 b   \r\n   " +
				             "0 0 0 1 0 0 a 0 0   \r\n   " +
				             "0 0 0 0 1 0 0 a 0   \r\n   " +
				             "0 0 0 0 0 1 0 0 a   \r\n   " +
				             "0 0 0 0 0 0 1 0 0   \r\n   " +
				             "0 0 0 0 0 0 0 1 0   \r\n   " +
				             "0 0 0 0 0 0 0 0 1";

			string Fo_input = (f_input.Replace ("a", dt.ToString ())).Replace("b", dtSq.ToString ());

			Fo = Matrix.Parse (Fo_input);

			// H only cares about last 3 values (Acceleration)
			//H = Matrix.Parse ("0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  1  0  0   \r\n   0  0  0  0  0  0  0  1  0   \r\n   0  0  0  0  0  0  0  0  1");
			Ho = Matrix.Parse ("0  0  0  1  0  0  0  0  0   \r\n   " +
				               "0  0  0  0  1  0  0  0  0   \r\n   " +
				               "0  0  0  0  0  1  0  0  0");

			Ha = Matrix.Parse ("1  0  0  0  0  0  0  0  0   \r\n   " +
			                   "0  1  0  0  0  0  0  0  0   \r\n   " +
			                   "0  0  1  0  0  0  0  0  0");

			//Identity matrix used by the program
			I = Matrix.IdentityMatrix (9, 9);

		}
		
		public Matrix predictionStepOrientation(Matrix X) 
		{
			//Not considering acceleration
			X [6] = 0;
			X [7] = 0;
			X [8] = 0;

			//PREDICTION STEP
			// X = F*X + H*U (U = 0 for now)
			X = Fo*X;			
			// P = F*P*F^T + Q
			P = Fo * P * Matrix.Transpose(Fo) + Q;
			
			return X;
		}

		//Accelerometers
		public Matrix updateStepOrientation(Matrix X,Matrix Mo)
		{
			//Measurement Step
			// Y = M – H*X  
			Y = Mo - Ho * X;
			
			// S = H*P*H^T + R ---> R = 0 for now
			S = Ho * P * Matrix.Transpose (Ho) + R;
			
			// K = P * H^T *S^-1 
			Matrix tmp = P * Matrix.Transpose (Ho);
			Matrix sinv;
			try{
				//				Debug.Log(S.ToString());
				sinv = S.Invert();
				K = tmp * sinv;
			}
			catch{
				//determinant corresponding to Zero matrix
				K = Matrix.ZeroMatrix(9,3);
				Debug.Log ("Not Determinant");
			}
			
			// X = X + K*Y
			X = X + K * Y;
			
			// P = (I – K * H) * P
			P = (I - K * Ho) * P;


			return X;
		}

		public Matrix updateStepAngles(Matrix X,Matrix Mangles)
		{
			//Measurement Step
			// Y = M – H*X  
			Y = Mangles - Ho * X;
			
			// S = H*P*H^T + R ---> R = 0 for now
			S = Ho * P * Matrix.Transpose (Ho) + R;
			
			// K = P * H^T *S^-1 
			Matrix tmp = P * Matrix.Transpose (Ho);
			Matrix sinv;
			try{
				//				Debug.Log(S.ToString());
				sinv = S.Invert();
				K = tmp * sinv;
			}
			catch{
				//determinant corresponding to Zero matrix
				K = Matrix.ZeroMatrix(9,3);
				Debug.Log ("Not Determinant");
			}
			
			// X = X + K*Y
			X = X + K * Y;
			
			// P = (I – K * H) * P
			P = (I - K * Ho) * P;
			
			
			return X;
		}


		public Matrix getState()
		{
			return X;
		}
	}
}