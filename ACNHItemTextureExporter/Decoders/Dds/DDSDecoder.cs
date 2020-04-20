using System;
using System.Collections.Generic;
using System.Drawing;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace ACNHItemTextureExporter
{
    public class DDSDecoder
    {
        private static HashSet<SurfaceFormat> _supportedFormats = new HashSet<SurfaceFormat>
        {
            SurfaceFormat.BC1_SRGB, 
            //SurfaceFormat.BC1_UNORM, 
            //SurfaceFormat.BC2_SRGB, 
            //SurfaceFormat.BC2_UNORM, 
            SurfaceFormat.BC3_SRGB,
            //SurfaceFormat.BC3_UNORM,
            //SurfaceFormat.BC4_UNORM,
            //SurfaceFormat.BC4_SNORM,
            //SurfaceFormat.BC5_UNORM,
            //SurfaceFormat.BC5_SNORM,
            //SurfaceFormat.BC6_FLOAT,
            //SurfaceFormat.BC6_UFLOAT,
            //SurfaceFormat.BC7_SRGB,
            //SurfaceFormat.BC7_UNORM
        };

        public static bool CanHandle(Texture texture)
        {
            return _supportedFormats.Contains(texture.Format);
        }

        private static byte[] BCnDecodeTile(ReadOnlySpan<byte> Input, int Offset, bool IsBC1)
        {
            Color[] CLUT = new Color[4];

            int c0 = Input.GetInt16(Offset);
            int c1 = Input.GetInt16(Offset + 2);

            CLUT[0] = DecodeRGB565(c0);
            CLUT[1] = DecodeRGB565(c1);
            CLUT[2] = CalculateCLUT2(CLUT[0], CLUT[1], c0, c1, IsBC1);
            CLUT[3] = CalculateCLUT3(CLUT[0], CLUT[1], c0, c1, IsBC1);

            int Indices = Input.GetInt32(Offset + 4);

            int IdxShift = 0;

            byte[] Output = new byte[4 * 4 * 4];

            int OOffset = 0;

            for (int TY = 0; TY < 4; TY++)
            {
                for (int TX = 0; TX < 4; TX++)
                {
                    int Idx = (Indices >> IdxShift) & 3;

                    IdxShift += 2;

                    Color Pixel = CLUT[Idx];

                    Output[OOffset + 0] = Pixel.B;
                    Output[OOffset + 1] = Pixel.G;
                    Output[OOffset + 2] = Pixel.R;
                    Output[OOffset + 3] = Pixel.A;

                    OOffset += 4;
                }
            }
            return Output;
        }

        private static Color DecodeRGB565(int Value)
        {
            int B = ((Value >> 0) & 0x1f) << 3;
            int G = ((Value >> 5) & 0x3f) << 2;
            int R = ((Value >> 11) & 0x1f) << 3;

            return Color.FromArgb(
                R | (R >> 5),
                G | (G >> 6),
                B | (B >> 5));
        }
        private static Color CalculateCLUT2(Color C0, Color C1, int c0, int c1, bool IsBC1)
        {
            if (c0 > c1 || !IsBC1)
            {
                return Color.FromArgb(
                    (2 * C0.R + C1.R) / 3,
                    (2 * C0.G + C1.G) / 3,
                    (2 * C0.B + C1.B) / 3);
            }
            else
            {
                return Color.FromArgb(
                    (C0.R + C1.R) / 2,
                    (C0.G + C1.G) / 2,
                    (C0.B + C1.B) / 2);
            }
        }
        private static Color CalculateCLUT3(Color C0, Color C1, int c0, int c1, bool IsBC1)
        {
            if (c0 > c1 || !IsBC1)
            {
                return
                    Color.FromArgb(
                        (2 * C1.R + C0.R) / 3,
                        (2 * C1.G + C0.G) / 3,
                        (2 * C1.B + C0.B) / 3);
            }

            return Color.Transparent;
        }
        private static void CalculateBC3Alpha(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if (Alpha[0] > Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0xff;
                }
            }
        }

        public static Bitmap Decompress(ReadOnlySpan<byte> data, int width, int height, SurfaceFormat format)
        {
            if (format == SurfaceFormat.BC1_SRGB)
            {
                return DecompressBC1(data, width, height, true);
            }
            if (format == SurfaceFormat.BC3_SRGB)
            {
                return DecompressBC3(data, width, height, true);
            }
            throw new NotSupportedException();
        }

        public static Bitmap DecompressBC1(ReadOnlySpan<byte> data, int width, int height, bool IsSRGB)
        {
            int W = (width + 3) / 4;
            int H = (height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = (Y * W + X) * 8;

                    byte[] Tile = BCnDecodeTile(data, IOffs, true);

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = Tile[TOffset + 3];

                            TOffset += 4;
                        }
                    }
                }
            }
            return BitmapExporter.GetBitmapFromBytes(Output, W * 4, H * 4);
        }

        public static Bitmap DecompressBC3(ReadOnlySpan<byte> data, int width, int height, bool IsSRGB)
        {
            int W = (width + 3) / 4;
            int H = (height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = (Y * W + X) * 16;

                    byte[] Tile = BCnDecodeTile(data, IOffs + 8, false);

                    byte[] Alpha = new byte[8];

                    Alpha[0] = data[IOffs + 0];
                    Alpha[1] = data[IOffs + 1];

                    CalculateBC3Alpha(Alpha);

                    int AlphaLow = data.GetInt32(IOffs + 2);
                    int AlphaHigh = data.GetInt16(IOffs + 6);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            byte AlphaPx = Alpha[(AlphaCh >> (TY * 12 + TX * 3)) & 7];

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = AlphaPx;

                            TOffset += 4;
                        }
                    }
                }
            }

            return BitmapExporter.GetBitmapFromBytes(Output, W * 4, H * 4);
        }
    }
}