'=================================
'========== UNSAFE TYPE ==========
'=================================
Public Class unsafeType

    Public StructName As Object
    Public Dimensions As New List(Of ValueType)

    Public Sub New(ByVal StructName As Object, ByVal Dimensions As List(Of ValueType))
        Me.StructName = StructName
        If Not Dimensions Is Nothing Then
            For Each Dimension As ValueType In Dimensions
                Me.Dimensions.Add(Dimension)
            Next
        End If
    End Sub

    Public Overrides Function ToString() As String
        Dim str As String = ""
        For Each dimension As ValueType In Dimensions
            Select Case dimension.ToString()
                Case "list"
                    str &= "[]"
                Case "map"
                    str &= "{}"
                Case Else
                    str &= "?"
            End Select
        Next
        Return StructName & str
    End Function

    Public Function getParentType() As unsafeType
        Dim parentDimension As New List(Of ValueType)
        For Each dimension As ValueType In Me.Dimensions
            parentDimension.Add(dimension)
        Next
        If parentDimension.Count > 0 Then
            parentDimension.RemoveAt(parentDimension.Count - 1)
        End If
        Return New unsafeType(Me.StructName, parentDimension)
    End Function

    Public Function ToSafeType(ByVal node As Node) As safeType

        'Name can be string or fromSpaceNode

    End Function

End Class

'================================
'========== VALUE TYPE ==========
'================================
Public Enum ValueType
    list
    map
End Enum

Public Module typeSystem

    '=================================
    '===== STRING TO UNSAFE TYPE =====
    '=================================
    Public Function stringToUnsafeType(ByVal str As String, ByVal filename As String, ByVal text As String, ByVal positionStart As Integer, ByVal positionEnd As Integer) As unsafeType

        'Variable
        Dim type As New unsafeType("", New List(Of ValueType))
        Dim exeptedCharacter As Char = Nothing
        str = str.Insert(0, "a")

        'Loop each character
        While str.Length > 1

            str = str.Substring(1)

            If Not exeptedCharacter = Nothing Then
                If str(0) = exeptedCharacter Then
                    exeptedCharacter = Nothing
                    Continue While
                Else
                    addCustomSyntaxError("CSTUT01", "The character """ & exeptedCharacter & """ was expected", filename, text, positionStart, positionEnd)
                End If
            End If

            Select Case str(0)

                Case "["
                    type.Dimensions.Add(ValueType.list)
                    exeptedCharacter = "]"
                    Continue While

                Case "{"
                    type.Dimensions.Add(ValueType.list)
                    exeptedCharacter = "}"
                    Continue While

            End Select

            type.StructName &= str(0)

        End While

        'Return
        Return type

    End Function

End Module

'===============================
'========== SAVE TYPE ==========
'===============================
Public Class safeType

    Public Struct As StructNode
    Public Dimensions As New List(Of ValueType)
    Private compiled As Boolean

    Public Sub New(ByVal Struct As StructNode, ByVal Dimensions As List(Of ValueType))
        Me.Struct = Struct
        Me.compiled = False
        If Not Dimensions Is Nothing Then
            For Each Dimension As ValueType In Dimensions
                Me.Dimensions.Add(Dimension)
            Next
        End If
    End Sub

    Public Overrides Function ToString() As String
        Dim str As String = ""
        For Each dimension As ValueType In Dimensions
            Select Case dimension.ToString()
                Case "list"
                    str &= "[]"
                Case "map"
                    str &= "{}"
                Case Else
                    str &= "?"
            End Select
        Next
        Return Struct.Name & str
    End Function

    Public Function compile(ByVal parentNode As Node) As String

        Me.compiled = True

        Dim content As String
        If Dimensions.Count = 0 Then
            content = Struct.compiledName
        Else
            content = ""
        End If


        Return content


    End Function

    Public Function getParentType() As safeType
        Dim parentDimension As New List(Of ValueType)
        For Each dimension As ValueType In Me.Dimensions
            parentDimension.Add(dimension)
        Next
        If parentDimension.Count > 0 Then
            parentDimension.RemoveAt(parentDimension.Count - 1)
        End If
        Return New safeType(Me.Struct, parentDimension)
    End Function

End Class