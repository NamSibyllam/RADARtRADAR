Indicators/RADAR.cs
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class RADAR : Indicator
	{
		private Series<double> btrSeries;
		private EMA btrEMA;
		private SMA radarSMA;
		private SMA triggerSMA;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"RADAR - Body to Range Ratio with EMA and SMA smoothing";
				Name = "RADAR";
				Calculate = Calculate.OnBarClose;
				IsOverlay = false;
				DisplayInDataBox = true;
				DrawOnPricePanel = false;
				PaintPriceMarkers = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;

				EMAPeriod = 13;
				SMAPeriod = 3;
				TriggerPeriod = 3;

				RadarColor = Brushes.DodgerBlue;
				TriggerColor = Brushes.Yellow;

				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "RADAR");
				AddPlot(new Stroke(Brushes.Yellow, 2), PlotStyle.Line, "Trigger");
			}
			else if (State == State.Configure)
			{
				btrSeries = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
				btrEMA = EMA(btrSeries, EMAPeriod);
				radarSMA = SMA(btrEMA, SMAPeriod);
				triggerSMA = SMA(radarSMA, TriggerPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;

			// Calculate Body to Range ratio (directional)
			double body = Close[0] - Open[0]; // Positive for bullish, negative for bearish
			double range = High[0] - Low[0];

			// Handle zero range (avoid division by zero)
			if (range == 0)
				btrSeries[0] = 0;
			else
				btrSeries[0] = body / range; // Will be positive or negative based on bar direction

			// Need enough bars for EMA, SMA, and Trigger
			if (CurrentBar < EMAPeriod + SMAPeriod + TriggerPeriod - 2)
				return;

			// Set RADAR value (EMA of BTR, smoothed with SMA)
			Values[0][0] = radarSMA[0];

			// Set Trigger value (SMA of RADAR)
			Values[1][0] = triggerSMA[0];
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "EMA Period", Description = "Period for EMA of Body to Range ratio", Order = 1, GroupName = "Parameters")]
		public int EMAPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "SMA Period", Description = "Period for SMA smoothing of EMA", Order = 2, GroupName = "Parameters")]
		public int SMAPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Trigger Period", Description = "Period for Trigger SMA", Order = 3, GroupName = "Parameters")]
		public int TriggerPeriod { get; set; }

		[XmlIgnore]
		[Display(Name = "RADAR Color", Description = "Color of RADAR line", Order = 4, GroupName = "Visual")]
		public Brush RadarColor { get; set; }

		[XmlIgnore]
		[Display(Name = "Trigger Color", Description = "Color of Trigger line", Order = 5, GroupName = "Visual")]
		public Brush TriggerColor { get; set; }

		[Browsable(false)]
		public Series<double> RADARValue
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		public Series<double> TriggerValue
		{
			get { return Values[1]; }
		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

#endregion
