'================================
'========= SET VARIABLE =========
'================================
Public Class SetVariableNode
    Inherits Node

    'Variable
    Public Target As Node
    Public NewValue As Node

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal Target As Node, ByVal NewValue As Node)
        MyBase.New(positionStart, positionEnd)
        Me.Target = Target
        Me.Target.parentNode = Me
        Me.NewValue = NewValue
        Me.NewValue.parentNode = Me
    End Sub

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
    Public Overrides Function ToString() As String
        Return Target.ToString() & " = " & NewValue.ToString()
    End Function

End Class

'====================================
'========= DECLARE VARIABLE =========
'====================================
Public Class DeclareVariableNode
    Inherits Node

    'Variables
    Public variableName As String
    Public variableUnsafeType As unsafeType
    Public value As Node
    Public variableIsReference As Boolean
    Public declarationType As VariableDeclarationType

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal declarationType As VariableDeclarationType, ByVal variableName As String, ByVal value As Node, ByVal variableUnsafeType As unsafeType, Optional ByVal variableIsReference As Boolean = False)
        MyBase.New(positionStart, positionEnd)
        Me.variableUnsafeType = variableUnsafeType
        Me.variableName = variableName
        Me.declarationType = declarationType
        Me.variableIsReference = variableIsReference
        Me.value = value
        If Not value Is Nothing Then
            Me.value.parentNode = Me
        End If
    End Sub

    'Compile
    Public Overrides Function compile() As String
        Throw New NotImplementedException()
    End Function

    'ToString
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