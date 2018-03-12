Imports System.Net
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json
Imports System.Text

Public Class Credentials
    Public Property Username As String
    Public Property Password As String
    Public Shared Superadmin As New Credentials
    Public Shared Empty As New Credentials

End Class
Public Class Ajax
    Public Shared Property Server As String
    Public Shared Property ApiDb As String
    Public Shared Property RequireCORS As Boolean
    Public Shared Property CredentialMode As CredentialModes = CredentialModes.UseInitialToken

    Public Enum Methods
        [GET]
        POST
        PUT
        PATCH
        DELETE
    End Enum
    Public Const DefaultCredentialMode = CredentialModes.UseInitialToken

    Public Shared LastResponseCode As String
    Public Shared LastErrorResponseContent As String
    Public Shared LastResponseHeaders As WebHeaderCollection

    Public Shared FirstUseCredentialsUser As String
    Public Shared FirstUseCredentialsPassword As String

    Public Shared LastUseCredentialsUser As String
    Public Shared LastUseCredentialsPassword As String

    Public Shared CORSOrigin As String = "http://localhost"
    Public Shared UseTimestamps As Boolean

    Class Payload
        Inherits Dictionary(Of String, Object)
        Public Overrides Function ToString() As String
            Return JObject.FromObject(Me).ToString
        End Function
        Public Shared Empty As New Payload From {}

    End Class
    Public Enum CredentialModes
        Unauthenticated
        Superadmin
        UseInitialToken
        UseMostRecentToken
        UseExplicit
    End Enum

    Protected Shared Sub SetCredentials()
        Select Case CredentialMode
            Case CredentialModes.Unauthenticated
                Credentials = Credentials.Empty
            Case CredentialModes.Superadmin
                Credentials = Credentials.Superadmin
            Case CredentialModes.UseInitialToken
                If FirstUseCredentialsUser = "" Then
                    Credentials = Credentials.Superadmin
                Else
                    Credentials = New Credentials With {.Username = FirstUseCredentialsUser, .Password = FirstUseCredentialsPassword}
                End If
            Case CredentialModes.UseMostRecentToken
                If FirstUseCredentialsUser = "" Then
                    Credentials = Credentials.Superadmin
                Else
                    Credentials = New Credentials With {.Username = LastUseCredentialsUser, .Password = LastUseCredentialsPassword}
                End If
            Case CredentialModes.UseExplicit
                Credentials = ExplicitCredentials
        End Select
    End Sub

    Protected Shared Property Credentials As Credentials
    Public Shared Property ExplicitCredentials As Credentials

    Public Shared Property RequestCount As Integer

    Public Shared Function ResourceApiPath(typeName As String, id As String) As String
        If id <> "" Then
            id = "/" & id
        End If
        Dim url As String = Server & typeName & id
        Return url
    End Function
    Public Shared Function MakeApiRequest(apiPath As String, method As Methods,
                                   payload As Payload) As Object
        Return MakeApiRequest(apiPath, method, payload.ToString)
    End Function
    Public Shared Function MakeApiRequest(apiPath As String, method As Methods,
                                   Optional payload As String = "") As Object
#If LOG_PAYLOADS Then

        Dim logFile As String = apiPath
        If logFile.Contains("?") Then
            logFile = logFile.Split("?")(0)
        End If
        Dim restype As String = logFile.Split("/")(1)

        If Not IO.Directory.Exists("c:\temp\payloads\" & restype) Then
            IO.Directory.CreateDirectory("c:\temp\payloads\" & restype)

        End If
        logFile = String.Format("c:\temp\payloads\{2}\{0}@{1}.txt", method.ToString, logFile.Replace("/", "-"), restype)

#End If

        Dim url As String = Server + apiPath
        Dim result As Object = MakeRequest(url, method, payload)
#If LOG_PAYLOADS Then

        Using sw As New StreamWriter(logFile)
            sw.WriteLine(method.ToString & " " & apiPath)
            sw.WriteLine("")
            sw.WriteLine(payload)
            sw.WriteLine("")
            sw.WriteLine(LastResponseCode)
            If Not result Is Nothing Then
                sw.WriteLine(result.ToString)
            Else
                sw.WriteLine("(no content)")
            End If
        End Using
#End If

        Return result
    End Function
    Public Shared Function MakeRequest(url As String, method As Methods,
                                       payload As Payload) As Object
        Return MakeRequest(url, method, payload.ToString)
    End Function
    Public Shared Function MakeRequest(url As String, method As Methods,
                                       Optional payload As String = "") As Object

        RequestCount += 1
        LastErrorResponseContent = ""
        LastResponseCode = 0

        If UseTimestamps Then
            If url.Contains("?") Then
                url = url & "&_timestamp="
            Else
                url = url & "?_timestamp="
            End If
            url = url & Now.ToLongTimeString
        End If

        Dim req As WebRequest = WebRequest.Create(url)
        req.Method = method.ToString

        SetCredentials()

        If Not Credentials Is Nothing AndAlso Credentials.Username <> "" Then
            'req.Credentials = New NetworkCredential(Credentials.Username, Credentials.Password)


            Dim authInfo As String = Credentials.Username + ":" + Credentials.Password
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo))
            req.Headers("Authorization") = "Basic " + authInfo
        End If



        Dim sw As StreamWriter = Nothing
        If payload <> "" Then
            Dim s As Stream = req.GetRequestStream
            sw = New StreamWriter(s)
            sw.Write(payload)
            sw.Flush()
            req.ContentType = "text/json"
        End If

        req.Headers.Add("Accept-Encoding:gzip,deflate,sdch")
        req.Headers.Add("Accept-Language:en-GB,en-US;q=0.8,en;q=0.6")
        req.Headers.Add("Access-Control-Request-Headers:accept, content-type, authorization, api-database")
        req.Headers.Add("Access-Control-Request-Method:" & req.Method)
        req.Headers.Add(String.Format("Origin:{0}", CORSOrigin))
        If ApiDb <> "" Then
            req.Headers.Add("API-Database:" & ApiDb)
        End If
        req.Timeout = 240000
        Dim t As DateTime = Now

        Dim resp As HttpWebResponse
        Dim sr As StreamReader = Nothing
        Dim result As String
        Try
            resp = req.GetResponse
            LastResponseCode = resp.StatusCode
            LastResponseHeaders = resp.Headers
            ' The response must have CORS headers
            If Not resp.Headers.AllKeys.Contains("Access-Control-Allow-Origin") And RequireCORS Then
                Throw New ApplicationException("Missing CORS headers")
            End If

            ' The response should have a Use-Credentials header unless
            ' the request was unauthenticated
            Dim creds As String = resp.Headers("Use-Credentials")
            If creds <> "" Then

                LastUseCredentialsPassword = creds.Split(":")(1)
                LastUseCredentialsUser = creds.Split(":")(0)
                If FirstUseCredentialsPassword = "" Then
                    FirstUseCredentialsPassword = LastUseCredentialsPassword
                    FirstUseCredentialsUser = LastUseCredentialsUser
                End If
            End If

        Catch ex As WebException
            Dim timeout As TimeSpan = Now - t

            LastResponseCode = CType(ex.Response, HttpWebResponse).StatusCode
            If CType(ex.Response, HttpWebResponse).StatusCode = 422 Or
                CType(ex.Response, HttpWebResponse).StatusCode = 500 Or
                CType(ex.Response, HttpWebResponse).StatusCode = 400 Or
                CType(ex.Response, HttpWebResponse).StatusCode = 404 Then
                sr = New StreamReader(ex.Response.GetResponseStream)
                result = sr.ReadToEnd

                If Not ex.Response.Headers.AllKeys.Contains("Access-Control-Allow-Origin") And RequireCORS Then
                    Throw New ApplicationException("Missing CORS headers")
                End If
                LastErrorResponseContent = result
            End If

            Throw
        Finally
            If Not sr Is Nothing Then
                sr.Close()
                sr.Dispose()
            End If
        End Try

        sr = New StreamReader(resp.GetResponseStream)
        result = sr.ReadToEnd

        If Not sw Is Nothing Then
            sw.Dispose()
        End If

        If (method = Methods.DELETE Or method = Methods.PATCH) And resp.StatusCode = 204 Then
            Return Nothing
        End If

        Select Case resp.StatusCode
            Case HttpStatusCode.OK
                Try
                    Dim oResult As JObject = JObject.Parse(result)
                    Return oResult
                Catch ex As Exception
                    Return result
                End Try
            Case HttpStatusCode.NoContent
                Return Nothing
            Case Else
                Throw New ApplicationException

        End Select




    End Function


End Class

