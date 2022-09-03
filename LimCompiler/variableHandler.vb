'============================
'========= VARIABLE =========
'============================
Public Class Variable

    Public type As safeType
    Public name As String
    Public compiledName As String

    Public Sub New(ByVal name As String, ByVal type As safeType, Optional ByVal compiledName As String = Nothing)

        Me.name = name
        Me.type = type
        If compiledName = Nothing Then
            Me.compiledName = getVariableName()
        Else
            Me.compiledName = compiledName
        End If

    End Sub

End Class