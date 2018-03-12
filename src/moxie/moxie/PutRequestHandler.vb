Imports System.IO
Imports System.Net

Public Class PutRequestHandler
    Inherits RequestHandler
    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub
    Protected Overrides Sub ProcessRequest()



        Dim entityPath As String = LocalPath

        If Repository.DocumentExists(entityPath) Then
            Repository.UpdateDocument(RequestContent, RequestMediaType, entityPath)
        Else
            entityPath = Repository.CreateNamedDocument(RequestContent, RequestMediaType,
                                                   entityPath)
        End If

        SetStringResult(URLFromPath(entityPath))

    End Sub
End Class
