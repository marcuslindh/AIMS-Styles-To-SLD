Public Class SLD_Window

    Public Property FileName As String = ""

    Private Sub SLD_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        SLD.Text = My.Computer.FileSystem.ReadAllText(FileName)
    End Sub

    Private Sub ShowLog_Click(sender As Object, e As RoutedEventArgs)
        Dim FileName As String = Environment.CurrentDirectory & "\SLD_Log.txt"
        If IO.File.Exists(FileName) Then
            System.Diagnostics.Process.Start(FileName)
        Else
            MessageBox.Show("There is no log file!", "", MessageBoxButton.OK, MessageBoxImage.Error)
        End If
    End Sub
End Class
