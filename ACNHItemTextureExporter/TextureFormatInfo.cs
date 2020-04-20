using System;
using System.Collections.Generic;
using System.Text;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace ACNHItemTextureExporter
{
    class TextureFormatInfo
    {
        public enum TargetBuffer
        {
            Color,
            Depth,
            DepthStencil
        }

        public uint BytesPerPixel { get; }
        public uint BlockWidth { get; }
        public uint BlockHeight { get; }
        public TargetBuffer Buffer { get; }

        public TextureFormatInfo(uint bytesPerPixel, uint blockWidth, uint blockHeight, TargetBuffer buffer)
        {
            BytesPerPixel = bytesPerPixel;
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Buffer = buffer;
        }

        public static readonly Dictionary<SurfaceFormat, TextureFormatInfo> FormatTable = new Dictionary<SurfaceFormat, TextureFormatInfo>()
        {
            { SurfaceFormat.R4_G4_UNORM,           new TextureFormatInfo(1,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R4_G4_B4_A4_UNORM,     new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.A4_B4_G4_R4_UNORM,     new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R5_G5_B5_A1_UNORM,     new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.A1_B5_G5_R5_UNORM,     new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R5_G6_B5_UNORM,        new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.B5_G6_R5_UNORM,        new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R8_G8_UNORM,           new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R8_G8_SNORM,           new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R8_G8_B8_A8_UNORM,     new TextureFormatInfo(4,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R8_G8_B8_A8_SNORM,     new TextureFormatInfo(4,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R8_G8_B8_A8_SRGB,      new TextureFormatInfo(4,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.B8_G8_R8_A8_UNORM,     new TextureFormatInfo(4,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.B8_G8_R8_A8_SRGB,      new TextureFormatInfo(4,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R9_G9_B9_E5_UNORM,     new TextureFormatInfo(4,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R10_G10_B10_A2_UNORM,  new TextureFormatInfo(4, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R11_G11_B10_UNORM,     new TextureFormatInfo(4, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.B10_G11_R11_UNORM,     new TextureFormatInfo(4, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R16_G16_UNORM,         new TextureFormatInfo(4, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R24_G8_UNORM,          new TextureFormatInfo(4, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R32_UNORM,             new TextureFormatInfo(4, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R16_G16_B16_A16_UNORM, new TextureFormatInfo(8, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R32_G8_X24_UNORM,      new TextureFormatInfo(8, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R32_G32_UNORM,         new TextureFormatInfo(8, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R32_G32_B32_UNORM,     new TextureFormatInfo(8, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R32_G32_B32_A32_UNORM, new TextureFormatInfo(8, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.B5_G5_R5_A1_UNORM,     new TextureFormatInfo(2, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R16_UINT,              new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R16_UNORM,             new TextureFormatInfo(2,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.R8_UNORM,              new TextureFormatInfo(1,  1,  1, TargetBuffer.Color) },
            { SurfaceFormat.BC1_SRGB,              new TextureFormatInfo(8,  4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC1_UNORM,             new TextureFormatInfo(8,  4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC2_SRGB,              new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC2_UNORM,             new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC3_SRGB,              new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC3_UNORM,             new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC4_UNORM,             new TextureFormatInfo(8,  4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC4_SNORM,             new TextureFormatInfo(8,  4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC5_UNORM,             new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC5_SNORM,             new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC6_FLOAT,             new TextureFormatInfo(16, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.BC6_UFLOAT,            new TextureFormatInfo(16, 1,  1, TargetBuffer.Color) },
            { SurfaceFormat.BC7_SRGB,              new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.BC7_UNORM,             new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },

            { SurfaceFormat.ASTC_4x4_UNORM,        new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_4x4_SRGB,         new TextureFormatInfo(16, 4,  4, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_5x4_UNORM,        new TextureFormatInfo(16, 5,  4, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_5x4_SRGB,         new TextureFormatInfo(16, 5,  4, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_5x5_UNORM,        new TextureFormatInfo(16, 5,  5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_5x5_SRGB,         new TextureFormatInfo(16, 5,  5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_6x5_UNORM,        new TextureFormatInfo(16, 6,  5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_6x5_SRGB,         new TextureFormatInfo(16, 6,  5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_6x6_UNORM,        new TextureFormatInfo(16, 6,  6, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_6x6_SRGB,         new TextureFormatInfo(16, 6,  6, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_8x5_UNORM,        new TextureFormatInfo(16, 8,  5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_8x5_SRGB,         new TextureFormatInfo(16, 8,  5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_8x6_UNORM,        new TextureFormatInfo(16, 8,  6, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_8x6_SRGB,         new TextureFormatInfo(16, 8,  6, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_8x8_UNORM,        new TextureFormatInfo(16, 8,  8, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_8x8_SRGB,         new TextureFormatInfo(16, 8,  8, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x5_UNORM,       new TextureFormatInfo(16, 10, 5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x5_SRGB,        new TextureFormatInfo(16, 10, 5, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x6_UNORM,       new TextureFormatInfo(16, 10, 6, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x6_SRGB,        new TextureFormatInfo(16, 10, 6, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x8_UNORM,       new TextureFormatInfo(16, 10, 8, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x8_SRGB,        new TextureFormatInfo(16, 10, 8, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x10_UNORM,      new TextureFormatInfo(16, 10, 10, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_10x10_SRGB,       new TextureFormatInfo(16, 10, 10, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_12x10_UNORM,      new TextureFormatInfo(16, 12, 10, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_12x10_SRGB,       new TextureFormatInfo(16, 12, 10, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_12x12_UNORM,      new TextureFormatInfo(16, 12, 12, TargetBuffer.Color) },
            { SurfaceFormat.ASTC_12x12_SRGB,       new TextureFormatInfo(16, 12, 12, TargetBuffer.Color) },
            { SurfaceFormat.ETC1_UNORM,            new TextureFormatInfo(4, 1, 1, TargetBuffer.Color) },
            { SurfaceFormat.ETC1_SRGB,             new TextureFormatInfo(4, 1, 1, TargetBuffer.Color) },
            { SurfaceFormat.D32_FLOAT_S8X24_UINT,  new TextureFormatInfo(8, 1, 1, TargetBuffer.DepthStencil)}
        };
    }
}
