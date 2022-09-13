'============================
'========= VARIABLE =========
'============================
Public Class Variable

    Public type As safeType
    Public name As String
    Public compiledName As String

    Public Sub New(ByVal name As String, ByVal type As safeType, ByVal compiledName As String)

        Me.name = name
        Me.type = type
        Me.compiledName = compiledName

    End Sub

End Class