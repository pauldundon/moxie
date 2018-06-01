Imports System.Net
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

<TestClass()> Public Class MoxieTests
    Const SERVER = "http://localhost:904/"
    <TestInitialize> Public Sub InitAjax()
        Ajax.Server = SERVER

        Ajax.CredentialMode = Ajax.CredentialModes.Unauthenticated

    End Sub
    <TestMethod()> Public Sub StaticFile()
        Dim doc As JObject = Ajax.MakeApiRequest("examples/eg1", Ajax.Methods.GET)
        Assert.AreEqual("Example JSON Document", doc("content").Value(Of String))
    End Sub
    <TestMethod()> Public Sub StaticFileWithQuery()
        Dim doc As JObject = Ajax.MakeApiRequest("examples/eg2?name=ben&type=1", Ajax.Methods.GET)
        Assert.AreEqual("Example JSON Document", doc("content").Value(Of String))
    End Sub
    <TestMethod()> Public Sub StaticProcessor()
        Dim result As String = Ajax.MakeApiRequest("examples/processor", Ajax.Methods.POST, Ajax.Payload.Empty)

        Assert.AreEqual("Hello from the example Processor", result)
    End Sub
    Protected Sub ResetDb()
        Ajax.MakeApiRequest("examples/resetDb", Ajax.Methods.POST, Ajax.Payload.Empty)
    End Sub
    Protected Function CreatePayload() As Ajax.Payload
        Dim rnd As New Random
        Dim sig As Integer = rnd.Next(0, 1000000)
        Return New Ajax.Payload From {{"type", "example document"},
            {"signature", sig}}
    End Function
    Protected Function IsExampleDocument(doc As JObject) As Boolean
        Return doc("type").Value(Of String) = "example document"
    End Function
    <TestMethod()> Public Sub PostAndGet()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))
    End Sub
    <TestMethod()> Public Sub PutAndGet()
        ResetDb()
        Dim docPath As String = "tests/prenamed"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(docPath, Ajax.Methods.PUT, doc)
        Assert.IsTrue(newPath.EndsWith("/tests/prenamed"))
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))
    End Sub
    <TestMethod()> Public Sub PutOverwrites()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        doc = CreatePayload()
        Ajax.MakeRequest(newPath, Ajax.Methods.PUT, doc)
        newDoc = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))
    End Sub
    <TestMethod()> Public Sub PostCreatesCollection()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))
        Dim collectionDoc As JObject = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Dim newPathInCollection As String = collectionDoc("links")(0)("href").Value(Of String)
        newDoc = Ajax.MakeRequest(newPathInCollection, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
    End Sub

    <TestMethod()> Public Sub PostCreatesCollection2()
        ResetDb()
        Dim collectionPath As String = "processes"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))
        Dim collectionDoc As JObject = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Dim newPathInCollection As String = collectionDoc("links")(0)("href").Value(Of String)
        newDoc = Ajax.MakeRequest(newPathInCollection, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
    End Sub

    <TestMethod()> Public Sub PutCreatesCollection()
        ResetDb()
        Dim docPath As String = "tests/prenamed"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(docPath, Ajax.Methods.PUT, doc)
        Assert.IsTrue(newPath.EndsWith("/tests/prenamed"))
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))

        Dim collectionDoc As JObject = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Dim newPathInCollection As String = collectionDoc("links")(0)("href").Value(Of String)
        newDoc = Ajax.MakeRequest(newPathInCollection, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
    End Sub
    <TestMethod()> Public Sub PostAddsToCollection()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))

        doc = CreatePayload()
        newPath = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        newDoc = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))

        Dim collectionDoc As JObject = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Assert.AreEqual(2, collectionDoc("links").Count)

    End Sub

    <TestMethod()> Public Sub PutAddsToCollection()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))

        doc = CreatePayload()
        newPath = Ajax.MakeApiRequest("tests/prenamed", Ajax.Methods.PUT, doc)
        newDoc = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))

        Dim collectionDoc As JObject = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Assert.AreEqual(2, collectionDoc("links").Count)
    End Sub

    <TestMethod()> Public Sub DeleteRemovesFromCollection()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))

        doc = CreatePayload()
        newPath = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        newDoc = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))

        Dim collectionDoc As JObject = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Assert.AreEqual(2, collectionDoc("links").Count)

        Ajax.MakeRequest(newPath, Ajax.Methods.DELETE)

        collectionDoc = Ajax.MakeApiRequest("tests", Ajax.Methods.GET)
        Assert.AreEqual(1, collectionDoc("links").Count)

        Dim exceptionThrown As Boolean
        Try
            Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Catch ex As WebException
            Assert.AreEqual(HttpStatusCode.NotFound, CType(ex.Response, HttpWebResponse).StatusCode)
            exceptionThrown = True
        End Try
    End Sub
    <TestMethod()> Public Sub Patch()
        ResetDb()
        Dim collectionPath As String = "tests"
        Dim doc As Ajax.Payload = CreatePayload()
        Dim newPath As String = Ajax.MakeApiRequest(collectionPath, Ajax.Methods.POST, doc)
        Dim newDoc As JObject = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.IsTrue(IsExampleDocument(newDoc))

        Dim patch As Ajax.Payload = New Ajax.Payload
        patch("type") = "patched document"
        patch("newField") = "added"
        Ajax.MakeRequest(newPath, Ajax.Methods.PATCH, patch)
        newDoc = Ajax.MakeRequest(newPath, Ajax.Methods.GET)
        Assert.AreEqual(doc("signature"), newDoc("signature").Value(Of Integer))
        Assert.AreEqual("patched document", newDoc("type").Value(Of String))
        Assert.AreEqual("added", newDoc("newField").Value(Of String))

    End Sub
End Class