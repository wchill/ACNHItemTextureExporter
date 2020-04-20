using System;

namespace ACNHItemTextureExporter.Decoders.Astc
{
    public class AstcDecoderException : Exception
    {
        public AstcDecoderException(string exMsg) : base(exMsg) { }
    }
}