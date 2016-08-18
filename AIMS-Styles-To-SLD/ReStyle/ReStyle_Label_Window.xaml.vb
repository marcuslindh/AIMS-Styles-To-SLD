Imports AIMS
Public Class ReStyle_Label_Window

    Public Property RootStyle As StyleRuleLabel

    Private Sub ReStyle_Label_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LabelStyle.DataContext = RootStyle
    End Sub


End Class
