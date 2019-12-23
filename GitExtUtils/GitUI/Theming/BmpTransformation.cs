using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GitExtUtils.GitUI.Theming
{
    public abstract class BmpTransformation
    {
        private readonly Bitmap _bmp;
        protected const int B = 0;
        protected const int G = 1;
        protected const int R = 2;
        protected const int A = 3;

        private const int BytesPerPixel = 4;
        private const PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;

        protected Rectangle Rect { get; private set; }
        protected byte[] BgraValues { get; private set; }
        protected bool ImageChanged { get; set; }

        protected BmpTransformation(Bitmap bmp)
        {
            _bmp = bmp;
            Rect = new Rectangle(location: default, size: bmp.Size);
        }

        public void Execute()
        {
            var bmpData = _bmp.LockBits(Rect, ImageLockMode.ReadWrite, PixelFormat);

            try
            {
                int numBytes = bmpData.Stride * bmpData.Height;
                BgraValues = new byte[numBytes];
                Marshal.Copy(bmpData.Scan0, BgraValues, 0, numBytes);

                ExecuteRaw();

                if (ImageChanged)
                {
                    Marshal.Copy(BgraValues, 0, bmpData.Scan0, numBytes);
                }
            }
            finally
            {
                _bmp.UnlockBits(bmpData);
            }
        }

        protected abstract void ExecuteRaw();

        protected int GetLocation(int x, int y) =>
            BytesPerPixel * ((Rect.Width * y) + x);
    }
}
