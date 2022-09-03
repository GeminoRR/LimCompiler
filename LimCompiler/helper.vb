Module helper

    '===============================
    '========== VARIABLES ==========
    '===============================
    Private CompiledNameAlphabet As String = "abcdefghijklmnoqrstuvwxyz"
    Private variableCount As Integer = 0
    Private helpVariableCount As Integer = 0
    Private functionCount As Integer = 0
    Private structCount As Integer = 0

    Public compiledListTemplate As String
    Public compiledMapTemplate As String

    '============================
    '========= GET FILE =========
    '============================
    Public Function getFileFromNode(ByVal currentNode As Node) As FileNode

        Dim parentNode As Node = currentNode

        While Not parentNode.parentNode Is Nothing
            parentNode = parentNode.parentNode
            If TypeOf parentNode Is FileNode Then
                Exit While
            End If
        End While

        If Not TypeOf parentNode Is FileNode Then
            addCustomError("Internal Link error", "Cannot find the parent File")
        End If

        Return parentNode

    End Function

    '================================
    '========= GET COMPILER =========
    '================================
    Public Function getCompiler(ByVal currentNode As Node) As compiler

        Dim ParentFile As FileNode = getFileFromNode(currentNode)
        Return ParentFile.compiler

    End Function


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

    '=================================================
    '========== GENERATE HELP VARIABLE NAME ==========
    '=================================================
    Public Function getHelpVariableName() As String
        Return "hv_" & generateCompiledName(helpVariableCount)
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

    '==========================================
    '========== GENERATE STRUCT NAME ==========
    '==========================================
    Public Function getStructName() As String
        Return "s_" & generateCompiledName(structCount)
    End Function

End Module
