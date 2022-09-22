Imports System.IO
Module helper

    '============================
    '========= GET FILE =========
    '============================
    Public Function getFileFromNode(ByVal currentNode As Node) As FileNode

        Dim parentNode As Node = currentNode
        While Not parentNode.parentNode Is Nothing
            parentNode = parentNode.parentNode
            If TypeOf parentNode Is FileNode Then
                Exit While
            End If
        End While

        If Not TypeOf parentNode Is FileNode Then
            addCustomError("Internal Link error", "Cannot find the parent File, current node is <" & parentNode.GetType.ToString() & ">")
        End If

        Return parentNode

    End Function

    '==================================
    '========== GET TEMPLATE ==========
    '==================================
    Public Function getTemplate(ByVal templateFolder As String, ByVal filename As String)
        If Not File.Exists(templateFolder & "/" & filename) Then
            addCustomError("Cannot read file", """" & templateFolder & "/" & filename & """ is missing")
        End If
        Dim result As String = ""
        Try
            result = File.ReadAllText(templateFolder & "/" & filename)
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try
        Return result
    End Function

    '==========================================
    '========== GET PARENT CONTAINER ==========
    '==========================================
    Public Function getParentContainer(ByVal node As Node) As containerNode

        Dim parent As Node = node.parentNode
        While Not parent.parentNode Is Nothing

            If TypeOf parent Is containerNode Then
                Return DirectCast(parent, containerNode)
            End If

            parent = parent.parentNode

        End While

        addSyntaxError("HGPC01", "Could not find parent container", node)
        Return Nothing

    End Function

    '===========================================
    '========== STRING TO UNSAFE TYPE ==========
    '===========================================
    Public Function StringToNodeType(ByVal text As String, Optional ByVal positionStart As Integer = 0, Optional ByVal positionEnd As Integer = 0) As typeNode

        'Create node
        Dim node As New typeNode(positionStart, positionEnd, New ElementPathNode(positionStart, positionEnd, New List(Of String)), New List(Of ValueType))

        'Get elementPath
        While True

            If text = "" Then
                Exit While
            End If

            Dim lastWord As String = ""
            While Not {"-", "{", "["}.Contains(text(0))
                lastWord += text(0)
                text = text.Substring(1)
                If text = "" Then
                    Exit While
                End If
            End While
            If Not lastWord = "" Then
                node.StructName.childs.Add(lastWord)
            End If

            If text = "" Then
                Exit While
            End If

            If text(0) = "-" Then
                text = text.Substring(1)
                If Not text(0) = ">" Then
                    addCustomSyntaxError("HSTNT01", "A "">"" was expected here", "<unknown>")
                End If
                text = text.Substring(1)
                Continue While
            End If

            If text(0) = "[" Then
                text = text.Substring(1)
                If Not text(0) = "]" Then
                    addCustomSyntaxError("HSTNT02", "A ""]"" was expected here", "<unknown>")
                End If
                node.Dimensions.Add(ValueType.list)
                text = text.Substring(1)
                Continue While
            End If

            If text(0) = "{" Then
                text = text.Substring(1)
                If Not text(0) = "}" Then
                    addCustomSyntaxError("HSTNT03", "A ""}"" was expected here", "<unknown>")
                End If
                node.Dimensions.Add(ValueType.map)
                text = text.Substring(1)
                Continue While
            End If

        End While

        'Return
        Return node

    End Function

    Public Function StringToFunctionNode(ByVal text As String) As FunctionNode

        'Get name
        Dim name As String = ""
        While Not text(0) = ":"
            name += text(0)
            text = text.Substring(1)
        End While
        text = text.Substring(1)

        'Get compiled name
        Dim compiledName As String = ""
        While Not text(0) = ":"
            compiledName += text(0)
            text = text.Substring(1)
        End While
        text = text.Substring(1)

        'Get type
        Dim typeStr As String = ""
        While Not text(0) = "("
            typeStr += text(0)
            text = text.Substring(1)
        End While
        text = text.Substring(1)

        'Get argument
        Dim arguments As New List(Of FunctionArgument)
        While Not text(0) = ")"

            'Get ref
            Dim isRef As Boolean = False
            If text(0) = "@" Then
                text = text.Substring(1)
                isRef = True
            End If

            'Get name
            Dim argName As String = ""
            While Not text(0) = ":"
                argName += text(0)
                text = text.Substring(1)
            End While
            text = text.Substring(1)

            'Get type
            Dim argType As String = ""
            While Not (text(0) = "," Or text(0) = ")")
                argType += text(0)
                text = text.Substring(1)
            End While
            If text(0) = "," Then
                text = text.Substring(1)
            End If

            'Add arg
            arguments.Add(New FunctionArgument(argName, helper.StringToNodeType(argType), isRef))

        End While

        'Create struct
        Dim fun As New FunctionNode(0, 0, name, arguments, StringToNodeType(typeStr))
        fun.compiledName = compiledName

        'Return
        Return fun

    End Function

End Module