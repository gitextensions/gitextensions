using System.Drawing;

namespace GitExtUtils.GitUI.Theming
{
    public class LightnessCorrection : BmpTransformation
    {
        private readonly HslColor _textColor = new HslColor(SystemColors.WindowText);
        private readonly HslColor _bgColor = new HslColor(SystemColors.Window);

        public LightnessCorrection(Bitmap bmp)
            : base(bmp)
        {
        }

        protected override void ExecuteRaw()
        {
            ImageChanged = true;
            for (int i = Rect.Left; i < Rect.Right; i++)
            {
                for (int j = Rect.Top; j < Rect.Bottom; j++)
                {
                    Transform(BgraValues, GetLocation(i, j));
                }
            }
        }

        private HslColor Transform(HslColor hsl) =>
            new HslColor(hsl.H, TransformS(hsl), TransformL(hsl.L));

        private void Transform(byte[] bgraValues, int location)
        {
            var rgb = Color.FromArgb(
                bgraValues[location + R],
                bgraValues[location + G],
                bgraValues[location + B]);
            var hsl = rgb.ToPerceptedHsl();
            var hslTransformed = Transform(hsl);
            ToBgra(hslTransformed.ToActualHsl(rgb), bgraValues, location);
        }

        private double TransformL(double l) =>
            _textColor.L + (l * (_bgColor.L - _textColor.L));

        private static double TransformS(HslColor hsl)
        {
            // mathematically near black color can have high saturation
            // practically though the hue (color) is not distinguishable
            // so "perceived" saturation is near 0
            return hsl.L > 0.1
                ? hsl.S
                : hsl.S * hsl.L / 0.1;
        }

        private static void ToBgra(HslColor c, byte[] rgbArr, int location)
        {
            var rgb = c.ToColor();
            rgbArr[location + B] = rgb.B;
            rgbArr[location + G] = rgb.G;
            rgbArr[location + R] = rgb.R;
        }
    }
}
