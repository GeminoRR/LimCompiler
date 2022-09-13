Imports System.IO
Public Class VB_Compiler

    '===============================
    '========== VARIABLES ==========
    '===============================
    Public spaces As New List(Of SpaceNode)
    Public compiledClass As String
    Public compiledFunctions As String
    Public compiledVariables As String

    Private CompiledNameAlphabet As String = "abcdefghijklmnoqrstuvwxyz"
    Private variableCount As Integer = 0
    Private helpVariableCount As Integer = 0
    Private functionCount As Integer = 0
    Private structCount As Integer = 0

    '=============================
    '========== COMPILE ==========
    '=============================
    Public Sub compile(ByVal target As String, ByVal output As String, ByVal flags As List(Of String))

        'Set variables
        compiledVariables = ""
        compiledClass = ""
        compiledFunctions = ""
        variableCount = 0
        helpVariableCount = 0
        functionCount = 0
        structCount = 0

        'Get template
        Dim templateFolder As String = System.Reflection.Assembly.GetExecutingAssembly().Location().Replace("\", "/")
        templateFolder = templateFolder.Substring(0, templateFolder.LastIndexOf("/")) & "/templates"

        'Generate AST
        Dim mainFile As New FileNode(target)
        spaces.AddRange(mainFile.spaces)

        'Get entry point
        Dim entryPoint As FunctionNode = Nothing
        For Each space As SpaceNode In spaces
            If space.name = "init" Then
                For Each fun As Node In space.codes
                    If TypeOf fun Is FunctionNode Then
                        Dim castedFunction As FunctionNode = DirectCast(fun, FunctionNode)
                        If castedFunction.Name = "main" Then
                            entryPoint = castedFunction
                            Exit For
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

        'Compile code
        compileFunction(entryPoint)

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

        'Merge template & code
        Dim finalCode As String
        finalCode = "Imports System" & Environment.NewLine & Environment.NewLine & "Module Program" & Environment.NewLine & Environment.NewLine
        finalCode &= vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & "'//////// VARIABLES ////////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & compiledVariables.Replace(Environment.NewLine, Environment.NewLine & vbTab) & Environment.NewLine & Environment.NewLine
        finalCode &= vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & "'////////// CLASS //////////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & compiledClass.Replace(Environment.NewLine, Environment.NewLine & vbTab) & Environment.NewLine & Environment.NewLine
        finalCode &= vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & "'//////// FUNCTIONS ////////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & compiledFunctions.Replace(Environment.NewLine, Environment.NewLine & vbTab) & Environment.NewLine & Environment.NewLine
        finalCode &= vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & "'/////// ENTRY POINT ///////" & Environment.NewLine & vbTab & "'///////////////////////////" & Environment.NewLine & vbTab & "Sub Main()" & Environment.NewLine & vbTab & vbTab & entryPoint.compiledName & "()" & Environment.NewLine & vbTab & "End Sub"
        finalCode &= Environment.NewLine & Environment.NewLine & "End Module"

        'Save code
        Dim appData As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\", "/") & "/Lim"
        If Not Directory.Exists(appData & "/compile/VB") Then
            Directory.CreateDirectory(appData & "/compile/VB")
        End If
        If Not File.Exists(appData & "/compile/VB/VB.vbproj") Then
            If Not File.Exists(templateFolder & "/VB.vbproj") Then
                addCustomError("Cannot copy file", """" & templateFolder & "/VB.vbproj"" is missing")
            End If
            Try
                File.Copy(templateFolder & "/VB.vbproj", appData & "/compile/VB/VB.vbproj")
            Catch ex As Exception
                addCustomError("Cannot copy file", ex.Message)
            End Try
        End If
        Dim finalCodePath As String = appData & "/compile/VB/Program.vb"
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

    '====================================
    '========== NODE TO STRUCT ==========
    '====================================
    Public Function nodeToStruct(ByVal node As ElementPathNode) As StructNode

        Dim parentNode As Node = node
        Dim resultStruct As StructNode = Nothing
        While Not parentNode.parentNode Is Nothing

            'Search node
            If TypeOf parentNode Is SpaceNode Or TypeOf parentNode Is FileNode Then

                'Is spacenode
                Dim codes As IEnumerable(Of Node)
                If TypeOf parentNode Is SpaceNode Then
                    codes = DirectCast(parentNode, SpaceNode).codes
                Else
                    codes = DirectCast(parentNode, FileNode).spaces
                End If

                'Get element
                resultStruct = recursiveStructSearch(codes, node)

            End If

            'Find check
            If Not resultStruct Is Nothing Then
                Return resultStruct
            End If

            'Advance
            parentNode = parentNode.parentNode

        End While

        'Error
        addSyntaxError("VBCNTS01", "Unable to access """ & node.ToString() & """ structure", node)
        Return Nothing

    End Function
    Private Function recursiveStructSearch(ByVal codes As IEnumerable(Of Node), ByVal name As ElementPathNode) As StructNode

        'Check
        If name.childs.Count < 1 Then
            Return Nothing
        End If

        'Loop
        For Each elm As Node In codes

            If TypeOf elm Is StructNode Then

                Dim castedStructNode As StructNode = DirectCast(elm, StructNode)
                If castedStructNode.Name = name.childs(0) Then
                    Return castedStructNode
                End If

            ElseIf TypeOf elm Is SpaceNode Then

                Dim castedSpaceNode As SpaceNode = DirectCast(elm, SpaceNode)
                If castedSpaceNode.name = name.childs(0) Then
                    Return recursiveStructSearch(castedSpaceNode.codes, New ElementPathNode(name.positionStart, name.positionEnd, name.childs.GetRange(1, name.childs.Count - 1)))
                End If

            End If

        Next

        'Return
        Return Nothing

    End Function

    '======================================
    '========== TYPENODE TO TYPE ==========
    '======================================
    Public Function typenodeToSafeType(ByVal typenode As typeNode) As safeType

        Dim struct As StructNode = nodeToStruct(typenode.StructName)
        compileStruct(struct)
        Return New safeType(struct, typenode.Dimensions)

    End Function

    '==================================
    '========== COMPILE TYPE ==========
    '==================================
    Public Function compileType(ByVal type As safeType) As String

        'Variable
        Dim result As String

        'Compile
        If type.Dimensions.Count > 0 Then
            If type.Dimensions(type.Dimensions.Count - 1) = ValueType.list Then
                result = String.Format("List(Of {0})", compileType(type.getParentType()))
            ElseIf type.Dimensions(type.Dimensions.Count - 1) = ValueType.map Then
                result = String.Format("Dictionary(Of String, {0})", compileType(type.getParentType()))
            Else
                Throw New NotImplementedException()
            End If
        Else
            result = type.Struct.compiledName
        End If

        'Return
        Return result

    End Function

    '==================================
    '========== COMPILE FILE ==========
    '==================================
    Public Sub compileFunction(ByRef fun As FunctionNode)

        'State handler
        If fun.compiled Then
            If fun.compiling Then
                addSyntaxError("VBCCF01", "The type of this function must be explicitly noted by its use. Example: ""func myFunction():str[]""", fun)
            End If
            Exit Sub
        End If
        fun.compiled = True
        fun.compiling = True

        'Get name
        fun.compiledName = getFunctionName()

        'Some variables
        Dim content As New List(Of String)

        'Argument list
        Dim compiled_arguments As String = ""
        For Each arg As FunctionArgument In fun.Arguments
            Dim var As New Variable(arg.name, typenodeToSafeType(arg.type), getVariableName())
            If arg.ref Then
                compiled_arguments &= ", " & var.compiledName & " As " & compileType(var.type)
            Else
                compiled_arguments &= ", " & var.compiledName & " As " & compileType(var.type)
                content.Add("'Copy " & arg.name)
                content.Add(String.Format("{0} = {0}.clone()", var.compiledName))
            End If
        Next
        If compiled_arguments.StartsWith(", ") Then
            compiled_arguments = compiled_arguments.Substring(2)
        End If

        'Final
        Dim finalString As String = "Public Function " & fun.compiledName & "(" & compiled_arguments & ")" & Environment.NewLine
        For i As Integer = 0 To content.Count - 1
            finalString &= Environment.NewLine & vbTab & content(i)
        Next
        finalString &= Environment.NewLine & Environment.NewLine & vbTab & "'Return empty" & Environment.NewLine & vbTab & "Return Nothing" & Environment.NewLine & Environment.NewLine & "End Function"
        compiledFunctions &= Environment.NewLine & Environment.NewLine & "'" & fun.Name & Environment.NewLine & finalString

        'End compiling
        fun.compiling = False

    End Sub


    '====================================
    '========== COMPILE STRUCT ==========
    '====================================
    Public Sub compileStruct(ByRef struct As StructNode)

        'State handler
        If struct.compiled Then
            Exit Sub
        End If
        struct.compiled = True

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

                    'Set type
                    var.type = getNodeType(def.value) 'TODO: Check if brut value

                    'Compile
                    content.Add(String.Format("Public {0} As {1} = {2}", var.compiledName, compileType(var.type), compileNode(def.value)))

                Else

                    'Set type
                    var.type = typenodeToSafeType(def.variableUnsafeType)

                    'Compile
                    content.Add(String.Format("Public {0} As {1}", var.compiledName, compileType(var.type)))

                End If

            Next
        End If

        'Get functions
        If functions.Count > 0 Then
            content.Add("'Methods")
            For Each def As FunctionNode In functions

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

    '===================================
    '========== GET NODE TYPE ==========
    '===================================
    Public Function getNodeType(ByVal node As Node) As safeType

        'Return
        addSyntaxError("VBGNT01", "Unable to resolve node type", node)
        Return Nothing

    End Function

    '==================================
    '========== COMPILE NODE ==========
    '==================================
    Public Function compileNode(ByVal node As Node) As String

        'Return
        addSyntaxError("VBCN01", "Unable to resolve node type", node)
        Return Nothing

    End Function

End Class

