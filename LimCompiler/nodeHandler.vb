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

'========================
'========= FILE =========
'========================
Public Class FileNode
    Inherits Node

    Public text As String
    Public name As String
    Public path As String

    Public spaces As List(Of SpaceNode)
    Public Sub New(ByVal path As String)

        'Inherits
        MyBase.New(0, 0)

        'Load infos
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
    Inherits Node

    Public name As String
    Public code As New List(Of Node)

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal name As String)
        MyBase.New(positionStart, positionEnd)
        Me.name = name
    End Sub

    Public Overrides Function ToString() As String
        Dim content As String = ""
        For Each code As Node In Me.code
            content &= Environment.NewLine & vbTab & code.ToString()
        Next
        Return "space " & name & "" & content
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

'====================================
'========= DECLARE VARIABLE =========
'====================================
Public Class DeclareVariableNode
    Inherits Node

    Public variableName As String
    Public variableUnsafeType As unsafeType
    Public value As Node
    Public variableIsReference As Boolean

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal variableName As String, ByVal value As Node, ByVal variableUnsafeType As unsafeType, Optional ByVal variableIsReference As Boolean = False)
        MyBase.New(positionStart, positionEnd)
        Me.variableName = variableName
        Me.variableIsReference = variableIsReference
        Me.value = value
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
            unsafeTypeSTR = ":" & unsafeTypeSTR.ToString()
        End If
        Return "(VAR " & refSTR & variableName & unsafeTypeSTR & valueSTR & ")"
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
        Return "" & tok.ToString() & ""
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