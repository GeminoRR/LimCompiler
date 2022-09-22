Class str

	'Variables
	Public value As String = ""

	'New
	Public Sub New(Byval value As String)
		Me.value = value
	End Sub

	'Clone
	Public Function Clone() As str
		Return DirectCast(Me.MemberwiseClone(), str)
	End Function

	'ToString
    Public Overrides Function ToString() As String
        Return Me.value
    End Function

End Class