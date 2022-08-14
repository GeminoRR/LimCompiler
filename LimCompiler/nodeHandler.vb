'========================
'========= NODE =========
'========================
Public Class node

    Public startPos As Integer
    Public endPos As Integer
    Public parentStatement As statement

    Public Sub New(ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)
        Me.parentStatement = parentStatement
        Me.startPos = startPos
        Me.endPos = endPos
    End Sub

End Class

'===========================
'========= NewNode =========
'===========================
Public Class newNode
    Inherits node

    Public createFunction As funCallNode

    Public Sub New(ByVal createFunction As funCallNode, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.createFunction = createFunction

    End Sub

    Public Overrides Function ToString() As String
        Return "new -> (" & createFunction.ToString() & ")"
    End Function

End Class

'=============================
'========= ValueNode =========
'=============================
Public Class valueNode
    Inherits node

    Public tok As token

    Public Sub New(ByVal tok As token, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.tok = tok

    End Sub

    Public Overrides Function ToString() As String
        Return tok.ToString()
    End Function

End Class

'=============================
'========= ChildNode =========
'=============================
Public Class childNode
    Inherits node

    Public parentNode As node
    Public childNode As node

    Public Sub New(ByVal parentNode As node, ByVal childNode As node, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.parentNode = parentNode
        Me.childNode = childNode

    End Sub

    Public Overrides Function ToString() As String
        Return "(" & parentNode.ToString() & ").(" & childNode.ToString() & ")"
    End Function

End Class

'===============================
'========= BooleanNode =========
'===============================
Public Class booleanNode
    Inherits node

    Public value As Boolean

    Public Sub New(ByVal value As Boolean, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.value = value

    End Sub

    Public Overrides Function ToString() As String
        Return value.ToString()
    End Function

End Class

'==============================
'========= StringNode =========
'==============================
Public Class stringNode
    Inherits node

    Public tok As token

    Public Sub New(ByVal tok As token, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.tok = tok

    End Sub

    Public Overrides Function ToString() As String
        Return tok.ToString()
    End Function

End Class

'============================
'========= NullNode =========
'============================
Public Class nullNode
    Inherits node

    Public Sub New(ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)

    End Sub

    Public Overrides Function ToString() As String
        Return "null"
    End Function

End Class

'================================
'========= variableNode =========
'================================
Public Class variableNode
    Inherits node

    Public variableName As String
    Public listIndex As New List(Of node)

    Public Sub New(ByVal variableName As String, ByVal listIndex As List(Of node), ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.variableName = variableName
        If Not listIndex Is Nothing Then
            For Each idx As node In listIndex
                Me.listIndex.Add(idx)
            Next
        End If

    End Sub

    Public Overrides Function ToString() As String
        Dim str = ""
        For Each idx As Object In listIndex
            str &= "[" & idx.ToString & "]"
        Next
        Return variableName & str
    End Function

End Class

'============================
'========= ListNode =========
'============================
Public Class listNode
    Inherits node

    Public elements As New List(Of node)

    Public Sub New(ByVal elements As List(Of Object), ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        For Each elm As node In elements
            Me.elements.Add(elm)
        Next

    End Sub

    Public Overrides Function ToString() As String

        Dim str As String = ""
        For Each elm As Object In elements
            str &= ", " & elm.ToString()
        Next
        If Not str = "" Then
            str = str.Substring(2)
        End If
        Return "[" & str & "]"

    End Function

End Class

'===============================
'========= FunCallNode =========
'===============================
Public Class funCallNode
    Inherits node

    Public functionName As String
    Public arguments As New List(Of Object)

    Public Sub New(ByVal functionName As String, ByVal arguments As Object, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)

        MyBase.New(parentStatement, startPos, endPos)
        Me.functionName = functionName
        If Not arguments Is Nothing Then
            For Each arg As Object In arguments
                Me.arguments.Add(arg)
            Next
        End If

    End Sub

    Public Overrides Function ToString() As String
        Dim args As String = ""
        For Each arg As Object In arguments
            args &= ", " & arg.ToString()
        Next
        If args.Length > 0 Then
            Return String.Format("{0}({1})", functionName, args.Substring(2))
        Else
            Return String.Format("{0}()", functionName)
        End If
    End Function

End Class

'=============================
'========= BinOpNode =========
'=============================
Public Class binOpNode
    Inherits node

    Public leftNode As Object
    Public op As token
    Public rightNode As Object

    Public Sub New(ByVal leftNode As Object, ByVal op As token, ByVal rightNode As Object, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)
        MyBase.New(parentStatement, startPos, endPos)
        Me.leftNode = leftNode
        Me.op = op
        Me.rightNode = rightNode
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0}, {1}, {2})", leftNode, op, rightNode)
    End Function

End Class

'=============================
'========= TxtOpNode =========
'=============================
Public Class txtOpNode
    Inherits node

    Public leftNode As node
    Public op As token
    Public rightNode As node

    Public Sub New(ByVal leftNode As node, ByVal op As token, ByVal rightNode As node, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)
        MyBase.New(parentStatement, startPos, endPos)
        Me.leftNode = leftNode
        Me.op = op
        Me.rightNode = rightNode
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0}, {1}, {2})", leftNode, op, rightNode)
    End Function

End Class

'==============================
'========= CompOpNode =========
'==============================
Public Class compOpNode
    Inherits node

    Public leftNode As Object
    Public op As token
    Public rightNode As Object

    Public Sub New(ByVal leftNode As Object, ByVal op As token, ByVal rightNode As Object, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)
        MyBase.New(parentStatement, startPos, endPos)
        Me.leftNode = leftNode
        Me.op = op
        Me.rightNode = rightNode
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0}, {1}, {2})", leftNode, op, rightNode)
    End Function

End Class

'===============================
'========= UnaryOpNode =========
'===============================
Public Class UnaryOpNode
    Inherits node

    Public op As token
    Public node As Object

    Public Sub New(ByVal op As token, ByVal node As Object, ByVal parentStatement As statement, ByVal startPos As Integer, ByVal endPos As Integer)
        MyBase.New(parentStatement, startPos, endPos)
        Me.node = node
        Me.op = op
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0}, {1})", op, node)
    End Function

End Class