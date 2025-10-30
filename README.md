# SimPacker

SimPacker is a simple, modern .NET8 packer for Windows executables. It compresses and encrypts PE files, generating self-extracting stubs with optional obfuscation. SimPacker is designed for educational and research purposes, demonstrating practical C#12.0 techniques for file packing, cryptography, and dynamic code generation.

---

## Features

- **PE File Packing**: Compresses and encrypts Windows executables (PE files) using AES-256 and GZip.
- **Self-Extracting Stub**: Generates a standalone executable that unpacks and runs the original file.
- **Obfuscation Option**: Splits encryption keys and IVs into fragments and applies XOR-based obfuscation using a SHA256 hash for added security.
- **GUI Frontend**: User-friendly Windows Forms interface for selecting, packing, and unpacking files.

---

## Getting Started

### Prerequisites

- .NET8 SDK ([Download](https://dotnet.microsoft.com/download))
- Windows OS
- Visual Studio2022 (recommended)

### Building SimPacker

1. Clone the repository:
 ```sh
 git clone https://github.com/ptg14/SimPacker.git
 cd SimPacker
 ```
2. Open `SimPacker.sln` in Visual Studio or build from the command line:
 ```sh
 dotnet build
 ```
3. Run the application:
 ```sh
 dotnet run --project SimPacker
 ```

---

## Usage

### Packing a File

1. Launch SimPacker.
2. Click **Open** and select a Windows executable (`.exe`).
3. Optionally, enable **Obfuscation** for enhanced key/IV protection.
4. Click **Pack**.
5. The packed file will be created in the same directory, named `<original>_packed.exe`.

### Unpacking a File

1. Click **Open** and select a packed executable (`_packed.exe`).
2. Click **Unpack**.
3. The unpacked file will be created as `<original>_unpacked.exe`.

### Log Output

- All actions and errors are displayed in the log window with color-coded messages for clarity.

---

## How It Works

- **Packing**: The original PE file is compressed (GZip), encrypted (AES-256), and embedded as a Base64 string in a generated stub loader source file. The stub is compiled into a new executable.
- **Obfuscation**: When enabled, the AES key and IV are split into fragments and reconstructed at runtime, with additional XOR-based obfuscation using a SHA256 hash of the packed data.
- **Unpacking**: The stub loader extracts, decrypts, and decompresses the original PE file, then executes it in a temporary location.

---

## Security & Limitations

- **Educational Use Only**: SimPacker is intended for learning and research. Do not use for malicious purposes.
- **PE File Support**: Only standard Windows executables are supported. Non-PE files will be rejected.
- **Antivirus Detection**: Packed files may trigger antivirus alerts due to self-extracting behavior.

---

## Advanced

### Customizing the Stub Loader

- The stub loader source is generated dynamically in `StubLoader.cs`.
- You can modify the template or add features (e.g., anti-debugging, custom extraction logic).

### Compiler Selection

- SimPacker automatically locates the best available C# compiler.
- If compilation fails, ensure Visual Studio or the .NET SDK is installed.

---

**Happy Packing!**
