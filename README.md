# DotNET XorCryptor
## [You can also pay attention to an older project - NET-MalwareCryptor](https://github.com/DosX-dev/NET-MalwareCryptor)

This is a .NET executable packer with payload encryption. Use for educational purposes only.

## How to use?
 * `--file {path}` - specify file name
 * `--flood` - add junk classes to file
 * `--proxy` - move original entry point to from Main()

For example:
```
xor-pack.exe --file "stub.exe" --proxy --flood
```

For enhanced effectiveness, it is strongly recommended to heavily obfuscate the assembly before applying the encryption process.
## [DOWNLOAD COMPILED](https://github.com/DosX-dev/DotNET_XorCryptor/releases/tag/Builds)


![](https://raw.githubusercontent.com/DosX-dev/DotNET_XorCryptor/main/detects.png)
![](https://raw.githubusercontent.com/DosX-dev/DotNET_XorCryptor/main/presentation.png)
