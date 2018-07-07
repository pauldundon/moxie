Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Public Class PostRequestHandler
    Inherits RequestHandler
    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub
    Protected Overrides Sub ProcessRequest()

        Dim docContent As String = RequestContent()
        Dim path As String = Repository.CreateDocument(docContent,
                                                       RequestMediaType, LocalPath)
        Dim resultURL = URLFromPath(path)

        Dim idField As String = Nothing
        If RequestHeaders.AllKeys.Contains("Register-ID") Then
            idField = RequestHeaders("Register-ID")
            Dim docObject As JObject = JObject.Parse(docContent)
            docObject.Add(idField, resultURL)
            Repository.UpdateDocument(docObject.ToString, RequestMediaType, path)
        End If

        SetStringResult(resultURL)

    End Sub
End Class
