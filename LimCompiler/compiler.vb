Imports System.IO
Public Class compiler

    '===============================
    '========== VARIABLES ==========
    '===============================
    Public files As New List(Of FileNode)

    '=============================
    '========== COMPILE ==========
    '=============================
    Public Sub compile(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Add file
        files.Add(New FileNode(target))

    End Sub

End Class