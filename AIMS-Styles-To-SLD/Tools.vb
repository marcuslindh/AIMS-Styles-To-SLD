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

    Public Shared Function IfElementToString(obj As XElement) As String
        If obj IsNot Nothing Then
            Return obj.Value
        End If
        Return ""
    End Function

    Public Shared Function IfElementToBoolean(obj As XElement) As Boolean
        If obj IsNot Nothing Then
            If obj.Value = "true" Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Shared Function IfElementToLong(obj As XElement) As Long
        If obj IsNot Nothing Then
            If Not String.IsNullOrEmpty(obj.Value) Then
                Return obj.Value
            End If
        End If
        Return 0
    End Function

    Public Shared Function IfElementToInteger(obj As XElement) As Integer
        If obj IsNot Nothing Then
            If Not String.IsNullOrEmpty(obj.Value) Then
                Return obj.Value
            End If
        End If
        Return -1
    End Function

    Public Shared Function FormatSize(s As Long) As String
        If s < 1024 ^ 2 Then
            Return Math.Round((s / 1024), 1) & " KB"
        ElseIf s < 1024 ^ 3 Then
            Return Math.Round((s / 1024 ^ 2), 1) & " MB"
        ElseIf s > 1024 ^ 3 Then
            Return Math.Round((s / 1024 ^ 3), 1) & " GB"
        End If
    End Function


End Class

