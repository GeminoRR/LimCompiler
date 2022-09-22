'Gets
Public Function gets(ByVal message As str) As str
	Console.Write(message.value)
	Return New str(Console.ReadLine())
End Function