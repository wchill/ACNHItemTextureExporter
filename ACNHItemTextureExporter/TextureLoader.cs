using Syroot.NintenTools.NSW.Bfres;
using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACNHItemTextureExporter
{
    static class TextureLoader
    {
        private static List<Texture> LoadFromResFile(ResFile file)
        {
            var bntxFiles = file.ExternalFiles.Select(f => new BntxFile(f.GetStream()));
            return bntxFiles.SelectMany(f => f.Textures).ToList();
        }

        public static List<Texture> Load(string path)
        {
            return LoadFromResFile(new ResFile(path));
        }

        public static List<Texture> Load(Stream stream)
        {
            return LoadFromResFile(new ResFile(stream));
        }
    }
}
