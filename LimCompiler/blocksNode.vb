'==========================
'========= STRUCT =========
'==========================
Public Class StructNode
    Inherits containerNode

    'Variable
    Public Name As String
    Public compiledName As String = ""
    Public compiled As Boolean

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Name As String)

        MyBase.New(positionStart, positionEnd)
        Me.Name = Name
        Me.compiled = False

    End Sub

    'ToString
    Public Overrides Function ToString() As String

        'Variable
        Dim varSTR As String = ""
        Dim funcSTR As String = ""
        Dim otherSTR As String = ""

        'Loop
        For Each code As Node In Me.codes
            If TypeOf code Is DeclareVariableNode Then
                varSTR &= Environment.NewLine & code.ToString()
            ElseIf TypeOf code Is FunctionNode Then
                funcSTR &= Environment.NewLine & Split(code.ToString(), Environment.NewLine)(0).Replace(Environment.NewLine, "")
            Else
                otherSTR &= Environment.NewLine & code.ToString()
            End If
        Next

        'Return
        Return Name & "{" & varSTR & funcSTR & otherSTR & Environment.NewLine & "}"

    End Function

End Class

'============================
'========= FUNCTION =========
'============================
Public Class FunctionNode
    Inherits containerNode

    'Variable
    Public Name As String
    Public Arguments As List(Of FunctionArgument)

    Private unsafeReturnType As typeNode = Nothing
    Private safeReturnType As safeType = Nothing
    Public ReadOnly Property returnType As safeType
        Get
            Return safeReturnType
        End Get
    End Property


    Public compiledName As String = ""
    Public compiled As Boolean
    Public compiling As Boolean

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Name As String, ByVal Arguments As List(Of FunctionArgument), ByVal unsafeReturnType As typeNode)
        MyBase.New(positionStart, positionEnd)
        Me.Name = Name
        Me.Arguments = Arguments
        Me.unsafeReturnType = unsafeReturnType
        If Not Me.unsafeReturnType Is Nothing Then
            Me.unsafeReturnType.parentNode = Me
        End If
        Me.compiled = False
        Me.compiling = False
        For Each arg As FunctionArgument In Me.Arguments
            arg.type.parentNode = Me
        Next
    End Sub

    'ToString
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
            LTS &= Environment.NewLine & Name & " -> " & action.ToString()
        Next

        'Return
        Return Name & "(" & ATS & ")" & UST & LTS

    End Function

End Class
Public Class FunctionArgument

    Public name As String
    Public type As typeNode
    Public ref As Boolean
    Public Sub New(ByVal name As String, ByVal type As typeNode, ByVal ref As Boolean)
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