Imports System.Net
Imports Newtonsoft.Json.Linq

Public Class DeleteRequestHandler
    Inherits RequestHandler
    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub
    Protected Overrides Sub ProcessRequest()


        Repository.DeleteDocument(LocalPath)

        SetStringResult(RequestEntityURL)



    End Sub

End Class
