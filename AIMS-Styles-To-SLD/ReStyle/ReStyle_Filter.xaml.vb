Public Class ReStyle_Filter

    Public Property RootStyle As ReStyleRule

    Private Sub ReStyle_Filter_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Filter.Text = RootStyle.Condition
    End Sub

    Private Sub Save_Click(sender As Object, e As RoutedEventArgs)
        RootStyle.Condition = Filter.Text
    End Sub
End Class
