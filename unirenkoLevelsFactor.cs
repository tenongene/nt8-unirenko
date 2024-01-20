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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class UniRenkoChangeTest : Indicator
	{
		
		bool	uniRenkoIsOnTheChart		=	false;
		double	reversalLevel				=	0;
		
		#region Properties
		
		[Range(1,Int32.MaxValue)]
		[Display(Name="Levels Distance", Order = 1, GroupName="Parameters")]
        public int LevelsDistance
        {
            get;set;
        }
		
		[Display(Name="Stop Line Style", Order = 2, GroupName="Parameters")]
        public Stroke StopLineStroke
        {
            get;set;
        }
		
		[Display(Name="Entry Line Style", Order = 4, GroupName="Parameters")]
        public Stroke EntryLineStroke
        {
            get;set;
        }
		
		[Range(1,Int32.MaxValue)]
		[Display(Name="Level Width", Order = 5, GroupName="Parameters")]
        public int LevelWidth
        {
            get;set;
        }
		
		[Display(Name="Text Font", Order = 6, GroupName="Parameters")]
        public SimpleFont TextFont
        {
            get;set;
        }
		
		[XmlIgnore]
		[Display(Name = "Text Color", GroupName = "Parameters", Order = 6)]
		public Brush TextBrush
		{ 	
			get;set; 
		}
		[Browsable(false)]
		public string TextBrushSerialize
		{
			get { return NinjaTrader.Gui.Serialize.BrushToString(TextBrush); }
			set { TextBrush = NinjaTrader.Gui.Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "UniRenkoChangeTest";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;
				
				LevelsDistance								= 20;
				StopLineStroke								= new Stroke(Brushes.Pink, 2);
				EntryLineStroke								= new Stroke(Brushes.DodgerBlue, 2);
				TextFont									= new SimpleFont("Calibri", 14);
				TextBrush									= Brushes.Black;
				
				LevelWidth									= 300;
				
				AddPlot(StopLineStroke, 	PlotStyle.Line,	"Stop Line");
				AddPlot(EntryLineStroke, 	PlotStyle.Line, "Entry Level 1");
				AddPlot(EntryLineStroke, 	PlotStyle.Line, "Entry Level 2");
				AddPlot(EntryLineStroke, 	PlotStyle.Line, "Entry Level 3");
			}
			else if (State == State.Configure)
			{
				
			}
			else if (State == State.DataLoaded)
			{
				Plots[0].Brush	=	StopLineStroke.Brush;
				Plots[1].Brush	=	EntryLineStroke.Brush;
				Plots[2].Brush	=	EntryLineStroke.Brush;
				Plots[3].Brush	=	EntryLineStroke.Brush;
			}
		}

		protected override void OnBarUpdate()
		{
			int idx	=	State == State.Historical || Calculate == Calculate.OnBarClose ? 0 : 1;
			
			if(CurrentBars[0] < 1)
				return;
			
			uniRenkoIsOnTheChart	=	BarsArray[0].BarsType.Name.Contains("UniRenko");
			
			bool	uptrend	=	Closes[0].GetValueAt(CurrentBars[0] - 1) > Opens[0].GetValueAt(CurrentBars[0] - 1);
			double	stopLvl	=	uptrend ? Opens[0].GetValueAt(CurrentBars[0]) - (BarsArray[0].BarsPeriod.Value2 + 2) * TickSize : Opens[0].GetValueAt(CurrentBars[0]) + (BarsArray[0].BarsPeriod.Value2 + 2) * TickSize;
			double	level3	=	uptrend ? stopLvl + LevelsDistance * TickSize : stopLvl - LevelsDistance * TickSize;
			double	level2	=	uptrend ? stopLvl + LevelsDistance * TickSize * 1.5 : stopLvl - LevelsDistance * TickSize * 1.5;
			double	level1	=	uptrend ? stopLvl + LevelsDistance * TickSize * 2.5 : stopLvl - LevelsDistance * TickSize * 2.5;
			
			Values[0][0]	=	stopLvl;
			Values[1][0]	=	level3;
			Values[2][0]	=	level2;
			Values[3][0]	=	level1;
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			StopLineStroke.RenderTarget					=	RenderTarget;
			EntryLineStroke.RenderTarget				=	RenderTarget;
			
			SharpDX.DirectWrite.TextFormat	tf			=	TextFont.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextFormat	errorTF		=	new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory,"Calibri", 25);
			SharpDX.Direct2D1.Brush			tBrush		=	chartControl.Properties.ChartText.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush			txtBrush	=	TextBrush.ToDxBrush(RenderTarget);
			
			SharpDX.Direct2D1.AntialiasMode	oldMode		=	RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode					=	SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			if(uniRenkoIsOnTheChart)
			{
				
				bool	uptrend	=	Closes[0].GetValueAt(CurrentBars[0] - 1) > Opens[0].GetValueAt(CurrentBars[0] - 1);
				double	stopLvl	=	uptrend ? Opens[0].GetValueAt(CurrentBars[0]) - (BarsArray[0].BarsPeriod.Value2 + 2) * TickSize : Opens[0].GetValueAt(CurrentBars[0]) + (BarsArray[0].BarsPeriod.Value2 + 2) * TickSize;
				double	level3	=	uptrend ? stopLvl + LevelsDistance * TickSize : stopLvl - LevelsDistance * TickSize;
				double	level2	=	uptrend ? stopLvl + LevelsDistance * TickSize * 1.5 : stopLvl - LevelsDistance * TickSize * 1.5;
				double	level1	=	uptrend ? stopLvl + LevelsDistance * TickSize * 2.5 : stopLvl - LevelsDistance * TickSize * 2.5;
				
				float	stopLabelWidth			=	0;
				RenderTextLabel("Stop Level | "+Instrument.MasterInstrument.FormatPrice(stopLvl), out stopLabelWidth, chartControl, chartScale, ChartPanel, tf, 10f, stopLvl, StopLineStroke, txtBrush);
				SharpDX.Vector2 StopEndVec		=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - LevelWidth, chartScale.GetYByValue(stopLvl));
				SharpDX.Vector2 StopStartVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - stopLabelWidth, chartScale.GetYByValue(stopLvl));
				RenderTarget.DrawLine(StopStartVec, StopEndVec, StopLineStroke.BrushDX, StopLineStroke.Width, StopLineStroke.StrokeStyle);
				
				float	level1LabelWidth		=	0;
				RenderTextLabel("Entry Level 1 | "+Instrument.MasterInstrument.FormatPrice(level1), out level1LabelWidth, chartControl, chartScale, ChartPanel, tf, 10f, level1, EntryLineStroke, txtBrush);
				SharpDX.Vector2 Level1EndVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - LevelWidth, chartScale.GetYByValue(level1));
				SharpDX.Vector2 Level1StartVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - level1LabelWidth, chartScale.GetYByValue(level1));
				RenderTarget.DrawLine(Level1StartVec, Level1EndVec, EntryLineStroke.BrushDX, EntryLineStroke.Width, EntryLineStroke.StrokeStyle);
				
				float	level2LabelWidth		=	0;
				RenderTextLabel("Entry Level 2 | "+Instrument.MasterInstrument.FormatPrice(level2), out level2LabelWidth, chartControl, chartScale, ChartPanel, tf, 10f, level2, EntryLineStroke, txtBrush);
				SharpDX.Vector2 Level2EndVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - LevelWidth, chartScale.GetYByValue(level2));
				SharpDX.Vector2 Level2StartVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - level2LabelWidth, chartScale.GetYByValue(level2));
				RenderTarget.DrawLine(Level2StartVec, Level2EndVec, EntryLineStroke.BrushDX, EntryLineStroke.Width, EntryLineStroke.StrokeStyle);
				
				float	level3LabelWidth		=	0;
				RenderTextLabel("Entry Level 3 | "+Instrument.MasterInstrument.FormatPrice(level3), out level3LabelWidth, chartControl, chartScale, ChartPanel, tf, 10f, level3, EntryLineStroke, txtBrush);
				SharpDX.Vector2 Level3EndVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - LevelWidth, chartScale.GetYByValue(level3));
				SharpDX.Vector2 Level3StartVec	=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - level3LabelWidth, chartScale.GetYByValue(level3));
				RenderTarget.DrawLine(Level3StartVec, Level3EndVec, EntryLineStroke.BrushDX, EntryLineStroke.Width, EntryLineStroke.StrokeStyle);
				
			}
			else
			{
				string							errorNotification	=	"Current Bar Type is not UniRenko!";
				SharpDX.DirectWrite.TextLayout	errorTL				=	new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, errorNotification, errorTF, ChartPanel.W, ChartPanel.H);
				SharpDX.Vector2					errorTLVector		=	new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - errorTL.Metrics.Width - 10f, ChartPanel.Y + ChartPanel.H - errorTL.Metrics.Height - 10f);
				RenderTarget.DrawTextLayout(errorTLVector, errorTL, tBrush);
				
			}
			
			RenderTarget.AntialiasMode	=	oldMode;
			
			tf.Dispose();
			errorTF.Dispose();
			tBrush.Dispose();
			txtBrush.Dispose();
		}
		
		
		
		private void RenderTextLabel(string Text, out float LabelWidth, ChartControl chartControl, ChartScale chartScale, 
									ChartPanel chartPanel, SharpDX.DirectWrite.TextFormat tf, float TrSize, double PriceLevel, Stroke stroke, SharpDX.Direct2D1.Brush textBrush)
		{
			SharpDX.DirectWrite.TextLayout	tl		=	new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, Text, tf, chartPanel.W, chartPanel.H);
			LabelWidth								=	tl.Metrics.Width + TrSize + 10f;
			float	LabelHeight						=	tl.Metrics.Height + 4f;
			SharpDX.Vector2					tlVec	=	new SharpDX.Vector2(chartPanel.X + chartPanel.W - LabelWidth + TrSize + 5f, chartScale.GetYByValue(PriceLevel) - tl.Metrics.Height / 2f);
			
			SharpDX.Direct2D1.PathGeometry	figure	=	LabelGeometry(LabelWidth, LabelHeight, TrSize, PriceLevel, chartPanel, chartScale);
			
			RenderTarget.FillGeometry(figure, stroke.BrushDX);
			RenderTarget.DrawTextLayout(tlVec, tl, textBrush);
		}
		
		private SharpDX.Direct2D1.PathGeometry LabelGeometry(float LabelWidth, float LabelHeight, float TrSize, double PriceLevel, ChartPanel chartPanel, ChartScale chartScale)
		{
			SharpDX.Direct2D1.PathGeometry	geo		=	new SharpDX.Direct2D1.PathGeometry(NinjaTrader.Core.Globals.D2DFactory);
			SharpDX.Direct2D1.GeometrySink	path 	= 	geo.Open();
			
			path.BeginFigure(new SharpDX.Vector2(chartPanel.X + chartPanel.W - LabelWidth, chartScale.GetYByValue(PriceLevel)),SharpDX.Direct2D1.FigureBegin.Filled);
			path.AddLine(new SharpDX.Vector2(chartPanel.X + chartPanel.W - LabelWidth + TrSize, chartScale.GetYByValue(PriceLevel) - LabelHeight / 2f));
			path.AddLine(new SharpDX.Vector2(chartPanel.X + chartPanel.W, chartScale.GetYByValue(PriceLevel) - LabelHeight / 2f));
			path.AddLine(new SharpDX.Vector2(chartPanel.X + chartPanel.W, chartScale.GetYByValue(PriceLevel) + LabelHeight / 2f));
			path.AddLine(new SharpDX.Vector2(chartPanel.X + chartPanel.W - LabelWidth + TrSize, chartScale.GetYByValue(PriceLevel) + LabelHeight / 2f));
			path.AddLine(new SharpDX.Vector2(chartPanel.X + chartPanel.W - LabelWidth, chartScale.GetYByValue(PriceLevel)));
			path.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			path.Close();
			
			return geo;
		}
	}
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UniRenkoChangeTest[] cacheUniRenkoChangeTest;
		public UniRenkoChangeTest UniRenkoChangeTest()
		{
			return UniRenkoChangeTest(Input);
		}

		public UniRenkoChangeTest UniRenkoChangeTest(ISeries<double> input)
		{
			if (cacheUniRenkoChangeTest != null)
				for (int idx = 0; idx < cacheUniRenkoChangeTest.Length; idx++)
					if (cacheUniRenkoChangeTest[idx] != null &&  cacheUniRenkoChangeTest[idx].EqualsInput(input))
						return cacheUniRenkoChangeTest[idx];
			return CacheIndicator<UniRenkoChangeTest>(new UniRenkoChangeTest(), input, ref cacheUniRenkoChangeTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UniRenkoChangeTest UniRenkoChangeTest()
		{
			return indicator.UniRenkoChangeTest(Input);
		}

		public Indicators.UniRenkoChangeTest UniRenkoChangeTest(ISeries<double> input )
		{
			return indicator.UniRenkoChangeTest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UniRenkoChangeTest UniRenkoChangeTest()
		{
			return indicator.UniRenkoChangeTest(Input);
		}

		public Indicators.UniRenkoChangeTest UniRenkoChangeTest(ISeries<double> input )
		{
			return indicator.UniRenkoChangeTest(input);
		}
	}
}

#endregion
