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
                var nameWithoutExtensions = TextureLoader.GetCleanFilenameWithoutExtensions(f);

                var texture = TextureLoader.DecompressAndLoadFile(f);
                var basename = Path.GetFileName(f);
                if (texture == null)
                {
                    return;
                }

                try
                {
                    string path;
                    string name;
                    if (nameWithoutExtensions.Contains("Layout_"))
                    {
                        var split = nameWithoutExtensions.Split('_', 3);
                        name = options.UseTextureName ? $"{texture.Name}.png" : $"{split[2]}.png";
                        Directory.CreateDirectory(Path.Combine(options.OutputFolder, split[0], split[1]));
                        path = Path.Combine(options.OutputFolder, split[0], split[1], name);
                    }
                    else
                    {
                        name = options.UseTextureName ? $"{texture.Name}.png" : $"{nameWithoutExtensions}.png";
                        path = Path.Combine(options.OutputFolder, name);
                    }

                    if (!File.Exists(path))
                    {
                        BitmapExporter.SaveBitmap(texture, path);
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
    }
}
