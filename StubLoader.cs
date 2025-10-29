using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SimPacker
{
    /// <summary>
    /// Stub loader template that will be compiled and embedded with the packed payload
    /// This creates a self-extracting/self-executing packed file
    /// </summary>
    internal class StubLoader
    {
        /// <summary>
        /// Generates a standalone executable with embedded packed payload
        /// </summary>
        public static bool CreateStandaloneExecutable(string originalFilePath,
        byte[] packedData,
        byte[] key,
        byte[] iv,
        string outputPath,
        out string compilerOutput,
        bool obfuscation)
        {
            compilerOutput = string.Empty;
            try
            {
                // Create stub loader source code with embedded data
                string stubCode = GenerateStubCode(packedData, key, iv, obfuscation);

                // Save stub source to temporary file
                string tempStubPath = Path.Combine(Path.GetTempPath(), "stub_loader.cs");
                File.WriteAllText(tempStubPath, stubCode);

                // Compile the stub into an executable
                var result = CompileStub(tempStubPath, outputPath, originalFilePath);
                compilerOutput = result.output;
                bool compiled = result.success;

                // Clean up temporary file
                try { File.Delete(tempStubPath); } catch { }

                return compiled;
            }
            catch (Exception ex)
            {
                compilerOutput = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Generates the stub loader C# source code with embedded payload.
        /// If obfuscation is true, splits key/iv into fragments and reconstructs them in code.
        /// </summary>
        private static string GenerateStubCode(byte[] packedData, byte[] key, byte[] iv, bool obfuscation)
        {
            string packedDataBase64 = Convert.ToBase64String(packedData);
            string keyBase64 = Convert.ToBase64String(key);
            string ivBase64 = Convert.ToBase64String(iv);

            var lines = new System.Collections.Generic.List<string>
            {
                "using System;",
                "using System.Diagnostics;",
                "using System.IO;",
                "using System.IO.Compression;",
                "using System.Security.Cryptography;",
                "",
                "namespace StubLoader",
                "{",
                "class Program",
                "{",
                $"private const string PACKED_DATA = \"{packedDataBase64}\";"
            };

            if (obfuscation)
            {
                int fragmentLength = 16;
                var keyFragments = Enumerable.Range(0, (keyBase64.Length + fragmentLength - 1) / fragmentLength)
                    .Select(i => keyBase64.Substring(i * fragmentLength, Math.Min(fragmentLength, keyBase64.Length - i * fragmentLength)))
                    .ToArray();
                var ivFragments = Enumerable.Range(0, (ivBase64.Length + fragmentLength - 1) / fragmentLength)
                    .Select(i => ivBase64.Substring(i * fragmentLength, Math.Min(fragmentLength, ivBase64.Length - i * fragmentLength)))
                    .ToArray();

                lines.AddRange(keyFragments.Select((frag, idx) => $"private const string KEY_FRAG_{idx} = \"{frag}\";"));
                lines.AddRange(ivFragments.Select((frag, idx) => $"private const string IV_FRAG_{idx} = \"{frag}\";"));
            }
            else
            {
                lines.Add($"private const string KEY = \"{keyBase64}\";");
                lines.Add($"private const string IV = \"{ivBase64}\";");
            }

            lines.Add("");
            lines.Add("static int Main(string[] args)");
            lines.Add("{");
            lines.Add("// Credit header");
            lines.Add("Console.WriteLine(\"========================================\");");
            lines.Add(obfuscation
                ? "Console.WriteLine(\"Packed by SimPacker (obfuscation)\");"
                : "Console.WriteLine(\"Packed by SimPacker\");");
            lines.Add("Console.WriteLine(\"https://github.com/ptg14/SimPacker.git\");");
            lines.Add("Console.WriteLine(\"Educational purpose only!\");");
            lines.Add("Console.WriteLine(\"========================================\");");
            lines.Add("");
            lines.Add("try");
            lines.Add("{");
            lines.Add("byte[] packedData = Convert.FromBase64String(PACKED_DATA);");

            if (obfuscation)
            {
                string keyReconstruct = $"string keyBase64 = string.Concat({string.Join(", ", Enumerable.Range(0, (keyBase64.Length + 15) / 16).Select(idx => $"KEY_FRAG_{idx}"))});";
                string ivReconstruct = $"string ivBase64 = string.Concat({string.Join(", ", Enumerable.Range(0, (ivBase64.Length + 15) / 16).Select(idx => $"IV_FRAG_{idx}"))});";
                lines.Add(keyReconstruct);
                lines.Add(ivReconstruct);
                lines.Add("byte[] key = Convert.FromBase64String(keyBase64);");
                lines.Add("byte[] iv = Convert.FromBase64String(ivBase64);");
            }
            else
            {
                lines.Add("byte[] key = Convert.FromBase64String(KEY);");
                lines.Add("byte[] iv = Convert.FromBase64String(IV);");
            }

            lines.Add("");
            lines.Add("byte[] peData = UnpackPE(packedData, key, iv);");
            lines.Add("");
            lines.Add("if (peData.Length < 64 || peData[0] != 0x4D || peData[1] != 0x5A)");
            lines.Add("{");
            lines.Add("Console.WriteLine(\"Error: Invalid PE file after unpacking\");");
            lines.Add("return 1;");
            lines.Add("}");
            lines.Add("");
            lines.Add("string tempExePath = Path.Combine(Path.GetTempPath(), \"unpacked_\" + Guid.NewGuid().ToString() + \".exe\");");
            lines.Add("File.WriteAllBytes(tempExePath, peData);");
            lines.Add("");
            lines.Add("var psi = new ProcessStartInfo");
            lines.Add("{");
            lines.Add("FileName = tempExePath,");
            lines.Add("Arguments = string.Join(\" \", args),");
            lines.Add("UseShellExecute = false");
            lines.Add("};");
            lines.Add("");
            lines.Add("var proc = Process.Start(psi);");
            lines.Add("proc.WaitForExit();");
            lines.Add("int exitCode = proc.ExitCode;");
            lines.Add("");
            lines.Add("try { File.Delete(tempExePath); } catch { }");
            lines.Add("");
            lines.Add("return exitCode;");
            lines.Add("}");
            lines.Add("catch (Exception ex)");
            lines.Add("{");
            lines.Add("Console.WriteLine(\"Error: \" + ex.Message);");
            lines.Add("return 1;");
            lines.Add("}");
            lines.Add("}");

            // UnpackPE function
            lines.Add("");
            if (obfuscation)
            {
                lines.Add("private static byte[] UnpackPE(byte[] packedData, byte[] key, byte[] iv)");
                lines.Add("{");
                lines.Add("using (SHA256 sha = SHA256.Create())");
                lines.Add("{");
                lines.Add("byte[] hash = sha.ComputeHash(packedData);");
                lines.Add("byte[] origKey = new byte[key.Length];");
                lines.Add("byte[] origIv = new byte[iv.Length];");
                lines.Add("for (int i = 0; i < key.Length; i++) origKey[i] = (byte)(key[i] ^ hash[i % hash.Length]);");
                lines.Add("for (int i = 0; i < iv.Length; i++) origIv[i] = (byte)(iv[i] ^ hash[(i + key.Length) % hash.Length]);");
                lines.Add("using (Aes aes = Aes.Create())");
                lines.Add("{");
                lines.Add("aes.Key = origKey;");
                lines.Add("aes.IV = origIv;");
                lines.Add("using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))");
                lines.Add("using (var msDecrypt = new MemoryStream(packedData))");
                lines.Add("using (var cs = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))");
                lines.Add("using (var msOut = new MemoryStream())");
                lines.Add("{");
                lines.Add("cs.CopyTo(msOut);");
                lines.Add("return Decompress(msOut.ToArray());");
                lines.Add("}");
                lines.Add("}");
                lines.Add("}");
                lines.Add("}");
            }
            else
            {
                lines.Add("private static byte[] UnpackPE(byte[] packedData, byte[] key, byte[] iv)");
                lines.Add("{");
                lines.Add("using (Aes aes = Aes.Create())");
                lines.Add("{");
                lines.Add("aes.Key = key;");
                lines.Add("aes.IV = iv;");
                lines.Add("using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))");
                lines.Add("using (var msDecrypt = new MemoryStream(packedData))");
                lines.Add("using (var cs = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))");
                lines.Add("using (var msOut = new MemoryStream())");
                lines.Add("{");
                lines.Add("cs.CopyTo(msOut);");
                lines.Add("return Decompress(msOut.ToArray());");
                lines.Add("}");
                lines.Add("}");
                lines.Add("}");
            }

            lines.Add("");
            lines.Add("private static byte[] Decompress(byte[] data)");
            lines.Add("{");
            lines.Add("using (var input = new MemoryStream(data))");
            lines.Add("using (var output = new MemoryStream())");
            lines.Add("using (var gzip = new GZipStream(input, CompressionMode.Decompress))");
            lines.Add("{");
            lines.Add("gzip.CopyTo(output);");
            lines.Add("return output.ToArray();");
            lines.Add("}");
            lines.Add("}");
            lines.Add("}");
            lines.Add("}");

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Compiles the stub loader using Roslyn or csc.exe
        /// </summary>
        private static (bool success, string output) CompileStub(string stubSourcePath, string outputPath, string originalFilePath)
        {
            try
            {
                // Try to use csc.exe from .NET SDK
                string cscPath = FindCscCompiler();

                if (string.IsNullOrEmpty(cscPath))
                {
                    string msg = "C# compiler not found. Install .NET SDK or Visual Studio.";
                    return (false, msg);
                }

                // Get subsystem type from original file (Console vs Windows)
                bool isWindowsApp = DetermineIsWindowsApp(originalFilePath);
                Console.WriteLine($" Target: {(isWindowsApp ? "Windows GUI" : "Console")} application");

                // Build compiler arguments based on subsystem
                string arguments;
                bool isDotnetCommand = cscPath.Contains("dotnet.exe");

                if (isWindowsApp)
                {
                    arguments = $"/target:winexe /platform:anycpu /optimize+ /out:\"{outputPath}\" /nologo \"{stubSourcePath}\"";
                }
                else
                {
                    arguments = $"/target:exe /platform:anycpu /optimize+ /out:\"{outputPath}\" /nologo \"{stubSourcePath}\"";
                }

                string fileName;
                string processArguments;

                if (isDotnetCommand)
                {
                    // For dotnet command with csc.dll
                    string[] parts = cscPath.Split(new[] { " \"" }, StringSplitOptions.None);
                    fileName = parts[0]; // dotnet.exe path
                    string cscDllPath = parts[1].TrimEnd('"'); // csc.dll path
                    processArguments = $"exec \"{cscDllPath}\" {arguments}";
                }
                else
                {
                    // For direct csc.exe
                    fileName = cscPath;
                    processArguments = arguments;
                }

                var startTime = DateTime.Now;
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = processArguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                var duration = DateTime.Now - startTime;

                string combined = $"=== stdout ===\n{output}\n=== stderr ===\n{errors}\n";

                if (process.ExitCode != 0)
                {
                    combined = $"Compilation failed (exit code {process.ExitCode}) in {duration.TotalSeconds:F1}s\n" + combined;
                    return (false, combined);
                }

                if (!File.Exists(outputPath))
                {
                    combined = "Compilation finished but output file not created." + "\n" + combined;
                    return (false, combined);
                }

                combined = $"Compilation successful in {duration.TotalSeconds:F1}s\n" + combined;
                return (true, combined);
            }
            catch (Exception ex)
            {
                return (false, ex.ToString());
            }
        }

        /// <summary>
        /// Finds the C# compiler (csc.exe) on the system
        /// </summary>eturn
        private static string FindCscCompiler()
        {
            // Priority order:
            // 1. Visual Studio Roslyn (best compatibility)
            // 2. .NET Framework (fallback)
            // 3. .NET SDK (last resort, requires special handling)

            // Try Visual Studio compilers first (most reliable)
            string[] vsCompilerPaths = new[]
           {
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe",
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\Roslyn\csc.exe",
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"
            };

            foreach (string path in vsCompilerPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Try .NET Framework compilers
            string[] frameworkPaths = new[]
            {
            @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
            @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
            };

            foreach (string path in frameworkPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Last resort: Try .NET SDK (requires dotnet exec)
            string[] sdkPaths = new[]
            {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\sdk"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"dotnet\sdk")
            };

            foreach (string sdkPath in sdkPaths)
            {
                if (Directory.Exists(sdkPath))
                {
                    // Find the latest SDK version
                    var versions = Directory.GetDirectories(sdkPath)
                           .Select(d => new DirectoryInfo(d).Name)
                      .Where(v => Version.TryParse(v.Split('-')[0], out _))
                       .OrderByDescending(v => v)
                               .ToArray();

                    foreach (var version in versions)
                    {
                        string cscDllPath = Path.Combine(sdkPath, version, "Roslyn", "bincore", "csc.dll");
                        if (File.Exists(cscDllPath))
                        {
                            // Find dotnet.exe
                            string dotnetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\dotnet.exe");
                            if (!File.Exists(dotnetPath))
                                dotnetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"dotnet\dotnet.exe");

                            if (File.Exists(dotnetPath))
                            {
                                // Return in format: "dotnet.exe path" "csc.dll path"
                                return $"{dotnetPath} \"{cscDllPath}\"";
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines if the original file is Console or Windows subsystem
        /// </summary>
        private static bool DetermineIsWindowsApp(string filePath)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                int peOffset = BitConverter.ToInt32(fileData, 60);
                int subsystemOffset = peOffset + 4 + 20 + 68; // PE + FileHeader + Optional.Subsystem
                ushort subsystem = BitConverter.ToUInt16(fileData, subsystemOffset);

                // 2 = Windows GUI, 3 = Console
                return subsystem == 2;
            }
            catch
            {
                return false; // Default to console
            }
        }
    }
}
