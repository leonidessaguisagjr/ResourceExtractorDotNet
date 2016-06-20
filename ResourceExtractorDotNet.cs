// <copyright file="ResourceExtractorDotNet.cs">
// Copyright (c) 2016 Leonides T. Saguisag, Jr.
//
// This file is part of ResourceExtractorDotNet.
//
// ResourceExtractorDotNet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// ResourceExtractorDotNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ResourceExtractorDotNet.  If not, see http://www.gnu.org/licenses/.
// </copyright>

namespace Name.LeonidesSaguisagJr.ResourceExtractorDotNet {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Class for extracting resources from a .NET assembly
    /// </summary>
    internal sealed class Program {

        private const string resourcesFileExt = ".resources";
        private const string resXFileExt = ".resx";

        private static string inputAssemblyName;
        private static string outputDirectory;

        private static TraceSource traceSource =
            new TraceSource("ResourceExtractorDotNet");

        private static readonly string appName =
            Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
        private static readonly Version appVersion =
            Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// Main entry point for the application
        /// </summary>
        /// <param name="args">Arguments passed to the application on the command line</param>
        [STAThread]
        internal static void Main(string[] args) {

            ShowHeaderScreen();
            ParseCommandLineArgs(args);

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve +=
                new ResolveEventHandler(ReflectionOnlyAssemblyResolveEventHandler);
            Assembly asm = null;
            try {
                traceSource.TraceEvent(TraceEventType.Information, 0,
                    string.Format(CultureInfo.CurrentCulture,
                                  "Reading assembly: {0}",
                                  inputAssemblyName));
                asm = Assembly.ReflectionOnlyLoadFrom(inputAssemblyName);
            } catch (FileNotFoundException) {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                Strings.FormatErrorInputFileDoesNotExist,
                                                inputAssemblyName));
                Environment.Exit(1);
            } catch (FileLoadException) {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                Strings.FormatErrorUnableToLoadAssembly,
                                                inputAssemblyName));
                Environment.Exit(1);
            } catch (BadImageFormatException) {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                Strings.FormatErrorNotValidNETAssembly,
                                                inputAssemblyName));
                Environment.Exit(1);
            }

            foreach (var referencedAssemblyName in asm.GetReferencedAssemblies()) {
                traceSource.TraceEvent(TraceEventType.Information, 1,
                    string.Format(CultureInfo.CurrentCulture,
                                  "Loading referenced assembly: {0}",
                                  referencedAssemblyName.Name));
                try {
                    Assembly.ReflectionOnlyLoad(referencedAssemblyName.FullName);
                } catch (FileNotFoundException) {
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                    Strings.FormatErrorUnableToLoadReferencedAssembly,
                                                    referencedAssemblyName.Name));
                } catch (FileLoadException) {
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                    Strings.FormatErrorUnableToLoadReferencedAssembly,
                                                    referencedAssemblyName.Name));
                } catch (BadImageFormatException) {
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                    Strings.FormatErrorUnableToLoadReferencedAssembly,
                                                    referencedAssemblyName.Name));
                }
            }

            if (!Directory.Exists(outputDirectory)) {
                traceSource.TraceEvent(TraceEventType.Information, 2,
                    string.Format(CultureInfo.CurrentCulture,
                                  "Output directory \"{0}\" does not exist and will be created.",
                                  outputDirectory));
                Directory.CreateDirectory(outputDirectory);
            } else {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                Strings.FormatWarningOutputDirectoryAlreadyExists,
                                                outputDirectory));
            }

            string[] resourceNames = asm.GetManifestResourceNames();
            if (0 == resourceNames.Length) {
                Console.WriteLine(Strings.NoResourcesFound);
                Environment.Exit(0);
            }
            foreach (string currentResourceName in resourceNames) {
                using (Stream resourceStream = asm.GetManifestResourceStream(currentResourceName)) {
                    if (currentResourceName.EndsWith(resourcesFileExt, StringComparison.OrdinalIgnoreCase)) {
                        string outputResXFilename = Path.ChangeExtension(currentResourceName, resXFileExt);
                        outputResXFilename = Path.Combine(outputDirectory, outputResXFilename);
                        traceSource.TraceEvent(TraceEventType.Information, 3,
                            string.Format(CultureInfo.CurrentCulture,
                                          "Extracting resource: {0}",
                                          outputResXFilename));
                        ExtractToResXFile(resourceStream, outputResXFilename);
                    } else {
                        string outputResourceName = Path.Combine(outputDirectory, currentResourceName);
                        traceSource.TraceEvent(TraceEventType.Information, 3,
                            string.Format(CultureInfo.CurrentCulture,
                                          "Extracting resource: {0}",
                                          outputResourceName));
                        ExtractToBinaryFile(resourceStream, outputResourceName);
                    }
                }
            }
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                            Strings.FormatTotalExtracted,
                                            resourceNames.Length,
                                            inputAssemblyName,
                                            outputDirectory));
        }

        /// <summary>
        /// Method for reading command-line arguments and populating the
        /// appropriate fields.
        /// </summary>
        private static void ParseCommandLineArgs(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine(Strings.ErrorMissingUnrecognizedArgs);
                ShowUsageScreen();
                Environment.Exit(1);
            } else {
                inputAssemblyName = Path.GetFullPath(args[0]);
                outputDirectory = Path.GetFullPath(args[1]);
            }
            Debug.Assert(!string.IsNullOrEmpty(inputAssemblyName), "Missing assembly name");
            Debug.Assert(!string.IsNullOrEmpty(outputDirectory), "Missing assembly name");
        }

        /// <summary>
        /// Method for displaying a header screen on the console.
        /// </summary>
        private static void ShowHeaderScreen() {
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                            Strings.FormatAppNameAndVersion,
                                            appName,
                                            appVersion.Major,
                                            appVersion.Minor));
            var assemblyCopyrightAttribute =
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>();
            Console.WriteLine(assemblyCopyrightAttribute.Copyright);
        }

        /// <summary>
        /// Method for displaying a usage and examples screen on the console.
        /// </summary>
        private static void ShowUsageScreen() {
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                            Strings.FormatUsage,
                                            appName));
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                            Strings.FormatExample,
                                            appName));
        }

        /// <summary>
        /// Method for extracting the embedded resource to a binary file.
        /// </summary>
        /// <param name="resourceStream">Stream of the embedded resource to extract.</param>
        /// <param name="outputFilename">The filename of the output binary file.</param>
        private static void ExtractToBinaryFile(Stream resourceStream, string outputFilename) {
            Debug.Assert(resourceStream != null, "Received null resourceStream");
            const int BUFFERSIZE = 8192;
            using (var writer = new FileStream(outputFilename, FileMode.Create, FileAccess.Write)) {
                byte[] buffer = new byte[BUFFERSIZE];
                int numBytesRead = resourceStream.Read(buffer, 0, BUFFERSIZE);
                while (numBytesRead > 0) {
                    writer.Write(buffer, 0, numBytesRead);
                    numBytesRead = resourceStream.Read(buffer, 0, BUFFERSIZE);
                }
            }
        }

        /// <summary>
        /// Method for extracting the embedded resource to a .resx (XML resource) file.
        /// </summary>
        /// <param name="resourceStream">Stream of the embedded resource to extract.</param>
        /// <param name="outputFilename">The filename of the output binary file.</param>
        private static void ExtractToResXFile(Stream resourceStream, string outputFilename) {
            Debug.Assert(resourceStream != null, "Received null resourceStream");
            using (var reader = new ResourceReader(resourceStream)) {
                using (var writer = new ResXResourceWriter(outputFilename)) {
                    foreach (DictionaryEntry entry in reader) {
                        writer.AddResource(entry.Key.ToString(), entry.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for ReflectionOnlyAssemblyResolve.
        /// </summary>
        /// <param name="sender">Object that raised the event</param>
        /// <param name="args">event data</param>
        /// <returns>
        /// The loaded assmembly.  If any errors were encountered, returns null.
        /// </returns>
        private static Assembly ReflectionOnlyAssemblyResolveEventHandler(object sender, ResolveEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Information, 1,
                string.Format(CultureInfo.CurrentCulture, "Loading referenced assembly: {0}", args.Name));
            var asm = Assembly.ReflectionOnlyLoad(args.Name);
            if (null == asm) {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                                Strings.FormatErrorUnableToLoadReferencedAssembly,
                                                args.Name));
            }
            return asm;
        }
    }
}
