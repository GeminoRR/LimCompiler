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

        'Target exist
        If Not File.Exists(target) Then
            addCustomError("Missing file", "File """ & target & """ does not exist")
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

End Module
