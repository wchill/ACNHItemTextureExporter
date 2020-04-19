using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Syroot.NintenTools.NSW.Bntx;
using System.Collections.Generic;
using System.Threading;

namespace ACNHItemTextureExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine($"Usage: {Environment.CommandLine} <output dir> [input files/directories]");
            }

            SaveTextures(args[0], args.Skip(1));

            Console.WriteLine("Complete");
        }

        static void SaveTextures(string outputDir, IEnumerable<string> inputPaths)
        {
            var files = inputPaths.SelectMany(path =>
            {
                if (File.Exists(path)) return new[] { path };
                if (Directory.Exists(path)) return DirectoryHelper.EnumerateFilesWithRegex(path, @"*\.(?:bfres|zs)", SearchOption.AllDirectories);
                return new string[] { };
            });

            var textureCount = 0;

            Parallel.ForEach(files, f =>
            {
                var textures = DecompressAndLoadTextures(f);
                if (textures.Count != 1)
                {
                    var basename = Path.GetFileName(f);
                    Console.Error.WriteLine($"Skipping {basename} because it has {textures.Count} textures so is probably not a layout file");
                }
                foreach (var texture in textures)
                {
                    BitmapExporter.SaveBitmap(texture, Path.Combine(outputDir, $"{texture.Name}.png"));
                    Console.WriteLine($"Exported {texture.Name}.png");
                    Interlocked.Increment(ref textureCount);
                }
            });

            Console.WriteLine($"Saved {textureCount} textures");
        }

        static List<Texture> DecompressAndLoadTextures(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == ".zs")
            {
                var compressed = File.ReadAllBytes(path);
                return TextureLoader.Load(ZstdDecompressor.DecompressToStream(compressed));
            }
            else
            {
                return TextureLoader.Load(path);
            }
        }
    }
}
