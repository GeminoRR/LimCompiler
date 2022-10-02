'========================================
'========= BracketsSelectorNode =========
'========================================
Public Class BracketsSelectorNode
    Inherits Node

    'Variable
    Public Target As Node
    Public index As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Target As Node, ByVal index As Node)

        MyBase.New(positionStart, positionEnd)
        Me.Target = Target
        Me.Target.parentNode = Me
        Me.index = index
        Me.index.parentNode = Me

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0}[{1}])", Target.ToString(), index.ToString())
    End Function

End Class

'============================
'========= ListNode =========
'============================
Public Class ListNode
    Inherits Node

    'Variable
    Public elements As New List(Of Node)

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer)

        MyBase.New(positionStart, positionEnd)

    End Sub

    'Add element
    Public Sub addElement(ByVal elm As Node)
        elm.parentNode = Me
        elements.Add(elm)
    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Dim elementToString As String = ""
        For Each elm As Node In elements
            elementToString &= ", " & elm.ToString()
        Next
        If elementToString.StartsWith(", ") Then
            elementToString = elementToString.Substring(2)
        End If
        Return "[" & elementToString & "]"
    End Function

End Class

'===========================
'========= MapNode =========
'===========================
Public Class MapNode
    Inherits Node

    'Variable
    Public elements As New List(Of List(Of Node))

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer)

        MyBase.New(positionStart, positionEnd)

    End Sub

    'Add element
    Public Sub addElement(ByVal key As Node, ByVal value As Node)
        key.parentNode = Me
        value.parentNode = Me
        elements.Add({key, value}.ToList())
    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Dim elementToString As String = ""
        For Each elm As List(Of Node) In elements
            elementToString &= ", " & elm(0).ToString() & ":" & elm(1).ToString()
        Next
        If elementToString.StartsWith(", ") Then
            elementToString = elementToString.Substring(2)
        End If
        Return "{" & elementToString & "}"
    End Function

End Class

'====================================
'========= FunctionCallNode =========
'====================================
Public Class FunctionCallNode
    Inherits Node

    'Variable
    Public FunctionPath As ElementPathNode
    Public Arguments As New List(Of Node)

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal FunctionName As ElementPathNode, ByVal Arguments As List(Of Node))

        MyBase.New(positionStart, positionEnd)
        Me.FunctionPath = FunctionName
        Me.FunctionPath.parentNode = Me
        For Each Arg As Node In Arguments
            Arg.parentNode = Me
            Me.Arguments.Add(Arg)
        Next

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Dim ATS As String = ""
        For Each arg As Node In Arguments
            ATS &= ", " & arg.ToString()
        Next
        If ATS.StartsWith(", ") Then
            ATS = ATS.Substring(2)
        End If
        Return FunctionPath.ToString & "(" & ATS & ")"
    End Function

End Class

'================================
'========= VariableNode =========
'================================
Public Class VariableNode
    Inherits Node

    'Variable
    Public VariableName As ElementPathNode

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal VariableName As ElementPathNode)

        MyBase.New(positionStart, positionEnd)
        Me.VariableName = VariableName
        Me.VariableName.parentNode = Me

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return VariableName.ToString()
    End Function

End Class

'==================================
'========= ComparisonNode =========
'==================================
Public Class ComparisonNode
    Inherits Node

    'Variable
    Public leftNode As Node
    Public op As token
    Public rightNode As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal leftNode As Node, ByVal op As token, ByVal rightNode As Node)

        MyBase.New(positionStart, positionEnd)
        Me.leftNode = leftNode
        Me.leftNode.parentNode = Me
        Me.op = op
        Me.rightNode = rightNode
        Me.rightNode.parentNode = Me

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0} {1} {2})", leftNode, op, rightNode)
    End Function

End Class

'===============================
'========= BooleanNode =========
'===============================
Public Class BooleanNode
    Inherits Node

    'Variable
    Public value As Boolean

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal value As Boolean)

        MyBase.New(positionStart, positionEnd)
        Me.value = value

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return value.ToString()
    End Function

End Class

'==============================
'========= StringNode =========
'==============================
Public Class StringNode
    Inherits Node

    'Variables
    Public value As String

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal value As String)

        MyBase.New(positionStart, positionEnd)
        Me.value = value

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return """" & value & """"
    End Function

End Class

'=============================
'========= ValueNode =========
'=============================
Public Class valueNode
    Inherits Node

    'Variables
    Public tok As token

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal tok As token)

        MyBase.New(positionStart, positionEnd)
        Me.tok = tok

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Select Case tok.type
            Case tokenType.CT_FLOAT
                If tok.value.Contains(".") Then
                    Return tok.value
                Else
                    Return tok.value & ".0"
                End If
            Case tokenType.CT_INTEGER
                Return tok.value
            Case Else
                Return "" & tok.ToString() & ""
        End Select
    End Function

End Class

'===============================
'========= UnaryOpNode =========
'===============================
Public Class UnaryOpNode
    Inherits Node

    'Variables
    Public op As token
    Public node As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal op As token, ByVal node As Node)

        MyBase.New(positionStart, positionEnd)
        Me.node = node
        Me.node.parentNode = Me
        Me.op = op

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0} {1})", op, node)
    End Function

End Class


'=============================
'========= BinOpNode =========
'=============================
Public Class binOpNode
    Inherits Node

    'Variables
    Public leftNode As Node
    Public op As token
    Public rightNode As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal leftNode As Node, ByVal op As token, ByVal rightNode As Node)

        MyBase.New(positionStart, positionEnd)
        Me.leftNode = leftNode
        Me.leftNode.parentNode = Me
        Me.op = op
        Me.rightNode = rightNode
        Me.rightNode.parentNode = Me

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0} {1} {2})", leftNode, op, rightNode)
    End Function

End Class

'==============================
'========= Child Node =========
'==============================
Public Class childNode
    Inherits Node

    'Variables
    Public parentStruct As Node
    Public childNode As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal left As Node, ByVal right As Node)

        MyBase.New(positionStart, positionEnd)

        Me.parentStruct = left
        Me.parentStruct.parentNode = Me

        Me.childNode = right
        Me.childNode.parentNode = Me

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0}.{1})", parentStruct.ToString(), childNode.ToString())
    End Function

End Class

'=====================================
'========= Element Path Node =========
'=====================================
Public Class ElementPathNode
    Inherits Node

    'Variables
    Public childs As New List(Of String)

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, Optional childs As List(Of String) = Nothing)

        MyBase.New(positionStart, positionEnd)
        If Not childs Is Nothing Then
            For Each child As String In childs
                Me.childs.Add(child)
            Next
        End If

    End Sub

    'ToString
    Public Overrides Function ToString() As String
        Dim childsToString As String = ""
        For Each child As String In childs
            childsToString &= "->" & child
        Next
        If childsToString.StartsWith("->") Then
            childsToString = childsToString.Substring(2)
        End If
        Return childsToString
    End Function

End Class