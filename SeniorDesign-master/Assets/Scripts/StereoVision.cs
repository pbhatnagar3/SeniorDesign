/**
 * @author: Qisi Wang
 * Class facilitating pixy integration with Unity.
 *
 * for any questions, please don't hestitate to contact qisi.wang@gmail.com
 * 
 * May the force of compiler be with you. - Pujun
 */

using UnityEngine;
using MatrixLibrary;
using System;
using UnityEngine;
using System.Collections;	

namespace StereoVision{
	public class StereoCamera {
		private Camera leftCam;
		private Camera rightCam;
		private double[] R = {	0.9999, 	0.0134, 	0.0075,
			                   -0.0133, 	0.9999,    -0.0090,
			                   -0.0076, 	0.0089, 	0.9999};
		private Vector3 Tvec = new Vector3(-52.89272f,-0.31508f,-0.64784f);
		
		public StereoCamera()
		{
			
			leftCam = new Camera(243.76014,   241.61370, 170.94012,   91.58828,  -0.42595,   0.18390,   0.00160,   -0.00186);
			rightCam = new Camera(240.05098,   238.91093, 158.97058,   89.72589, -0.42929,   0.16953,   -0.00193,   -0.00057);
		}
		
		public Vector3 triangulation(int xl, int yl, int xr, int yr)
		{
			// Normalize hte image projection according ot the intrinsic parameters of the left and right cameras
			Vector2 xl2 = leftCam.normalization(new Vector2(xl, yl));
			Vector2 xr2 = rightCam.normalization(new Vector2(xr, yr));
			
			// Extend the normalized projections in homogeneous coordinates
			Vector3 xl3 = new Vector3(xl2.x,xl2.y,1);
			Vector3 xr3 = new Vector3(xr2.x,xr2.y,1);
			
			Vector3 u = new Vector3((float)(R[0]*xl3.x+R[1]*xl3.y+R[2]*xl3.z),
			                        (float)(R[3]*xl3.x+R[4]*xl3.y+R[5]*xl3.z),
			                        (float)(R[6]*xl3.x+R[7]*xl3.y+R[8]*xl3.z));
			
			double n_xl3_2 = Vector3.Dot(xl3,xl3);
			double n_xr3_2 = Vector3.Dot(xr3,xr3);
			
			double DD = n_xl3_2 * n_xr3_2 - (Vector3.Dot(xr3,u))*(Vector3.Dot(u,xr3));
			
			double dot_uT = Vector3.Dot(u,Tvec);
			double dot_xrT = Vector3.Dot(Tvec,xr3);
			double dot_xru = Vector3.Dot(xr3,u);
			
			double NN1 = dot_xru*dot_xrT - n_xr3_2 * dot_uT;
			double NN2 = n_xl3_2*dot_xrT - dot_uT*dot_xru;
			
			double Zl = NN1/DD;
			double Zr = NN2/DD;
			
			double x1 = xl3.x*Zl;
			double y1 = xl3.y*Zl;
			double z1 = xl3.z*Zl;
			
			double xTemp = xr3.x*Zr - Tvec.x;
			double yTemp = xr3.y*Zr - Tvec.y;
			double zTemp = xr3.z*Zr - Tvec.z;
			
			double x2 = R[0]*xTemp + R[3]*yTemp + R[6]*zTemp;
			double y2 = R[1]*xTemp + R[4]*yTemp + R[7]*zTemp;
			double z2 = R[2]*xTemp + R[5]*yTemp + R[8]*zTemp;
			
			xTemp = (x1+x2)/2;
			yTemp = (y1+y2)/2;
			zTemp = (z1+z2)/2;
			
			
			return new Vector3((float)xTemp, (float)yTemp, (float)zTemp);
			
		}
		
	}
	
	public class Camera{
		public Vector2 fc;
		public Vector2 cc;
		public double k1;
		public double k2;
		public double p1;
		public double p2;
		public double k3;
		public double alpha;
		
		public Camera(double _fcX, double _fcY, double _ccX = 0, double _ccY = 0, double _kc1 = 0, double _kc2 = 0, double _kc3 = 0, double _kc4 = 0, double _kc5 = 0, double _alpha = 0)
		{
			fc = new Vector2 ((float)_fcX, (float)_fcY);
			
			cc = new Vector2 ((float)_ccX, (float)_ccY);
			
			k1 = _kc1;
			k2 = _kc2;
			p1 = _kc3;
			p2 = _kc4;
			k3 = _kc5;
		}
		
		public Vector2 normalization(Vector2 p)
		{
			Vector2 rp = new Vector2(0,0);
			// Subtract pincipal point and divide by the focal length
			rp.x = (p.x-cc.x)/fc.x;
			rp.y = (p.y-cc.y)/fc.y;
			
			// Undo skew
			rp.x = (float)(rp.x - alpha*rp.y);
			
			
			// Compensate for lens distortion
//						if ((k1 != 0)||(k2 != 0)||(p1 != 0)||(p2 != 0)||(k3 != 0)) {
//							return undistortion(rp);
//						} else {
			return rp;
//			}
		}
		
		public Vector2 undistortion(Vector2 p)
		{
			Vector2 rp = p;
			double r_2,kRadial,dx,dy;
			for (int i = 0; i < 20; i++) {
				r_2 = Vector2.Dot(rp,rp);
				kRadial = 1 + k1 * r_2+k2*r_2*r_2+k3*r_2*r_2*r_2; // radial distortion
				dx = 2*p1*rp.x*rp.y+p2*(r_2+2*rp.x*rp.x);
				dy = p1*(r_2+2*rp.y*rp.y)+2*p2*rp.x*rp.y;
				rp.x = (float)((p.x-dx)/kRadial);
				rp.y = (float)((p.y-dy)/kRadial);
			}
			return rp;
		}
		
		
		
		
	}
	
	
}