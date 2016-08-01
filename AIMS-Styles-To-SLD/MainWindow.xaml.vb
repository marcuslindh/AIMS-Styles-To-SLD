Class MainWindow

    Public App As New AIMS

    Public Sub Login(Optional ByVal Username As String = "", Optional ByVal Site As String = "")
        Dim Log As New Login

        If Not Username = "" And Not Site = "" Then
            Log.Username.Text = Username
            Log.Site.Text = Site
        Else
            If IO.File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\AIMS_settings.txt") Then
                Try
                    Dim txt As String = My.Computer.FileSystem.ReadAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "\AIMS_settings.txt")
                    Dim lines() As String = txt.Split(vbCrLf)
                    Log.Username.Text = lines(0).Trim
                    Log.Site.Text = lines(1).Trim


                Catch ex As Exception
                End Try

            End If
        End If


        If Log.ShowDialog Then
            Try
                App.Login(Log.Username.Text, Log.Password.Password, Log.Site.Text)

                Dim Version = App.GetServerVersion

                Dim text As String = ""
                text += Log.Username.Text & vbCrLf
                text += Log.Site.Text & vbCrLf

                My.Computer.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "\AIMS_settings.txt", text, False)


            Catch ex As Exception
                MessageBox.Show("Can not sign in, may be due to the wrong username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error)

                Login()
            End Try
        End If
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Login()





    End Sub
End Class
