Imports System.Net

Public Class Tools

    Public Shared Function StringToBase64(text As String) As String
        Dim buffer = System.Text.Encoding.UTF8.GetBytes(text)
        Return System.Convert.ToBase64String(buffer)
    End Function

    Public Shared Sub SetBasicAuthHeader(Request As WebRequest, Username As String, Password As String)
        Dim AuthInfo As String = Username & ":" & Password
        AuthInfo = StringToBase64(AuthInfo)
        Request.Headers("Authorization") = "Basic " & AuthInfo
    End Sub



End Class

