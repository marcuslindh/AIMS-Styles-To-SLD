Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class ColorTextbox

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        CType(Me.Content, FrameworkElement).DataContext = Me

        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Public Property Text As String
        Get
            Return CType(Me.GetValue(TextProperty), String)
        End Get
        Set(value As String)
            SetValueDp(TextProperty, value)
            UpdateText(Text)
            TextColor.Text = value
        End Set
    End Property
    Public Shared Property TextProperty As DependencyProperty = DependencyProperty.Register("Text",
                                                                                            GetType(String),
                                                                                            GetType(ColorTextbox),
                                                                                            New PropertyMetadata(""))

    Public Event PropertyChanged As PropertyChangedEventHandler
    Public Sub SetValueDp([property] As DependencyProperty, value As Object, <CallerMemberName()> Optional ByVal p As String = "")
        SetValue([property], value)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(p))
    End Sub

    Private Sub UpdateText(txt As String)

        Try
            ColorPik.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString(txt), Color))
        Catch ex As Exception
        End Try
    End Sub


    Private Sub TextColor_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TextColor.TextChanged
        Text = TextColor.Text
        UpdateText(TextColor.Text)
    End Sub
End Class
