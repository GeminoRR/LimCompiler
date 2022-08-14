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
    Public Function parse(ByVal line As String, ByVal filename As String, ByVal linePosition As Integer) As List(Of token)

        Dim tokens As New List(Of token)
        If line = Nothing Then
            Return tokens
        End If
        text = line
        charCounter = -1
        advance()

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
                tokens.Add(New token(tokenType.CT_STRING, posStart, charCounter, create_string))
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
                tokens.Add(New token(tokenType.CT_STRING, posStart, charCounter, create_string))
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
                'Plus operator
                tokens.Add(New token(tokenType.OP_MINUS, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "*" Then
                'Plus operator
                tokens.Add(New token(tokenType.OP_MULTIPLICATION, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "/" Then
                'Plus operator
                tokens.Add(New token(tokenType.OP_DIVISION, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "%" Then
                'Modulo operator
                tokens.Add(New token(tokenType.OP_MODULO, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "=" Then
                'Plus operator
                tokens.Add(New token(tokenType.OP_EQUAL, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = ":" Then
                'Plus operator
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
                    addSyntaxError("A number cannot contain more than one point", filename, linePosition, "LHP01", True, Nothing, line, startPos, charCounter)
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
                    Case Else
                        tokens.Add(New token(tokenType.CT_TEXT, startPos, charCounter, create_var))
                End Select

            ElseIf currentChar = " " Then
                'Pass
                advance()

            ElseIf currentChar = vbTab Then
                advance()

            ElseIf currentChar = "," Then
                'Separator
                tokens.Add(New token(tokenType.OP_COMMA, charCounter, charCounter + 1))
                advance()

            ElseIf currentChar = "@" Then
                'At
                tokens.Add(New token(tokenType.OP_AT, charCounter, charCounter + 1))
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

            Else
                'Error : Unauthorized character
                addSyntaxError("The """ & currentChar & """ character was unexpected.", filename, linePosition, "PLLGTF02", True, Nothing, line, charCounter, charCounter + 1)
            End If

        End While

        Return tokens

    End Function

End Class