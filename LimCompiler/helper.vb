Module helper

    '============================
    '========= GET FILE =========
    '============================
    Public Function getFileFromNode(ByVal currentNode As Node) As FileNode

        Dim parentNode As Node = currentNode
        While Not parentNode.parentNode Is Nothing
            parentNode = parentNode.parentNode
            If TypeOf parentNode Is FileNode Then
                Exit While
            End If
        End While

        If Not TypeOf parentNode Is FileNode Then
            addCustomError("Internal Link error", "Cannot find the parent File, current node is <" & parentNode.GetType.ToString() & ">")
        End If

        Return parentNode

    End Function

End Module