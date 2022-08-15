Imports System.IO

Public Class compiler

    '===============================
    '========== VARIABLES ==========
    '===============================

    '=============================
    '========== COMPILE ==========
    '=============================
    Public Sub compile(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Read file & Get tokens
        Dim fileContent As String = ""
        Dim tokens As New List(Of token)
        Try
            Dim lexer As New lexer()
            Dim linePosition As Integer = 0
            Dim sr As New StreamReader(target)
            Do Until sr.EndOfStream
                linePosition += 1
                Dim line As String = sr.ReadLine()
                tokens.AddRange(lexer.parse(line, target, linePosition))
                tokens.Add(New token(tokenType.CT_NEWLINE, 0, 0))
            Loop
            sr.Close()
        Catch ex As Exception
            addCustomError("Unable to read", ex.Message)
        End Try

        'Tokens
        For Each tok As token In tokens
            Console.WriteLine(tok.ToString())
        Next

    End Sub


End Class