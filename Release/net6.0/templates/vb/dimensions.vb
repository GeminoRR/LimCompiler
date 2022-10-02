Class LimMap(Of T)
    Inherits Dictionary(Of String, T)

    Public Function Clone() As LimMap(Of T)
        Return DirectCast(Me.MemberwiseClone(), LimMap(Of T))
    End Function

    Public Overrides Function ToString() As String
        Return Me.str().value
    End Function

    Public Function str() As str
        Dim valuesToString As String = ""
        For Each key As String In MyBase.Keys
            Dim sep As String = ""
            If TypeOf MyBase.Item(key) Is str Then
                sep = """"
            End If
            valuesToString &= ", " & sep & "" & key.ToString() & "" & sep & ":" & sep & MyBase.Item(key).ToString() & sep
        Next
        If valuesToString.StartsWith(", ") Then
            valuesToString = valuesToString.Substring(2)
        End If
        Return New str("{" & valuesToString & "}")
    End Function

End Class

Class LimList(Of T)
    Inherits List(Of T)

    Public Function Clone() As LimList(Of T)
        Return DirectCast(Me.MemberwiseClone(), LimList(Of T))
    End Function

    Public Overrides Function ToString() As String
        Return Me.str().value
    End Function

    Public Function str() As str
        Dim valuesToString As String = ""
        For i As Integer = 0 To MyBase.Count - 1
            If TypeOf MyBase.Item(i) Is str Then
                valuesToString &= ", """ & MyBase.Item(i).ToString() & """"
            Else
                valuesToString &= ", " & MyBase.Item(i).ToString()
            End If
        Next
        If valuesToString.StartsWith(", ") Then
            valuesToString = valuesToString.Substring(2)
        End If
        Return New str("[" & valuesToString & "]")
    End Function

End Class