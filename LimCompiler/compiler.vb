Imports System.IO
Public Class compiler


    '===============================
    '========== VARIABLES ==========
    '===============================
    Public files As New List(Of FileNode)
    Private CompiledNameAlphabet As String = "abcdefghijklmnoqrstuvwxyz"
    Private variableCount As Integer = 0
    Private functionCount As Integer = 0
    Private spaceCount As Integer = 0

    '=============================
    '========== COMPILE ==========
    '=============================
    Public Sub compile(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Reset values
        variableCount = 0
        functionCount = 0
        spaceCount = 0

        'Add file
        files.Add(New FileNode(target, Me))

    End Sub

    '===================================
    '========== GENERATE NAME ==========
    '===================================
    Public Function generateCompiledName(ByRef counter As Integer) As String

        counter += 1

        Dim colones As New List(Of Integer) From {-1}
        For i As Integer = 0 To counter

            colones(colones.Count - 1) += 1
            For x As Integer = 0 To colones.Count - 1
                Dim currentColoneCheck As Integer = colones.Count - 1 - x
                If colones(currentColoneCheck) >= CompiledNameAlphabet.Count Then
                    colones(currentColoneCheck) = 0
                    If currentColoneCheck = 0 Then
                        colones.Add(0)
                    Else
                        colones(currentColoneCheck - 1) += 1
                    End If
                End If
            Next
        Next

        Dim str As String = ""
        For Each col As Integer In colones
            str &= CompiledNameAlphabet(col)
        Next
        Return str

    End Function

    '============================================
    '========== GENERATE VARIABLE NAME ==========
    '============================================
    Public Function getVariableName() As String
        Return "v_" & generateCompiledName(variableCount)
    End Function
    '============================================
    '========== GENERATE FUNCTION NAME ==========
    '============================================
    Public Function getFunctionName() As String
        Return "f_" & generateCompiledName(functionCount)
    End Function
    '=========================================
    '========== GENERATE SPACE NAME ==========
    '=========================================
    Public Function getSpaceName() As String
        Return "s_" & generateCompiledName(spaceCount)
    End Function

End Class