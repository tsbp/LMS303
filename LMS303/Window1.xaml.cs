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
			sPort.PortName = "COM14";
			sPort.ReadTimeout = 200;
			sPort.WriteTimeout = 10;
			sPort.DataReceived += READ;
			
			//============ DispatcherTimer =====================
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(10);//new TimeSpan(0, 0, 1);
			//dispatcherTimer.Start();
		}
		//======================================================================
		double  Mag_minx = 0;
		double  Mag_miny = 0;
		double  Mag_minz = 0;
		double  Mag_maxx = 0;
		double  Mag_maxy = 0;
		double  Mag_maxz = 0;
		
		const double toDeg = 180/Math.PI;
		
		const int ARR_LNG = 30;
		
		double [] headings = new double[ARR_LNG];
		
		//string vvvv;
		//======================================================================
		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			
			double Magx = (double)values[0];// * ((double)1100 / 1000);
			double Magy = (double)values[1];// * ((double)1100 / 1000);
			double Magz = (double)values[2];// * ((double)980  / 1000);
			
//			if (Magx < Mag_minx)  Mag_minx = Magx;
//			if (Magy < Mag_miny)  Mag_miny = Magy;
//			if (Magz < Mag_minz)  Mag_minz = Magz;
//			if (Magx > Mag_maxx)  Mag_maxx = Magx;
//			if (Magy > Mag_maxy)  Mag_maxy = Magy;
//			if (Magz > Mag_maxz)  Mag_maxz = Magz;
//			
//
//			// use calibration values to shift and scale magnetometer measurements
//			Magx = (Magx-Mag_minx)/(Mag_maxx-Mag_minx)*2-1;
//			Magy = (Magy-Mag_miny)/(Mag_maxy-Mag_miny)*2-1;
//			Magz = (Magz-Mag_minz)/(Mag_maxz-Mag_minz)*2-1;

			double RollAng  = toDeg * values[3]/10000;
			double PitchAng = toDeg * values[4]/10000;

			double Roll  = (RollAng ) * (Math.PI/180);
			double Pitch = (PitchAng) * (Math.PI/180);

			// tilt compensated magnetic sensor measurements
			double magxcomp = Magx*Math.Cos(Pitch)+Magz*Math.Sin(Pitch);
			double magycomp = Magx*Math.Sin(Roll)*Math.Sin(Pitch)+Magy*Math.Cos(Roll)-Magz*Math.Sin(Roll)*Math.Cos(Pitch);

			// arctangent of y/x converted to degrees
			double Heading;// = (Math.Atan2(magycomp,magxcomp));
            
				
			//==== get avg heading ==============
			for(int k = ARR_LNG-1; k > 0; k--)
				headings[k] = headings[k-1];
			
			headings[0] = (Math.Atan2(-magycomp,magxcomp));//Heading;
			
			double sum = 0;
			for(int i = 0; i < ARR_LNG; i++) sum += headings[i];
			Heading = (sum / ARR_LNG);
			//================================
			
			double HeadingDeg = toDeg * Heading;
			
			if (HeadingDeg < 0)
				HeadingDeg += 360;
			
			
			rotate.Angle = -HeadingDeg;
			
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
//				       a_minx.ToString() +
//				"\t" + a_miny.ToString() +
//				"\t" + a_minz.ToString() +
//				"\t" + a_maxx.ToString() +
//				"\t" + a_maxy.ToString() +
//				"\t" + a_maxz.ToString() +
//				"\r\n" +
				"\r\n" + String.Format("RollAng: {0:N1}", RollAng) +
				"\r\n" + String.Format("PitchAng: {0:N1}", PitchAng) +
				"\r\n" + String.Format("Heading, rads: {0:N5}", Heading) +
				"\r\n" + String.Format("Heading, degs: {0:N1}", HeadingDeg);
			
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
					dispatcherTimer.Start();
					sPort.Open();
					btnGo.Content = "Стоп";
					
					Mag_minx = 0;
					Mag_miny = 0;
					Mag_minz = 0;
					Mag_maxx = 0;
					Mag_maxy = 0;
					Mag_maxz = 0;
				}
				catch
				{
					
				}
			}
		}
	}
}