using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mono.Cecil;

public static class Patcher
{
    public static IEnumerable<string> TargetDLLs { get; } = new[] {"Assembly-CSharp.dll"};

    public static void Patch(AssemblyDefinition assembly)
    {
    }

    public static void Initialize()
    {
        Console.WriteLine("Patching Patching StanleyVR...");
        
        var patcherAssemblyPath = Assembly.GetExecutingAssembly().Location;
        Console.WriteLine("installerPath " + patcherAssemblyPath);

        var gameExePath = Process.GetCurrentProcess().MainModule?.FileName;
        Console.WriteLine("gameExePath " + gameExePath);

        var gameFolderPath = Path.GetDirectoryName(gameExePath);
        var patcherFolderPath = Path.GetDirectoryName(patcherAssemblyPath);

        if (patcherFolderPath == null)
        {
            throw new DirectoryNotFoundException(
                $"Failed to find patcherFolderPath in patcherAssemblyPath ({patcherAssemblyPath})");
        }
        
        var copyFolderPath = Path.Combine(patcherFolderPath, "CopyToGame");
        CopyDirectory(copyFolderPath, gameFolderPath, true);
    }
    
    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        Console.WriteLine($"Copying files from {sourceDir} to {destinationDir} {(recursive ? "recursively" : "non-recursively")}");
        
        var directoryInfo = new DirectoryInfo(sourceDir);

        if (!directoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {directoryInfo.FullName}");
        }
        
        var directories = directoryInfo.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (var file in directoryInfo.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        if (!recursive) return;

        foreach (var subDir in directories)
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}