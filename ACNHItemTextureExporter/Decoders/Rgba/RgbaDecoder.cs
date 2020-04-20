using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace ACNHItemTextureExporter.Decoders.Rgba
{
    public class RgbaDecoder
    {
        public static bool CanHandle(Texture texture)
        {
            return texture.Format == SurfaceFormat.R8_G8_B8_A8_SRGB;
        }

        public static Bitmap Decode(ReadOnlySpan<byte> data, int width, int height, SurfaceFormat format)
        {
            return BitmapExporter.GetBitmapFromBytes(BitmapExporter.ConvertBgraToRgba(data.ToArray()), width, height, PixelFormat.Format32bppArgb);
        }
    }
}
