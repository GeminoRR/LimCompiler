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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0}[{1}])", Target.ToString(), index.ToString())
    End Function

End Class

'====================================
'========= FunctionCallNode =========
'====================================
Public Class FunctionCallNode
    Inherits Node

    'Variable
    Public FunctionName As String
    Public Arguments As New List(Of Node)

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal FunctionName As String, ByVal Arguments As List(Of Node))

        MyBase.New(positionStart, positionEnd)
        Me.FunctionName = FunctionName
        For Each Arg As Node In Arguments
            Arg.parentNode = Me
            Me.Arguments.Add(Arg)
        Next

    End Sub

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Dim ATS As String = ""
        For Each arg As Node In Arguments
            ATS &= ", " & arg.ToString()
        Next
        Return FunctionName & "(" & ATS.Substring(2) & ")"
    End Function

End Class

'================================
'========= VariableNode =========
'================================
Public Class VariableNode
    Inherits Node

    'Variable
    Public VariableName As String

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal VariableName As String)

        MyBase.New(positionStart, positionEnd)
        Me.VariableName = VariableName

    End Sub

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Return VariableName
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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

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

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("({0} {1} {2})", leftNode, op, rightNode)
    End Function

End Class

'==================================
'========= Child Variable =========
'==================================
Public Class childVariableNode
    Inherits Node

    'Variables
    Public parentStruct As String
    Public variableName As String

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal parentStruct As String, ByVal variableName As String)

        MyBase.New(positionStart, positionEnd)
        Me.parentStruct = parentStruct
        Me.variableName = variableName

    End Sub

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("{0}.{1}", parentStruct, variableName)
    End Function

End Class

'==============================
'========= From Space =========
'==============================
Public Class fromSpaceNode
    Inherits Node

    'Variables
    Public name As String
    Public child As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal spaceName As String, ByVal child As Node)

        MyBase.New(positionStart, positionEnd)
        Me.name = name
        Me.child = child
        Me.child.parentNode = Me

    End Sub

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Return String.Format("{0}->{1}", name, child.ToString())
    End Function

End Class