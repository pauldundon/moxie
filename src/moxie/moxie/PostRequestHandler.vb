Imports System.IO
Imports System.Net

Public Class PostRequestHandler
    Inherits RequestHandler
    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub
    Protected Overrides Sub ProcessRequest()

        Dim path As String = Repository.CreateDocument(RequestContent(),
                                                       RequestMediaType, LocalPath)
        SetStringResult(URLFromPath(path))

    End Sub
End Class
