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
	public class Kalman
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

		Matrix R;

		Matrix Fp, Fa;
		Matrix Hp, Ha;
		Matrix I;
		double LastGain;

		Vector3 pos;

		double dt1, dt2;
		double dt1Sq, dt2Sq;

		public Kalman(Matrix iniVariance, double dt1, double dt2, double r, double q)
		{
			//Use an initial guess for covariance matrices, P. Diagonal elements = initial variance guess
			P = iniVariance * Matrix.IdentityMatrix (9, 9);
			//Q - added to P
//			if (q == -827.649)
				Q = iniVariance * Matrix.IdentityMatrix (9, 9);		
//			else
//				Q = q * Matrix.IdentityMatrix (9, 9);		
			//Add values every iteration (not needed for now)
			U = Matrix.ZeroMatrix(9, 1);
			//Y - innovation, state - measurement. Used to measure new state. Initial value doesn't matter
			Y = Matrix.ZeroMatrix(3, 1);
			//Reinitialized in code. Initial value don't matter
			S = Matrix.ZeroMatrix(3, 3);
			//Reinitialize R to a small value to prevent locking
			R = 10000*Matrix.IdentityMatrix (3, 3);
			
			//K - Kalman gain
			K = Matrix.IdentityMatrix (9, 9);

			//Process model - relate new state to old (based on http://campar.in.tum.de/Chair/KalmanFilter, Lin with pos tracking only)
			//Note - the string for parsing is generated in MATLAB

			this.dt1 = dt1;
			dt1Sq = (dt1 * dt1) / 2;

			this.dt2 = dt2;
			dt2Sq = (dt2 * dt2) / 2;

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

			string Fa_input = (f_input.Replace ("a", dt1.ToString ())).Replace("b", dt1Sq.ToString ());
			string Fp_input = (f_input.Replace ("a", dt2.ToString ())).Replace("b", dt2Sq.ToString ());

			Fp = Matrix.Parse (Fp_input);
			Fa = Matrix.Parse (Fa_input);

			// H only cares about last 3 values (Acceleration)
			//H = Matrix.Parse ("0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  1  0  0   \r\n   0  0  0  0  0  0  0  1  0   \r\n   0  0  0  0  0  0  0  0  1");
			Ha = Matrix.Parse ("0  0  0  0  0  0  1  0  0   \r\n   " +
				               "0  0  0  0  0  0  0  1  0   \r\n   " +
				               "0  0  0  0  0  0  0  0  1");

			Hp = Matrix.Parse ("1  0  0  0  0  0  0  0  0   \r\n   " +
			                   "0  1  0  0  0  0  0  0  0   \r\n   " +
			                   "0  0  1  0  0  0  0  0  0");

			//Identity matrix used by the program
			I = Matrix.IdentityMatrix (9, 9);

		}
		
		public Matrix predictionStepAcceleration(Matrix X) 
		{
			//PREDICTION STEP
			// X = F*X + H*U (U = 0 for now)
			X = Fa*X;			
			// P = F*P*F^T + Q
			P = Fa * P * Matrix.Transpose(Fa) + Q;
			
			return X;
		}

		public Matrix predictionStepPosition(Matrix X) 
		{

			//PREDICTION STEP
			// X = F*X + H*U (U = 0 for now)
			X = Fp*X;			
			// P = F*P*F^T + Q
			P = Fp * P * Matrix.Transpose(Fp) + Q;
			
			return X;
		}

		//Accelerometers
		public Matrix updateStepAcceleration(Matrix X,Matrix Ma)
		{
			//Measurement Step
			// Y = M – H*X  
			Y = Ma - Ha * X;
			
			// S = H*P*H^T + R ---> R = 0 for now
			S = Ha * P * Matrix.Transpose (Ha) + R;
			
			// K = P * H^T *S^-1 
			Matrix tmp = P * Matrix.Transpose (Ha);
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
			P = (I - K * Ha) * P;
			
			return X;
		}

		//PIXY
		public Matrix updateStepPosition(Matrix X,Matrix Mp)
		{
			//Measurement Step
			// Y = M – H*X  
			Y = Mp - Hp * X;
			
			// S = H*P*H^T + R ---> R = 0 for now
			S = Hp * P * Matrix.Transpose (Hp) + R;
			
			// K = P * H^T *S^-1 
			Matrix tmp = P * Matrix.Transpose (Hp);
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
			P = (I - K * Hp) * P;

			return X;
		}

		public Matrix getState()
		{
			return X;
		}
	}
}