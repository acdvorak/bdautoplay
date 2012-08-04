using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace BlurayAutoPlay
{
    class MediaPlayerClassic : IMediaPlayer
    {
        private static List<String> FILENAMES = new List<String>() {
            @"mplayerc.exe",
            @"mpc-hc.exe",
            @"mpc-hc64.exe"
        };
        private static List<String> FOLDERS = new List<String>() {
            @"MPC HomeCinema",
            @"Media Player Classic",
            @"Media Player Classic - Home Cinema",
            @"MPC-HC"
        };
        private static string PF64_1 = Environment.GetEnvironmentVariable("ProgramW6432");
        private static string PF64_2 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
        private static string PF32_1 = Environment.GetEnvironmentVariable("ProgramFiles");
        private static string PF32_2 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

        private string exePath = null;
        private string exeName = null;
        private HashSet<string> programFilesPaths = new HashSet<string>();

        public MediaPlayerClassic()
        {
            if (PF64_1 != null) programFilesPaths.Add(PF64_1);
            if (PF64_2 != null) programFilesPaths.Add(PF64_2);
            if (PF32_1 != null) programFilesPaths.Add(PF32_1);
            if (PF32_2 != null) programFilesPaths.Add(PF32_2);
        }

        public string GetPath() { return exePath; }
        public string GetName() { return exeName; }
        public string GetProgId() { return "MediaPlayerClassic.Autorun"; }
        public string GetHandlerName() { return "MPCPlayBluRayOnArrival"; }
        public string GetExeAutoplayArgs() { return @"%1 \BDMV\INDEX.BDMV"; }
        public string GetInitCmdLine() { return @"\\BDMV\\STREAM\"; }

        public bool IsInstalled()
        {
            MPCSearchResult searchResult;
            /*if ((searchResult = QuickSearch()) != null)
            {
                exePath = searchResult.Path;
                exeName = searchResult.Name;
                return true;
            }
            else */if ((searchResult = FullSearch()) != null)
            {
                exePath = searchResult.Path;
                exeName = searchResult.Name;
                return true;
            }
            return false;
        }

        private MPCSearchResult QuickSearch()
        {
            List<string> quickPaths = new List<string>();
            foreach (var programFilesPath in programFilesPaths)
            {
                foreach (var folder in FOLDERS)
                {
                    foreach (var filename in FILENAMES)
                    {
                        quickPaths.Add(programFilesPath + @"\" + folder + @"\" + filename);
                    }
                }
            }
            return Find(quickPaths);
        }

        private MPCSearchResult FullSearch()
        {
            HashSet<string> folders = new HashSet<string>();
            foreach (var programFilesPath in programFilesPaths)
            {
                foreach (var dir in Directory.GetDirectories(programFilesPath)) { folders.Add(dir); }
            }

            List<string> allPaths = new List<string>();
            foreach (var folder in folders)
            {
                foreach (var filename in FILENAMES)
                {
                    allPaths.Add(folder + @"\" + filename);
                    allPaths.Add(folder + @"\" + filename);
                }
            }
            return Find(allPaths);
        }

        private MPCSearchResult Find(ICollection<string> paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(path);
                    return new MPCSearchResult(path, fileInfo.FileDescription);
                }
            }
            return null;
        }
    }

    class MPCSearchResult
    {
        private string path;
        private string name;
        public MPCSearchResult(string path, string name)
        {
            this.path = path;
            this.name = name;
        }
        public string Path { get { return path; } }
        public string Name { get { return name; } }
    }
}
