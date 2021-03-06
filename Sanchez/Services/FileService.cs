﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Globbing;
using Sanchez.Models;

namespace Sanchez.Services
{
    public interface IFileService
    {
        /// <summary>
        ///     Creates the target directory if required.
        /// </summary>
        void PrepareOutput(CommandLineOptions options);

        /// <summary>
        ///     Returns the output filename, based on whether we are processing a single or multiple source files. For multiple source
        ///     files, the output filename is the source filename with a <see cref="FileService.BatchFileSuffix"/> suffix.
        /// </summary>
        string GetOutputFilename(CommandLineOptions options, string sourceFile);

        /// <summary>
        ///     Returns a list of files to process, based on <see cref="CommandLineOptions.SourcePath"/>. This property
        ///     can be a single file, a directory or a glob and wildcard pattern (such as <c>source/**/*IR.jpg</c>)
        /// </summary>
        List<string> GetSourceFiles(CommandLineOptions options);
    }

    internal class FileService : IFileService
    {
        /// <summary>
        ///     Suffix applied to filenames when converting files in bulk.
        /// </summary>
        private const string BatchFileSuffix = "-fc";

        /// <summary>
        ///     Creates the target directory if required.
        /// </summary>
        public void PrepareOutput(CommandLineOptions options)
        {
            if (options.IsBatch && !Directory.Exists(options.OutputPath!))
            {
                Directory.CreateDirectory(options.OutputPath!);
            }
        }

        /// <summary>
        ///     Returns the output filename, based on whether we are processing a batch. For batches, , the output
        ///     filename is the source filename with a <see cref="BatchFileSuffix"/> suffix.
        /// </summary>
        public string GetOutputFilename(CommandLineOptions options, string sourceFile)
        {
            return options.IsBatch
                ? Path.Combine(options.OutputPath!, $"{Path.GetFileNameWithoutExtension(sourceFile)}{BatchFileSuffix}{Path.GetExtension(sourceFile)}"!)
                : options.OutputPath!;
        }

        /// <summary>
        ///     Returns a list of files to process, based on <see cref="CommandLineOptions.SourcePath"/>. This property
        ///     can be a single file, a directory or a glob and wildcard pattern (such as <c>source/**/*IR.jpg</c>)
        /// </summary>
        public List<string> GetSourceFiles(CommandLineOptions options)
        {
            var absolutePath = Path.GetFullPath(options.SourcePath!);

            // Source is a single file
            if (!options.IsBatch) return new List<string> { absolutePath };

            // If the source is a directory, enumerate all files
            if (Directory.Exists(absolutePath))
            {
                return Directory
                    .GetFiles(absolutePath, "*.*", SearchOption.AllDirectories)
                    .OrderBy(file => file)
                    .ToList();
            }

            // Source is a glob, so enumerate all files in its base directory directory and return
            // glob matches
            var sourceGlob = Glob.Parse(absolutePath);

            return Directory
                .GetFiles(GetGlobBase(absolutePath), "*.*", SearchOption.AllDirectories)
                .Where(file => sourceGlob.IsMatch(file))
                .OrderBy(file => file)
                .ToList();
        }

        private static string GetGlobBase(string path)
        {
            // Normalise separators
            path = path.Replace('\\', '/');

            // Extract all directories in the path prior to the glob pattern. Note that the glob library
            // also supports [a-z] style ranges, however we don't.
            var directorySegments = path
                .Split('/')
                .TakeWhile(segment => !segment.Contains('?') && !segment.Contains('*'))
                .ToList();

            // Recombine path
            return string.Join(Path.DirectorySeparatorChar, directorySegments);
        }
    }
}