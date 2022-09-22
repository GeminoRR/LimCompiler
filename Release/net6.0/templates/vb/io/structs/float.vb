Class float

	'Variables
	Public value As Double = 0.0

	'New
	Public Sub New(Byval value As Double)
		Me.value = value
	End Sub

	'Clone
	Public Function Clone() As float
		Return DirectCast(Me.MemberwiseClone(), float)
	End Function

	'ToString
    Public Overrides Function ToString() As String
        Return Me.value.ToString()
    End Function

End Class