'============================
'========= VARIABLE =========
'============================
Public Class Variable

    Public type As safeType
    Public name As String
    Public compiledName As String
    Public ref As Boolean

    Public Sub New(ByVal name As String, ByVal type As safeType, ByVal compiledName As String, Optional ref As Boolean = False)

        Me.name = name
        Me.type = type
        Me.compiledName = compiledName
        Me.ref = ref

    End Sub

End Class