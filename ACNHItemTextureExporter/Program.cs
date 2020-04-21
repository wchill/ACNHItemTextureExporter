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
            [Option('n', "use-file-name", HelpText = "Use original file name instead of texture name for output files")]
            public bool UseFileName { get; set; }

            [Option('c', "cropped", HelpText = "Output cropped textures instead of original")]
            public bool CropTextures { get; set; }

            [Option('t', "threads", Default = -1, HelpText = "Max number of threads")]
            public int Threads { get; set; }

            [Option('f', "force", HelpText = "Force overwrite if file exists")]
            public bool ForceOverwrite { get; set; }

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
                var texture = TextureLoader.DecompressAndLoadFile(f);
                var basename = Path.GetFileName(f);
                if (texture == null)
                {
                    return;
                }

                try
                {
                    string path = GenerateFileName(f, texture.Name, options);
                    string name = Path.GetFileName(path);

                    if (!File.Exists(path) || options.ForceOverwrite)
                    {
                        BitmapExporter.SaveBitmap(texture, path, options.CropTextures);
                        Console.WriteLine($"[OUT] {basename} -> {name} ({texture.Format})");
                    }
                    else
                    {
                        Console.WriteLine($"[DUPE] {basename} -> {name} ({texture.Format})");
                    }
                    
                    Interlocked.Increment(ref textureCount);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERR] {basename} - texture ({texture.Format} failed to load ({e.Message})");
                }

            });

            timer.Stop();
            Console.WriteLine($"Saved {textureCount} textures");
            Console.WriteLine($"Took {timer.ElapsedMilliseconds}ms");
        }

        private static string GenerateFileName(string name, string textureName, CommandLineOptions options)
        {
            var nameWithoutExtensions = TextureLoader.GetCleanFilenameWithoutExtensions(name);
            string filename;
            string path;

            if (nameWithoutExtensions.Contains("Layout_"))
            {
                var split = nameWithoutExtensions.Split('_', 3);
                filename = options.UseFileName ? split[2] : textureName;
                Directory.CreateDirectory(Path.Combine(options.OutputFolder, split[0], split[1]));
                path = Path.Combine(options.OutputFolder, split[0], split[1]);
            }
            else
            {
                filename = options.UseFileName ? nameWithoutExtensions : textureName;
                path = options.OutputFolder;
            }

            if (options.CropTextures)
            {
                filename += "Cropped";
            }

            return Path.Combine(path, $"{filename}.png");
        }
    }
}
