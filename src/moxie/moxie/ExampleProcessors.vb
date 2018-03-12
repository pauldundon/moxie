Imports System.Net
Imports moxie

Public Class ExampleProcessor
    Inherits RequestHandler

    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub

    Protected Overrides Sub ProcessRequest()
        SetStringResult("Hello from the example Processor")
    End Sub


End Class
Public Class ResetDatabase
    Inherits RequestHandler

    Public Sub New(context As HttpListenerContext)
        MyBase.New(context)
    End Sub

    Protected Overrides Sub ProcessRequest()
        Repository.Reset()

        SetStringResult(String.Empty)
    End Sub
End Class