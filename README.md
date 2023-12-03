# DotNET XorCryptor
## [You can also pay attention to an older project - NET-MalwareCryptor](https://github.com/DosX-dev/NET-MalwareCryptor)

This is a .NET executable packer with payload encryption. Use for educational purposes only.

## How to use?
 * `--file {path}` - specify file name
 * `--flood` - add junk classes to file
 * `--proxy` - move original entry point from Main()

For example:
```
xor-pack.exe --file "stub.exe" --proxy --flood
```
This packer is suitable for all applications written in: VB NET, C#, C++/CLR, JScript .NET, MSIL, etc. (on the .NET Framework)

For enhanced effectiveness, it is strongly recommended to heavily obfuscate the assembly before and after applying the encryption process.

## [DOWNLOAD COMPILED](https://github.com/DosX-dev/DotNET_XorCryptor/releases/tag/Builds)


![](https://raw.githubusercontent.com/DosX-dev/DotNET_XorCryptor/main/detects.png)
![](https://raw.githubusercontent.com/DosX-dev/DotNET_XorCryptor/main/presentation.png)
