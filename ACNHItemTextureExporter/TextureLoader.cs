using System;
using System.IO;
using System.Linq;
using ACNHItemTextureExporter.Decoders.Astc;
using ACNHItemTextureExporter.Decoders.Rgba;
using Syroot.NintenTools.NSW.Bfres;
using Syroot.NintenTools.NSW.Bntx;

namespace ACNHItemTextureExporter
{
    public class TextureLoader
    {
        public static Texture DecompressAndLoadFile(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == ".bfres")
            {
                return GetTexture(path, new ResFile(path));
            }
            else
            {
                var compressed = File.ReadAllBytes(path);
                using (var sarcStream = ZstdDecompressor.DecompressToStream(compressed))
                {
                    using (var sarc = new SarcLoader(sarcStream))
                    {
                        var resFiles = sarc.Where(f => Path.GetExtension(f.Item1) == ".bfres");
                        if (resFiles.Count() != 1) return null;
                        using (var resFileStream = sarc.ExportStream(resFiles.First().Item2))
                        {
                            var resFile = new ResFile(resFileStream);
                            return GetTexture(path, resFile);
                        }
                    }
                }
            }
        }

        static Texture GetTexture(string path, ResFile file)
        {
            var basename = Path.GetFileName(path);
            if (file.ExternalFiles.Count() > 1)
            {
                Console.WriteLine($"[SKIP] {basename} - contains multiple external files");
                return null;
            }
            try
            {
                using (var bntxStream = file.ExternalFiles.First().GetStream())
                {
                    var bntxFile = new BntxFile(bntxStream);
                    if (bntxFile.Textures.Count != 1)
                    {
                        Console.WriteLine($"[SKIP] {basename} - contains {bntxFile.Textures.Count} textures");
                        return null;
                    }
                    var texture = bntxFile.Textures.First();
                    if (IsTextureDecodable(texture))
                    {
                        return texture;
                    }

                    if (TextureFormatInfo.FormatTable.ContainsKey(texture.Format))
                    {
                        Console.WriteLine($"[SKIP] {basename} - unhandled texture format {texture.Format}");
                        return null;
                    }

                    Console.WriteLine($"[SKIP] {basename} - unknown texture format {texture.Format}");
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetCleanFilenameWithoutExtensions(string path)
        {
            var nameWithoutExtensions = Path.GetFileNameWithoutExtension(path);
            return nameWithoutExtensions.Replace(".Nin_NX_NVN", "").Replace(".sarc", "");
        }

        private static bool IsTextureDecodable(Texture texture)
        {
            if (AstcDecoder.CanHandle(texture))
            {
                return true;
            }
            if (DDSDecoder.CanHandle(texture))
            {
                return true;
            }
            if (RgbaDecoder.CanHandle(texture))
            {
                return true;
            }

            return false;
        }
    }
}