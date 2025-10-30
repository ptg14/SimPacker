using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;

namespace SimPacker
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private string? selectedFilePath;

        private void AppendColoredText(string text, Color color)
        {
            RTB_log.SelectionStart = RTB_log.TextLength;
            RTB_log.SelectionLength = 0;
            RTB_log.SelectionColor = color;
            RTB_log.AppendText(text);
            RTB_log.SelectionColor = RTB_log.ForeColor; // Reset to default
        }

        public static bool IsValidPE(byte[] data)
        {
            if (data.Length < 64)
                return false;

            // Check DOS signature "MZ"
            if (data[0] != 0x4D || data[1] != 0x5A)
                return false;

            // Get PE header offset
            int peOffset = BitConverter.ToInt32(data, 60);
            if (peOffset < 0 || peOffset + 4 >= data.Length)
                return false;

            // Check PE signature "PE\0\0"
            if (data[peOffset] != 0x50 || data[peOffset + 1] != 0x45 ||
                data[peOffset + 2] != 0x00 || data[peOffset + 3] != 0x00)
                return false;

            return true;
        }

        private void BT_open_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(selectedFilePath);
                // Measure text width and truncate if needed
                Size textSize = TextRenderer.MeasureText(fileName, LB_open.Font);
                if (textSize.Width > LB_open.Width)
                {
                    // Calculate how many characters fit
                    int maxLength = fileName.Length;
                    while (maxLength > 0)
                    {
                        string truncated = $"{fileName[..maxLength]}...";
                        if (TextRenderer.MeasureText(truncated, LB_open.Font).Width <= LB_open.Width)
                        {
                            LB_open.Text = truncated;
                            break;
                        }
                        maxLength--;
                    }
                }
                else
                {
                    LB_open.Text = fileName;
                }

                // Add tooltip to show full filename
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(LB_open, fileName);

                AppendColoredText($"File selected: {selectedFilePath}\n", Color.Green);

                // Try to load icon if it's an executable
                try
                {
                    Icon icon = Icon.ExtractAssociatedIcon(selectedFilePath);
                    if (icon != null)
                    {
                        P_icon.Image = icon.ToBitmap();
                        P_icon.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                catch
                {
                    P_icon.Image = null;
                }
            }
        }

        private void BT_pack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                AppendColoredText("Error: No file selected. Please open a file first.\n", Color.Red);
                LB_pack.ForeColor = Color.Red;
                LB_pack.Text = "Failed";
                return;
            }

            if (!File.Exists(selectedFilePath))
            {
                AppendColoredText($"Error: File not found: {selectedFilePath}\n", Color.Red);
                LB_pack.ForeColor = Color.Red;
                LB_pack.Text = "Failed";
                return;
            }

            // Use the selected file for packing
            AppendColoredText($"Packing file: {selectedFilePath}\n", Color.Blue);
            LB_pack.Text = "Packing...";

            try
            {
                if (packFile(selectedFilePath))
                {
                    LB_pack.ForeColor = Color.Green;
                    LB_pack.Text = "Packed";
                    AppendColoredText("Packing completed successfully.\n", Color.Green);
                }
                else
                {
                    LB_pack.ForeColor = Color.Red;
                    LB_pack.Text = "Failed";
                    AppendColoredText("Packing failed.\n", Color.Red);
                }

            }
            catch (Exception ex)
            {
                LB_pack.ForeColor = Color.Red;
                LB_pack.Text = "Failed";
                AppendColoredText($"Error during packing: {ex.Message}\n", Color.Red);
            }
        }

        private void BT_unPack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                AppendColoredText("Error: No file selected. Please open a file first.\n", Color.Red);
                LB_unPack.ForeColor = Color.Red;
                LB_unPack.Text = "Failed";
                return;
            }

            if (!File.Exists(selectedFilePath))
            {
                AppendColoredText($"Error: File not found: {selectedFilePath}\n", Color.Red);
                LB_unPack.ForeColor = Color.Red;
                LB_unPack.Text = "Failed";
                return;
            }

            // Use the selected file for unpacking
            AppendColoredText($"Unpacking file: {selectedFilePath}\n", Color.Blue);
            LB_unPack.Text = "Unpacking...";

            try
            {
                //if (unPackFile(selectedFilePath))
                if (unPackFile(selectedFilePath))
                {
                    LB_unPack.ForeColor = Color.Green;
                    LB_unPack.Text = "Unpacked";
                    AppendColoredText("Unpacking completed successfully.\n", Color.Green);
                }
                else
                {
                    LB_unPack.ForeColor = Color.Red;
                    LB_unPack.Text = "Failed";
                    AppendColoredText("Unpacking failed.\n", Color.Red);
                }

            }
            catch (Exception ex)
            {
                LB_unPack.ForeColor = Color.Red;
                LB_unPack.Text = "Failed";
                AppendColoredText($"Error during unpacking: {ex.Message}\n", Color.Red);
            }
        }

        private bool packFile(string filePath)
        {
            try
            {
                PB_loading.Value = 0;
                // Read the original file
                AppendColoredText($"[1/5] Reading file...\n", Color.Blue);
                byte[] fileData = File.ReadAllBytes(filePath);
                AppendColoredText($"    Read {fileData.Length:N0} bytes\n", Color.Black);

                // Validate if it's a PE file
                if (!IsValidPE(fileData))
                {
                    AppendColoredText("Error: Not a valid PE file (MZ signature not found)\n", Color.Red);
                    return false;
                }
                AppendColoredText($"    Valid PE file detected\n", Color.Black);
                PB_loading.Value = 20;

                // Pack the file (compress and encrypt)
                AppendColoredText($"[2/5] Compressing and encrypting...\n", Color.Blue);
                byte[] packedData = PackPE(fileData, out byte[] key, out byte[] iv, CB_obfuscation.Checked);

                double compressionRatio = (1 - (double)packedData.Length / fileData.Length) * 100;
                AppendColoredText($"    Compressed: {fileData.Length:N0} -> {packedData.Length:N0} bytes ({compressionRatio:F1}% reduction)\n", Color.Black);
                AppendColoredText($"    Encrypted with AES-256 ({key.Length * 8}-bit key)\n", Color.Black);

                // Create self-executing packed file
                string outputPath = Path.Combine(
                Path.GetDirectoryName(filePath) ?? "",
                Path.GetFileNameWithoutExtension(filePath) + "_packed.exe");
                PB_loading.Value = 40;

                AppendColoredText($"[3/5] Generating stub loader code...\n", Color.Blue);
                AppendColoredText($"    Embedding {packedData.Length:N0} bytes as Base64\n", Color.Black);
                AppendColoredText($"    Output: {Path.GetFileName(outputPath)}\n", Color.Black);
                PB_loading.Value = 60;

                // Redirect console output to capture compilation logs
                var originalOutput = Console.Out;
                var originalError = Console.Error;
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer);
                    Console.SetError(writer);

                    bool success = StubLoader.CreateStandaloneExecutable(
                    filePath,
                    packedData,
                    key,
                    iv,
                    outputPath,
                    out string compilerOutputFromStub,
                    CB_obfuscation.Checked);

                    Console.SetOut(originalOutput);
                    Console.SetError(originalError);

                    AppendColoredText($"[4/5] Compiling stub loader...\n", Color.Blue);
                    string compilerOutput = writer.ToString();

                    PB_loading.Value = 80;

                    // Prefer the stub returned compiler output if available
                    if (!string.IsNullOrWhiteSpace(compilerOutputFromStub))
                    {
                        compilerOutput = compilerOutputFromStub;
                    }

                    if (success && File.Exists(outputPath))
                    {
                        FileInfo outputFile = new FileInfo(outputPath);
                        AppendColoredText($"[5/5] Finalizing...\n", Color.Blue);
                        AppendColoredText($"    Compilation successful\n", Color.Black);
                        AppendColoredText($"    Final size: {outputFile.Length:N0} bytes\n", Color.Black);

                        AppendColoredText("\nSUCCESS - Packed file created\n", Color.Green);
                        AppendColoredText($"\nLocation: {outputPath}\n", Color.Blue);
                        AppendColoredText($"Size comparison:\n", Color.Blue);
                        AppendColoredText($"    Original: {fileData.Length:N0} bytes\n", Color.Black);
                        AppendColoredText($"    Packed: {outputFile.Length:N0} bytes\n", Color.Black);
                        AppendColoredText($"    Overhead: +{outputFile.Length - fileData.Length:N0} bytes (stub loader)\n", Color.Black);
                        PB_loading.Value = 100;

                        return true;
                    }
                    else
                    {
                        AppendColoredText("COMPILATION FAILED\n", Color.Red);
                        AppendColoredText($"Output was not created: {outputPath}\n", Color.Red);

                        // Show full compiler output if available to help diagnose
                        if (!string.IsNullOrWhiteSpace(compilerOutput))
                        {
                            AppendColoredText("\nFull compiler output:\n", Color.Black);
                            AppendColoredText(compilerOutput + "\n", Color.Black);
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                AppendColoredText($"\n==================================================\n", Color.Red);
                AppendColoredText($"EXCEPTION OCCURRED\n", Color.Red);
                AppendColoredText($"==================================================\n", Color.Red);
                AppendColoredText($"\nError: {ex.GetType().Name}\n", Color.Red);
                AppendColoredText($"Message: {ex.Message}\n", Color.Red);
                AppendColoredText($"\nStack Trace:\n", Color.DarkRed);

                foreach (var line in ex.StackTrace?.Split('\n') ?? Array.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        AppendColoredText($"  {line.Trim()}\n", Color.DarkRed);
                }

                return false;
            }
        }

        private bool unPackFile(string filePath)
        {
            try
            {
                PB_loading.Value = 0;
                AppendColoredText($"[1/4] Loading packed executable...\n", Color.Blue);

                // Load the packed assembly
                var asm = Assembly.LoadFile(filePath);

                // Find the type containing the constants (StubLoader.Program)
                var stubType = asm.GetType("StubLoader.Program");
                if (stubType == null)
                {
                    AppendColoredText("Error: StubLoader.Program type not found in packed file.\n", Color.Red);
                    return false;
                }

                // Try to get packed data, key, and IV (support both direct and fragment fields)
                string packedDataBase64 = stubType.GetField("PACKED_DATA", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null) as string;
                string keyBase64 = stubType.GetField("KEY", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null) as string;
                string ivBase64 = stubType.GetField("IV", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null) as string;
                bool obfuscation = false;

                // If KEY/IV are missing, reconstruct from fragments
                if (keyBase64 == null || ivBase64 == null)
                {
                    AppendColoredText($"    Detected obfuscation\n", Color.Black);
                    var keyFrags = stubType.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .Where(f => f.Name.StartsWith("KEY_FRAG_"))
                        .OrderBy(f => f.Name)
                        .Select(f => f.GetValue(null) as string)
                        .ToArray();
                    keyBase64 = string.Concat(keyFrags);

                    var ivFrags = stubType.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .Where(f => f.Name.StartsWith("IV_FRAG_"))
                        .OrderBy(f => f.Name)
                        .Select(f => f.GetValue(null) as string)
                        .ToArray();
                    ivBase64 = string.Concat(ivFrags);
                    obfuscation = true;
                }

                if (packedDataBase64 == null || string.IsNullOrEmpty(keyBase64) || string.IsNullOrEmpty(ivBase64))
                {
                    AppendColoredText("Error: Could not extract packed data, key, or IV from packed executable.\n", Color.Red);
                    return false;
                }

                PB_loading.Value = 25;
                AppendColoredText($"[2/4] Extracted packed data, key, and IV.\n", Color.Blue);

                byte[] packedData = Convert.FromBase64String(packedDataBase64);
                byte[] key = Convert.FromBase64String(keyBase64);
                byte[] iv = Convert.FromBase64String(ivBase64);

                PB_loading.Value = 50;
                AppendColoredText($"[3/4] Decrypting and decompressing...\n", Color.Blue);

                byte[] peData = UnpackPE(packedData, key, iv, obfuscation);

                if (!IsValidPE(peData))
                {
                    AppendColoredText("Error: Unpacked data is not a valid PE file.\n", Color.Red);
                    return false;
                }

                PB_loading.Value = 75;
                AppendColoredText($"    Valid PE file restored\n", Color.Black);

                string outputPath = Path.Combine(
                    Path.GetDirectoryName(filePath) ?? "",
                    Path.GetFileNameWithoutExtension(filePath) + "_unpacked.exe");

                File.WriteAllBytes(outputPath, peData);

                PB_loading.Value = 100;
                AppendColoredText($"[4/4] Unpacked file written: {outputPath}\n", Color.Blue);

                return true;
            }
            catch (Exception ex)
            {
                AppendColoredText($"Error during unpacking: {ex.Message}\n", Color.Red);
                return false;
            }
        }

        private static byte[] Compress(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        public static byte[] PackPE(byte[] peData, out byte[] key, out byte[] iv, bool obfuscation)
        {
            // Compress the PE data
            byte[] compressed = Compress(peData);

            // Encrypt the compressed data
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                key = aes.Key;
                iv = aes.IV;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(compressed, 0, compressed.Length);
                    csEncrypt.FlushFinalBlock();
                    byte[] encrypted = msEncrypt.ToArray();

                    if (obfuscation)
                    {
                        // Hash the encrypted data
                        using (SHA256 sha = SHA256.Create())
                        {
                            byte[] hash = sha.ComputeHash(encrypted);

                            // XOR hash with key and iv to generate new key and iv
                            byte[] newKey = new byte[key.Length];
                            byte[] newIv = new byte[iv.Length];

                            for (int i = 0; i < key.Length; i++)
                                newKey[i] = (byte)(key[i] ^ hash[i % hash.Length]);
                            for (int i = 0; i < iv.Length; i++)
                                newIv[i] = (byte)(iv[i] ^ hash[(i + key.Length) % hash.Length]);

                            key = newKey;
                            iv = newIv;
                        }
                    }

                    return encrypted;
                }
            }
        }

        private static byte[] Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            using (MemoryStream output = new MemoryStream())
            using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        public static byte[] UnpackPE(byte[] packedData, byte[] key, byte[] iv, bool obfuscation)
        {
            byte[] origKey = key;
            byte[] origIv = iv;

            if (obfuscation)
            {
                // Hash the encrypted data (packedData)
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(packedData);

                    // Reverse XOR to get original key and iv
                    origKey = new byte[key.Length];
                    origIv = new byte[iv.Length];

                    for (int i = 0; i < key.Length; i++)
                        origKey[i] = (byte)(key[i] ^ hash[i % hash.Length]);
                    for (int i = 0; i < iv.Length; i++)
                        origIv[i] = (byte)(iv[i] ^ hash[(i + key.Length) % hash.Length]);
                }
            }

            // Decrypt the data
            using (Aes aes = Aes.Create())
            {
                aes.Key = origKey;
                aes.IV = origIv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream msDecrypt = new MemoryStream(packedData))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream msOutput = new MemoryStream())
                {
                    csDecrypt.CopyTo(msOutput);
                    byte[] decrypted = msOutput.ToArray();

                    // Decompress the data
                    return Decompress(decrypted);
                }
            }
        }

    }
}
