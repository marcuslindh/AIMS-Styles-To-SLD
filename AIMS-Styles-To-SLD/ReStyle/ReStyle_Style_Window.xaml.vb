Public Class ReStyle_Style_Window

    Public Property Styles As ReStyleRuleStyle

    Private Sub ReStyle_PointStyle_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        StrokeList.ItemsSource = Styles.Stroke
        Fill.DataContext = Styles.Fill
    End Sub

    Private Sub StrokeList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles StrokeList.SelectionChanged
        Stroke.DataContext = StrokeList.SelectedItem
    End Sub
End Class
