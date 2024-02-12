'' Coded by DosX
'' GitHub: https://github.com/DosX-dev

Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices

Module PolymorphicGenerator

    Dim rnd As New Random()

    Sub Main()

        Dim fileNameToPack As String = String.Empty

        Dim args As String() = Environment.GetCommandLineArgs()

        Dim floodWithClasses As Boolean = False,
            numberOfClasses As Integer = 0

        Dim entryPointProxy As Boolean = False

        For i As Integer = 0 To args.Length - 1
            If args(i).StartsWith("--") Then
                Dim argKey As String = args(i).Substring(2)

                Select Case argKey
                    Case "help"
                        Console.WriteLine(" --file {path} - specify file name" & vbLf &
                                          " --flood - add junk classes to the file" & vbLf &
                                          " --proxy - proxy the original entry point several times")
                        End
                    Case "file"
                        If i < args.Length - 1 AndAlso Not args(i + 1).StartsWith("--") Then
                            fileNameToPack = args(i + 1)
                        Else
                            Console.WriteLine("Input data at --file not specified.")
                            End
                        End If
                    Case "flood"
                        floodWithClasses = True
                        numberOfClasses = rnd.Next(10, 50)
                    Case "proxy"
                        entryPointProxy = True
                    Case Else
                        Console.ForegroundColor = ConsoleColor.Black
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                        Console.WriteLine("WARNING: Unknown parameter: " & argKey)
                        Console.ResetColor()
                        End
                End Select
            End If
        Next

        If Not File.Exists(fileNameToPack) Then
            Console.WriteLine("File not found!")
            End
        End If

        Dim fileNameToPackNoExt = IO.Path.GetFileNameWithoutExtension(fileNameToPack)

        Directory.SetCurrentDirectory(IO.Path.GetDirectoryName(Path.GetFullPath(fileNameToPack)))

        Dim provider As New VBCodeProvider(),
            options As New CompilerParameters()

        Dim genBuildName = GenerateRandomString()

        Dim buildOutName = $"{genBuildName}.exe"

        options.OutputAssembly = buildOutName
        options.CompilerOptions = "/target:winexe"
        options.GenerateExecutable = True

        options.ReferencedAssemblies.Add("System.dll")
        options.ReferencedAssemblies.Add("System.IO.dll")

        Dim payloadName As String = GenerateRandomString(),
            injectionSubName As String = GenerateRandomString(),
            encryptedPayloadArgName As String = GenerateRandomString()

        File.Copy(fileNameToPack, payloadName)

        Dim keyLength As Integer = rnd.Next(10, 21),
            key(keyLength - 1) As Byte
        rnd.NextBytes(key)

        File.WriteAllBytes(payloadName, XorEnc(File.ReadAllBytes(fileNameToPack), key))

        options.EmbeddedResources.Add(payloadName)

        Dim keyArr As String = "{" & String.Join(", ", key.Select(Function(b) b.ToString())) & "}"


        If floodWithClasses Then
            Console.WriteLine("Number of junk classes: " & numberOfClasses)
        End If

        Dim sizeOfPayload = New FileInfo(payloadName).Length

        Console.WriteLine("Generated key:" & vbLf & " | BYTES: " & keyArr.Replace(" ", String.Empty) & vbLf & " | BASE64: " & Convert.ToBase64String(key) & vbLf &
                          "Payload size: " & sizeOfPayload & " bytes" & vbLf &
                          "Key size: " & keyLength & " bytes" & vbLf &
                          "EP proxy: " & If(entryPointProxy, "Enabled", "Disabled"))


        Dim sourceCode As New StringBuilder

        sourceCode.AppendLine($"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CompilerServices")

        sourceCode.AppendLine($"Module {GenerateRandomString()}")

        sourceCode.AppendLine($"Sub Main()")

        If entryPointProxy Then
            For x = 0 To rnd.Next(2, 8)
                Dim originalEpName = GenerateRandomString()

                ' Proxy methods
                sourceCode.AppendLine($"
{originalEpName}()
End Sub

Sub {originalEpName}
")
            Next
        End If

        sourceCode.AppendLine($"Using manifestResourceStream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(""{payloadName}"")
        If Not manifestResourceStream Is Nothing Then
            Using reader As New BinaryReader(manifestResourceStream)
                {injectionSubName}( reader.ReadBytes(CInt(manifestResourceStream.Length)) )
            End Using
        End If
    End Using
End Sub")

        sourceCode.AppendLine($"Sub {injectionSubName}({encryptedPayloadArgName}) ' injection

    Dim decryptedPayload({sizeOfPayload - 1}) As Byte
    For i As Integer = 0 To {sizeOfPayload - 1}
        decryptedPayload(i) = {encryptedPayloadArgName}(i) Xor {keyArr}(i Mod {keyLength})
    Next

    NewLateBinding.LateCall(
        NewLateBinding.LateGet(NewLateBinding.LateGet(Nothing, Type.GetType(""System.Reflection.Assembly""), ""Load"", New Object() {{ RuntimeHelpers.GetObjectValue(decryptedPayload) }}, Nothing, Nothing, {{ True }}),
    Nothing, ""EntryPoint"", New Object(-1) {{}}, Nothing, Nothing, Nothing), Nothing, ""Invoke"", New Object() {{ Nothing, Nothing }}, Nothing, Nothing, Nothing, True)
End Sub
End Module
")

        If floodWithClasses Then
            For i = 0 To numberOfClasses
                sourceCode.AppendLine($"Class {GenerateRandomString()} : End Class")
            Next
        End If

        Dim results As CompilerResults = provider.CompileAssemblyFromSource(options, sourceCode.ToString())


        If results.Errors.Count > 0 Then
            For Each errorObj As CompilerError In results.Errors
                Console.WriteLine(errorObj.ErrorText, &H10)
            Next
        Else
            Dim FileCreated = $"{fileNameToPack}_xor-packed.exe"
            If File.Exists(FileCreated) Then
                File.Delete(FileCreated)
            End If
            File.Delete(payloadName)
            File.Move(buildOutName, FileCreated)
            Console.WriteLine(vbLf & "* Stub with encrypted payload compiled successfully! Output: " & FileCreated)
        End If
    End Sub

    Function XorEnc(inputArray As Byte(), keyArray As Byte()) As Byte()
        Dim encryptedArray(inputArray.Length - 1) As Byte
        For i As Integer = 0 To inputArray.Length - 1
            encryptedArray(i) = inputArray(i) Xor keyArray(i Mod keyArray.Length)
        Next
        Return encryptedArray
    End Function

    Private Function GenerateRandomString() As String
        Dim allowedChars() As Char = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
        Dim length As Integer = rnd.Next(10, 31)
        Dim chars(length - 1) As Char

        For i As Integer = 0 To length - 1
            chars(i) = allowedChars(rnd.Next(0, allowedChars.Length))
        Next

        Return New String(chars)
    End Function
End Module
