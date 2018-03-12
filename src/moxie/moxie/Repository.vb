Imports System.IO

Public Class Repository
    Protected Property ctx As New MoxieDataContext

    Protected Function GetDirectoryName(path As String) As String
        Return String.Join("/", path.Split("/").Reverse().Skip(1).Reverse())
    End Function

    Protected Function FindCollection(collectionPath As String, forDocument As String,
                                      create As Boolean) As Collection
        Dim coll As Collection = (From item In ctx.Collections
                                  Where item.Path = collectionPath
                                  Select item).SingleOrDefault

        If coll Is Nothing And create Then
            coll = New Collection
            coll.Path = collectionPath
            ctx.Collections.InsertOnSubmit(coll)
            ctx.SubmitChanges()

        End If

        Return coll
    End Function

    Public Sub DeleteDocument(localPath As String)

        Dim result As Document = (From item In ctx.Documents
                                  Where item.Path = localPath
                                  Select item).SingleOrDefault

        If result Is Nothing Then
            Throw New DocumentNotFoundException
        Else
            ctx.Documents.DeleteOnSubmit(result)
            ctx.SubmitChanges()
        End If
    End Sub
    Public Function CreateDocument(content As String,
                                   mediaType As String,
                                   collectionPath As String) As String


        Dim coll As Collection = FindCollection(collectionPath, content, True)

        Dim post As New Document
        post.CollectionID = coll.CollectionID
        post.Content = content
        post.MediaType = mediaType
        post.Path = ""
        ctx.Documents.InsertOnSubmit(post)

        ctx.SubmitChanges()

        post.Path = collectionPath & "/" & post.DocumentID
        ctx.SubmitChanges()

        Return post.Path

    End Function

    Public Function CreateNamedDocument(content As String,
                                   mediaType As String,
                                   documentPath As String) As String

        Dim collectionPath As String = GetDirectoryName(documentPath)

        Dim coll As Collection = FindCollection(collectionPath, content, True)

        Dim post As New Document
        post.CollectionID = coll.CollectionID
        post.Content = content
        post.MediaType = mediaType
        post.Path = documentPath
        ctx.Documents.InsertOnSubmit(post)

        ctx.SubmitChanges()

        Return post.Path

    End Function

    Public Sub UpdateDocument(content As String,
                                   mediaType As String,
                                   documentPath As String)


        Dim collectionPath As String = GetDirectoryName(documentPath)

        Dim coll As Collection = FindCollection(collectionPath, content, True)

        Dim put As Document = (From item In ctx.Documents
                               Where item.Path = documentPath
                               Select item).SingleOrDefault

        If put Is Nothing Then
            Throw New DocumentNotFoundException
        Else
            put.Content = content
            put.MediaType = mediaType
            ctx.SubmitChanges()
        End If
    End Sub

    Public Function ReadDocument(documentPath As String) As Document

        Dim doc As Document = (From item In ctx.Documents
                               Where item.Path = documentPath
                               Select item).SingleOrDefault

        If Not doc Is Nothing Then
            Return doc
        Else
            Throw New DocumentNotFoundException
        End If
    End Function
    Public Function ListDocuments(collectionPath As String) As IEnumerable(Of Document)
        Dim coll As Collection = FindCollection(collectionPath, Nothing, False)

        If coll Is Nothing Then
            Throw New DocumentNotFoundException
        Else
            Return From item In ctx.Documents
                   Where item.CollectionID = coll.CollectionID
                   Select item
        End If



    End Function
    Public Function DocumentExists(documentPath As String) As Boolean

        Dim doc As Integer = (From item In ctx.Documents
                              Where item.Path = documentPath
                              Select item.DocumentID).SingleOrDefault
        Return doc <> 0

    End Function

    Public Function IsCollectionPath(path As String) As Boolean
        Return Not FindCollection(path, Nothing, False) Is Nothing
    End Function



    Public Sub Reset()
        Dim ctx As New MoxieDataContext
        ctx.ExecuteCommand("DELETE FROM Collection")
        ctx.ExecuteCommand("DELETE FROM Document")
    End Sub

End Class

Public Class DocumentNotFoundException
    Inherits ApplicationException

End Class

