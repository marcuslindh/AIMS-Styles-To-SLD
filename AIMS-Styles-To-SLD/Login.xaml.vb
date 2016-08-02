Public Class Login

    Private Sub Login_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = True
    End Sub

    Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
    End Sub

    Private Sub Login_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Password.Focusable = True
        Password.Focus()
    End Sub
End Class
