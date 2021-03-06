﻿Imports AIMS
Imports System.Collections.ObjectModel
Imports System.Runtime.CompilerServices
Imports System.ComponentModel
Imports Microsoft.Win32
Imports Newtonsoft.Json
Imports System.Text

Public Class ReStyle_Window
    Public Property StartStyle As LayerDefinition

    Private Shared FileFilter As String = "ReStyle (*.restyle)|*.restyle|JSON (*.json)|*.json|All files (*.*)|*.*"

    Public Property Scales As New ObservableCollection(Of ReStyleScale)

    Public Function GetDataInAIMSFormat() As LayerDefinition
        Dim NewDef As New LayerDefinition
        NewDef.FeatureName = LayerName.Text
        'NewDef.FeatureNameType = LayerName.Text
        'NewDef.Geometry = StartStyle.Geometry
        'NewDef.ResourceId = StartStyle.ResourceId
        'NewDef.ToolTip = StartStyle.ToolTip
        'NewDef.PropertyMapping = StartStyle.PropertyMapping

        For Each Scale In Scales
            Dim NewScale As New VectorScaleRange
            NewScale.MinScale = Scale.From
            NewScale.MaxScale = Scale.To

            Dim NewLayerStyle As New LayerStyle
            For Each Rule In Scale.Rules
                NewLayerStyle.Type = Rule.Type

                Dim NewRule As New LayerStyleRule

                NewRule.Filter = Rule.Condition
                NewRule.Label = Rule.FeatureLabel
                NewRule.LegendLabel = Rule.Legendlabel
                NewRule.Fill = Rule.Style.Fill
                NewRule.Stroke = Rule.Style.Stroke

                NewLayerStyle.Rules.Add(NewRule)
            Next
            NewScale.Style.Add(NewLayerStyle)

            NewDef.VectorScaleRange.Add(NewScale)
        Next
        Return NewDef
    End Function
    Public Sub GetDataInReStyleFormat()
        For Each item In StartStyle.VectorScaleRange
            Dim VScale As New ReStyleScale
            VScale.From = item.MinScale
            VScale.To = item.MaxScale

            For Each itemStyle In item.Style
                itemStyle = SLD.ConvertFieldsToSLDFields(itemStyle)
                Dim i As Integer = 1
                For Each itemRule In itemStyle.Rules
                    Dim Rule As New ReStyleRule
                    Rule.Type = itemStyle.Type
                    Rule.Condition = itemRule.Filter
                    Rule.FeatureLabel = itemRule.Label
                    Rule.Legendlabel = itemRule.LegendLabel
                    Rule.Style.Fill = itemRule.Fill
                    Rule.Style.Stroke = itemRule.Stroke
                    Rule.Style.Name = "Line " & i
                    VScale.Rules.Add(Rule)
                    i += 1
                Next
            Next
            Scales.Add(VScale)
        Next
    End Sub

    Public Sub Test()
        Dim TestScale As New ReStyleScale With {.From = 0, .To = 500}
        Dim TestRule As New ReStyleRule With {.Condition = "VALUE   IN (  '0' ,  '1' , '2' , '3' , '4' )"}
        TestRule.FeatureLabel.Text = "TEST"
        'TestRule.Style.Fill.BackgroundColor = "FF00FF"
        TestRule.Legendlabel = "Större bilväg"
        TestScale.Rules.Add(TestRule)

        Scales.Add(TestScale)
    End Sub

    Private Sub ReStyle_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ScaleRanges.ItemsSource = Scales
        GetDataInReStyleFormat()
    End Sub
    Private Sub ScaleRanges_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ScaleRanges.SelectionChanged
        Try
            StyleRules.ItemsSource = CType(ScaleRanges.SelectedItem, ReStyleScale).Rules
        Catch ex As Exception
            StyleRules.ItemsSource = Nothing
        End Try
    End Sub

    Private Sub ConditionButton_Click(sender As Object, e As RoutedEventArgs)
        Dim B = CType(sender, Button)
        Dim style = CType(B.CommandParameter, ReStyleRule)
        'MessageBox.Show(style.Condition)

        Dim w As New ReStyle_Filter
        w.RootStyle = style
        w.Show()

    End Sub
    Private Sub StyleButton_Click(sender As Object, e As RoutedEventArgs)
        Dim B = CType(sender, Button)
        Dim style = CType(B.CommandParameter, ReStyleRule)
        'MessageBox.Show(style.Style.Fill.BackgroundColor)

        Dim Style_Window As New ReStyle_Style_Window
        Style_Window.Styles = style.Style

        AddHandler Style_Window.Closing, Sub()
                                             ScaleRanges.Items.Refresh()
                                             StyleRules.Items.Refresh()
                                         End Sub

        Style_Window.Show()
    End Sub
    Private Sub FeatureLabelButton_Click(sender As Object, e As RoutedEventArgs)
        Dim B = CType(sender, Button)
        Dim style = CType(B.CommandParameter, ReStyleRule)

        Dim labelstyle As New ReStyle_Label_Window
        labelstyle.RootStyle = style.FeatureLabel

        AddHandler labelstyle.Closing, Sub()
                                           ScaleRanges.Items.Refresh()
                                           StyleRules.Items.Refresh()
                                       End Sub

        labelstyle.Show()



        'MessageBox.Show(style.FeatureLabel.Text)
    End Sub

    Private Sub DeleteStyle_Click(sender As Object, e As RoutedEventArgs)

        Dim RemoveItems As New ArrayList

        Dim Scale = CType(ScaleRanges.SelectedItem, ReStyleScale)

        For Each style As ReStyleRule In StyleRules.SelectedItems
            RemoveItems.Add(style)
        Next

        For Each item In RemoveItems
            Scale.Rules.Remove(item)
        Next

        ScaleRanges.Items.Refresh()

        'Dim selected = CType(StyleRules.SelectedItem, ReStyleRule)

        'Scale.Rules.Remove(selected)
    End Sub

    Private Sub DeleteScale_Click(sender As Object, e As RoutedEventArgs)

        Dim RemoveItems As New ArrayList


        For Each Scale As ReStyleScale In ScaleRanges.SelectedItems
            RemoveItems.Add(Scale)
        Next

        For Each item In RemoveItems
            Scales.Remove(item)
        Next


        'Dim Scale = CType(ScaleRanges.SelectedItem, ReStyleScale)
        'Scales.Remove(Scale)
    End Sub

    Private Sub ExportToSLD_Click(sender As Object, e As RoutedEventArgs)
        SLD.CreateSLD(SLD.RemoveÅÄÖFromLayerDefinition(GetDataInAIMSFormat()))
    End Sub

    Private Sub SaveAs_Click(sender As Object, e As RoutedEventArgs)
        Dim s As New SaveFileDialog
        s.Filter = FileFilter
        s.FileName = StartStyle.FeatureName
        If s.ShowDialog = True Then
            My.Computer.FileSystem.WriteAllText(s.FileName, JsonConvert.SerializeObject(Scales, Formatting.Indented), False)
        End If

    End Sub

    Private Sub OpenAs_Click(sender As Object, e As RoutedEventArgs)
        Dim o As New OpenFileDialog
        o.Filter = FileFilter

        If o.ShowDialog = True Then
            Scales.Clear()
            Dim json As String = My.Computer.FileSystem.ReadAllText(o.FileName)
            Dim SavedScales = JsonConvert.DeserializeObject(json, GetType(List(Of ReStyleScale)))

            Dim f As New IO.FileInfo(o.FileName)

            LayerName.Text = f.Name.Replace(f.Extension, "")

            Scales.Clear()
            For Each item In SavedScales
                Scales.Add(item)
            Next

            'LayerName.Text = StartStyle.FeatureName
            'GetDataInReStyleFormat()
        End If
    End Sub

 

    Private Sub AddScale_Click(sender As Object, e As RoutedEventArgs)
        Dim add As New ReStyle_AddScale

        If add.ShowDialog = True Then
            Scales.Add(New ReStyleScale With {.From = add.From_TextBox.Text, .To = add.To_TextBox.Text})
        End If

    End Sub

    Private Sub AddStyle_Click(sender As Object, e As RoutedEventArgs)
        CType(ScaleRanges.SelectedItem, ReStyleScale).Rules.Add(New ReStyleRule)
    End Sub

    Private Sub EditScale_Click(sender As Object, e As RoutedEventArgs)

        Dim Scale = CType(ScaleRanges.SelectedItem, ReStyleScale)

        Dim add As New ReStyle_AddScale
        add.From_TextBox.Text = Scale.From
        add.To_TextBox.Text = Scale.From

        If add.ShowDialog = True Then
            Scale.From = add.From_TextBox.Text
            Scale.To = add.To_TextBox.Text
            'Scales.Add(New ReStyleScale With {.From = add.From_TextBox.Text, .To = add.To_TextBox.Text})
        End If
    End Sub
End Class

Public Class ReStyleScale
    Implements INotifyPropertyChanged
#Region "PropertyChanged"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub update(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
        'Trace.WriteLine(propertyName)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
#End Region

    Private Property _From As String = ""
    Public Property From As String
        Get
            Return _From
        End Get
        Set(value As String)
            _From = value
            update()
        End Set
    End Property
    Private Property _To As String = ""
    Public Property [To] As String
        Get
            Return _To
        End Get
        Set(value As String)
            _To = value
            update()
        End Set
    End Property
    Private Property _Rules As New ObservableCollection(Of ReStyleRule)
    Public Property Rules As ObservableCollection(Of ReStyleRule)
        Get
            Return _Rules
        End Get
        Set(value As ObservableCollection(Of ReStyleRule))
            _Rules = value
            update()
            update("Preview")
        End Set
    End Property

    Public ReadOnly Property Preview As String
        Get
            Dim Res As New StringBuilder

            For Each item In Rules
                Res.Append(item.Formated)
            Next
            Return Res.ToString
        End Get
    End Property


End Class
Public Class ReStyleRule
    Implements INotifyPropertyChanged
#Region "PropertyChanged"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub update(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
        'Trace.WriteLine(propertyName)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
#End Region
    Private Property _Condition As String = ""
    Public Property Condition As String
        Get
            Return _Condition
        End Get
        Set(value As String)
            _Condition = value
            update()
        End Set
    End Property
    Private Property _Style As New ReStyleRuleStyle
    Public Property Style As ReStyleRuleStyle
        Get
            Return _Style
        End Get
        Set(value As ReStyleRuleStyle)
            _Style = value
            update()
            update("Formated")
        End Set
    End Property
    Private Property _FeatureLabel As New StyleRuleLabel
    Public Property FeatureLabel As StyleRuleLabel
        Get
            Return _FeatureLabel
        End Get
        Set(value As StyleRuleLabel)
            _FeatureLabel = value
            update()
            update("Formated")
        End Set
    End Property
    Private Property _Legendlabel As String = ""
    Public Property Legendlabel As String
        Get
            Return _Legendlabel
        End Get
        Set(value As String)
            _Legendlabel = value
            update()
        End Set
    End Property
    Public Property Type As LayerStyle.LayerStyleType


    Public ReadOnly Property Formated As String
        Get
            Dim str As New StringBuilder

            If Not Style.Fill.BackgroundColor = "" Then
                str.Append(Style.Fill.BackgroundColor)
                str.Append(" ")
            End If
            If Not Style.Fill.ForegroundColor = "" Then
                str.Append(Style.Fill.ForegroundColor)
                str.Append(" ")
            End If

            For Each item In Style.Stroke
                str.Append(item.Thickness & "px")
                str.Append(" ")
                str.Append(item.Color)
                str.Append(", ")
            Next

            Return str.ToString
        End Get
    End Property


End Class
Public Class ReStyleRuleStyle
    Implements INotifyPropertyChanged
#Region "PropertyChanged"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub update(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
        'Trace.WriteLine(propertyName)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
#End Region
    Private Property _Fill As New StyleRuleFill
    Public Property Fill As StyleRuleFill
        Get
            Return _Fill
        End Get
        Set(value As StyleRuleFill)
            _Fill = value
            update()
            update("Formated")
        End Set
    End Property
    Private Property _Stroke As New List(Of StyleRuleStroke)
    Public Property Stroke As List(Of StyleRuleStroke)
        Get
            Return _Stroke
        End Get
        Set(value As List(Of StyleRuleStroke))
            _Stroke = value
            update()
            update("Formated")
        End Set
    End Property

    Private Property _Name As String = ""
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property


End Class