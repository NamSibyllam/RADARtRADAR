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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	public class RADARtRADAR2 : Strategy
	{
		private RADAR radar;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"RADAR crossover strategy - Hybrid manual/auto trading";
				Name										= "RADARtRADAR2";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;

				// RADAR parameters
				EMAPeriod		= 3;
				SMAPeriod		= 3;
				TriggerPeriod	= 3;
			}
			else if (State == State.DataLoaded)
			{
				radar = RADAR(EMAPeriod, SMAPeriod, TriggerPeriod);
				AddChartIndicator(radar);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;

			// Long when RADAR crosses above Trigger
			if (CrossAbove(radar.RADARValue, radar.TriggerValue, 1))
			{
				// Flatten ALL positions (strategy + manual trades)
				Account.Flatten();

				// Enter 1 contract Long
				EnterLong(1, "");
			}

			// Short when RADAR crosses below Trigger
			if (CrossBelow(radar.RADARValue, radar.TriggerValue, 1))
			{
				// Flatten ALL positions (strategy + manual trades)
				Account.Flatten();

				// Enter 1 contract Short
				EnterShort(1, "");
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "EMA Period", Description = "Period for EMA of Body to Range ratio", Order = 1, GroupName = "RADAR Parameters")]
		public int EMAPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "SMA Period", Description = "Period for SMA smoothing of EMA", Order = 2, GroupName = "RADAR Parameters")]
		public int SMAPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Trigger Period", Description = "Period for Trigger SMA", Order = 3, GroupName = "RADAR Parameters")]
		public int TriggerPeriod { get; set; }
		#endregion
	}
}
