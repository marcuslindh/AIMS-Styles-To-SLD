Public Class SLD_Window

    Public Property FileName As String = ""

    Private Sub SLD_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        SLD.Text = My.Computer.FileSystem.ReadAllText(FileName)
    End Sub
End Class
