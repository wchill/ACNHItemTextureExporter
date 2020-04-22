using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ACNHItemTextureExporter.Decoders.Astc;
using ACNHItemTextureExporter.Decoders.Rgba;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace ACNHItemTextureExporter
{
    public class BitmapExporter
    {
        public static void SaveBitmap(Texture texture, string FileName, bool crop = false, bool ExportSurfaceLevel = false, int SurfaceLevel = 0, int MipLevel = 0)
        {
            // TODO: Handle array bitmaps
            /*
            var arrayCount = 1;
            if (arrayCount > 1 && !ExportSurfaceLevel)
            {
                string ext = Path.GetExtension(FileName);

                int index = FileName.LastIndexOf('.');
                string name = index == -1 ? FileName : FileName.Substring(0, index);

                for (int i = 0; i < arrayCount; i++)
                {
                    Bitmap arrayBitMap = GetBitmap(texture, i, 0);

                    if (arrayBitMap != null)
                    {
                        arrayBitMap.Save($"{name}_Slice_{i}_{ext}");
                        arrayBitMap.Dispose();
                    }
                }

                return;
            }
            */

            Bitmap bitMap = GetBitmap(texture, SurfaceLevel, MipLevel);

            if (!crop)
            {
                if (bitMap != null)
                {
                    bitMap.Save(FileName);
                    bitMap.Dispose();
                }
            }
            else
            {
                if (bitMap != null)
                {
                    using var newBitmap = CropBitmap(bitMap);
                    bitMap.Dispose();
                    newBitmap.Save(FileName);
                }
            }
        }

        private static Bitmap CropBitmap(Bitmap bitmap)
        {
            Bitmap bmpImage = new Bitmap(bitmap);
            var bb = GetBoundingBox(bmpImage);
            return bmpImage.Clone(bb, bmpImage.PixelFormat);
        }

        private static Rectangle GetBoundingBox(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            IntPtr ptr = bitmapData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            var stride = Math.Abs(bitmapData.Stride);
            int bytes = stride * bitmap.Height;
            byte[] data = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, data, 0, bytes);

            bitmap.UnlockBits(bitmapData);

            // Find top non-empty scanline
            int topY;
            for (topY = 0; topY < bitmapData.Height; topY++)
            {
                var span = data.AsSpan(topY * stride, stride);
                if (DoesScanlineHaveNontransparentPixel(span))
                {
                    break;
                }
            }

            int bottomY;
            for (bottomY = bitmapData.Height - 1; bottomY > topY; bottomY--)
            {
                var span = data.AsSpan(bottomY * stride, stride);
                if (DoesScanlineHaveNontransparentPixel(span))
                {
                    break;
                }
            }

            int leftX;
            for (leftX = 0; leftX < bitmap.Width; leftX++)
            {
                if (DoesColumnHaveNontransparentPixel(leftX, topY, bottomY, stride, data))
                {
                    break;
                }
            }

            int rightX;
            for (rightX = bitmap.Width - 1; rightX > leftX; rightX--)
            {
                if (DoesColumnHaveNontransparentPixel(rightX, topY, bottomY, stride, data))
                {
                    break;
                }
            }

            //return new Rectangle(leftX, topY, rightX - leftX + 1, bottomY - topY + 1);
            return new Rectangle(leftX, topY, rightX - leftX + 1, bottomY - topY + 1);
        }

        private static bool DoesScanlineHaveNontransparentPixel(Span<byte> data)
        {
            for (var i = 3; i < data.Length; i += 4)
            {
                if (data[i] != 0) return true;
            }

            return false;
        }

        private static bool DoesColumnHaveNontransparentPixel(int x, int topY, int bottomY, int stride, byte[] data)
        {
            for (var y = topY; y <= bottomY; y++)
            {
                var index = y * stride + (4 * x) + 3;
                if (data[index] != 0) return true;
            }

            return false;
        }

        public static bool SaveBitmap(Texture texture, Stream stream, bool ExportSurfaceLevel = false,
            int SurfaceLevel = 0, int MipLevel = 0)
        {
            Bitmap bitMap = GetBitmap(texture, SurfaceLevel, MipLevel);

            if (bitMap != null)
            {
                bitMap.Save(stream, ImageFormat.Png);
                bitMap.Dispose();
                return true;
            }

            return false;
        }

        public static Span<byte> ConvertBgraToRgba(Span<byte> bytes)
        {
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var temp = bytes[i];
                bytes[i] = bytes[i + 2];
                bytes[i + 2] = temp;
            }
            return bytes;
        }

        public static Bitmap GetBitmap(Texture texture, int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0)
        {
            int width = (int)Math.Max(1, texture.Width >> MipLevel);
            int height = (int)Math.Max(1, texture.Height >> MipLevel);
            TextureFormatInfo formatInfo = TextureFormatInfo.FormatTable[texture.Format];
            Memory<byte> data = GetImageData(texture, ArrayLevel, MipLevel, DepthLevel);

            if (AstcDecoder.CanHandle(texture))
            {
                if (AstcDecoder.TryDecodeToRgba8(data, (int)formatInfo.BlockWidth, (int)formatInfo.BlockHeight, width, height, 1, 1, out var decoded))
                {
                    return GetBitmapFromBytes(ConvertBgraToRgba(decoded), width, height, PixelFormat.Format32bppArgb);
                }
            }
            else if (DDSDecoder.CanHandle(texture))
            {
                return DDSDecoder.Decompress(data.Span, width, height, texture.Format);
            }
            else if (RgbaDecoder.CanHandle(texture))
            {
                return RgbaDecoder.Decode(data.Span, width, height, texture.Format);
            }

            return null;
        }

        public static Bitmap GetBitmapFromBytes(Span<byte> Buffer, int Width, int Height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            Rectangle Rect = new Rectangle(0, 0, Width, Height);

            Bitmap Img = new Bitmap(Width, Height, pixelFormat);

            BitmapData ImgData = Img.LockBits(Rect, ImageLockMode.WriteOnly, Img.PixelFormat);

            if (Buffer.Length > ImgData.Stride * Img.Height)
            {
                Img.UnlockBits(ImgData);
                Img.Dispose();
                throw new Exception($"Invalid Buffer Length ({Buffer.Length})!!!");
            }

            Marshal.Copy(Buffer.ToArray(), 0, ImgData.Scan0, Buffer.Length);

            Img.UnlockBits(ImgData);

            return Img;
        }

        public static Memory<byte> GetImageData(Texture texture, int targetArrayLevel = 0, int targetMipLevel = 0, int targetDepthLevel = 0)
        {
            TextureFormatInfo formatInfo = TextureFormatInfo.FormatTable[texture.Format];
            int target = 1;
            uint blkWidth = formatInfo.BlockWidth;
            uint blkHeight = formatInfo.BlockHeight;

            int linesPerBlockHeight = (1 << (int)texture.BlockHeightLog2) * 8;
            uint bpp = formatInfo.BytesPerPixel;

            uint numDepth = 1;

            for (int depthLevel = 0; depthLevel < numDepth; depthLevel++)
            {
                for (int arrayLevel = 0; arrayLevel < texture.TextureData.Count; arrayLevel++)
                {
                    int blockHeightShift = 0;

                    for (int mipLevel = 0; mipLevel < texture.TextureData[arrayLevel].Count; mipLevel++)
                    {
                        uint width = Math.Max(1, texture.Width >> mipLevel);
                        uint height = Math.Max(1, texture.Height >> mipLevel);

                        uint size = TegraX1Swizzle.DivRoundUp(width, blkWidth) * TegraX1Swizzle.DivRoundUp(height, blkHeight) * bpp;

                        if (TegraX1Swizzle.Pow2RoundUp(TegraX1Swizzle.DivRoundUp(height, blkWidth)) < linesPerBlockHeight)
                            blockHeightShift += 1;

                        if (targetArrayLevel == arrayLevel && targetMipLevel == mipLevel && targetDepthLevel == depthLevel)
                        {
                            //Console.WriteLine($"{width} {height} {depth} {blkWidth} {blkHeight} {blkDepth} {target} {bpp} {texture.TileMode} {(int)Math.Max(0, texture.BlockHeightLog2 - blockHeightShift)} {texture.TextureData[arrayLevel][mipLevel].Length}");
                            var result = TegraX1Swizzle.Deswizzle(width, height, blkWidth, blkHeight, target, bpp, (uint)texture.TileMode, (int)Math.Max(0, texture.BlockHeightLog2 - blockHeightShift), texture.TextureData[arrayLevel][mipLevel]);
                            return new ArraySegment<byte>(result, 0, (int)size);
                        }
                    }
                }
            }

            return new byte[0];
        }
    }
}
