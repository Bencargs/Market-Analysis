﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MarketAnalysis
{
    public static class DirectoryManager
    {
        private const int Max_File_Count = 10;
        private const int Max_File_Size = 1000000; // 1 MB

        public static string GetLatestReport()
        {
            const string extension = "html";
            var directory = Configuration.ReportsPath;
            var filename = $"Market Report {DateTime.Now.ToString("yyyy-MM-dd")}";

            return GetLatestFile(directory, extension, filename);
        }

        public static string GetLatestLogFile()
        {
            const string extension = "log";
            var directory = Configuration.LogPath;
            var filename = $"{DateTime.Now.ToString("yyyyMMddTHHmmss")}";

            return GetLatestFile(directory, extension, filename);
        }

        private static string GetLatestFile(string directory, string extension, string filename)
        {
            var files = GetFiles(directory, $"*.{extension}");
            if (files.Length > Max_File_Count)
                ArchiveFiles(files);

            var latestFile = files.FirstOrDefault();
            if (latestFile == null || latestFile.Length > Max_File_Size)
                latestFile = CreateNewFile(directory, filename, extension);

            return latestFile.FullName;
        }

        private static FileInfo CreateNewFile(string directory, string filename, string extension)
        {
            var filepath = Path.Combine(directory, $"{filename}.{extension}");
            for (var i = 1; ; i++)
            {
                if (!File.Exists(filepath))
                    break;

                filepath = Path.Combine(directory, $"{filename}({i}).{extension}");
            }

            using (File.Create(filepath))
                return new FileInfo(filepath);
        }

        private static FileInfo[] GetFiles(string directory, string searchPattern = "*.*")
        {
            return Directory.GetFiles(directory, searchPattern)
                   .Select(f => new FileInfo(f))
                   .Where(f => f.Extension != ".zip")
                   .ToArray();
        }

        private static void ArchiveFiles(FileInfo[] files)
        {
            var surplus = files
                .OrderByDescending(x => x.CreationTime)
                .Skip(Max_File_Count)
                .ToArray();

            foreach (var file in surplus)
            {
                var filename = file.FullName;
                using (var fileStream = new FileStream($"{filename}.zip", FileMode.CreateNew))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    var entry = archive.CreateEntryFromFile(filename, file.Name);
                }
                file.Delete();
            }
        }
    }
}