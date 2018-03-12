Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Public Class PatchRequestHandler
    Inherits RequestHandler
    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub
    Protected Overrides Sub ProcessRequest()



        Dim entityPath As String = LocalPath

        Dim doc As Document = Repository.ReadDocument(entityPath)

        Dim existing As JObject = JObject.Parse(doc.Content)
        Dim patch As JObject = JObject.Parse(RequestContent)

        For Each jp As JProperty In patch.Properties
            existing(jp.Name) = patch(jp.Name)
        Next

        Repository.UpdateDocument(existing.ToString, RequestMediaType, entityPath)

        SetStringResult(RequestEntityURL)

    End Sub
End Class
