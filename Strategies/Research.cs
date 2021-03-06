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
	public class Research : Strategy
	{
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";

        GuerillaAcdMasterIndicator guerillaAcdMasterIndicator;
        GuerillaTickProfile guerillaTickProfile;
        GuerillaStickSimple greenBarStick;
        GuerillaStickSimple redBarStick;
        GuerillaStickSimple fiftyHammerStick;
        GuerillaStickSimple fiftyManStick;
        GuerillaStickSimple fiftyBarStick;
        GuerillaStickSimple bigBarStick;

        int tradeCount = 0;
        bool printTrades = false;
        double stopPrice = 0;
        int longTimePeriod = 15;
        int shortTimePeriod = 5;
        bool goLong = false;
        bool goShort = false;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "Research";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 2;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
                StartTradingHour = 7;
                StartTradingMinute = 45;
                StopTradingHour = 12;
                StopTradingMinute = 45;
                ProfitTicks = 5;
                HoldPeriods = 1;
            }
            else if (State == State.Configure)
            {
                SetProfitTarget(CalculationMode.Ticks, 50);
                SetStopLoss(CalculationMode.Ticks, 40);

                AddDataSeries(Data.BarsPeriodType.Minute, 5);
                //SetProfitTarget(CalculationMode.Ticks, 6);

                //guerillaAcdMasterIndicator = new GuerillaAcdMasterIndicator();
                //AddChartIndicator(guerillaAcdMasterIndicator);

                guerillaTickProfile = GuerillaTickProfile(BarsArray[0], Brushes.MediumVioletRed, Brushes.LightGray, 28, 200);
                AddChartIndicator(guerillaTickProfile);

                greenBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(greenBarStick);

                redBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(redBarStick);

                fiftyHammerStick = GuerillaStickSimple(BarsArray[0], Brushes.Chartreuse, Brushes.Firebrick, GuerillaChartPattern.FiftyHammer, 0, 0);
                AddChartIndicator(fiftyHammerStick);

                fiftyManStick = GuerillaStickSimple(BarsArray[0], Brushes.Chartreuse, Brushes.Firebrick, GuerillaChartPattern.FiftyMan, 0, 0);
                AddChartIndicator(fiftyManStick);

                fiftyBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.FiftyBar, 0, 0);
                AddChartIndicator(fiftyBarStick);

                bigBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.BigBar, 0, 0);
                AddChartIndicator(bigBarStick);
            }
        } 
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            DateTime debugDate = new DateTime(2018, 9, 17, 11, 10, 0);

            if (debugDate == Time[0])
            {
                int d = 0;
            }

            int heldPeriods = (Time[0].Minute % longTimePeriod) / shortTimePeriod;

            if (BarsInProgress == 0 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 0)
            {
                DateTime startTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StartTradingHour, this.StartTradingMinute, 0);
                DateTime stopTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StopTradingHour, this.StopTradingMinute, 0);

                if (Time[0] >= startTime && Time[0] <= stopTime && this.guerillaTickProfile.WithinThreshold[0] && Position.MarketPosition == MarketPosition.Flat)
                {
                    if (this.greenBarStick[0] > 0)
                    {
                        goLong = false;

                        if (this.fiftyHammerStick[0] > 0)
                        {
                            tradeCount += 1;
                            goLong = true;
                            if (printTrades) PrintValues(Account.Name, Time[0], "greenBarStick", "fiftyHammerStick", tradeCount);
                        }
                        //else if (this.bigBarStick[0] > 0)
                        //{
                        //    tradeCount += 1;
                        //    goLong = true;
                        //    if (printTrades) PrintValues(Account.Name, Time[0], "greenBarStick", "bigBarStick", tradeCount);
                        //}
                        //else if (this.redBarStick[1] > 0 && this.fiftyHammerStick[1] > 0)
                        //{
                        //    tradeCount += 1;
                        //    goLong = true;
                        //    if (printTrades) PrintValues(Account.Name, Time[0], "greenBarStick", "redBarStick[1]", "fiftyHammerStick[1]", tradeCount);
                        //}

                        if (goLong)
                        {
                            stopPrice = Low[0] - TickSize;
                            EnterLong(ENTER_LONG);
                        }
                    }
                    else if (this.redBarStick[0] > 0)
                    {
                        goShort = false;

                        if (this.fiftyManStick[0] > 0)
                        {
                            tradeCount += 1;
                            goShort = true;
                            if (printTrades) PrintValues(Account.Name, Time[0], "redBarStick", "fiftyManStick", tradeCount);
                        }
                        //else if (this.bigBarStick[0] > 0)
                        //{
                        //    tradeCount += 1;
                        //    goShort = true;
                        //    if (printTrades) PrintValues(Account.Name, Time[0], "redBarStick", "bigBarStick", tradeCount);
                        //}
                        //else if (this.greenBarStick[1] > 0 && this.fiftyManStick[1] > 0)
                        //{
                        //    tradeCount += 1;
                        //    goShort = true;
                        //    if (printTrades) PrintValues(Account.Name, Time[0], "redBarStick", "greenBarStick[1]", "fiftyManStick[1]", tradeCount);
                        //}

                        if (goShort)
                        {
                            stopPrice = High[0] + TickSize;
                            EnterShort(ENTER_SHORT);
                        }
                    }
                }
            }
            else if (BarsInProgress == 1 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 1)
            {
                DateTime startTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StartTradingHour, this.StartTradingMinute, 0);
                DateTime stopTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StopTradingHour, this.StopTradingMinute, 0);

                if (Time[0] >= startTime && Time[0] <= stopTime && this.guerillaTickProfile.WithinThreshold[0])
                {
                    bool isGreen = Close[0] > Open[0];
                    bool isRed = Close[0] < Open[0];

                    if (isGreen && this.redBarStick[0] > 0 && this.fiftyHammerStick[0] > 0)
                    {
                        stopPrice = Lows[0][0] - TickSize;
                        EnterLong(ENTER_LONG);
                    }
                    else if (isRed && this.greenBarStick[0] > 0 && this.fiftyManStick[0] > 0)
                    {
                        stopPrice = Highs[0][0] + TickSize;
                        EnterShort(ENTER_SHORT);
                    }
                }
            }
            else if (BarsInProgress == 1 && Position.MarketPosition != MarketPosition.Flat && heldPeriods >= this.HoldPeriods)
            {              
                if (Position.MarketPosition == MarketPosition.Long)
                {
                    ExitLong(EXIT, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short)
                {
                    ExitShort(EXIT, ENTER_SHORT);
                }
            }
        }
        #endregion

        #region OnExecutionUpdate
        protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity,
            Cbi.MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (execution.Order != null && execution.Order.OrderState == OrderState.Filled)
            {
                if (execution.Order.Name == ENTER_LONG)
                {
                    //ExitLongLimit(execution.Order.AverageFillPrice + (50 * TickSize), EXIT, ENTER_LONG);
                    //ExitLongStopMarket(execution.Order.AverageFillPrice - (40 * TickSize), EXIT, ENTER_LONG);
                }
                else if(execution.Order.Name == ENTER_SHORT)
                {
                    //ExitShortLimit(execution.Order.AverageFillPrice - (50 * TickSize), EXIT, ENTER_SHORT);
                    //ExitShortStopMarket(execution.Order.AverageFillPrice + (40 * TickSize), EXIT, ENTER_LONG);
                }
            }
        }
        #endregion

        #region PrintValues
        private void PrintValues(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="StartTradingHour", Order=1, GroupName="Parameters")]
		public int StartTradingHour
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="StartTradingMinute", Order=2, GroupName="Parameters")]
		public int StartTradingMinute
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="StopTradingHour", Order=3, GroupName="Parameters")]
		public int StopTradingHour
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="StopTradingMinute", Order=4, GroupName="Parameters")]
		public int StopTradingMinute
		{ get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProfitTicks", Order = 5, GroupName = "Parameters")]
        public int ProfitTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "HoldPeriods", Order = 6, GroupName = "Parameters")]
        public int HoldPeriods
        { get; set; }
        #endregion

    }
}
