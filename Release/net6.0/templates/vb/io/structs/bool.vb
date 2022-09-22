Class bool

	'Variables
	Public value As Boolean = True

	'New
	Public Sub New(Byval value As Boolean)
		Me.value = value
	End Sub

	'Clone
	Public Function Clone() As bool
		Return DirectCast(Me.MemberwiseClone(), bool)
	End Function

	'ToString
    Public Overrides Function ToString() As String
        Return Me.value.ToString()
    End Function

End Class