Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq
Imports System.Configuration
Imports System.Reflection
Imports System.Web

Public Interface IRequestHandler
    Sub ProcessRequest()

End Interface

Public Class RequestHandlerBase
    Public Property Context As HttpListenerContext
    Public Property Repository As New Repository
    Protected Sub AttachContext(context As HttpListenerContext)
        Me.Context = context
    End Sub

End Class
Public MustInherit Class RequestHandler
    Implements IRequestHandler
    Public Property Context As HttpListenerContext
    Public Property Repository As New Repository

    Public Sub New(context As HttpListenerContext)
        Me.Context = context
    End Sub
    Sub PublicProcessRequest() Implements IRequestHandler.ProcessRequest
        Try
            Dim stat As JObject = StaticResponse()

            If stat Is Nothing Then
                ProcessRequest()
            Else
                ProcessStatic(stat)
            End If
        Catch ex As DocumentNotFoundException
            Context.Response.StatusCode = 404
            SetStringResult("Not Found")
        End Try

    End Sub
    Protected MustOverride Sub ProcessRequest()
    Protected Sub SetStringResult(content As String)
        Dim response As HttpListenerResponse = Context.Response

        Dim buffer As Byte() = System.Text.Encoding.UTF8.GetBytes(content)
        response.ContentLength64 = buffer.Length
        Dim output As System.IO.Stream = response.OutputStream
        output.Write(buffer, 0, buffer.Length)
        output.Close()


    End Sub
    Protected Sub SetJSONResult(content As Dictionary(Of String, Object))
        SetJSONResult(JObject.FromObject(content))
    End Sub
    Protected Sub SetJSONResult(content As JObject)
        SetStringResult(content.ToString)
    End Sub
    Protected Function URLFromPath(path As String) As String
        Dim requestUrl As Uri = Context.Request.Url
        Return requestUrl.AbsoluteUri.Substring(0, requestUrl.AbsoluteUri.Length - requestUrl.PathAndQuery.Length) & path
    End Function

    Protected Function RequestContent() As String
        Dim sr As New StreamReader(Context.Request.InputStream)
        Dim content As String = sr.ReadToEnd
        sr.Close()
        Return content
    End Function
    Protected ReadOnly Property LocalPath As String
        Get
            Return Context.Request.Url.LocalPath
        End Get
    End Property

    Protected ReadOnly Property RequestEntityURL As String
        Get
            Return URLFromPath(LocalPath)
        End Get
    End Property

    Protected ReadOnly Property RequestMediaType As String
        Get
            Return Context.Request.ContentType
        End Get
    End Property
    Protected Sub ProcessStatic(stat As JObject)
        Dim jpProc As JValue = stat("processor")
        If Not jpProc Is Nothing Then
            Dim procName As String = jpProc.Value(Of String)
            Dim proc As RequestHandler = Activator.CreateInstance(Type.GetType(procName), {Context})
            proc.ProcessRequest()
        Else
            SetJSONResult(stat)
        End If

    End Sub
    Protected Function StaticResponse() As JObject
        Dim fn As String = StaticResponseFile()
        If fn <> String.Empty Then
            Dim sr As New StreamReader(fn)
            Dim content As String = sr.ReadToEnd
            Dim result As JObject = JObject.Parse(content)
            Return result
        Else
            Return Nothing
        End If
    End Function
    Protected Function StaticResponseFile() As String
        Dim root As String = ConfigurationManager.AppSettings("responseFolder")
        If root = "" Then
            Dim asm As Assembly = Assembly.GetExecutingAssembly
            root = asm.Location
            For i As Integer = 1 To 3
                root = Path.GetDirectoryName(root)
            Next
            root = $"{root}\response"
        End If
        root = $"{root}\{Context.Request.HttpMethod}"

        Dim seekable As Func(Of String, String) = Function(s) $"{root}\{s}.json"

        Dim pathAsWindows = LocalPath.Replace("/", "\")

        If Context.Request.Url.Query <> "" Then

            Dim query As String = HttpUtility.UrlEncode(Context.Request.Url.Query)
            query = query.Replace("%", "@")
            If File.Exists(seekable(pathAsWindows & query)) Then
                Return seekable(pathAsWindows & query)
            End If
        End If

        Do While pathAsWindows <> "\"
            If File.Exists(seekable(pathAsWindows)) Then
                Return seekable(pathAsWindows)
            End If
            pathAsWindows = Path.GetDirectoryName(pathAsWindows)
        Loop

        Return String.Empty
    End Function
End Class
