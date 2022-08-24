'===========================
'========== LEXER ==========
'===========================
Public Class lexer

    '=============================
    '========= VARIABLES =========
    '=============================
    Dim text As String

    Dim charCounter As Integer
    Dim currentChar As Char

    Private Const authorizedNameCharacters As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_"
    Private Const digits As String = "1234567890"

    '===========================
    '========= ADVANCE =========
    '===========================
    Private Sub advance(Optional ByVal times As Integer = 1)

        For i As Integer = 0 To times - 1
            charCounter += 1

            If charCounter < text.Length Then
                currentChar = text(charCounter)
            Else
                currentChar = Nothing
            End If
        Next

    End Sub

    '=====================================
    '========= GetTokensFromLine =========
    '=====================================
    Public Function parse(ByVal text As String, ByVal filename As String) As List(Of token)

        Dim tokens As New List(Of token)
        If text = Nothing Then
            Return tokens
        End If
        Me.text = text
        charCounter = -1
        advance()

        Dim indentationCounter As Integer = 0

        tokens.Add(New token(tokenType.CT_LINESTART, 0, 0, "0"))
        While Not currentChar = Nothing

            If currentChar = """" Then
                'Create string token
                Dim posStart As Integer = charCounter
                advance()
                Dim create_string As String = ""
                While Not currentChar = """" And Not currentChar = Nothing
                    create_string &= currentChar
                    advance()
                End While
                tokens.Add(New token(tokenType.CT_STRING, posStart, charCounter + 1, create_string))
                advance()

            ElseIf currentChar = "'" Then
                'Create string token
                Dim posStart As Integer = charCounter
                advance()
                Dim create_string As String = ""
                While Not currentChar = "'" And Not currentChar = Nothing
                    create_string &= currentChar
                    advance()
                End While
                tokens.Add(New token(tokenType.CT_STRING, posStart, charCounter + 1, create_string))
                advance()

            ElseIf currentChar = "." Then
                'Point
                tokens.Add(New token(tokenType.OP_POINT, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "+" Then
                'Plus operator
                tokens.Add(New token(tokenType.OP_PLUS, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "-" Then
                'Minus operator
                tokens.Add(New token(tokenType.OP_MINUS, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "*" Then
                'Multiply operator
                tokens.Add(New token(tokenType.OP_MULTIPLICATION, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "/" Then
                'Divide operatorf
                If charCounter + 1 < text.Length Then
                    If text(charCounter + 1) = "/" Then
                        advance(2)
                        While Not (currentChar = vbCr Or currentChar = Environment.NewLine)
                            advance()
                        End While
                        Continue While
                    End If
                End If
                tokens.Add(New token(tokenType.OP_DIVISION, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "%" Then
                'Modulo operator
                tokens.Add(New token(tokenType.OP_MODULO, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "=" Then
                'Equal operator
                tokens.Add(New token(tokenType.OP_EQUAL, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = ":" Then
                'TwoPoint operator
                tokens.Add(New token(tokenType.OP_TWOPOINT, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "!" Then
                'Not operator
                If charCounter + 1 < text.Length Then
                    If text(charCounter + 1) = "=" Then
                        tokens.Add(New token(tokenType.OP_NOTEQUAL, charCounter, charCounter + 2))
                        advance(2)
                        Continue While
                    End If
                End If
                tokens.Add(New token(tokenType.OP_NOT, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = ">" Then
                'MoreThan operator
                If charCounter + 1 < text.Length Then
                    If text(charCounter + 1) = "=" Then
                        tokens.Add(New token(tokenType.OP_MORETHANEQUAL, charCounter, charCounter + 2))
                        advance(2)
                        Continue While
                    End If
                End If
                tokens.Add(New token(tokenType.OP_MORETHAN, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "<" Then
                'LessThan operator
                If charCounter + 1 < text.Length Then
                    If text(charCounter + 1) = "=" Then
                        tokens.Add(New token(tokenType.OP_LESSTHANEQUAL, charCounter, charCounter + 2))
                        advance(2)
                        Continue While
                    End If
                End If
                tokens.Add(New token(tokenType.OP_LESSTHAN, charCounter, charCounter + 1))
                advance()

            ElseIf digits.Contains(currentChar) Then
                'Create number
                Dim create_number As String = currentChar
                Dim startPos As Integer = charCounter
                advance()
                Dim dot_count As Integer = 0
                While Not currentChar = Nothing And Not currentChar = " " And (digits & ".").Contains(currentChar)
                    If currentChar = "." Then
                        dot_count += 1
                    End If
                    create_number &= currentChar
                    advance()
                End While
                If dot_count = 0 Then
                    tokens.Add(New token(tokenType.CT_INTEGER, startPos, charCounter, create_number))
                ElseIf dot_count = 1 Then
                    tokens.Add(New token(tokenType.CT_FLOAT, startPos, charCounter, create_number))
                Else
                    addCustomSyntaxError("LP02", "A number cannot contain more than one point", filename, text, startPos, charCounter)
                End If

            ElseIf authorizedNameCharacters.Contains(currentChar) Then

                'Create the var
                Dim create_var As String = currentChar
                Dim startPos As Integer = charCounter
                advance()
                While Not currentChar = Nothing And (authorizedNameCharacters & digits).Contains(currentChar)

                    create_var &= currentChar
                    advance()

                End While

                Select Case create_var.ToLower()
                    Case "true"
                        tokens.Add(New token(tokenType.CT_TRUE, startPos, charCounter))
                    Case "false"
                        tokens.Add(New token(tokenType.CT_FALSE, startPos, charCounter))
                    Case "null"
                        tokens.Add(New token(tokenType.CT_NULL, startPos, charCounter))
                    Case "new"
                        tokens.Add(New token(tokenType.KW_NEW, startPos, charCounter))
                    Case "in"
                        tokens.Add(New token(tokenType.OP_IN, startPos, charCounter))
                    Case "let"
                        tokens.Add(New token(tokenType.KW_LET, startPos, charCounter))
                    Case "var"
                        tokens.Add(New token(tokenType.KW_VAR, startPos, charCounter))
                    Case "const"
                        tokens.Add(New token(tokenType.KW_CONST, startPos, charCounter))
                    Case "return"
                        tokens.Add(New token(tokenType.KW_RETURN, startPos, charCounter))
                    Case "struct"
                        tokens.Add(New token(tokenType.KW_STRUCT, startPos, charCounter))
                    Case "func"
                        tokens.Add(New token(tokenType.KW_FUNC, startPos, charCounter))
                    Case "space"
                        tokens.Add(New token(tokenType.KW_SPACE, startPos, charCounter))
                    Case "ref"
                        tokens.Add(New token(tokenType.KW_REF, startPos, charCounter))
                    Case Else
                        tokens.Add(New token(tokenType.CT_TEXT, startPos, charCounter, create_var))
                End Select

            ElseIf currentChar = " " Then
                'Pass
                advance()

            ElseIf currentChar = vbCr Then
                indentationCounter = 0
                tokens.Add(New token(tokenType.CT_LINESTART, charCounter, charCounter + 1, indentationCounter.ToString()))
                advance(2) 'Cariage Return + Line feed

            ElseIf currentChar = vbLf Then
                advance()

            ElseIf currentChar = Environment.NewLine Then
                indentationCounter = 0
                tokens.Add(New token(tokenType.CT_LINESTART, charCounter, charCounter + 1, indentationCounter.ToString()))
                advance()

            ElseIf currentChar = vbTab Then
                'Get indentation
                If indentationCounter = -1 Then
                    advance()
                    Continue While
                End If
                Dim posStart As Integer = charCounter
                While currentChar = vbTab
                    indentationCounter += 1
                    advance()
                End While
                If tokens(tokens.Count - 1).type = tokenType.CT_LINESTART Then
                    tokens(tokens.Count - 1).positionStart = posStart
                    tokens(tokens.Count - 1).positionEnd = charCounter
                    tokens(tokens.Count - 1).value = indentationCounter.ToString()
                Else
                    addCustomError("lexer error", "The lexer tries to find the indentation of a start of line that does not exist.")
                End If
                indentationCounter = -1


            ElseIf currentChar = "," Then
                'Separator
                tokens.Add(New token(tokenType.OP_COMMA, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "@" Then
                'At
                tokens.Add(New token(tokenType.KW_REF, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "(" Then
                'lPar
                tokens.Add(New token(tokenType.OP_LPAR, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = ")" Then
                'rPar
                tokens.Add(New token(tokenType.OP_RPAR, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "[" Then
                'lBracket
                tokens.Add(New token(tokenType.OP_LBRACKET, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "]" Then
                'rBracket
                tokens.Add(New token(tokenType.OP_RBRACKET, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "{" Then
                'Left Brace
                tokens.Add(New token(tokenType.OP_LBRACE, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "}" Then
                'Right Brace
                tokens.Add(New token(tokenType.OP_RBRACE, charCounter, charCounter + 1))
                advance()

            Else
                'Error : Unauthorized character
                addCustomSyntaxError("LP01", "The """ & currentChar & """ character was unexpected.", filename, text, charCounter, charCounter + 1)
            End If

        End While

        'Remove useless lines
        Dim i As Integer = 0
        While i < tokens.Count

            If tokens(i).type = tokenType.CT_LINESTART Then

                While i + 1 < tokens.Count
                    If tokens(i + 1).type = tokenType.CT_LINESTART Then
                        tokens.RemoveAt(i)
                    Else
                        Exit While
                    End If
                End While

            End If

            i += 1

        End While

        tokens.Add(New token(tokenType.CT_LINESTART, charCounter, charCounter + 1, "0"))
        Return tokens


    End Function

End Class