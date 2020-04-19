using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Syroot.NintenTools.NSW.Bntx;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CommandLine;
using Syroot.NintenTools.NSW.Bfres;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace ACNHItemTextureExporter
{
    class Program
    {
        class CommandLineOptions
        {
            [Option('n', "use-texture-name", Default = true, HelpText = "Use texture name instead of file name for output files")]
            public bool UseTextureName { get; set; }

            [Option('t', "threads", Default = -1, HelpText = "Max number of threads")]
            public int Threads { get; set; }

            [Option('r', "regex", Default = @"\.(?:bfres|zs)$", HelpText = "Filter filenames by regex")]
            public string Regex { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output folder")]
            public string OutputFolder { get; set; }

            [Option('i', "input", Required = true, HelpText = "Input paths")]
            public IEnumerable<string> InputPaths { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(o =>
                {
                    Directory.CreateDirectory(o.OutputFolder);
                    SaveTextures(o);

                    Console.WriteLine("Complete");
                });
        }

        static void SaveTextures(CommandLineOptions options)
        {
            var files = options.InputPaths.SelectMany(path =>
            {
                if (File.Exists(path)) return new[] { path };
                if (Directory.Exists(path)) return DirectoryHelper.EnumerateFilesWithRegex(path, options.Regex, SearchOption.AllDirectories);
                return new string[] { };
            });

            var textureCount = 0;
            var timer = new Stopwatch();
            timer.Start();
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = options.Threads }, f =>
            {
                var nameWithoutExtensions = GetCleanFilenameWithoutExtensions(f);

                var texture = DecompressAndLoadFile(f);
                var basename = Path.GetFileName(f);
                if (texture == null)
                {
                    return;
                }

                try
                {
                    var name = options.UseTextureName ? $"{texture.Name}.png" : $"{nameWithoutExtensions}.png";

                    BitmapExporter.SaveBitmap(texture, Path.Combine(options.OutputFolder, name));
                    Console.WriteLine($"[OUT] {basename} -> {name}");
                    Interlocked.Increment(ref textureCount);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERR] {basename} - texture failed to load ({e.Message})");
                }

            });

            timer.Stop();
            Console.WriteLine($"Saved {textureCount} textures");
            Console.WriteLine($"Took {timer.ElapsedMilliseconds}ms");
        }

        static Texture DecompressAndLoadFile(string path)
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
                    if (texture.Format.ToString().Contains("ASTC"))
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

        static string GetCleanFilenameWithoutExtensions(string path)
        {
            var nameWithoutExtensions = Path.GetFileNameWithoutExtension(path);
            return nameWithoutExtensions.Replace(".Nin_NX_NVN", "").Replace(".sarc", "");
        }
    }
}
