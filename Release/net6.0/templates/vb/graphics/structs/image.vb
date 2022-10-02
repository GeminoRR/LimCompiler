Class image

	'Variables
	Public img As Bitmap
	Public gr As Graphics

	'New
	Public Sub New(Byval width As int, Byval height As int)
		Me.img = New Bitmap(width.value, height.value)
		Me.gr = Graphics.FromImage(Me.img)
	End Sub

	'Clone
	Public Function Clone() As image
		Return DirectCast(Me.MemberwiseClone(), image)
	End Function

	'ToString
    Public Overrides Function ToString() As String
        Return "[image]"
    End Function

End Class