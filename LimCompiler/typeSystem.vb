﻿'=================================
'========== UNSAFE TYPE ==========
'=================================
Public Class typeNode
    Inherits Node

    Public StructName As ElementPathNode
    Public Dimensions As New List(Of ValueType)

    Public Sub New(ByVal positionStart As Integer, ByVal positionEnd As Integer, ByVal StructName As ElementPathNode, ByVal Dimensions As List(Of ValueType))
        MyBase.New(positionStart, positionEnd)
        Me.StructName = StructName
        Me.StructName.parentNode = Me
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
        Return StructName.ToString() & str
    End Function

    Public Function getParentType() As typeNode
        Dim parentDimension As New List(Of ValueType)
        For Each dimension As ValueType In Me.Dimensions
            parentDimension.Add(dimension)
        Next
        If parentDimension.Count > 0 Then
            parentDimension.RemoveAt(parentDimension.Count - 1)
        Else
            addCustomError("Cannot resolve the type", "Cannot get parent type of a simple type")
        End If
        Dim returnType As New typeNode(Me.positionStart, Me.positionEnd - 2, Me.StructName, parentDimension)
        returnType.parentNode = Me
        Return returnType
    End Function

End Class

'================================
'========== VALUE TYPE ==========
'================================
Public Enum ValueType
    list
    map
End Enum

'===============================
'========== SAVE TYPE ==========
'===============================
Public Class safeType

    Public Struct As StructNode
    Public Dimensions As New List(Of ValueType)

    Public Sub New(ByVal Struct As StructNode, Optional ByVal Dimensions As List(Of ValueType) = Nothing)
        Me.Struct = Struct
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

    Public Function IsTheSameAs(ByVal other As safeType) As Boolean

        'Name
        If Not Me.Struct.compiledName = other.Struct.compiledName Then
            Return False
        End If

        'Dimensions
        For i As Integer = 0 To Me.Dimensions.Count - 1
            If Not i < other.Dimensions.Count Then
                Return False
            End If
            If Not Me.Dimensions(0) = other.Dimensions(0) Then
                Return False
            End If
        Next
        'Pass all checks
        Return True

    End Function

End Class