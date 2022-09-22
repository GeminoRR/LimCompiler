Class int

	'Variables
	Public value As Integer = 0
	
	'New
	Public Sub New(Byval value As Integer)
		Me.value = value
	End Sub

	'Clone
	Public Function Clone() As int
		Return DirectCast(Me.MemberwiseClone(), int)
	End Function

	'ToString
    Public Overrides Function ToString() As String
        Return Me.value.ToString()
    End Function

End Class