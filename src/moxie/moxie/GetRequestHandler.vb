Imports System.Net
Imports Newtonsoft.Json.Linq

Public Class GetRequestHandler
    Inherits RequestHandler
    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub
    Protected Overrides Sub ProcessRequest()

        If Not Repository.IsCollectionPath(LocalPath) Then
            GetEntity()
        Else
            GetCollection()
        End If

    End Sub

    Protected Sub GetEntity()

        Dim result As Document

        result = Repository.ReadDocument(LocalPath)
        Context.Response.ContentType = result.MediaType


        SetStringResult(result.Content)
    End Sub


    Protected Sub GetCollection()
        Dim links As New List(Of LinkResult)
        For Each des As Document In Repository.ListDocuments(LocalPath)
            links.Add(New LinkResult With {.href = URLFromPath(des.Path)})
        Next

        Dim dict As New Dictionary(Of String, Object) From {{"links", links}}
        SetJSONResult(dict)


    End Sub
End Class
