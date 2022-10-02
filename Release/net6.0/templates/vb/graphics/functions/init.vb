Public Sub initWindow(Byval name As str, Byval width As int, Byval height As int)
	Me.text = name.value
	Me.Width = width.value
	Me.Height = height.value
	screen = New image(New int(Me.Width), New int(Me.Height))
End Sub