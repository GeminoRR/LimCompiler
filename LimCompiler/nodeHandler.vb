Imports System.IO
'========================
'========= NODE =========
'========================
Public MustInherit Class Node


    Public positionStart As Integer
    Public positionEnd As Integer
    Public parentNode As Node = Nothing

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer)
        Me.positionStart = positionStart
        Me.positionEnd = positionEnd
    End Sub

    Public Overrides Function ToString() As String
        Return "()"
    End Function

End Class

'==================================
'========= CONTAINER NODE =========
'==================================
Public MustInherit Class containerNode
    Inherits Node

    Public variables As New List(Of Object)
    Public codes As New List(Of Node)

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer)
        MyBase.New(positionStart, positionEnd)
    End Sub

    Public Sub addNodeToCode(ByVal node As Node)
        node.parentNode = Me
        codes.Add(node)
    End Sub

End Class

'========================
'========= FILE =========
'========================
Public Class FileNode
    Inherits Node

    Public text As String
    Public name As String
    Public path As String

    Public compiler As compiler

    Public spaces As List(Of SpaceNode)
    Public Sub New(ByVal path As String, ByVal compiler As compiler)

        'Inherits
        MyBase.New(0, 0)

        'Load infos
        Me.compiler = compiler
        Me.text = text
        path = path.Replace("\", "/")
        Me.path = path
        If path.Contains("/") Then
            Me.name = path.Substring(path.LastIndexOf("/"))
        Else
            Me.name = path
        End If

        'Read file
        Try
            Me.text = File.ReadAllText(path)
        Catch ex As Exception
            addCustomError("Unable to read", ex.Message)
        End Try

        'Generates Tokens
        Dim lexer As New lexer()
        Dim tokens As List(Of token) = lexer.parse(Me.text, Me.name)

        'Generate AST
        Dim parser As New nodeParser()
        spaces = parser.parse(tokens, text, Me.name)

        'Set parents
        For i As Integer = 0 To spaces.Count - 1
            spaces(i).parentNode = Me
        Next

        For Each st As SpaceNode In spaces
            Console.WriteLine(st.ToString())
        Next


    End Sub
    Public Overrides Function ToString() As String
        Return "file " & name
    End Function

End Class

'=========================
'========= SPACE =========
'=========================
Public Class SpaceNode
    Inherits containerNode

    Public name As String

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal name As String)
        MyBase.New(positionStart, positionEnd)
        Me.name = name
    End Sub

    Public Overrides Function ToString() As String
        Dim content As String = ""
        For Each code As Node In Me.codes
            content &= Environment.NewLine & vbTab & code.ToString()
        Next
        Return "space " & name & "" & content
    End Function

End Class


'============================
'========= FUNCTION =========
'============================
Public Class FunctionNode
    Inherits containerNode

    Public Name As String
    Public Arguments As List(Of FunctionArgument)
    Public unsafeReturnType As unsafeType = Nothing

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Name As String, ByVal Arguments As List(Of FunctionArgument))
        MyBase.New(positionStart, positionEnd)
        Me.Name = Name
        Me.Arguments = Arguments
    End Sub
    Public Overrides Function ToString() As String

        'Unsafe type
        Dim UST As String = ""
        If Not unsafeReturnType Is Nothing Then
            UST = ":" & unsafeReturnType.ToString()
        End If

        'Argument
        Dim ATS As String = ""
        If Arguments.Count > 0 Then
            For Each arg As FunctionArgument In Arguments
                ATS &= ", " & arg.ToString()
            Next
            ATS = ATS.Substring(2)
        End If

        'Actions
        Dim LTS As String = ""
        For Each action As Node In codes
            LTS &= Environment.NewLine & "-> " & action.ToString()
        Next

        'Return
        Return Name & "(" & ATS & ")" & UST & LTS

    End Function

End Class
Public Class FunctionArgument

    Public name As String
    Public type As unsafeType
    Public ref As Boolean
    Public Sub New(ByVal name As String, ByVal type As unsafeType, ByVal ref As Boolean)
        Me.name = name
        Me.type = type
        Me.ref = ref
    End Sub
    Public Overrides Function ToString() As String

        If ref Then

            'Ref
            Return "@" & name & ":" & type.ToString()

        Else

            'Copy
            Return name & ":" & type.ToString()

        End If

    End Function

End Class

'================================
'========= SET VARIABLE =========
'================================
Public Class SetVariableNode
    Inherits Node

    Public Target As Node
    Public NewValue As Node

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Target As Node, ByVal NewValue As Node)
        MyBase.New(positionStart, positionEnd)
        Me.Target = Target
        Me.Target.parentNode = Me
        Me.NewValue = NewValue
        Me.NewValue.parentNode = Me
    End Sub

    Public Overrides Function ToString() As String
        Return Target.ToString() & " = " & NewValue.ToString()
    End Function

End Class

'====================================
'========= DECLARE VARIABLE =========
'====================================
Public Class DeclareVariableNode
    Inherits Node

    Public variableName As String
    Public variableUnsafeType As unsafeType
    Public value As Node
    Public variableIsReference As Boolean
    Public declarationType As VariableDeclarationType

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal declarationType As VariableDeclarationType, ByVal variableName As String, ByVal value As Node, ByVal variableUnsafeType As unsafeType, Optional ByVal variableIsReference As Boolean = False)
        MyBase.New(positionStart, positionEnd)
        Me.variableName = variableName
        Me.declarationType = declarationType
        Me.variableIsReference = variableIsReference
        Me.value = value
        Me.value.parentNode = Me
        Me.variableUnsafeType = variableUnsafeType
    End Sub

    Public Overrides Function ToString() As String
        Dim refSTR As String = ""
        If variableIsReference Then
            refSTR = "@"
        End If
        Dim valueSTR As String = ""
        If Not value Is Nothing Then
            valueSTR = " = " & value.ToString()
        End If
        Dim unsafeTypeSTR As String = ""
        If Not variableUnsafeType Is Nothing Then
            unsafeTypeSTR = ":" & variableUnsafeType.ToString()
        End If
        Dim declareSTR As String = "UnknownDeclaration"
        Select Case declarationType
            Case VariableDeclarationType._let_
                declareSTR = "LET"
            Case VariableDeclarationType._var_
                declareSTR = "VAR"
        End Select
        Return declareSTR & " " & refSTR & variableName & unsafeTypeSTR & valueSTR
    End Function

End Class
Public Enum VariableDeclarationType
    _let_
    _var_
End Enum

'========================================
'========= BracketsSelectorNode =========
'========================================
Public Class BracketsSelectorNode
    Inherits Node

    Public Target As Node
    Public index As Node

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Target As Node, ByVal index As Node)

        MyBase.New(positionStart, positionEnd)
        Me.Target = Target
        Me.Target.parentNode = Me
        Me.index = index
        Me.index.parentNode = Me

    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0}[{1}])", Target.ToString(), index.ToString())
    End Function

End Class

'====================================
'========= FunctionCallNode =========
'====================================
Public Class FunctionCallNode
    Inherits Node

    Public FunctionName As String
    Public Arguments As New List(Of Node)

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal FunctionName As String, ByVal Arguments As List(Of Node))

        MyBase.New(positionStart, positionEnd)
        Me.FunctionName = FunctionName
        For Each Arg As Node In Arguments
            Arg.parentNode = Me
            Me.Arguments.Add(Arg)
        Next

    End Sub

    Public Overrides Function ToString() As String
        Dim ATS As String = ""
        For Each arg As Node In Arguments
            ATS &= ", " & arg.ToString()
        Next
        Return FunctionName & "(" & ATS.Substring(2) & ")"
    End function

End Class

'================================
'========= VariableNode =========
'================================
Public Class VariableNode
    Inherits Node

    Public VariableName As String

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal VariableName As String)

        MyBase.New(positionStart, positionEnd)
        Me.VariableName = VariableName

    End Sub

    Public Overrides Function ToString() As String
        Return VariableName
    End Function

End Class

'==================================
'========= ComparisonNode =========
'==================================
Public Class ComparisonNode
    Inherits Node

    Public leftNode As Node
    Public op As token
    Public rightNode As Node

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal leftNode As Node, ByVal op As token, ByVal rightNode As Node)

        MyBase.New(positionStart, positionEnd)
        Me.leftNode = leftNode
        Me.leftNode.parentNode = Me
        Me.op = op
        Me.rightNode = rightNode
        Me.rightNode.parentNode = Me

    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0} {1} {2})", leftNode, op, rightNode)
    End Function

End Class

'===============================
'========= BooleanNode =========
'===============================
Public Class BooleanNode
    Inherits Node

    Public value As Boolean

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal value As Boolean)

        MyBase.New(positionStart, positionEnd)
        Me.value = value

    End Sub

    Public Overrides Function ToString() As String
        Return value.ToString()
    End Function

End Class

'==============================
'========= StringNode =========
'==============================
Public Class StringNode
    Inherits Node

    Public value As String

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal value As String)

        MyBase.New(positionStart, positionEnd)
        Me.value = value

    End Sub

    Public Overrides Function ToString() As String
        Return """" & value & """"
    End Function

End Class

'=============================
'========= ValueNode =========
'=============================
Public Class valueNode
    Inherits Node

    Public tok As token

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal tok As token)

        MyBase.New(positionStart, positionEnd)
        Me.tok = tok

    End Sub

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

    Public op As token
    Public node As Node

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal op As token, ByVal node As Node)

        MyBase.New(positionStart, positionEnd)
        Me.node = node
        Me.node.parentNode = Me
        Me.op = op

    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0} {1})", op, node)
    End Function

End Class


'=============================
'========= BinOpNode =========
'=============================
Public Class binOpNode
    Inherits Node

    Public leftNode As Node
    Public op As token
    Public rightNode As Node

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal leftNode As Node, ByVal op As token, ByVal rightNode As Node)

        MyBase.New(positionStart, positionEnd)
        Me.leftNode = leftNode
        Me.leftNode.parentNode = Me
        Me.op = op
        Me.rightNode = rightNode
        Me.rightNode.parentNode = Me

    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("({0} {1} {2})", leftNode, op, rightNode)
    End Function

End Class