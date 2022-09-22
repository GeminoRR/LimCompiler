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
    Public variableUnsafeType As typeNode
    Public value As Node
    Public variableIsReference As Boolean
    Public declarationType As VariableDeclarationType
    Public compiled As Boolean

    'New
    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal declarationType As VariableDeclarationType, ByVal variableName As String, ByVal value As Node, ByVal variableUnsafeType As typeNode, Optional ByVal variableIsReference As Boolean = False)
        MyBase.New(positionStart, positionEnd)
        Me.variableUnsafeType = variableUnsafeType
        If Not variableUnsafeType Is Nothing Then
            Me.variableUnsafeType.parentNode = Me
        End If
        Me.variableName = variableName
        Me.declarationType = declarationType
        Me.variableIsReference = variableIsReference
        Me.value = value
        If Not value Is Nothing Then
            Me.value.parentNode = Me
        End If
        Me.compiled = False
    End Sub

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