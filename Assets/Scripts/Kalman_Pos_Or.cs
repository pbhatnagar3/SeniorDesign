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
	//Acceleration, postion and orientation



	public class Kalman_Pos_Or
	{
		int	POSX = 0, POSY = 1 , POSZ = 2, VELX = 3, VELY = 4, VELZ = 5,
		ACCX = 6, ACCY = 7, ACCZ = 8, ANG_X = 9, ANG_Y = 10, ANG_Z = 11,
		VEL_ANGX = 12, VEL_ANGY = 13, VEL_ANGZ = 14, ANG_DDX = 15, ANG_DDY = 16, ANG_DDZ = 17;


		Matrix X;
		//Something about U
		Matrix U;
		//Something about Y
		Matrix Y;

		Matrix P;

		Matrix Q;

		//Something about M
		Matrix S;

		Matrix K;


		Matrix R;

		Matrix F;
		Matrix H;
		Matrix I;
		double LastGain;
		double dt, dt_Square;

		public Kalman_Pos_Or(Matrix iniVariance, double dt, double q, double r)
		{
			//Use an initial guess for covariance matrices, P. Diagonal elements = initial variance guess
			P = iniVariance;
			//Q - added to P
			if (q == -827.649)
				Q = iniVariance * Matrix.IdentityMatrix (18, 18);		
			else
				Q = q * Matrix.IdentityMatrix (18, 18);		
			//Add values every iteration (not needed for now)
			U = 0*Matrix.ZeroMatrix(18, 1);
			//Y - innovation, state - measurement. Used to measure new state. Initial value doesn't matter
			Y = Matrix.ZeroMatrix(9, 1);
			//Reinitialized in code. Initial value don't matter
			S = Matrix.ZeroMatrix(9, 9);
			//Reinitialize R to a small value to prevent locking
			R = r*Matrix.IdentityMatrix (9, 9);

			//K - Kalman gain
			K = Matrix.IdentityMatrix (9, 9);

			//Process model - relate new state to old (based on http://campar.in.tum.de/Chair/KalmanFilter, Lin with pos tracking only)
			//Note - the string for parsing is generated in MATLAB
			this.dt = dt;
			dt_Square = (dt * dt) / 2;

			//String created in matlab (much easier)
			string f_input = "1  0  0  a  0  0  b  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  1  0  0  a  0  0  b  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  1  0  0  a  0  0  b  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  1  0  0  a  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  0  1  0  0  a  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  0  0  1  0  0  a  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  1  0  0  a  0  0  b  0  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  1  0  0  a  0  0  b  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  1  0  0  a  0  0  b   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  a  0  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  a  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  a   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0   \r\n   " +
				             "0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  1          " ;

			f_input = f_input.Replace ("a", dt.ToString ());
			f_input = f_input.Replace ("b", dt_Square.ToString ());

			F = Matrix.Parse (f_input);

			// H only cares about last 3 values (Acceleration)
			//H = Matrix.Parse ("0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  1  0  0   \r\n   0  0  0  0  0  0  0  1  0   \r\n   0  0  0  0  0  0  0  0  1");
			H = Matrix.Parse ("1  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				              "0  1  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				              "0  0  1  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				              "0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				              "0  0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0  0   \r\n   " +
				              "0  0  0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0   \r\n   " +
				              "0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  0  0  0   \r\n   " +
				              "0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  0  0   \r\n   " +
				              "0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  0          ");

			//Identity matrix used by the program
			I = Matrix.IdentityMatrix (18,18);
		}
		
	
		/// <summary>
		/// Update the state by measurement m at dt time from last measurement.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="dt"></param>
		/// <returns></returns>
		public void Update(double dt) //Matrix m, double dt
		{
//			this.dt = dt;
//			dt_Square = dt * dt / 2;
//			string f_input = "1  0  0  a  0  0  b  0  0  0  0  0  0  0  0  0  0  0   \r\n   0  1  0  0  a  0  0  b  0  0  0  0  0  0  0  0  0  0   \r\n   0  0  1  0  0  a  0  0  b  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  1  0  0  a  0  0  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  1  0  0  a  0  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  1  0  0  a  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  1  0  0  0  0  0  0  0  0  0   \r\n   0  0  0  0  0  0  0  0  0  1  0  0  a  0  0  b  0  0   \r\n   0  0  0  0  0  0  0  0  0  0  1  0  0  a  0  0  b  0   \r\n   0  0  0  0  0  0  0  0  0  0  0  1  0  0  a  0  0  b   \r\n   0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  a  0  0   \r\n   0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  a  0   \r\n   0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0  a   \r\n   0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0  0   \r\n   0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  1  0   \r\n   0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  1";
//			f_input = f_input.Replace ("a", dt.ToString ());
//			f_input = f_input.Replace ("b", dt_Square.ToString ());

			// Predict to now, then update.
			// Predict:
			//   X = F*X + H*U
			//   P = F*P*F^T + Q.
			// Update:
			//   Y = M – H*X          Called the innovation = measurement – state transformed by H.	
			//   S = H*P*H^T + R      S= Residual covariance = covariane transformed by H + R
			//   K = P * H^T *S^-1    K = Kalman gain = variance / residual covariance.
			//   X = X + K*Y          Update with gain the new measurement
			//   P = (I – K * H) * P  Update covariance to this time.
			//
			// Same as 1D but mv is used instead of delta m_x[0], and H = [1,1].
			// Return latest estimate.

		}

		public Matrix predictionStep(Matrix X) 
		{
			//PREDICTION STEP
			// X = F*X + H*U (U = 0 for now)
			X = F*X;			
			// P = F*P*F^T + Q

			P = F * P * Matrix.Transpose(F) + Q;

			return X;
		}

		public Matrix updateStep(Matrix X,Matrix M)
		{
			//Measurement Step
			// Y = M – H*X  
			Y = M - H * X;
			
			// S = H*P*H^T + R ---> R = 0 for now
			S = H * P * Matrix.Transpose (H) + R;
			
			// K = P * H^T *S^-1 
			Matrix tmp = P * Matrix.Transpose (H);
			Matrix sinv;
			try{
				//				Debug.Log(S.ToString());
				sinv = S.Invert();
				K = tmp * sinv;
			}
			catch{
				//determinant corresponding to Zero matrix
				K = Matrix.ZeroMatrix(18,9);
				Debug.Log ("Not Determinant");
			}
			
			// X = X + K*Y
			X = X + K * Y;
			
			// P = (I – K * H) * P
			P = (I - K * H) * P;



			//X [ANG_DDX] = 0;
			//X [ANG_DDY] = 0;
			//X [ANG_DDZ] = 0;

			return X;
		}

		public Matrix getState()
		{
			return X;
		}
		
	}
}

