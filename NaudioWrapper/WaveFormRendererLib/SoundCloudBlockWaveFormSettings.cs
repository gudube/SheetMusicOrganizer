using System.Drawing;

namespace NAudioWrapper.WaveFormRendererLib
{
    public class SoundCloudBlockWaveFormSettings : WaveFormRendererSettings
    {
        private readonly Color _topSpacerStartColor;
        private Pen? _topPen;
        private Pen? _topSpacerPen;
        private Pen? _bottomPen;
        private Pen? _bottomSpacerPen;

        private int _lastTopHeight;
        private int _lastBottomHeight;

        public SoundCloudBlockWaveFormSettings(Color topPeakColor, Color topSpacerStartColor, Color bottomPeakColor, Color bottomSpacerColor)
        {
            this._topSpacerStartColor = topSpacerStartColor;
            _topPen = new Pen(topPeakColor);
            _bottomPen = new Pen(bottomPeakColor);
            _bottomSpacerPen = new Pen(bottomSpacerColor);
            PixelsPerPeak = 4;
            SpacerPixels = 2;
            BackgroundColor = Color.White;
            TopSpacerGradientStartColor = Color.White;
        }

        public override Pen? TopPeakPen
        {
            get { return _topPen; }
            set { _topPen = value; }
        }

        public Color TopSpacerGradientStartColor { get; set; }

        public override Pen? TopSpacerPen
        {
            get
            {
                if (_topSpacerPen == null || _lastBottomHeight != BottomHeight || _lastTopHeight != TopHeight)
                {
                    _topSpacerPen = CreateGradientPen(TopHeight, TopSpacerGradientStartColor, _topSpacerStartColor);
                    _lastBottomHeight = BottomHeight;
                    _lastTopHeight = TopHeight;
                }
                return _topSpacerPen;
            }
            set { _topSpacerPen = value; }
        }


        public override Pen? BottomPeakPen
        {
            get { return _bottomPen; }
            set { _bottomPen = value; }
        }


        public override Pen? BottomSpacerPen
        {
            get { return _bottomSpacerPen; }
            set { _bottomSpacerPen = value; }
        }

    }
}