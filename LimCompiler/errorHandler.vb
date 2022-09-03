Module errorHandler

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

    '============================================
    '========== CUSTOM SYNTAX WARNING  ==========
    '============================================
    Public Sub addCustomSyntaxWarning(ByVal code As String, ByVal message As String, ByVal filename As String, Optional ByRef text As String = "", Optional ByVal positionStart As Integer = 0, Optional ByVal positionEnd As Integer = 0)

        Dim beforeColor As ConsoleColor = Console.ForegroundColor

        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("SYNTAX WARNING: " & message)

        If Not text = "" Then

            Dim infos As List(Of String) = getLineAndPositionFromIndex(text, positionStart, positionEnd)
            Dim line As String = infos(0)
            Dim linePosition As String = infos(1)
            Dim lineStartPosition As Integer = Convert.ToInt32(infos(2))

            'While line.StartsWith(vbTab)
            '    line = line.Substring(1)
            '    lineStartPosition += 1
            'End While

            positionStart -= lineStartPosition
            positionEnd -= lineStartPosition

            Console.ForegroundColor = beforeColor
            Dim preLine As String = " -> "
            Console.WriteLine(preLine & line)
            Dim errorUnderline As String = StrDup(positionStart, " ") & StrDup((positionEnd) - (positionStart), "^")

            Console.WriteLine(StrDup(preLine.Length, " ") & errorUnderline)
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("<" & filename & "> at line " & linePosition & ", Code " & code)

        Else

            Console.WriteLine("<" & filename & "> , Code " & code)

        End If

        Console.ForegroundColor = beforeColor

    End Sub

    '==========================================
    '========== CUSTOM SYNTAX ERROR  ==========
    '==========================================
    Public Sub addCustomSyntaxError(ByVal code As String, ByVal message As String, ByVal filename As String, Optional ByRef text As String = "", Optional ByVal positionStart As Integer = 0, Optional ByVal positionEnd As Integer = -1)

        Dim beforeColor As ConsoleColor = Console.ForegroundColor

        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("SYNTAX ERROR: " & message)

        If Not text = "" Then

            Dim infos As List(Of String) = getLineAndPositionFromIndex(text, positionStart, positionEnd)
            Dim line As String = infos(0)
            Dim linePosition As String = infos(1)
            Dim lineStartPosition As Integer = Convert.ToInt32(infos(2))

            If positionEnd = -1 Then
                positionEnd = positionStart
            End If

            'While line.StartsWith(vbTab)
            '    line = line.Substring(1)
            '    lineStartPosition += 1
            'End While

            positionStart -= lineStartPosition
            positionEnd -= lineStartPosition

            If positionStart < 0 Or positionEnd < 0 Then
                positionStart = lineStartPosition
                positionEnd = lineStartPosition
            End If

            Console.ForegroundColor = beforeColor
            Dim preLine As String = " -> "
            Console.WriteLine(preLine & line)
            Dim errorUnderline As String = StrDup(positionStart, " ") & StrDup((positionEnd) - (positionStart), "^")

            Console.WriteLine(StrDup(preLine.Length, " ") & errorUnderline)
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("<" & filename & "> at line " & linePosition & ", Code " & code)

        Else

            Console.WriteLine("<" & filename & "> , Code " & code)

        End If

        Console.ForegroundColor = beforeColor
        endApp()

    End Sub

    '==================================================
    '========== GET LINE POSITION FROM INDEX ==========
    '==================================================
    Private Function getLineAndPositionFromIndex(ByVal text As String, ByVal indexStart As Integer, Optional ByVal indexEnd As Integer = -1) As List(Of String)

        'Return {Line, LinePosition, LineStartCharacterPosition}

        Dim finalLinePosition As String = ""
        Dim startIndexLinePosition As Integer = 0
        Dim lineCounter As Integer = 1
        Dim LineStartCharacterPosition As Integer = 0

        If Not indexStart < text.Length Then
            indexStart = text.Length - 1
        End If
        If indexEnd < indexStart Then
            indexEnd = indexStart
        End If

        For i As Integer = 0 To text.Length - 1

            If text(i) = vbLf Then
                lineCounter += 1
                If i <= indexStart Then
                    LineStartCharacterPosition = i
                End If
            End If

            If i = indexStart Then
                finalLinePosition = lineCounter.ToString()
                startIndexLinePosition = lineCounter - 1
                If indexEnd = indexStart Then
                    Exit For
                End If
            End If

            If i = indexEnd Then
                If Not lineCounter = Convert.ToInt32(finalLinePosition) Then
                    finalLinePosition &= " to " & lineCounter.ToString()
                End If
                Exit For
            End If

        Next

        Return {text.Split(Environment.NewLine)(startIndexLinePosition).Replace(vbLf, "").Replace(vbCr, ""), finalLinePosition, (LineStartCharacterPosition + 1).ToString()}.ToList()

    End Function

    '==================================
    '========== SYNTAX ERROR ==========
    '==================================
    Public Sub addSyntaxError(ByVal code As String, ByVal message As String, ByVal node As Node)

        Dim file As FileNode = getFileFromNode(node)
        addCustomSyntaxError(code, message, file.name, file.text, node.positionStart, node.positionEnd)

    End Sub

End Module
