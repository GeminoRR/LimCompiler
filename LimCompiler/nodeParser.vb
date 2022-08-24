Public Class nodeParser

    '=============================
    '========= VARIABLES =========
    '=============================
    Dim tokens As New List(Of token)
    Dim tok_index As Integer
    Dim current_tok As token
    Dim filename As String
    Dim text As String

    '===========================
    '========= ADVANCE =========
    '===========================
    Private Sub advance()
        tok_index += 1
        If tok_index < tokens.Count Then
            current_tok = tokens(tok_index)
        Else
            'addCustomSyntaxError("NPA01", "Something was expected here", filename, text, current_tok.positionEnd, current_tok.positionEnd + 1)
        End If
    End Sub

    '==========================
    '========= RECEDE =========
    '==========================
    Private Sub recede()
        tok_index -= 1
        If tok_index >= 0 And tok_index < tokens.Count Then
            current_tok = tokens(tok_index)
        End If
    End Sub

    '=========================
    '========= PARSE =========
    '=========================
    Public Function parse(ByVal tokens As List(Of token), ByVal text As String, ByVal filename As String) As List(Of SpaceNode)

        'Reload informations
        Dim spaces As New List(Of SpaceNode)
        Me.filename = filename
        Me.text = text
        Me.tokens.Clear()
        Dim fileSpace As New SpaceNode(0, text.Length - 1, "init")
        spaces.Add(fileSpace)
        For Each tok As token In tokens
            Me.tokens.Add(tok)
        Next
        tok_index = -1
        advance()

        While tok_index < tokens.Count - 1

            Dim obj As Node = space()
            If Not TypeOf obj Is SpaceNode Then
                fileSpace.code.Add(obj)
            Else
                spaces.Add(DirectCast(obj, SpaceNode))
            End If

        End While

        Return spaces

    End Function

    '========================
    '========= TYPE =========
    '========================
    Private Function type() As unsafeType

        'Variables
        Dim waitTokenType As tokenType = Nothing

        'Handle error
        If Not current_tok.type = tokenType.CT_TEXT Then
            addCustomSyntaxError("NPT01", "A name of a type was expected here", filename, text, current_tok.positionStart, current_tok.positionEnd)
        End If

        'Create unsafe type
        Dim currentType As New unsafeType(current_tok.value, New List(Of ValueType))

        'Get dimensions
        While True

            advance()

            If Not waitTokenType = Nothing Then
                If Not current_tok.type = waitTokenType Then
                    addCustomSyntaxError("NPT02", "The token <" & waitTokenType.ToString() & "> was expected here", filename, text, current_tok.positionStart, current_tok.positionEnd)
                End If
                waitTokenType = Nothing
                Continue While
            End If

            Select Case current_tok.type

                Case tokenType.OP_LBRACKET
                    waitTokenType = tokenType.OP_RBRACKET
                    currentType.Dimensions.Add(ValueType.list)

                Case tokenType.OP_LBRACE
                    waitTokenType = tokenType.OP_RBRACE
                    currentType.Dimensions.Add(ValueType.map)

                Case Else
                    Exit While

            End Select

        End While

        'Return type
        Return currentType

    End Function

    '==========================
    '========= FACTOR =========
    '==========================
    Private Function factor() As Node

        Dim tok As token = current_tok

        If tok.type = tokenType.OP_PLUS Or tok.type = tokenType.OP_MINUS Then
            advance()
            Dim fac = factor()
            Return New UnaryOpNode(tok.positionStart, fac.positionEnd, tok, fac)

        ElseIf tok.type = tokenType.CT_INTEGER Or tok.type = tokenType.CT_FLOAT Then
            advance()
            Return New valueNode(tok.positionStart, tok.positionEnd, tok)

        ElseIf tok.type = tokenType.OP_LPAR Then
            advance()
            Dim com = expr()
            If current_tok.type = tokenType.OP_RPAR Then
                advance()
                Return com
            Else
                addCustomSyntaxError("NPF02", "A parenthesis was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
            End If

        End If

        addCustomSyntaxError("NPF01", "Something else was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
        Return Nothing

    End Function

    '========================
    '========= TERM =========
    '========================
    Private Function term() As Node

        Dim left = factor()
        While current_tok.type = tokenType.OP_MULTIPLICATION Or current_tok.type = tokenType.OP_DIVISION Or current_tok.type = tokenType.OP_MODULO
            Dim op = current_tok
            advance()
            Dim right = factor()
            left = New binOpNode(left.positionStart, right.positionEnd, left, op, right)
        End While

        Return left

    End Function

    '========================
    '========= EXPR =========
    '========================
    Private Function expr() As Node

        Dim left As Node = term()
        While current_tok.type = tokenType.OP_PLUS Or current_tok.type = tokenType.OP_MINUS
            Dim op As token = current_tok
            advance()
            Dim right As Node = term()
            left = New binOpNode(left.positionStart, right.positionEnd, left, op, right)
        End While

        Return left

    End Function

    '========================
    '========= LINE =========
    '========================
    Private Function line() As Node

        Return expr()

    End Function

    '========================
    '========= FUNC =========
    '========================
    Private Function func() As Node

        'Check indentation
        Dim needToRecede As Boolean = False
        Dim funcIndentation As Integer = 0
        If current_tok.type = tokenType.CT_LINESTART Then
            funcIndentation = Convert.ToInt32(current_tok.value)
            advance()
            needToRecede = True
        End If

        'Check if node is func
        If current_tok.type = tokenType.KW_FUNC Then

            'Save start pos
            Dim startPosition As Integer = current_tok.positionStart
            advance()

            'Get error
            If Not current_tok.type = tokenType.CT_TEXT Then
                addCustomSyntaxError("NPF01", "A name was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
            End If

            'Get name
            Dim currentFunction As New FunctionNode(startPosition, startPosition + 1, current_tok.value, New List(Of FunctionArgument))
            advance()

            'Get arguments
            If current_tok.type = tokenType.OP_LPAR Then 'func Name(username:str, @id:int[])

                'First arg
                advance()

                'Direct ending ?
                If current_tok.type = tokenType.OP_RPAR Then

                    'Direct ending
                    advance()

                Else

                    'Arguments in there
                    While True

                        'Variables
                        Dim LastArgumentName As String = ""
                        Dim LastArgumentRef As Boolean = False
                        Dim LastArgumentUnsafeType As unsafeType = Nothing

                        'Search for a ref
                        If current_tok.type = tokenType.KW_REF Then
                            LastArgumentRef = True
                            advance()
                        End If

                        'Search for a name
                        If Not current_tok.type = tokenType.CT_TEXT Then
                            addCustomSyntaxError("NPF02", "A argument name was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
                        End If
                        LastArgumentName = current_tok.value
                        advance()

                        'Search for a type
                        If Not current_tok.type = tokenType.OP_TWOPOINT Then
                            addCustomSyntaxError("NPF03", "A argument type was expected here (example : ""my_argument:str[]"")", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
                        End If
                        advance()
                        LastArgumentUnsafeType = type()

                        'Add argument
                        currentFunction.Arguments.Add(New FunctionArgument(LastArgumentName, LastArgumentUnsafeType, LastArgumentRef))

                        'Search for end
                        If current_tok.type = tokenType.OP_COMMA Then

                            advance()

                        ElseIf current_tok.type = tokenType.OP_RPAR Then

                            advance()
                            Exit While

                        Else

                            addCustomSyntaxError("NPF04", "An end of parenthesis or a comma was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)

                        End If

                    End While

                End If

            End If

            'Unsafe type
            If current_tok.type = tokenType.OP_TWOPOINT Then

                advance()
                currentFunction.unsafeReturnType = type()

            End If

            'Get error
            If Not current_tok.type = tokenType.CT_LINESTART Then
                addCustomSyntaxError("NPF02", "A newline was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
            End If

            'Get codes
            While True

                If Not current_tok.type = tokenType.CT_LINESTART Then
                    addCustomSyntaxError("NPF05", "A newline was expected here", filename, text, current_tok.positionStart, current_tok.positionEnd)
                End If
                Dim currentLineIndentation As Integer = Convert.ToInt32(current_tok.value)

                If currentLineIndentation <= funcIndentation Then
                    Exit While
                End If

                'currentFunction.codes.Add(line())

            End While

            'Add node
            Return currentFunction

        End If

        'It's another thing that a space (namespace)
        If needToRecede = True Then
            recede()
        End If
        Return line()

    End Function

    '=========================
    '========= SPACE =========
    '=========================
    Private Function space() As Node

        'Check indentation
        Dim needToRecede As Boolean = False
        Dim nameIndentation As Integer = 0
        If current_tok.type = tokenType.CT_LINESTART Then
            nameIndentation = Convert.ToInt32(current_tok.value)
            advance()
            needToRecede = True
        End If

        'Check if node is space
        If current_tok.type = tokenType.KW_SPACE Then 'Not " " char but space Keyword

            'Save start pos
            Dim startPosition As Integer = current_tok.positionStart
            advance()

            'Get error
            If Not current_tok.type = tokenType.CT_TEXT Then
                addCustomSyntaxError("NPS01", "A name was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
            End If

            'Get name
            Dim currentSpace As New SpaceNode(startPosition, startPosition + 1, current_tok.value)
            advance()

            'Get error
            If Not current_tok.type = tokenType.CT_LINESTART Then
                addCustomSyntaxError("NPS02", "A new line was expected here", filename, Me.text, current_tok.positionStart, current_tok.positionEnd)
            End If

            'Get codes
            While True

                If Not current_tok.type = tokenType.CT_LINESTART Then
                    addCustomSyntaxError("NPS03", "A newline was expected here", filename, text, current_tok.positionStart, current_tok.positionEnd)
                End If
                Dim currentLineIndentation As Integer = Convert.ToInt32(current_tok.value)

                If currentLineIndentation <= nameIndentation Then
                    Exit While
                End If

                Dim toAdd As Node = space()
                If TypeOf toAdd Is SpaceNode Or TypeOf toAdd Is FunctionNode Then
                    currentSpace.code.Add(toAdd)
                Else
                    addCustomSyntaxWarning("NPSW01", "The following line of code cannot be located in this frame, it will not be taken into account.", filename, text, toAdd.positionStart, toAdd.positionEnd)
                End If

            End While

            'Add node
            Return currentSpace

        End If

        'It's another thing that a space (namespace)
        If needToRecede = True Then
            recede()
        End If
        Return func()

    End Function

End Class