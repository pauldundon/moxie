Partial Public Class MoxieDataContext
    Public Sub New()
        MyBase.New(Configuration.ConfigurationManager.ConnectionStrings("moxie").ConnectionString)
    End Sub
End Class
