﻿Imports System.IO
Public Class VB_Compiler

    '===============================
    '========== VARIABLES ==========
    '===============================
    Private init_space As SpaceNode
    Private compiledClass As String
    Private compiledFunctions As String
    Private compiledVariables As String

    Private CompiledNameAlphabet As String = "abcdefghijklmnoqrstuvwxyz"
    Private variableCount As Integer = 0
    Private helpVariableCount As Integer = 0
    Private functionCount As Integer = 0
    Private structCount As Integer = 0

    Private structSTR As StructNode
    Private structINT As StructNode
    Private structFLOAT As StructNode
    Private structBOOL As StructNode

    Private compilingType As compileWay

    Private appData As String
    Private compileFolder As String

    '=========================
    '========== RUN ==========
    '=========================
    Public Sub run(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Compile code
        compileCode(target, flags)

        'Run
        Console.Clear()
        Dim runtime As New Process()
        runtime.StartInfo.FileName = "cmd.exe"
        runtime.StartInfo.Arguments = "/c cd """ & compileFolder & """ & dotnet run"
        runtime.StartInfo.WorkingDirectory = Directory.GetParent(target).FullName
        runtime.Start()

    End Sub

    '==================================
    '========== COMPILE CODE ==========
    '==================================
    Public Sub compile(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Compile code
        compileCode(target, flags)

        'Compile file to target
        If Directory.Exists(appData & "/compile/final") Then
            Try
                Directory.Delete(appData & "/compile/final", True)
            Catch ex As Exception
                addCustomError("Can't delete folder", ex.Message)
            End Try
        End If
        Try
            Directory.CreateDirectory(appData & "/compile/final")
        Catch ex As Exception
            addCustomError("unable to create folder", ex.Message)
        End Try
        Dim dotnetCompiler As New Process()
        If flags.Contains("-windows") Then
            dotnetCompiler.StartInfo.FileName = "cmd.exe"
            dotnetCompiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            dotnetCompiler.StartInfo.Arguments = "/c cd """ & Directory.GetParent(compileFolder).FullName & """ & dotnet publish VB/VB.vbproj --configuration final --framework net6.0 --self-contained True --output Final --runtime win-x64 --verbosity Normal /property:PublishTrimmed=False /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False /property:EnableCompressionInSingleFile=True>nul"
        Else
            Throw New NotImplementedException()
        End If

        addLog("Final compiling")
        dotnetCompiler.Start()
        While Not dotnetCompiler.HasExited
            Console.Write(".")
            Threading.Thread.Sleep(200)
        End While
        Console.Write(Environment.NewLine)

        'Final stage
        If Not dotnetCompiler.ExitCode = 0 Then
            'Error
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("La compilation a réussi à échouer")
            Console.ResetColor()
            Console.WriteLine("An error occurred during the final stage of the compilation. Try again in another environment. If the problem persists please contact the developers.")
            endApp()
        End If

        'Move executable
        Try
            File.Move(Directory.GetFiles(appData & "/Compile/final")(0), output)
        Catch ex As Exception
            addCustomError("Unable to move a file", ex.Message)
        End Try

        'Sucess
        Console.ForegroundColor = ConsoleColor.DarkGreen
        Console.WriteLine("Compilation successful.")
        Console.ResetColor()

    End Sub

    '=============================
    '========== COMPILE ==========
    '=============================
    Private Sub compileCode(ByVal target As String, ByVal flags As List(Of String))

        'Set variables
        compiledVariables = ""
        compiledClass = ""
        compiledFunctions = ""
        variableCount = 0
        helpVariableCount = 0
        functionCount = 0
        structCount = 0
        compilingType = compileWay.console
        appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\", "/") & "/Lim"
        compileFolder = appData & "/compile/VB"

        'Get template
        Dim templateFolder As String = System.Reflection.Assembly.GetExecutingAssembly().Location().Replace("\", "/")
        templateFolder = templateFolder.Substring(0, templateFolder.LastIndexOf("/")) & "/templates/vb"

        'Generate AST
        Dim filesToImports As New HashSet(Of String)
        Dim mainFile As New FileNode(target, filesToImports)

        'Get entry point
        Dim structContext As New ElementPathNode(0, 0, New List(Of String))
        Dim graphicsDrawFunction As FunctionNode = Nothing
        Dim entryPoint As FunctionNode = Nothing
        For Each space As SpaceNode In mainFile.spaces
            If space.name = "__init__" Then
                init_space = space
                structContext.parentNode = space
                loadLib(templateFolder & "/io", space)
                For Each fun As Node In space.codes
                    If TypeOf fun Is FunctionNode Then
                        Dim castedFunction As FunctionNode = DirectCast(fun, FunctionNode)
                        If castedFunction.Name = "main" Then
                            entryPoint = castedFunction
                        ElseIf castedFunction.Name = "drawFrame" Then
                            graphicsDrawFunction = fun
                        End If
                    End If
                Next
                If Not entryPoint Is Nothing Then
                    Exit For
                End If
            End If
        Next
        If entryPoint Is Nothing Then
            addCustomError("ENTRY POINT MISSING", "The program has no entry point (""main"" function)")
        End If

        'Add dimensions
        compiledClass &= Environment.NewLine & Environment.NewLine & helper.getTemplate(templateFolder, "dimensions.vb")

        'Get structs
        structContext.childs = {"str"}.ToList()
        structSTR = getStruct(structContext)
        compileStruct(structSTR)

        structContext.childs = {"bool"}.ToList()
        structBOOL = getStruct(structContext)
        compileStruct(structBOOL)

        structContext.childs = {"int"}.ToList()
        structINT = getStruct(structContext)
        compileStruct(structINT)

        structContext.childs = {"float"}.ToList()
        structFLOAT = getStruct(structContext)
        compileStruct(structFLOAT)

        'Imports files
        While filesToImports.Count > 0

            Dim fileToImport As String = filesToImports(0)

            addLog("Importing <" & fileToImport & ">")

            Select Case fileToImport

                Case "graphics"
                    'GRAPHICS
                    loadLib(templateFolder & "/graphics", init_space)
                    If graphicsDrawFunction Is Nothing Then
                        'Console drawing
                        compilingType = compileWay.console_drawing

                    Else
                        'Window graphics
                        compilingType = compileWay.window
                        structContext.childs = {"image"}.ToList()
                        compileStruct(getStruct(structContext))

                    End If

                Case Else
                    'FILE IMPORT
                    If Not fileToImport.EndsWith(".lim") Then
                        addCustomError("Cannot import file", "The specified file is not a lim file : """ & fileToImport & """")
                    End If
                    If Not File.Exists(fileToImport) Then
                        addCustomError("Cannot import file", "The specified file is not accessible : """ & fileToImport & """")
                    End If
                    Dim importedFile As New FileNode(fileToImport, filesToImports)
                    For Each space As SpaceNode In importedFile.spaces
                        If space.name = "__init__" Then
                            For Each code As Node In space.codes
                                init_space.addNodeToCode(code)
                            Next
                        Else
                            init_space.codes.Add(space)
                        End If
                    Next

            End Select

            filesToImports.Remove(fileToImport)

        End While
        addLog("Successful code analysis")

        'Compile code
        compileFunction(entryPoint)

        'Compîle drawFrame(@screen:image)
        If compilingType = compileWay.window Then

            'Handle function problems
            If Not graphicsDrawFunction.Arguments.Count = 1 Then
                addSyntaxError("VBC01", "The drawFrame function must have as single argument ""@screen:image""", graphicsDrawFunction)
            End If
            Dim argumentType As safeType = typenodeToSafeType(graphicsDrawFunction.Arguments(0).type)
            If Not argumentType.Struct.compiledName = "image" Then
                addSyntaxError("VBC02", "The drawFrame function must have a reference to <image> as its only argument. Like this: ""func drawFrame(@screen:image)""", graphicsDrawFunction.Arguments(0).type)
            End If
            If Not graphicsDrawFunction.Arguments(0).ref = True Then
                addSyntaxError("VBC03", "The drawFrame function must have a reference to <image> as its only argument. Like this: ""func drawFrame(@screen:image)""", graphicsDrawFunction)
            End If
            If Not argumentType.Dimensions.Count = 0 Then
                addSyntaxError("VBC04", "The drawFrame function must have a reference to <image> as its only argument. Like this: ""func drawFrame(@screen:image)""", graphicsDrawFunction.Arguments(0).type)
            End If

            'Compile the function
            compileFunction(graphicsDrawFunction)

        End If

        'Clean code
        While compiledVariables.StartsWith(Environment.NewLine)
            compiledVariables = compiledVariables.Substring(Environment.NewLine.Length)
        End While
        While compiledClass.StartsWith(Environment.NewLine)
            compiledClass = compiledClass.Substring(Environment.NewLine.Length)
        End While
        While compiledFunctions.StartsWith(Environment.NewLine)
            compiledFunctions = compiledFunctions.Substring(Environment.NewLine.Length)
        End While

        'Get final code
        Dim finalCode As String = ""
        finalCode &= "'///////////////////////////" & Environment.NewLine & vbTab & "'//////// VARIABLES ////////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & compiledVariables.Replace(Environment.NewLine, Environment.NewLine & vbTab) & Environment.NewLine & Environment.NewLine & vbTab
        finalCode &= "'///////////////////////////" & Environment.NewLine & vbTab & "'////////// CLASS //////////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & compiledClass.Replace(Environment.NewLine, Environment.NewLine & vbTab) & Environment.NewLine & Environment.NewLine & vbTab
        finalCode &= "'///////////////////////////" & Environment.NewLine & vbTab & "'//////// FUNCTIONS ////////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & compiledFunctions.Replace(Environment.NewLine, Environment.NewLine & vbTab)
        addLog("Finish code compiling")

        'Reload compile environment
        Try
            If Directory.Exists(compileFolder) Then
                Directory.Delete(compileFolder, True)
            End If
            Directory.CreateDirectory(compileFolder)
        Catch ex As Exception
            addCustomError("Cannot reload directory", ex.Message)
        End Try

        'Get template
        Dim folderTemplatePath As String = ""
        Select Case compilingType

            Case compileWay.console
                folderTemplatePath = "console"

            Case compileWay.window
                folderTemplatePath = "window"

            Case compileWay.console_drawing
                folderTemplatePath = "console_drawing"

        End Select
        If Not Directory.Exists(templateFolder & "/compileWay/" & folderTemplatePath) Then
            addCustomError("File missing", "The """ & folderTemplatePath & """ model folder could not be found. Try reinstalling Lim.")
        End If

        'Copy template
        Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(templateFolder & "/compileWay/" & folderTemplatePath, compileFolder)

        'Check template file
        If Not File.Exists(compileFolder & "/Program.vb") Then
            addCustomError("File missing", """Program.vb"" is missing")
        End If

        'Write
        Dim Program As String = ""
        Try
            Program = File.ReadAllText(compileFolder & "/Program.vb")
        Catch ex As Exception
            addCustomError("Cannot read file", ex.Message)
        End Try

        'Add code
        Program = Program.Replace("'{CODES}'", finalCode)
        Program = Program.Replace("'{ENTRY_POINT}'", entryPoint.compiledName & "()")
        Select Case compilingType

            Case compileWay.window
                If graphicsDrawFunction Is Nothing Then
                    addCustomSyntaxWarning("VBCW01", "No ""drawFrame"" function was found in the __init__ space. The window will therefore remain empty throughout the execution of the program. Proposal: Add the following function: ""func drawFrame(@screen:image)""", mainFile.name)
                Else
                    Program = Program.Replace("'{DRAWFRAME}'", graphicsDrawFunction.compiledName)
                End If

        End Select

        'Write file
        Try
            File.WriteAllText(compileFolder & "/Program.vb", Program)
        Catch ex As Exception
            addCustomError("Cannot write file", ex.Message)
        End Try

        'Open file
        Process.Start("cmd.exe", "/c """ & compileFolder & "/Program.vb" & """")

    End Sub

    '=================================
    '========== COMPILE WAY ==========
    '=================================
    Private Enum compileWay
        console
        console_drawing
        window
    End Enum

    '==============================
    '========== LOAD LIB ==========
    '==============================
    Public Sub loadLib(ByVal path As String, ByVal space As SpaceNode)

        loadFunctions(path & "/functions", space)
        loadStructs(path & "/structs", space)

    End Sub

    '====================================
    '========== LOAD FUNCTIONS ==========
    '====================================
    Private Sub loadFunctions(ByVal path As String, ByVal space As SpaceNode)

        'Get structs
        For Each func_dat As String In Directory.GetFiles(path)

            'Check extension
            If Not func_dat.EndsWith(".dat") Then
                Continue For
            End If

            'Get infos
            Dim funcInfos As String = ""
            Try
                funcInfos = File.ReadAllText(func_dat)
            Catch ex As Exception
                addCustomError("read blueprint", ex.Message)
            End Try

            'Get function
            Dim fun As FunctionNode = StringToFunctionNode(funcInfos)

            'Get template
            Dim func_bp As String = func_dat.Substring(0, func_dat.Count - 3) & "vb"
            If Not File.Exists(func_bp) Then
                addLog("""" & func_bp & """ is missing")
                Continue For
            End If
            Try
                fun.compiledTemplate = File.ReadAllText(func_bp)
            Catch ex As Exception
                addCustomError("read blueprint", ex.Message)
            End Try

            'Add struct
            space.addNodeToCode(fun)

        Next

    End Sub

    '==================================
    '========== LOAD STRUCTS ==========
    '==================================
    Private Sub loadStructs(ByVal path As String, ByVal space As SpaceNode)

        'Get structs
        For Each struct_dat As String In Directory.GetFiles(path)

            'Check extension
            If Not struct_dat.EndsWith(".dat") Then
                Continue For
            End If

            'Get infos
            Dim structInfo As New List(Of String)
            Try
                structInfo = File.ReadAllText(struct_dat).Split(";").ToList()
            Catch ex As Exception
                addCustomError("read blueprint", ex.Message)
            End Try

            'Handle error
            If structInfo.Count < 4 Then
                addLog("Informations missing on """ & struct_dat & """")
                Continue For
            End If

            'Create struct
            Dim struct As New StructNode(0, 0, structInfo(0))

            'Get compiled name
            struct.Name = structInfo(0)
            struct.compiledName = structInfo(1)

            'Get template
            Dim struct_bp As String = struct_dat.Substring(0, struct_dat.Count - 3) & "vb"
            If Not File.Exists(struct_bp) Then
                addLog("""" & struct_bp & """ is missing")
                Continue For
            End If
            Try
                struct.compiledTemplate = File.ReadAllText(struct_bp)
            Catch ex As Exception
                addCustomError("read blueprint", ex.Message)
            End Try

            'Load propreties
            Dim propreties As New List(Of String) From {structInfo(2)}
            If propreties(0).Contains(",") Then
                propreties = propreties(0).Split(",").ToList()
            End If
            For Each propretie As String In propreties

                'Clear
                If propretie = "" Then
                    Continue For
                End If

                'Handle error
                If Not propretie.Contains(":") Then
                    addLog("Propretie invalid on """ & struct_dat & """")
                    Continue For
                End If

                'Divide
                Dim divider As List(Of String) = propretie.Split(":").ToList()

                'Handle error
                If Not divider.Count = 2 Then
                    addLog("Propertie invalid on """ & struct_dat & """")
                    Continue For
                End If

                'Type
                Dim isRef As Boolean = False
                If divider(1).StartsWith("@") Then
                    divider(1) = divider(1).Substring(1)
                    isRef = True
                End If

                'Add name
                struct.addNodeToCode(New DeclareVariableNode(0, 0, VariableDeclarationType._var_, divider(0), Nothing, StringToNodeType(divider(1)), isRef))


            Next

            'Add functions
            Dim functions As New List(Of String) From {structInfo(3)}
            If functions(0).Contains(",") Then
                functions = functions(0).Split(",").ToList()
            End If
            For Each func As String In functions

                'Clear
                If func = "" Then
                    Continue For
                End If

                'Add struct
                space.addNodeToCode(StringToFunctionNode(func))

            Next

            'Add space
            space.addNodeToCode(struct)

        Next

    End Sub

    '===================================
    '========== GENERATE NAME ==========
    '===================================
    Private Function generateCompiledName(ByRef counter As Integer) As String

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
    Private Function getHelpVariableName() As String
        Return "hv_" & generateCompiledName(helpVariableCount)
    End Function

    '============================================
    '========== GENERATE VARIABLE NAME ==========
    '============================================
    Private Function getVariableName() As String
        Return "v_" & generateCompiledName(variableCount)
    End Function

    '============================================
    '========== GENERATE FUNCTION NAME ==========
    '============================================
    Private Function getFunctionName() As String
        Return "f_" & generateCompiledName(functionCount)
    End Function

    '==========================================
    '========== GENERATE STRUCT NAME ==========
    '==========================================
    Private Function getStructName() As String
        Return "s_" & generateCompiledName(structCount)
    End Function

    '================================
    '========== GET STRUCT ==========
    '================================
    Private Function getStruct(ByVal path As Node) As StructNode

        'Child
        If TypeOf path Is childNode Then



        End If

        'Name
        Dim name As ElementPathNode = DirectCast(path, ElementPathNode)

        'Search stack
        Dim parentNode As Node = name
        While Not parentNode Is Nothing

            'Get parent
            parentNode = parentNode.parentNode

            'File node
            If TypeOf parentNode Is FileNode And name.childs.Count > 1 Then
                For Each space As SpaceNode In DirectCast(parentNode, FileNode).spaces
                    If space.name = name.childs(0) Then
                        Dim struct2 As StructNode = searchRecursivelyStruct(New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)), space)
                        If struct2 Is Nothing Then
                            addSyntaxError("VBGS02", "Unable to access struct """ & name.ToString() & """", name)
                        End If
                        Return struct2
                    End If
                Next
            End If

            'Spacenode
            If Not TypeOf parentNode Is containerNode Then
                Continue While
            End If

            'Container
            Dim struct As StructNode = searchRecursivelyStruct(New ElementPathNode(name.positionStart, name.positionEnd, name.childs), parentNode)
            If struct IsNot Nothing Then
                Return struct
            End If

        End While

        'Variable not found
        addSyntaxError("VBGS01", "Unable to access struct """ & name.ToString() & """", name)
        Return Nothing

    End Function
    Private Function searchRecursivelyStruct(ByVal name As ElementPathNode, ByVal context As containerNode) As StructNode

        'Get
        If name.childs.Count = 1 Then

            'Search declare variables
            If TypeOf context Is SpaceNode Then
                For Each action As Node In context.codes
                    If TypeOf action Is StructNode Then

                        'Cast node
                        Dim castedNode As StructNode = DirectCast(action, StructNode)

                        'Check name
                        If Not castedNode.Name = name.childs(0) Then
                            Continue For
                        End If

                        'Compile
                        compileStruct(castedNode)

                        'Return
                        Return castedNode

                    End If
                Next
            End If

        Else

            'Search space
            For Each node As Node In context.codes
                If TypeOf node Is SpaceNode Then

                    'Cast node
                    Dim castedSpace As SpaceNode = DirectCast(node, SpaceNode)

                    'Name
                    If castedSpace.name = name.childs(0) Then
                        Return searchRecursivelyStruct(New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)), castedSpace)
                    End If

                End If
            Next

        End If

        'Not find
        Return Nothing

    End Function

    '==================================
    '========== GET VARIABLE ==========
    '==================================
    Private Function getVariable(ByVal name As ElementPathNode) As Variable

        'Search stack
        Dim parentNode As Node = name
        While Not parentNode Is Nothing

            'Get parent
            parentNode = parentNode.parentNode

            'File node
            If TypeOf parentNode Is FileNode And name.childs.Count > 1 Then
                For Each space As SpaceNode In DirectCast(parentNode, FileNode).spaces
                    If space.name = name.childs(0) Then
                        Dim var2 As Variable = searchRecursivelyVariable(New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)), space)
                        If var2 Is Nothing Then
                            addSyntaxError("VBGV02", "Unable to access variable """ & name.ToString() & """", name)
                        End If
                        Return var2
                    End If
                Next
            End If

            'Spacenode
            If Not TypeOf parentNode Is containerNode Then
                Continue While
            End If

            'Container
            Dim var As Variable = searchRecursivelyVariable(New ElementPathNode(name.positionStart, name.positionEnd, name.childs), parentNode)
            If var IsNot Nothing Then
                Return var
            End If

        End While

        'Variable not found
        addSyntaxError("VBGV01", "Unable to access variable """ & name.ToString() & """", name)
        Return Nothing

    End Function
    Private Function searchRecursivelyVariable(ByVal name As ElementPathNode, ByVal context As containerNode) As Variable

        'Get
        If name.childs.Count = 1 Then

            'Search declare variables
            If TypeOf context Is SpaceNode Then
                For Each action As Node In context.codes
                    If TypeOf action Is DeclareVariableNode Then

                        'Cast node
                        Dim castedNode As DeclareVariableNode = DirectCast(action, DeclareVariableNode)

                        'Compiled
                        If castedNode.compiled Then
                            Continue For
                        End If

                        'Check name
                        If Not castedNode.variableName = name.childs(0) Then
                            Continue For
                        End If

                        'Check type ok
                        If Not castedNode.value Is Nothing Then
                            If Not ValueIsIndependent(castedNode.value) Then
                                addSyntaxError("VBRGV01", "Only constants can be entered as the initialization value of a variable here.", castedNode.value)
                            End If
                        End If

                        'Compile
                        Dim content As New List(Of String)
                        content.Add(compileDeclareVariable(castedNode, content))
                        Dim final As String = ""
                        For Each line As String In content
                            If Not line = "" Then
                                final &= Environment.NewLine & line
                            End If
                        Next
                        compiledVariables &= Environment.NewLine & final

                    End If
                Next
            End If

            'Search variable
            For Each var As Variable In context.variables
                If var.name = name.childs(0) Then
                    Return var
                End If
            Next

        Else

            'Search space
            For Each node As Node In context.codes
                If TypeOf node Is SpaceNode Then

                    'Cast node
                    Dim castedSpace As SpaceNode = DirectCast(node, SpaceNode)

                    'Name
                    If castedSpace.name = name.childs(0) Then
                        Return searchRecursivelyVariable(New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)), castedSpace)
                    End If

                End If
            Next

        End If

        'Not find
        Return Nothing

    End Function

    '==================================
    '========== GET FUNCTION ==========
    '==================================
    Private Function getFunction(ByVal name As ElementPathNode) As FunctionNode

        'Search stack
        Dim parentNode As Node = name
        While Not parentNode Is Nothing

            'Get parent
            parentNode = parentNode.parentNode

            'File node
            If TypeOf parentNode Is FileNode And name.childs.Count > 1 Then
                For Each space As SpaceNode In DirectCast(parentNode, FileNode).spaces
                    If space.name = name.childs(0) Then
                        Dim var2 As FunctionNode = searchRecursivelyFunction(New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)), space)
                        If var2 Is Nothing Then
                            addSyntaxError("VBGF02", "Unable to access function """ & name.ToString() & """", name)
                        End If
                        Return var2
                    End If
                Next
            End If

            'Spacenode
            If Not TypeOf parentNode Is SpaceNode Then
                Continue While
            End If

            'Container
            Dim var As FunctionNode = searchRecursivelyFunction(New ElementPathNode(name.positionStart, name.positionEnd, name.childs), parentNode)
            If var IsNot Nothing Then
                Return var
            End If

        End While

        'Variable not found
        addSyntaxError("VBGF01", "Unable to access function """ & name.ToString() & """", name)
        Return Nothing

    End Function
    Private Function searchRecursivelyFunction(ByVal name As ElementPathNode, ByVal context As SpaceNode) As FunctionNode

        'Get
        If name.childs.Count = 1 Then

            'Search for function
            For Each action As Node In context.codes
                If TypeOf action Is FunctionNode Then

                    'Cast node
                    Dim castedNode As FunctionNode = DirectCast(action, FunctionNode)

                    'Check name
                    If Not castedNode.Name = name.childs(0) Then
                        Continue For
                    End If

                    'Compile
                    compileFunction(castedNode)

                    'Return
                    Return castedNode

                End If
            Next

        Else

            'Search space
            For Each node As Node In context.codes
                If TypeOf node Is SpaceNode Then

                    'Cast node
                    Dim castedSpace As SpaceNode = DirectCast(node, SpaceNode)

                    'Name
                    If castedSpace.name = name.childs(0) Then
                        Return searchRecursivelyFunction(New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)), castedSpace)
                    End If

                End If
            Next

        End If

        'Not find
        Return Nothing

    End Function

    '======================================
    '========== TYPENODE TO TYPE ==========
    '======================================
    Private Function typenodeToSafeType(ByVal typenode As typeNode) As safeType

        Dim struct As StructNode = getStruct(typenode.StructName)
        compileStruct(struct)
        Return New safeType(struct, typenode.Dimensions)

    End Function

    '==================================
    '========== COMPILE TYPE ==========
    '==================================
    Private Function compileType(ByVal type As safeType) As String

        'Variable
        Dim result As String

        'Compile
        If type.Dimensions.Count > 0 Then
            If type.Dimensions(type.Dimensions.Count - 1) = ValueType.list Then
                result = String.Format("LimList(Of {0})", compileType(type.getParentType()))
            ElseIf type.Dimensions(type.Dimensions.Count - 1) = ValueType.map Then
                result = String.Format("LimMap(Of {0})", compileType(type.getParentType()))
            Else
                Throw New NotImplementedException()
            End If
        Else
            result = type.Struct.compiledName
        End If

        'Return
        Return result

    End Function

    '======================================
    '========== COMPILE FUNCTION ==========
    '======================================
    Private Function compileFunction(ByRef fun As FunctionNode) As String

        'State handler
        If fun.compiled Then
            If fun.compiling Then
                addSyntaxError("VBCCF01", "The type of this function must be explicitly noted by its use. Example: ""func myFunction():str[]""", fun)
            End If
            Exit Function
        End If
        fun.compiled = True
        fun.compiling = True

        'STD
        If stdFunctions(fun) Then
            Exit Function
        End If

        'Struct
        Dim parentStruct As StructNode = Nothing
        If TypeOf fun.parentNode Is StructNode Then
            parentStruct = DirectCast(fun.parentNode, StructNode)
        End If

        'Get unsafe type
        If Not fun.unsafeReturnType Is Nothing Then
            fun.ReturnType = typenodeToSafeType(fun.unsafeReturnType)
        End If

        'Get name
        If fun.compiledName = "" Then
            fun.compiledName = getFunctionName()
        End If

        'Some variables
        Dim content As New List(Of String)

        'Argument list
        Dim compiled_arguments As String = ""
        For Each arg As FunctionArgument In fun.Arguments
            Dim var As New Variable(arg.name, typenodeToSafeType(arg.type), getVariableName())
            compiled_arguments &= ", " & var.compiledName & " As " & compileType(var.type)
            var.ref = arg.ref
            fun.variables.Add(var)
        Next
        If compiled_arguments.StartsWith(", ") Then
            compiled_arguments = compiled_arguments.Substring(2)
        End If

        'Compile content
        For Each action As Node In fun.codes

            Dim compiledAction As String = compileNode(action, content)
            If Not compiledAction = "" Then
                content.Add(compiledAction)
            End If

        Next

        'Final
        Dim finalString As String
        If fun.ReturnType Is Nothing Then
            finalString = "Public Sub " & fun.compiledName & "(" & compiled_arguments & ")" & Environment.NewLine
        Else
            finalString = "Public Function " & fun.compiledName & "(" & compiled_arguments & ") As " & compileType(fun.ReturnType) & Environment.NewLine
        End If
        For i As Integer = 0 To content.Count - 1
            finalString &= Environment.NewLine & vbTab & content(i)
        Next
        If fun.ReturnType Is Nothing Then
            finalString &= Environment.NewLine & Environment.NewLine & "End Sub"
        Else
            finalString &= Environment.NewLine & Environment.NewLine & vbTab & "'Return empty" & Environment.NewLine & vbTab & "Return Nothing" & Environment.NewLine & Environment.NewLine & "End Function"
        End If
        finalString = "'" & fun.Name & Environment.NewLine & finalString

        'End compiling
        fun.compiling = False

        'Return
        If parentStruct Is Nothing Then
            compiledFunctions &= Environment.NewLine & Environment.NewLine & finalString
            Return ""
        Else
            Return finalString
        End If

    End Function

    '===================================
    '========== STD FUNCTIONS ==========
    '===================================
    Private Function stdFunctions(ByVal func As FunctionNode) As Boolean

        'Check
        If func.compiledTemplate = "" Then
            Return False
        End If

        'Get unsafe type
        If func.unsafeReturnType.Dimensions.Count = 0 And func.unsafeReturnType.StructName.childs.Count = 1 And func.unsafeReturnType.StructName.childs(0) = "null" Then
            func.ReturnType = Nothing
        Else
            func.ReturnType = typenodeToSafeType(func.unsafeReturnType)
        End If

        'Add template
        compiledFunctions &= Environment.NewLine & Environment.NewLine & func.compiledTemplate

        'Compiling
        func.compiling = False

        'Return
        Return True

    End Function

    '====================================
    '========== COMPILE STRUCT ==========
    '====================================
    Private Sub compileStruct(ByRef struct As StructNode)
        'State handler
        If struct.compiled Then
            Exit Sub
        End If
        struct.compiled = True

        'STD
        If stdStructs(struct) Then
            Exit Sub
        End If

        'Get name
        struct.compiledName = getStructName()

        'Some variables
        Dim content As New List(Of String)
        Dim variablesDefinitions As New List(Of DeclareVariableNode)
        Dim functions As New List(Of FunctionNode)

        'Sort
        For Each line As Node In struct.codes

            If TypeOf line Is DeclareVariableNode Then
                variablesDefinitions.Add(DirectCast(line, DeclareVariableNode))

            ElseIf TypeOf line Is FunctionNode Then
                functions.Add(DirectCast(line, FunctionNode))

            Else
                Throw New NotImplementedException()

            End If

        Next

        'Get variables
        If variablesDefinitions.Count > 0 Then
            content.Add("'Variables")
            For Each def As DeclareVariableNode In variablesDefinitions
                Dim var As New Variable(def.variableName, Nothing, getVariableName())
                If def.variableUnsafeType Is Nothing Then

                    'Check if brut value
                    If Not ValueIsIndependent(def.value) Then
                        addSyntaxError("VBCS01", "Only constants can be entered as the initialization value of a variable here.", def.value)
                    End If

                    'Set type
                    var.type = getNodeType(def.value)

                    'Compile
                    content.Add(String.Format("Public {0} As {1} = {2}", var.compiledName, compileType(var.type), compileNode(def.value, content)))

                Else

                    'Set type
                    var.type = typenodeToSafeType(def.variableUnsafeType)

                    'Compile
                    content.Add(String.Format("Public {0} As {1}", var.compiledName, compileType(var.type)))

                End If
                struct.variables.Add(var)
            Next
        End If

        'Get functions
        If functions.Count > 0 Then
            content.Add("'Methods")
            For Each def As FunctionNode In functions

                If def.Name = "create" Then

                    content.Add(compileFunction(def))

                End If

            Next
        End If

        'Final
        Dim finalString As String = "Public Class " & struct.compiledName & Environment.NewLine
        For i As Integer = 0 To content.Count - 1
            finalString &= Environment.NewLine & vbTab & content(i)
        Next
        finalString &= Environment.NewLine & Environment.NewLine & "End Class"
        compiledClass &= Environment.NewLine & Environment.NewLine & "'" & struct.Name & Environment.NewLine & finalString

    End Sub

    '=================================
    '========== STD STRUCTS ==========
    '=================================
    Private Function stdStructs(ByVal struct As StructNode) As Boolean

        'Check
        If struct.compiledTemplate = "" Then
            Return False
        End If

        'Add variables
        For Each action As Node In struct.codes
            If TypeOf action Is DeclareVariableNode Then

                'Cast
                Dim castedAction As DeclareVariableNode = DirectCast(action, DeclareVariableNode)

                'Variable
                Dim var As New Variable(castedAction.variableName, typenodeToSafeType(castedAction.variableUnsafeType), castedAction.variableName, castedAction.variableIsReference)

                'Add variable
                struct.variables.Add(var)

            End If
        Next

        'Add template
        compiledClass &= Environment.NewLine & Environment.NewLine & struct.compiledTemplate

        'Return
        Return True

    End Function

    '==========================================
    '========== VALUE IS INDEPENDENT ==========
    '==========================================
    Private Function ValueIsIndependent(ByVal value As Node) As Boolean

        'Value
        If TypeOf value Is valueNode Then
            Return True
        End If

        'String
        If TypeOf value Is StringNode Then
            Return True
        End If

        'Boolean
        If TypeOf value Is BooleanNode Then
            Return True
        End If

        'UnaryOp
        If TypeOf value Is UnaryOpNode Then

            'Cast
            Dim castedNode As UnaryOpNode = DirectCast(value, UnaryOpNode)

            'Get value
            Return ValueIsIndependent(castedNode.node)

        End If

        'ListNode
        If TypeOf value Is ListNode Then

            'Cast
            Dim castedNode As ListNode = DirectCast(value, ListNode)

            'Loop each element
            For Each elm As Node In castedNode.elements
                If Not ValueIsIndependent(elm) Then
                    Return False
                End If
            Next

            'Get value
            Return True

        End If

        'ListMap
        If TypeOf value Is MapNode Then

            'Cast
            Dim castedNode As MapNode = DirectCast(value, MapNode)

            'Loop each element
            For Each elm As List(Of Node) In castedNode.elements
                If Not ValueIsIndependent(elm(1)) Then
                    Return False
                End If
            Next

            'Get value
            Return True

        End If

        'Return
        Return False

    End Function

    '===================================
    '========== GET NODE TYPE ==========
    '===================================
    Private Function getNodeType(ByVal node As Node) As safeType

        'Unsafe type
        If TypeOf node Is typeNode Then

            'Return
            Return typenodeToSafeType(DirectCast(node, typeNode))

        End If

        'valueNode
        If TypeOf node Is valueNode Then

            'Casted Node
            Dim castedNode As valueNode = DirectCast(node, valueNode)

            'Return
            Select Case castedNode.tok.type
                Case tokenType.CT_INTEGER
                    Return New safeType(structINT)

                Case tokenType.CT_FLOAT
                    Return New safeType(structFLOAT)

                Case Else
                    addCustomError("Type error", "Cannot get type of the folowing token <" & castedNode.tok.ToString() & ">")
                    Return Nothing

            End Select

        End If

        'UnaryOpNode
        If TypeOf node Is UnaryOpNode Then

            'Casted node
            Dim castedNode As UnaryOpNode = DirectCast(node, UnaryOpNode)

            'Return
            Select Case castedNode.op.type
                Case tokenType.OP_MINUS
                    Return getNodeType(castedNode.node)

                Case tokenType.OP_PLUS
                    Return getNodeType(castedNode.node)

                Case Else
                    addCustomError("Type error", "Cannot get type of the folowing token <" & castedNode.op.ToString() & ">")
                    Return Nothing

            End Select

        End If

        'binOpNode
        If TypeOf node Is binOpNode Then

            'Casted node
            Dim castedNode As binOpNode = DirectCast(node, binOpNode)

            'Get type
            Dim left As safeType = getNodeType(castedNode.leftNode)
            Dim right As safeType = getNodeType(castedNode.rightNode)

            'Handle
            If left.Dimensions.Count > 0 Then
                addSyntaxError("VBGNT02", "The subtraction operation cannot be performed on a list", castedNode.leftNode)
            End If
            If right.Dimensions.Count > 0 Then
                addSyntaxError("VBGNT03", "The subtraction operation cannot be performed on a list", castedNode.rightNode)
            End If

            'Type
            Select Case castedNode.op.type
                Case tokenType.OP_MINUS
                    If left.Struct.compiledName = right.Struct.compiledName Then
                        Return New safeType(structINT)
                    End If
                    Return New safeType(structFLOAT)

                Case tokenType.OP_PLUS
                    If left.Struct.compiledName = "str" And right.Struct.compiledName = "str" Then
                        Return New safeType(structSTR)
                    ElseIf left.Struct.compiledName = "int" And right.Struct.compiledName = "int" Then
                        Return New safeType(structINT)
                    End If
                    Return New safeType(structFLOAT)

                Case tokenType.OP_MULTIPLICATION
                    If left.Struct.compiledName = "int" And right.Struct.compiledName = "int" Then
                        Return New safeType(structINT)
                    ElseIf left.Struct.compiledName = "int" And right.Struct.compiledName = "str" Then
                        Return New safeType(structSTR)
                    ElseIf left.Struct.compiledName = "str" And right.Struct.compiledName = "int" Then
                        Return New safeType(structSTR)
                    End If
                    Return New safeType(structFLOAT)

                Case tokenType.OP_MODULO
                    Return New safeType(structINT)
                    'TODO: Better modulo type

                Case tokenType.OP_DIVISION
                    Return New safeType(structFLOAT)
                    'TODO: Better modulo type

                Case Else
                    'Problem go brrr
                    Throw New NotImplementedException()

            End Select

        End If

        'StringNode
        If TypeOf node Is StringNode Then

            'Return
            Return New safeType(structSTR)

        End If

        'Boolean
        If TypeOf node Is BooleanNode Then

            'Return
            Return New safeType(structBOOL)

        End If

        'ComparisonNode
        If TypeOf node Is ComparisonNode Then

            'Return
            Return New safeType(structBOOL)

        End If

        'VariableNode
        If TypeOf node Is VariableNode Then

            'Castednode
            Dim castedNode As VariableNode = DirectCast(node, VariableNode)

            'Get variable
            Dim var As Variable = getVariable(castedNode.VariableName)

            'Return
            Return var.type

        End If

        'FunctionCallNode
        If TypeOf node Is FunctionCallNode Then

            'Castednode
            Dim castedNode As FunctionCallNode = DirectCast(node, FunctionCallNode)

            'Get variable
            Dim func As FunctionNode = getFunction(castedNode.FunctionPath)

            'Return
            Return func.ReturnType

        End If

        'BracketsSelectorNode
        If TypeOf node Is BracketsSelectorNode Then

            'Castednode
            Dim castedNode As BracketsSelectorNode = DirectCast(node, BracketsSelectorNode)

            'Get type
            Dim valueType As safeType = getNodeType(castedNode.Target)

            'Return
            Return valueType.getParentType()

        End If

        'ListNode
        If TypeOf node Is ListNode Then

            'Castednode
            Dim castedNode As ListNode = DirectCast(node, ListNode)

            'Handle no value
            If castedNode.elements.Count = 0 Then
                addSyntaxError("VBGNT04", "A list cannot be empty, as this does not identify its type.", node)
            End If

            'Get type
            Dim type As safeType = getNodeType(castedNode.elements(0))
            type.Dimensions.Add(ValueType.list)

            'Get type
            Return type

        End If

        'MapNode
        If TypeOf node Is MapNode Then

            'Castednode
            Dim castedNode As MapNode = DirectCast(node, MapNode)

            'Handle no value
            If castedNode.elements.Count = 0 Then
                addSyntaxError("VBGNT05", "A map cannot be empty, as this does not identify its type.", node)
            End If

            'Get type
            Dim type As safeType = getNodeType(castedNode.elements(0)(1))
            type.Dimensions.Add(ValueType.map)

            'Get type
            Return type

        End If

        'ChildNode
        If TypeOf node Is childNode Then

            'Castednode
            Dim castedNode As childNode = DirectCast(node, childNode)

            'Get struct
            Dim struct As StructNode = getStruct(Nothing)

            'Get ''

        End If

        'Return
        addSyntaxError("VBGNT01", "Unable to resolve node type", node)
        Return Nothing

    End Function

    '==================================
    '========== COMPILE NODE ==========
    '==================================
    Private Function compileNode(ByVal node As Node, ByVal content As List(Of String)) As String

        If TypeOf node Is DeclareVariableNode Then

            'Compile
            Return compileDeclareVariable(DirectCast(node, DeclareVariableNode), content)

        ElseIf TypeOf node Is SetVariableNode Then

            'Compîle
            Return compileSetVariable(DirectCast(node, SetVariableNode), content)

        ElseIf TypeOf node Is FunctionCallNode Then

            'Compile
            Return compileFunctionCall(DirectCast(node, FunctionCallNode), content)

        ElseIf TypeOf node Is StringNode Then

            'Compile
            Return compileString(DirectCast(node, StringNode), content)

        ElseIf TypeOf node Is valueNode Then

            'Compile
            Return compileValue(DirectCast(node, valueNode), content)

        ElseIf TypeOf node Is BooleanNode Then

            'Compile
            Return compileBoolean(DirectCast(node, BooleanNode), content)

        ElseIf TypeOf node Is UnaryOpNode Then

            'Compile
            Return compileUnaryOp(DirectCast(node, UnaryOpNode), content)

        ElseIf TypeOf node Is binOpNode Then

            'Compile
            Return compileBinOp(DirectCast(node, binOpNode), content)

        ElseIf TypeOf node Is VariableNode Then

            'Compile
            Return compileVariable(DirectCast(node, VariableNode), content)

        ElseIf TypeOf node Is BracketsSelectorNode Then

            'Compile
            Return compileBracketsSelector(DirectCast(node, BracketsSelectorNode), content)

        ElseIf TypeOf node Is ListNode Then

            'Compile
            Return compileList(DirectCast(node, ListNode), content)

        ElseIf TypeOf node Is MapNode Then

            'Compile
            Return compileMap(DirectCast(node, MapNode), content)

        End If

        'Return
        addSyntaxError("VBCN01", "Unable to resolve node type", node)
        Return Nothing

    End Function

    '======================================
    '========== COMPILE VARIABLE ==========
    '======================================
    Public Function compileVariable(ByVal node As VariableNode, ByVal content As List(Of String)) As String

        'Get variable
        Dim var As Variable = getVariable(node.VariableName)

        'Return
        Return var.compiledName

    End Function

    '====================================
    '========== COMPILE STRING ==========
    '====================================
    Public Function compileString(ByVal node As StringNode, ByVal content As List(Of String)) As String

        'Return
        Return "New str(""" & node.value.ToString() & """)"

    End Function

    '===================================
    '========== COMPILE VALUE ==========
    '===================================
    Public Function compileValue(ByVal node As valueNode, ByVal content As List(Of String)) As String

        'Return
        Select Case node.tok.type

            Case tokenType.CT_FLOAT
                Return "New float(" & node.tok.value.ToString() & ")"

            Case tokenType.CT_INTEGER
                Return "New int(" & node.tok.value.ToString() & ")"

            Case Else
                Throw New NotImplementedException

        End Select

    End Function

    '=====================================
    '========== COMPILE BOOLEAN ==========
    '=====================================
    Public Function compileBoolean(ByVal node As BooleanNode, ByVal content As List(Of String)) As String

        'Return
        If node.value Then
            Return "New bool(True)"
        Else
            Return "New bool(False)"
        End If

    End Function

    '======================================
    '========== COMPILE UNARY OP ==========
    '======================================
    Public Function compileUnaryOp(ByVal node As UnaryOpNode, ByVal content As List(Of String)) As String

        'Get value type
        Dim valueType As safeType = getNodeType(node.node)
        If valueType.Dimensions.Count > 0 Then
            addSyntaxError("VBCUO02", "The """ & node.op.ToString() & """ operator cannot be applied to a <" & valueType.ToString() & ">", node)
        End If

        'Return
        Select Case node.op.type

            Case tokenType.OP_PLUS
                If valueType.Struct.compiledName = "int" Then
                    Return "New int(+(" & compileNode(node.node, content) & ".value))"
                ElseIf valueType.Struct.compiledName = "float" Then
                    Return "New float(+(" & compileNode(node.node, content) & ".value))"
                End If

            Case tokenType.OP_MINUS
                If valueType.Struct.compiledName = "int" Then
                    Return "New int(-(" & compileNode(node.node, content) & ".value))"
                ElseIf valueType.Struct.compiledName = "float" Then
                    Return "New float(-(" & compileNode(node.node, content) & ".value))"
                End If

            Case Else
                Throw New NotImplementedException

        End Select

        'Error
        addSyntaxError("VBCUO01", "The """ & node.op.ToString() & """ operator cannot be applied to a <" & valueType.ToString() & ">", node)
        Return Nothing

    End Function

    '===============================================
    '========== COMPILE BRACKETS SELECTOR ==========
    '===============================================
    Public Function compileBracketsSelector(ByVal node As BracketsSelectorNode, ByVal content As List(Of String)) As String

        'Variables
        Dim targetType As safeType = getNodeType(node.Target)

        'Handle error
        If Not targetType.Dimensions.Count > 0 Then
            addSyntaxError("VBCBS01", "Chosen item must be at least map or list to select index/key", node.Target)
        End If
        If Not {ValueType.list, ValueType.map}.Contains(targetType.Dimensions(targetType.Dimensions.Count - 1)) Then
            addSyntaxError("VBCBS02", "Chosen item must be at least map or list to select index/key", node.Target)
        End If

        'Get type
        Dim indexType As safeType = getNodeType(node.index)

        'Compile
        Select Case targetType.Dimensions(targetType.Dimensions.Count - 1)
            Case ValueType.list
                If Not (indexType.Struct.compiledName = "int" And indexType.Dimensions.Count = 0) Then
                    addSyntaxError("VBCBS03", "An index of a list must be an integer (<int>)", node.index)
                End If
                Return compileNode(node.Target, content) & "(" & compileNode(node.index, content) & ".value)"

            Case ValueType.map
                If Not (indexType.Struct.compiledName = "str" And indexType.Dimensions.Count = 0) Then
                    addSyntaxError("VBCBS04", "An key name of a map must be an string (<str>)", node.index)
                End If
                Return compileNode(node.Target, content) & "(" & compileNode(node.index, content) & ".value)"

        End Select

        'Error
        Return ""

    End Function

    '====================================
    '========== COMPILE BIN OP ==========
    '====================================
    Public Function compileBinOp(ByVal node As binOpNode, ByVal content As List(Of String)) As String

        'Get value type
        Dim leftType As safeType = getNodeType(node.leftNode)
        Dim rightType As safeType = getNodeType(node.rightNode)

        'Return
        Select Case node.op.type

            Case tokenType.OP_PLUS
                'ADDITIONS

                'Check error
                If leftType.Dimensions.Count > 0 Or rightType.Dimensions.Count > 0 Then
                    addSyntaxError("VBCBO01", "Both elements cannot be a list to allow a ""+"" operation.", node)
                End If

                'Number operation
                If leftType.Struct.compiledName = "int" And rightType.Struct.compiledName = "int" Then
                    Return String.Format("New int({0}.value + {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If
                If (leftType.Struct.compiledName = "int" Or leftType.Struct.compiledName = "float") And (rightType.Struct.compiledName = "int" Or rightType.Struct.compiledName = "float") Then
                    Return String.Format("New float({0}.value + {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If

                'String operation
                If leftType.Struct.compiledName = "str" And rightType.Struct.compiledName = "str" Then
                    Return String.Format("New str({0}.value & {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If

            Case tokenType.OP_MINUS
                'SUBSTRACTION

                'Check error
                If leftType.Dimensions.Count > 0 Or rightType.Dimensions.Count > 0 Then
                    addSyntaxError("VBCBO02", "Both elements cannot be a list to allow a ""-"" operation.", node)
                End If

                'Number operation
                If leftType.Struct.compiledName = "int" And rightType.Struct.compiledName = "int" Then
                    Return String.Format("New int({0}.value - {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If
                If (leftType.Struct.compiledName = "int" Or leftType.Struct.compiledName = "float") And (rightType.Struct.compiledName = "int" Or rightType.Struct.compiledName = "float") Then
                    Return String.Format("New float({0}.value - {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If

            Case tokenType.OP_MULTIPLICATION
                'MULTIPLICATION

                'Check error
                If leftType.Dimensions.Count > 0 Or rightType.Dimensions.Count > 0 Then
                    addSyntaxError("VBCBO03", "Both elements cannot be a list to allow a ""*"" operation.", node)
                End If

                'Number operation
                If leftType.Struct.compiledName = "int" And rightType.Struct.compiledName = "int" Then
                    Return String.Format("New int({0}.value * {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If
                If (leftType.Struct.compiledName = "int" Or leftType.Struct.compiledName = "float") And (rightType.Struct.compiledName = "int" Or rightType.Struct.compiledName = "float") Then
                    Return String.Format("New float({0}.value * {1}.value)", compileNode(node.leftNode, content), compileNode(node.rightNode, content))
                End If

                'String & number
                If leftType.Struct.compiledName = "int" And rightType.Struct.compiledName = "str" Then
                    Dim helpVar As String = getHelpVariableName()
                    Dim iVar As String = getHelpVariableName()
                    content.Add(String.Format("Dim {0} As String = """"", helpVar))
                    content.Add(String.Format("For {0} As Integer = 0 To {1}.value - 1", iVar, compileNode(node.leftNode, content)))
                    content.Add(String.Format("{0}{1} &= {2}.value", vbTab, helpVar, compileNode(node.rightNode, content)))
                    content.Add("Next")
                    Return String.Format("New str({0})", helpVar)
                End If

                'Number & String
                If leftType.Struct.compiledName = "str" And rightType.Struct.compiledName = "int" Then
                    Dim helpVar As String = getHelpVariableName()
                    Dim iVar As String = getHelpVariableName()
                    content.Add(String.Format("Dim {0} As String = """"", helpVar))
                    content.Add(String.Format("For {0} As Integer = 0 To {1}.value - 1", iVar, compileNode(node.rightNode, content)))
                    content.Add(String.Format("{0}{1} &= {2}.value", vbTab, helpVar, compileNode(node.leftNode, content)))
                    content.Add("Next")
                    Return String.Format("New str({0})", helpVar)
                End If

        End Select

        'Error
        addSyntaxError("VBCBO01", "The """ & node.op.ToString() & """ operation between a member of type <" & leftType.ToString() & "> and <" & rightType.ToString() & "> is not possible.", node)
        Return Nothing

    End Function

    '==================================
    '========== COMPILE LIST ==========
    '==================================
    Public Function compileList(ByVal node As ListNode, ByVal content As List(Of String)) As String

        'Handle no value
        If node.elements.Count = 0 Then
            addSyntaxError("VBCL01", "A list cannot be empty, as this does not identify its type.", node)
        End If

        'Get type
        Dim listType As safeType = getNodeType(node.elements(0))

        'Compile each arguments
        Dim helpVar As String = getHelpVariableName()
        content.Add(String.Format("Dim {0} As New LimList(Of {1})", helpVar, compileType(listType)))
        For Each elm As Node In node.elements

            'Handle type error
            Dim elmType As safeType = getNodeType(elm)
            If Not listType.IsTheSameAs(elmType) Then
                addSyntaxError("VBCL02", "The following element <" & elmType.ToString() & "> is not of the type of the list <" & listType.ToString() & ">", elm)
            End If

            'Compile
            content.Add(String.Format("{0}.Add({1})", helpVar, compileNode(elm, content)))

        Next

        'Finish compile
        Return helpVar

    End Function

    '=================================
    '========== COMPILE MAP ==========
    '=================================
    Public Function compileMap(ByVal node As MapNode, ByVal content As List(Of String)) As String

        'Handle no value
        If node.elements.Count = 0 Then
            addSyntaxError("VBCM01", "A list cannot be empty, as this does not identify its type.", node)
        End If

        'Get type
        Dim valueType As safeType = getNodeType(node.elements(0)(1))
        Dim helpVar As String = getHelpVariableName()
        content.Add(String.Format("Dim {0} As New LimMap(Of {1})", helpVar, valueType))

        'Compile each arguments
        For Each elm As List(Of Node) In node.elements

            'Handle type error
            Dim elmType As safeType = getNodeType(elm(1))
            If Not valueType.IsTheSameAs(elmType) Then
                addSyntaxError("VBCM02", "The following element <" & elmType.ToString() & "> is not of the type of the map <" & valueType.ToString() & ">", elm(1))
            End If
            Dim keyType As safeType = getNodeType(elm(0))
            If Not (keyType.Dimensions.Count = 0 And keyType.Struct.compiledName = "str") Then
                addSyntaxError("VBCM03", "The key must be of type <str>, but is of type <" & keyType.ToString() & ">", elm(0))
            End If

            'Compile
            content.Add(String.Format("{0}.TryAdd({1}.value, {2})", helpVar, compileNode(elm(0), content), compileNode(elm(1), content)))

        Next

        'Finish compile
        Return helpVar

    End Function

    '===========================================
    '========== COMPILE FUNCTION CALL ==========
    '===========================================
    Private Function compileFunctionCall(ByVal node As FunctionCallNode, ByVal content As List(Of String)) As String

        'Get function
        Dim fun As FunctionNode = getFunction(node.FunctionPath)

        'Handle argument error
        If node.Arguments.Count < fun.Arguments.Count Then
            addSyntaxError("VBCFC01", (fun.Arguments.Count - node.Arguments.Count).ToString() & " arguments are missing", node)
        End If
        If node.Arguments.Count > fun.Arguments.Count Then
            addSyntaxError("VBCFC02", (node.Arguments.Count - fun.Arguments.Count).ToString() & " arguments are useless (too many arguments)", node)
        End If

        'Argument
        Dim arguments As String = ""
        For i As Integer = 0 To fun.Arguments.Count - 1

            'Variables
            Dim argModel As FunctionArgument = fun.Arguments(i)
            Dim argNode As Node = node.Arguments(i)

            'Handle type error
            If Not (typenodeToSafeType(argModel.type).IsTheSameAs(getNodeType(argNode))) Then
                addSyntaxError("VBCFC03", "The " & (i + 1).ToString() & " argument is of type <" & getNodeType(argNode).ToString() & "> instead of being <" & argModel.type.ToString() & ">", argNode)
            End If

            'Add node
            If argModel.ref Then
                arguments &= ", " & compileNode(argNode, content)
            Else
                arguments &= ", " & compileNode(argNode, content) & ".Clone()"
            End If

        Next
        If arguments.StartsWith(", ") Then
            arguments = arguments.Substring(2)
        End If

        'Return
        Return fun.compiledName & "(" & arguments & ")"

    End Function

    '==========================================
    '========== COMPILE SET VARIABLE ==========
    '==========================================
    Private Function compileSetVariable(ByVal node As SetVariableNode, ByVal content As List(Of String)) As String

        'Set variable
        Dim castedNode As SetVariableNode = DirectCast(node, SetVariableNode)

        'Get type
        Dim variableType As safeType = getNodeType(castedNode.Target)
        Dim valueType As safeType = getNodeType(castedNode.NewValue)

        'Handle error
        If Not variableType.IsTheSameAs(valueType) Then
            addSyntaxError("VBCSV01", "Cannot change a value of type <" & variableType.ToString() & "> to <" & valueType.ToString() & ">", castedNode)
        End If

        'Compile
        content.Add(String.Format("{0} = {1}.Clone()", compileNode(castedNode.Target, content), compileNode(castedNode.NewValue, content)))

        'Return
        Return ""

    End Function

    '==============================================
    '========== COMPILE DECLARE VARIABLE ==========
    '==============================================
    Private Function compileDeclareVariable(ByVal node As DeclareVariableNode, ByVal content As List(Of String)) As String

        'Declare variable
        Dim castedNode As DeclareVariableNode = DirectCast(node, DeclareVariableNode)
        castedNode.compiled = True
        content.Add(String.Format("'{0}", castedNode.variableName))

        'Set variable
        Dim var As New Variable(castedNode.variableName, Nothing, getVariableName(), castedNode.variableIsReference)

        'Get type
        If castedNode.value Is Nothing Then

            'Set type
            var.type = typenodeToSafeType(castedNode.variableUnsafeType)

            'List or something
            If var.type.Dimensions.Count > 0 Then
                'New
                content.Add(String.Format("Dim {0} As New {1}", var.compiledName, compileType(var.type)))
            Else
                'Normal
                content.Add(String.Format("Dim {0} As {1} = Nothing", var.compiledName, compileType(var.type)))
            End If

        Else

            'Set type
            var.type = getNodeType(castedNode.value)
            If Not castedNode.variableUnsafeType Is Nothing Then
                Dim defType As safeType = typenodeToSafeType(castedNode.variableUnsafeType)
                If Not var.type.IsTheSameAs(defType) Then
                    addSyntaxError("VBCCF02", "The defined type of the variable is not the same as that of its value.", castedNode.value)
                End If
            End If

            'Ref
            If var.ref Then
                'Is reference
                content.Add(String.Format("Dim {0} As {1} = {2}", var.compiledName, compileType(var.type), compileNode(castedNode.value, content)))
            Else
                'Lambda variable
                content.Add(String.Format("Dim {0} As {1} = {2}", var.compiledName, compileType(var.type), compileNode(castedNode.value, content)))
            End If

        End If

        'Add variable
        getParentContainer(castedNode).variables.Add(var)

        'Return
        Return ""

    End Function

End Class