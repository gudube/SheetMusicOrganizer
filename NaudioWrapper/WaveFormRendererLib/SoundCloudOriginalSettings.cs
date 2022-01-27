using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NAudioWrapper.WaveFormRendererLib
{
    public class SoundCloudOriginalSettings : WaveFormRendererSettings
    {
        private int _lastTopHeight;
        private int _lastBottomHeight;

        public SoundCloudOriginalSettings()
        {
            PixelsPerPeak = 1;
            SpacerPixels = 0;
            BackgroundColor = Color.White;
        }

        public Color TopColor1 = Color.FromArgb(120, 120, 120);
        public Color TopColor2 = Color.FromArgb(50, 50, 50);
        public Color BottomColor1 = Color.FromArgb(16, 16, 16);
        public Color BottomColor2 = Color.FromArgb(142, 142, 142);
        public Color BottomColor3 = Color.FromArgb(150, 150, 150);

        public override Pen? TopPeakPen
        {
            get
            {
                if (base.TopPeakPen == null || _lastTopHeight != TopHeight)
                {
                    base.TopPeakPen = CreateGradientPen(TopHeight, TopColor1, TopColor2);
                    _lastTopHeight = TopHeight;
                }
                return base.TopPeakPen;
            }
            set { base.TopPeakPen = value; }
        }


        public override Pen? BottomPeakPen 
        {
            get
            {
                if (base.BottomPeakPen == null || _lastBottomHeight != BottomHeight || _lastTopHeight != TopHeight)
                {
                    base.BottomPeakPen = CreateSoundcloudBottomPen(TopHeight, BottomHeight);
                    _lastBottomHeight = BottomHeight;
                    _lastTopHeight = TopHeight;
                }
                return base.BottomPeakPen;
            }
            set { base.BottomPeakPen = value; }
        }


        public override Pen BottomSpacerPen
        {
            get { throw new InvalidOperationException("No spacer pen required"); }
        }

        private Pen CreateSoundcloudBottomPen(int topHeight, int bottomHeight)
        {
            var bottomGradient = new LinearGradientBrush(new Point(0, topHeight), new Point(0, topHeight + bottomHeight),
                BottomColor1, BottomColor3);
            var colorBlend = new ColorBlend(3);
            colorBlend.Colors[0] = BottomColor1;
            colorBlend.Colors[1] = BottomColor2;
            colorBlend.Colors[2] = BottomColor3;

            colorBlend.Positions[0] = 0;
            colorBlend.Positions[1] = 0.1f;
            colorBlend.Positions[2] = 1.0f;
            bottomGradient.InterpolationColors = colorBlend;
            return new Pen(bottomGradient);
        }
    }
}