using System.Drawing;

namespace NAudioWrapper.WaveFormRendererLib
{
    public class StandardWaveFormRendererSettings : WaveFormRendererSettings
    {
        public StandardWaveFormRendererSettings()
        {
            PixelsPerPeak = 1;
            SpacerPixels = 0;
            TopPeakPen = Pens.Maroon;
            BottomPeakPen = Pens.Peru;
        }


        public sealed override Pen? TopPeakPen { get; set; }

        // not needed
        public override Pen? TopSpacerPen { get; set; }
        
        public sealed override Pen? BottomPeakPen { get; set; }
        
        // not needed
        public override Pen? BottomSpacerPen { get; set; }
    }
}