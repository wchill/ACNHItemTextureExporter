using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ACNHItemTextureExporter
{
    static class DirectoryHelper
    {
        // https://stackoverflow.com/a/3754470
        // Regex version
        public static IEnumerable<string> EnumerateFilesWithRegex(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption)
                            .Where(file => reSearchPattern.IsMatch(file));
        }
    }
}
