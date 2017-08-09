/*
 * Created by SharpDevelop.
 * User: Voodoo
 * Date: 02.08.2017
 * Time: 14:24
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.Ports;
using System.Windows.Threading;

namespace LMS303
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	/// 
	
	public class User
        {
                public string ax { get; set; }

                public string ay { get; set; }

                public string az { get; set; }
        }
	
	public partial class Window1 : Window
	{
		
		private SerialPort sPort;		
		DispatcherTimer dispatcherTimer;
		
		public Window1()
		{
			InitializeComponent();
			
			
			//============ Port =====================
			sPort = new SerialPort();
			sPort.BaudRate = 115200;
			sPort.PortName = "COM6";
			sPort.ReadTimeout = 200;
			sPort.WriteTimeout = 10;
			sPort.DataReceived += READ;
			
			//============ DispatcherTimer =====================
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);//new TimeSpan(0, 0, 1);
			//dispatcherTimer.Start();
		}
		//======================================================================
		double  Mag_minx = 0;
		double  Mag_miny = 0;// = -389;
		double  Mag_minz = 0;// = -532;
		double  Mag_maxx = 0;// =  523;
		double  Mag_maxy = 0;// =  657;
		double  Mag_maxz = 0;// =  408;
		
		double  a_minx = 0;
		double  a_miny = 0;// = -389;
		double  a_minz = 0;// = -532;
		double  a_maxx = 0;// =  523;
		double  a_maxy = 0;// =  657;
		double  a_maxz = 0;// =  408;
		
		const double toDeg = 180/Math.PI;
		
		string vvvv;
		//======================================================================
		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			
//			iecompass(values[0], values[1], values[2],
//			          values[3], values[4], values[5]);
			
			if (values[0] < Mag_minx)  Mag_minx = values[0];
			if (values[1] < Mag_miny)  Mag_miny = values[1];
			if (values[2] < Mag_minz)  Mag_minz = values[2];
			if (values[0] > Mag_maxx)  Mag_maxx = values[0];
			if (values[1] > Mag_maxy)  Mag_maxy = values[1];
			if (values[2] > Mag_maxz)  Mag_maxz = values[2];
			
			if (values[3] < a_minx)  a_minx = values[3];
			if (values[4] < a_miny)  a_miny = values[4];
			if (values[5] < a_minz)  a_minz = values[5];
			if (values[3] > a_maxx)  a_maxx = values[3];
			if (values[4] > a_maxy)  a_maxy = values[4];
			if (values[5] > a_maxz)  a_maxz = values[5];


//			double Accx = values[3];
//			double Accy = values[4];
//			double Accz = values[5];

			double Magx = values[0];
			double Magy = values[1];
			double Magz = values[2];
			
			double ax = values[3];
			double ay = values[4];
			double az = values[5];
			
			
			// use calibration values to shift and scale magnetometer measurements
			ax = (ax-a_minx)/(a_maxx-a_minx)*2-1;
			ay = (ay-a_miny)/(a_maxy-a_miny)*2-1;
			az = (az-a_minz)/(a_maxz-a_minz)*2-1;

			// use calibration values to shift and scale magnetometer measurements
			Magx = (Magx-Mag_minx)/(Mag_maxx-Mag_minx)*2-1;
			Magy = (Magy-Mag_miny)/(Mag_maxy-Mag_miny)*2-1;
			Magz = (Magz-Mag_minz)/(Mag_maxz-Mag_minz)*2-1;

			// Normalize acceleration measurements so they range from 0 to 1
			double accxnorm = ax/Math.Sqrt(ax*ax+ay*ay+az*az);
			double accynorm = ay/Math.Sqrt(ax*ax+ay*ay+az*az);

			// calculate pitch and roll
			double Pitch = Math.Asin(-accxnorm);
			double Roll =  Math.Asin(accynorm/Math.Cos(Pitch));

			double RollAng  = toDeg * Roll;
			double PitchAng = toDeg * Pitch;

			double pRoll = RollAng ;
			double pPitch = PitchAng;

			// tilt compensated magnetic sensor measurements
			double magxcomp = Magx*Math.Cos(Pitch)+Magz*Math.Sin(Pitch);
			double magycomp = Magx*Math.Sin(Roll)*Math.Sin(Pitch)+Magy*Math.Cos(Roll)-Magz*Math.Sin(Roll)*Math.Cos(Pitch);

			// arctangent of y/x converted to degrees
			double Heading = (toDeg * Math.Atan2(magycomp,magxcomp));

			//        double Heading = Math.toDegrees(Math.atan2(Magy,Magx)/PI);
			if (Heading < 0)
				Heading +=360;
			
			
			rotate.Angle = -Heading;
			
			vvvv +=  values[1].ToString() +
				"\t" + values[2].ToString() +
				"\t" + values[3].ToString() +
				"\r\n";
			tbAccVals.Text = vvvv;
            		
				
				tbValues.Text =
				       values[0].ToString() +
				"\t" + values[1].ToString() +
				"\t" + values[2].ToString() +
					"\r\n" +
				       Mag_minx.ToString() +
				"\t" + Mag_miny.ToString() +
				"\t" + Mag_minz.ToString() +
				"\t" + Mag_maxx.ToString() +
				"\t" + Mag_maxy.ToString() +
				"\t" + Mag_maxz.ToString() +
				"\r\n" +
				       values[3].ToString() +
				"\t" + values[4].ToString() +
				"\t" + values[5].ToString() +
				"\r\n" +
				       a_minx.ToString() +
				"\t" + a_miny.ToString() +
				"\t" + a_minz.ToString() +
				"\t" + a_maxx.ToString() +
				"\t" + a_maxy.ToString() +
				"\t" + a_maxz.ToString() +
				"\r\n" +
				"\r\n" + String.Format("RollAng: {0:N1}", RollAng) +
				"\r\n" + String.Format("PitchAng: {0:N1}", PitchAng) +
				"\r\n" + String.Format("Heading: {0:N1}", Heading);
			
		}
		//======================================================================
		Int16 [] values = new Int16[6];
		//======================================================================
		private void READ(object sender, SerialDataReceivedEventArgs e)
		{
			int bytesToRead = sPort.BytesToRead;
			byte[] tmpBuffer = new byte[bytesToRead];
			
			if(bytesToRead >= 18)
			{
				sPort.Read(tmpBuffer, 0, bytesToRead);
				
				if(tmpBuffer[0] == (byte)0xc0)
				{
					for(int i = 0; i < 6; i++)
					{
						Int16 a = (Int16)(tmpBuffer[i*2 + 2]);
						Int16 b = (Int16)(tmpBuffer[i*2 + 3] << 8);
						
						values[i] = (Int16)(a + b);
					}
				}
			}
		}
		//======================================================================
		void btnGo_Click(object sender, System.EventArgs e)
		{
			if(sPort.IsOpen)
			{
				sPort.Close();
				btnGo.Content = "Пуск";
				dispatcherTimer.Stop();
			}
			else
			{
				try
				{
					//sPort.PortName = comboBox_Port.Text;
					vvvv = "";
					dispatcherTimer.Start();
					sPort.Open();
					btnGo.Content = "Стоп";
					
					Mag_minx = 0;
					Mag_miny = 0;// = -389;
					Mag_minz = 0;// = -532;
					Mag_maxx = 0;// =  523;
					Mag_maxy = 0;// =  657;
					Mag_maxz = 0;// =  408;
					
					a_minx = 0;
					a_miny = 0;// = -389;
					a_minz = 0;// = -532;
					a_maxx = 0;// =  523;
					a_maxy = 0;// =  657;
					a_maxz = 0;// =  408;
				}
				catch
				{
					
				}
			}
		}
		
		const UInt16 MINDELTATRIG = 1;
		
		/* roll pitch and yaw angles computed by iecompass */
		static Int16 iPhi, iThe, iPsi;
		/* magnetic field readings corrected for hard iron effects and PCB orientation */
		static Int16 iBfx, iBfy, iBfz;
		/* hard iron estimate */
		static Int16 iVx, iVy, iVz;
		/* tilt-compensated e-Compass code */
		public static void iecompass(Int16 iBpx, Int16 iBpy, Int16 iBpz,
		                             Int16 iGpx, Int16 iGpy, Int16 iGpz)
		{
			/* stack variables */
			/* iBpx, iBpy, iBpz: the three components of the magnetometer sensor */
			/* iGpx, iGpy, iGpz: the three components of the accelerometer sensor */
			/* local variables */
			Int16 iSin, iCos; /* sine and cosine */
			/* subtract the hard iron offset */
			iBpx -= iVx; /* see Eq 16 */
			iBpy -= iVy; /* see Eq 16 */
			iBpz -= iVz; /* see Eq 16 */
			/* calculate current roll angle Phi */
			iPhi = iHundredAtan2Deg(iGpy, iGpz);/* Eq 13 */
			/* calculate sin and cosine of roll angle Phi */
			iSin = iTrig(iGpy, iGpz); /* Eq 13: sin = opposite / hypotenuse */
			iCos = iTrig(iGpz, iGpy); /* Eq 13: cos = adjacent / hypotenuse */
			/* de-rotate by roll angle Phi */
			iBfy = (Int16)((iBpy * iCos - iBpz * iSin) >> 15);/* Eq 19 y component */
			iBpz = (Int16)((iBpy * iSin + iBpz * iCos) >> 15);/* Bpy*sin(Phi)+Bpz*cos(Phi)*/
			iGpz = (Int16)((iGpy * iSin + iGpz * iCos) >> 15);/* Eq 15 denominator */
			/* calculate current pitch angle Theta */
			iThe = iHundredAtan2Deg((Int16)(-iGpx), iGpz);/* Eq 15 */
			/* restrict pitch angle to range -90 to 90 degrees */
			if (iThe > 9000) iThe = (Int16) (18000 - iThe);
			if (iThe < -9000) iThe = (Int16) (-18000 - iThe);
			/* calculate sin and cosine of pitch angle Theta */
			iSin = (Int16)(-iTrig(iGpx, iGpz)); /* Eq 15: sin = opposite / hypotenuse */
			iCos = iTrig(iGpz, iGpx); /* Eq 15: cos = adjacent / hypotenuse */
			/* correct cosine if pitch not in range -90 to 90 degrees */
			if (iCos < 0) iCos = (Int16)(-iCos);
			/* de-rotate by pitch angle Theta */
			iBfx = (Int16)((iBpx * iCos + iBpz * iSin) >> 15); /* Eq 19: x component */
			iBfz = (Int16)((-iBpx * iSin + iBpz * iCos) >> 15);/* Eq 19: z component */
			/* calculate current yaw = e-compass angle Psi */
			iPsi = iHundredAtan2Deg((Int16)(-iBfy), iBfx); /* Eq 22 */
		}

		/* function to calculate ir = ix / sqrt(ix*ix+iy*iy) using binary division */
		static Int16 iTrig(Int16 ix, Int16 iy)
		{
			UInt32 itmp; /* scratch */
			UInt32 ixsq; /* ix * ix */
			Int16 isignx; /* storage for sign of x. algorithm assumes x >= 0 then corrects later */
			UInt32 ihypsq; /* (ix * ix) + (iy * iy) */
			Int16 ir; /* result = ix / sqrt(ix*ix+iy*iy) range -1, 1 returned as signed Int16 */
			Int16 idelta; /* delta on candidate result dividing each stage by factor of 2 */
			/* stack variables */
			/* ix, iy: signed 16 bit integers representing sensor reading in range -32768 to 32767 */
			/* function returns signed Int16 as signed fraction (ie +32767=0.99997, -32768=-1.0000) */
			/* algorithm solves for ir*ir*(ix*ix+iy*iy)=ix*ix */
			/* correct for pathological case: ix==iy==0 */
			if ((ix == 0) && (iy == 0)) ix = iy = 1;
			/* check for -32768 which is not handled correctly */
			if (ix == -32768) ix = -32767;
			if (iy == -32768) iy = -32767;
			/* store the sign for later use. algorithm assumes x is positive for convenience */
			isignx = 1;
			if (ix < 0)
			{
				ix = (Int16)(-ix);
				isignx = -1;
			}
			/* for convenience in the boosting set iy to be positive as well as ix */
			iy = (Int16)Math.Abs(iy);
			/* to reduce quantization effects, boost ix and iy but keep below maximum signed 16 bit */
			while ((ix < 16384) && (iy < 16384))
			{
				ix = (Int16)(ix + ix);
				iy = (Int16)(iy + iy);
			}
			/* calculate ix*ix and the hypotenuse squared */
			ixsq = (UInt32)(ix * ix); /* ixsq=ix*ix: 0 to 32767^2 = 1073676289 */
			ihypsq = (UInt32)(ixsq + iy * iy); /* ihypsq=(ix*ix+iy*iy) 0 to 2*32767*32767=2147352578 */
			/* set result r to zero and binary search step to 16384 = 0.5 */
			ir = 0;
			idelta = 16384; /* set as 2^14 = 0.5 */
			/* loop over binary sub-division algorithm */
			do
			{
				/* generate new candidate solution for ir and test if we are too high or too low */
				/* itmp=(ir+delta)^2, range 0 to 32767*32767 = 2^30 = 1073676289 */
				itmp = (UInt32)((ir + idelta) * (ir + idelta));
				/* itmp=(ir+delta)^2*(ix*ix+iy*iy), range 0 to 2^31 = 2147221516 */
				itmp = (itmp >> 15) * (ihypsq >> 15);

				if (itmp <= ixsq) ir += idelta;
				idelta = (Int16)(idelta >> 1); /* divide by 2 using right shift one bit */
			} while (idelta >= MINDELTATRIG); /* last loop is performed for idelta=MINDELTATRIG */
			/* correct the sign before returning */
			return (Int16)(ir * isignx);
		}

		/* calculates 100*atan2(iy/ix)=100*atan2(iy,ix) in deg for ix, iy in range -32768 to 32767 */
		static Int16 iHundredAtan2Deg(Int16 iy, Int16 ix)
		{
			Int16 iResult; /* angle in degrees times 100 */
			/* check for -32768 which is not handled correctly */
			if (ix == -32768) ix = -32767;
			if (iy == -32768) iy = -32767;
			/* check for quadrants */
			if ((ix >= 0) && (iy >= 0)) /* range 0 to 90 degrees */
			iResult = iHundredAtanDeg(iy, ix);
			else if ((ix <= 0) && (iy >= 0)) /* range 90 to 180 degrees */
			iResult = (Int16)(18000 - (Int16)iHundredAtanDeg(iy, (Int16)(-ix)));
			else if ((ix <= 0) && (iy <= 0)) /* range -180 to -90 degrees */
			iResult = (Int16)((Int16)(-18000) + iHundredAtanDeg((Int16)(-iy), (Int16)(-ix)));
			else /* ix >=0 and iy <= 0 giving range -90 to 0 degrees */
				iResult = (Int16)(-iHundredAtanDeg((Int16)(-iy), ix));
			return (iResult);
		}

		/* fifth order of polynomial approximation giving 0.05 deg max error */
		const Int16 K1 = 5701;
		const Int16 K2 = -1645;
		const Int16 K3 = 446;
		/* calculates 100*atan(iy/ix) range 0 to 9000 for all ix, iy positive in range 0 to 32767 */
		static Int16 iHundredAtanDeg(Int16 iy, Int16 ix)
		{
			Int32 iAngle; /* angle in degrees times 100 */
			Int16 iRatio; /* ratio of iy / ix or vice versa */
			Int32 iTmp; /* temporary variable */
			/* check for pathological cases */
			if ((ix == 0) && (iy == 0)) return (0);
			if ((ix == 0) && (iy != 0)) return (9000);
			/* check for non-pathological cases */
			if (iy <= ix)
				iRatio = iDivide(iy, ix); /* return a fraction in range 0. to 32767 = 0. to 1. */
			else
				iRatio = iDivide(ix, iy); /* return a fraction in range 0. to 32767 = 0. to 1. */
			/* first, third and fifth order polynomial approximation */
			iAngle = (Int32) K1 * (Int32) iRatio;
			iTmp = ((Int32) iRatio >> 5) * ((Int32) iRatio >> 5) * ((Int32) iRatio >> 5);
			iAngle += (iTmp >> 15) * (Int32) K2;
			iTmp = (iTmp >> 20) * ((Int32) iRatio >> 5) * ((Int32) iRatio >> 5);
			iAngle += (iTmp >> 15) * (Int32) K3;
			iAngle = iAngle >> 15;
			/* check if above 45 degrees */
			if (iy > ix) iAngle = (Int16)(9000 - iAngle);
			/* for tidiness, limit result to range 0 to 9000 equals 0.0 to 90.0 degrees */
			if (iAngle < 0) iAngle = 0;
			if (iAngle > 9000) iAngle = 9000;
			return ((Int16) iAngle);
		}

		const UInt16 MINDELTADIV = 1; /* final step size for iDivide */
		/* function to calculate ir = iy / ix with iy <= ix, and ix, iy both > 0 */
		static Int16 iDivide(Int16 iy, Int16 ix)
		{
			Int16 itmp; /* scratch */
			Int16 ir; /* result = iy / ix range 0., 1. returned in range 0 to 32767 */
			Int16 idelta; /* delta on candidate result dividing each stage by factor of 2 */
			/* set result r to zero and binary search step to 16384 = 0.5 */
			ir = 0;
			idelta = 16384; /* set as 2^14 = 0.5 */
			/* to reduce quantization effects, boost ix and iy to the maximum signed 16 bit value */
			while ((ix < 16384) && (iy < 16384))
			{
				ix = (Int16)(ix + ix);
				iy = (Int16)(iy + iy);
			}
			/* loop over binary sub-division algorithm solving for ir*ix = iy */
			do
			{
				/* generate new candidate solution for ir and test if we are too high or too low */
				itmp = (Int16)(ir + idelta); /* itmp=ir+delta, the candidate solution */
				itmp = (Int16)((itmp * ix) >> 15);
				if (itmp <= iy) ir += idelta;
				idelta = (Int16)(idelta >> 1); /* divide by 2 using right shift one bit */
			} while (idelta >= MINDELTADIV); /* last loop is performed for idelta=MINDELTADIV */
			return (ir);
		}

		
	}
}