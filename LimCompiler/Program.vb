Imports System
Imports System.IO

Module Program

    '================================
    '========== MAIN ENTRY ==========
    '================================
    Sub Main(args As String())

        'Variables
        Dim target As String = ""
        Dim output As String = ""
        Dim flags As New List(Of String)

        'Loop each argument
        For Each arg As String In args

            If arg.StartsWith("-") Then
                flags.Add(arg)
            End If

            If target = "" Then
                target = arg
            Else
                output = arg
            End If

        Next

        'Error
        If target = "" Then
            showHelp()
            endApp()
        End If

        'Help
        If target = "help" Then
            showHelp()
            endApp()
        End If

        'Floppy
        If target = "floppy" Then
            Dim templateFile As String = System.Reflection.Assembly.GetExecutingAssembly().Location().Replace("\", "/")
            templateFile = templateFile.Substring(0, templateFile.LastIndexOf("/")) & "/templates/floppy.txt"
            If Not File.Exists(templateFile) Then
                endApp()
            End If
            Dim content As String = ""
            Try
                content = File.ReadAllText(templateFile)
            Catch ex As Exception
                endApp()
            End Try
            If flags.Contains("-char") Then
                For Each i As String In content.Split(Environment.NewLine)
                    For Each x As Char In i.Replace(vbLf, "").Replace(vbCr, "")
                        Console.Write(x)
                        If Not x = " " Then
                            Threading.Thread.Sleep(1)
                        End If
                    Next
                    Console.Write(Environment.NewLine)
                Next
            ElseIf flags.Contains("-line") Then
                For Each i As String In content.Split(Environment.NewLine)
                    Console.WriteLine(i.Replace(vbLf, "").Replace(vbCr, ""))
                    Threading.Thread.Sleep(1)
                Next
            Else
                Console.WriteLine(content)
            End If
            endApp()
        End If

        'Target exist
        If Not File.Exists(target) Then
            If Not File.Exists(target & ".lim") Then
                addCustomError("Missing file", "File """ & target & """ does not exist")
            Else
                target &= ".lim"
            End If
        End If

        'Run
        If output = "" Then
            Dim vb_compiler As New VB_Compiler()
            vb_compiler.run(target, "", flags)
            endApp()
        End If

        'Compiling output type
        If Not (flags.Contains("-windows") Or flags.Contains("-linux") Or flags.Contains("-macos")) Then
            flags.Add("-windows") 'TODO: Get current OS
        End If

        'Output
        If output = "" Then
            output = target.Substring(0, target.LastIndexOf(".")) & ".exe"
        End If
        If File.Exists(output) Then

            If output.EndsWith(".exe") Then
                output = output.Substring(0, output.Length - 4)
            End If

            Dim counter As Integer = 1
            While File.Exists(output & counter.ToString() & ".exe")
                counter += 1
            End While

            output = output & counter.ToString() & ".exe"

        End If
        If Not output.EndsWith(".exe") Then
            output &= ".exe"
        End If

        'Compile
        If flags.Contains("-c") Then
            Dim compiler As New CCompiler()
            compiler.compile(target, output, flags)
        Else
            Dim compiler As New VB_Compiler()
            compiler.compile(target, output, flags)
        End If

        'End app
        endApp()

    End Sub

    '===========================
    '========== CLOSE ==========
    '===========================
    Public Sub endApp()
        Console.ReadKey()
        End
    End Sub

    '===============================
    '========== SHOW HELP ==========
    '===============================
    Public Sub showHelp()

        'Example
        Console.ForegroundColor = ConsoleColor.DarkGreen
        Console.WriteLine("RUN : lim <input_file>")
        Console.WriteLine("COMPILE: lim <input_file> <output_file> [-arguments]")

        'Explains
        Console.ResetColor()
        Console.WriteLine("<input_file>" & vbTab & "Path of the .lim file to compile")
        Console.WriteLine("<output_file>" & vbTab & "Path of the future executable file. (This will be created by the compiler)")
        Console.WriteLine("[-arguments]" & vbTab & "Optional. Argument list.")
        Console.WriteLine(vbTab & "-vb" & vbTab & vbTab & "Compiles to a .vb file (VisualBasic)")
        Console.WriteLine(vbTab & "-c" & vbTab & vbTab & "Compiles to a .c file (C)")
        Console.WriteLine(vbTab & "-windows" & vbTab & "Compiles to a .exe file (Executable)")
        Console.WriteLine(vbTab & "-linux" & vbTab & vbTab & "Compiles to a linux executable")
        Console.WriteLine(vbTab & "-macos" & vbTab & vbTab & "Compiles to a MacOS executable")
        Console.WriteLine("If no arguments are entered, lim will compile to your operating system's executable.")

    End Sub

End Module
