Imports System.IO
Public Class CCompiler

    '===============================
    '========== VARIABLES ==========
    '===============================
    Public spaces As New List(Of SpaceNode)
    Public compiledStructs As String
    Public compiledStructsLogicDefinitions As String
    Public compiledStructsLogic As String
    Public compiledFunctionsDefinitions As String
    Public compiledFunctions As String

    Private CompiledNameAlphabet As String = "abcdefghijklmnoqrstuvwxyz"
    Private variableCount As Integer = 0
    Private helpVariableCount As Integer = 0
    Private functionCount As Integer = 0
    Private structCount As Integer = 0

    Public compiledListTemplate As String
    Public compiledMapTemplate As String

    '=============================
    '========== COMPILE ==========
    '=============================
    Public Sub compile(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Get template
        Dim headerTemplate As String = ""
        Dim templateFolder As String = System.Reflection.Assembly.GetExecutingAssembly().Location().Replace("\", "/")
        templateFolder = templateFolder.Substring(0, templateFolder.LastIndexOf("/")) & "/templates"
        If Not File.Exists(templateFolder & "/header.c") Then
            addCustomError("Cannot read file", """" & templateFolder & "/header.c"" is missing")
        End If
        Try
            headerTemplate = File.ReadAllText(templateFolder & "/header.c")
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try
        If Not File.Exists(templateFolder & "/list.c") Then
            addCustomError("Cannot read file", """" & templateFolder & "/list.c"" is missing")
        End If
        Try
            compiledListTemplate = File.ReadAllText(templateFolder & "/list.c")
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try
        If Not File.Exists(templateFolder & "/map.c") Then
            addCustomError("Cannot read file", """" & templateFolder & "/map.c"" is missing")
        End If
        Try
            compiledMapTemplate = File.ReadAllText(templateFolder & "/map.c")
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try

        'Add file
        Dim mainFile As New FileNode(target)
        spaces.AddRange(mainFile.spaces)

        'Compile code


        'Merge template & code
        Dim finalCode As String
        finalCode = headerTemplate & Environment.NewLine
        finalCode &= "////////////////////////" & Environment.NewLine & "//////// STRUCT ////////" & Environment.NewLine & "////////////////////////" & Environment.NewLine & compiledStructs & Environment.NewLine
        finalCode &= "/////////////////////////////////////////" & Environment.NewLine & "//////// STRUCT LOGIC DEFINTIONS ////////" & Environment.NewLine & "/////////////////////////////////////////" & Environment.NewLine & compiledStructsLogicDefinitions & Environment.NewLine
        finalCode &= "///////////////////////////////////////" & Environment.NewLine & "//////// FUNCTIONS DEFINITIONS ////////" & Environment.NewLine & "///////////////////////////////////////" & Environment.NewLine & compiledFunctionsDefinitions & Environment.NewLine
        finalCode &= "//////////////////////////////" & Environment.NewLine & "//////// STRUCT LOGIC ////////" & Environment.NewLine & "//////////////////////////////" & Environment.NewLine & compiledStructsLogic & Environment.NewLine
        finalCode &= "///////////////////////////" & Environment.NewLine & "//////// FUNCTIONS ////////" & Environment.NewLine & "///////////////////////////" & Environment.NewLine & compiledFunctions

        'Save code
        Dim appData As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\", "/") & "/Lim"
        If Not Directory.Exists(appData & "/compile/C/src") Then
            Directory.CreateDirectory(appData & "/compile/C/src")
        End If
        Dim finalCodePath As String = appData & "/compile/C/src/main.c"
        Try
            File.WriteAllText(finalCodePath, finalCode)
        Catch ex As Exception
            addCustomError("Cannot write file", ex.Message)
        End Try

        'Open file
        Process.Start("cmd.exe", "/c """ & finalCodePath & """")

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

End Class