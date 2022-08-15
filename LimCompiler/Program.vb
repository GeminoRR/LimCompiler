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
        Dim compiler As New compiler()
        compiler.compile(target, output, flags)

        'End app
        endApp()

    End Sub

    '===========================
    '========== CLOSE ==========
    '===========================
    Private Sub endApp()
        Console.ReadKey()
        End
    End Sub

    '===================================
    '========== CUSTOM ERROR  ==========
    '===================================
    Public Sub addCustomError(ByVal errorName As String, ByVal message As String)
        Dim beforeColor As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine(errorName.ToUpper() & ": " & message)
        Console.ForegroundColor = beforeColor
        endApp()
    End Sub

    '==========================================
    '========== CUSTOM SYNTAX ERROR  ==========
    '==========================================
    Public Sub addCustomSyntaxError(ByVal code As String, ByVal message As String, ByVal filename As String, ByVal linePosition As Integer, Optional ByVal line As String = "", Optional ByVal positionStart As Integer = 0, Optional ByVal positionEnd As Integer = 0)
        Dim beforeColor As ConsoleColor = Console.ForegroundColor

        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("SYNTAX ERROR: " & message)

        If Not line = "" Then
            Console.ForegroundColor = beforeColor
            Console.WriteLine(vbTab & line)
            If positionEnd = 0 Then
                positionEnd = line.Length - 1
            End If
            Dim errorUnderline As String = StrDup(positionStart, " ") & StrDup(positionEnd - positionStart, "^")
            Console.WriteLine(vbTab & errorUnderline)
        End If

        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("<" & filename & "> at line " & linePosition.ToString())

        Console.ForegroundColor = beforeColor
        endApp()
    End Sub

    '==================================
    '========== SYNTAX ERROR ==========
    '==================================
    Public Sub addSyntaxError(ByVal code As String, ByVal message As String, ByVal node As node)

        'Normal syntax error

    End Sub


End Module
