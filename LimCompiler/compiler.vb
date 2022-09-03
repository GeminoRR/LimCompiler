Imports System.IO
Public Class compiler


    '===============================
    '========== VARIABLES ==========
    '===============================
    Public spaces As New List(Of SpaceNode)
    Public compiledStructs As String
    Public compiledStructsLogicDefinitions As String
    Public compiledStructsLogic As String
    Public compiledFunctionsDefinitions As String
    Public compiledFunctions As String

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
            helper.compiledListTemplate = File.ReadAllText(templateFolder & "/list.c")
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try
        If Not File.Exists(templateFolder & "/map.c") Then
            addCustomError("Cannot read file", """" & templateFolder & "/map.c"" is missing")
        End If
        Try
            helper.compiledMapTemplate = File.ReadAllText(templateFolder & "/map.c")
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try

        'Add file
        Dim mainFile As New FileNode(target, Me)
        spaces.AddRange(mainFile.spaces)

        'Compile code
        mainFile.compile()

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
        If Not Directory.Exists(appData & "/compile/src") Then
            Directory.CreateDirectory(appData & "/compile/src")
        End If
        Dim finalCodePath As String = appData & "/compile/src/main.c"
        Try
            File.WriteAllText(finalCodePath, finalCode)
        Catch ex As Exception
            addCustomError("Cannot write file", ex.Message)
        End Try

        'Open file
        Process.Start("cmd.exe", "/c """ & finalCodePath & """")

    End Sub

End Class