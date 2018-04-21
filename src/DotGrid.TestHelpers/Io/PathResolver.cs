using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;

namespace DotGrid.TestHelpers.Io
{
    public static class PathResolver
    {
        private static readonly string Up = "../";

        public static string ApplicationRoot
        {
            get
            {            
                var exePath =   Path.GetDirectoryName(System.Reflection
                    .Assembly.GetExecutingAssembly().CodeBase);
                Regex appPathMatcher=new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
         
                var appRoot = appPathMatcher.Match(exePath).Value;
                return appRoot;             
            }
        }
        
        public static string ResolveUpward(string startPath, string pathToFind)
        {
            var increment = Up;
            var fullPath = Path.GetFullPath(Path.Combine(startPath, pathToFind));

            while (!IsRootDirectory(fullPath) && !Exists(fullPath))
            {
                fullPath = Path.GetFullPath(Path.Combine(startPath, increment, pathToFind));
                increment = increment + Up;
            }

            if (!Exists(fullPath))
            {
                throw new FileNotFoundException("Unable to resolve path", pathToFind);
            }

            return fullPath;
        }
        
        public static bool Exists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        public static bool IsRootDirectory(string path)
        {
            var directoryPath = Path.GetFullPath(Path.GetDirectoryName(path));
            
            return new DirectoryInfo(directoryPath).Parent == null;
        }
    }
}