﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Git
{
    [InitializeOnLoad]
    public class Git
    {
        public static DateTime LastStatusUpdate { get; private set; }
        public static DateTime LastLogUpdate { get; private set; }

        public static async Task<string> Status()
        {
            LastStatusUpdate = DateTime.Now;
            Process process = await ProcessAsync(GitSettings.GitPath, "status");
            if (process?.StandardError.ReadLine() is string line && line.StartsWith("fatal: not a git repository"))
            {
                throw new Exception("Unable to find Git repository.");
            }

            return process?.StandardOutput.ReadToEnd();
        }

        public static async Task<string> Log()
        {
            Process process = await ProcessAsync(GitSettings.GitPath, "log --pretty=oneline");

            LastLogUpdate = DateTime.Now;

            return process?.StandardOutput.ReadToEnd();
        }

        public static Task<Process> ProcessAsync(string path, string arguments)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                WorkingDirectory = GitSettings.RepositoryPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            return Task.Run(() => Process.Start(processInfo));
        }
    }
}
