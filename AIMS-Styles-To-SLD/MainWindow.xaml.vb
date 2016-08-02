Imports System.Text
Imports System.Xml

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

                StartTree()

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

#Region "Tree"
    Public Sub PopulateNodes(node As TreeViewItem)
        Dim Items = App.GetResources(node.Tag)

        'Test.Items.Add("GetResources: " & vbCrLf & String.Join(vbCrLf, Items.Select(Function(x) String.Join(vbCrLf, x.ResourceId))))

        For Each item In Items
            Dim TreeNode As New TreeViewItem

            Dim s() As String = item.ResourceId.Split("/")
            Dim Name As String = ""


            If item.ResourceId.EndsWith("/") Then

                Name = s(s.Length - 2)
                TreeNode.Items.Add("Laddar...")
            Else
                Name = s(s.Length - 1)
            End If

            TreeNode.Header = Name
            TreeNode.Tag = item.ResourceId
            node.Items.Add(TreeNode)
        Next

    End Sub

    Public Sub StartTree()

        Dim Root As New TreeViewItem With {.Header = App.Site, .Tag = "Library://"}
        Root.ExpandSubtree()
        Tree.Items.Add(Root)

        PopulateNodes(Root)
    End Sub

    Public Sub TreeViewItem_Expanded(sender As Object, e As RoutedEventArgs)
        Dim item = CType(e.Source, TreeViewItem)

        If item.Items.Count = 1 AndAlso TypeOf item.Items(0) Is String Then
            item.Items.Clear()
            PopulateNodes(item)
        End If


    End Sub

    Private Sub Tree_SelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object)) Handles Tree.SelectedItemChanged
        Dim ResourceId As String = CType(Tree.SelectedItem, TreeViewItem).Tag

        If ResourceId.Contains(".MapDefinition") Then
            PopulateLayers(ResourceId)
        End If

    End Sub
#End Region

#Region "Layers"
    Public Sub PopulateLayers(ResourceId As String)
        Dim def = App.GetMapDefinition(ResourceId)
        Layers.ItemsSource = def.MapLayers
    End Sub

    Private Sub Layers_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Layers.SelectionChanged
        Dim item = CType(Layers.SelectedItem, MapLayer)

        LayerName.Content = item.Name

        ExportSLD.IsEnabled = True

        Dim layer = App.GetLayerDefinition(item.ResourceId)

        Scales.ItemsSource = layer.VectorScaleRange



        'CreateSLD(layer.ResourceId)
        'PropertyMapping.ItemsSource = layer.PropertyMapping
        'MessageBox.Show(layer.PropertyMapping.Count)
    End Sub

    Private Sub ExportSLD_Click(sender As Object, e As RoutedEventArgs)
        Dim item = CType(Layers.SelectedItem, MapLayer)
        'Dim layer = App.GetLayerDefinition(item.ResourceId)

        CreateSLD(item.ResourceId)


    End Sub
#End Region

#Region "SLD"
    Public Function GenerateFilter(Filter As String)
        Dim result As New List(Of SyntaxPart)
        Dim word As String = ""
        Dim isString As Boolean = False
        For i As Integer = 0 To Filter.Length - 1
            If Filter.Substring(i, 1) = " " Then
                If isString = False Then
                    If word.Length > 0 Then
                        Dim SP As New SyntaxPart With {.text = word}
                        If SP.text.ToUpper = "AND" Then SP.Type = SyntaxPart.SyntaxPartType.AND
                        If SP.text.ToUpper = "OR" Then SP.Type = SyntaxPart.SyntaxPartType.OR
                        If SP.text.ToUpper = "=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                        If SP.text.ToUpper = ">" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                        If SP.text.ToUpper = "<" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                        If SP.text.ToUpper = ">=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                        If SP.text.ToUpper = "<=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator


                        result.Add(SP)
                        word = ""
                    End If
                Else
                    word += Filter.Substring(i, 1)
                End If
            Else
                word += Filter.Substring(i, 1)

                If isString = False Then
                    If Filter.Substring(i, 1) = "'" Or Filter.Substring(i, 1) = """" Then
                        isString = True
                    End If
                Else
                    If Filter.Substring(i, 1) = "'" Or Filter.Substring(i, 1) = """" Then
                        isString = False

                        If word.Length > 0 Then
                            Dim SP As New SyntaxPart With {.text = word}

                            If SP.text.ToUpper = "AND" Then SP.Type = SyntaxPart.SyntaxPartType.AND
                            If SP.text.ToUpper = "OR" Then SP.Type = SyntaxPart.SyntaxPartType.OR
                            If SP.text.ToUpper = "=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                            If SP.text.ToUpper = ">" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                            If SP.text.ToUpper = "<" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                            If SP.text.ToUpper = ">=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
                            If SP.text.ToUpper = "<=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator


                            result.Add(SP)
                            word = ""
                        End If
                    End If
                End If


            End If
        Next
        If word.Length > 0 Then
            Dim SP As New SyntaxPart With {.text = word}

            If SP.text.ToUpper = "AND" Then SP.Type = SyntaxPart.SyntaxPartType.AND
            If SP.text.ToUpper = "OR" Then SP.Type = SyntaxPart.SyntaxPartType.OR
            If SP.text.ToUpper = "=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = ">" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = "<" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = ">=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = "<=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator


            result.Add(SP)
        End If

        Dim NewResult As New List(Of SyntaxPart)
        Dim NSP As New SyntaxPart
        For i As Integer = 0 To result.Count - 1
            If result(i).Type = SyntaxPart.SyntaxPartType.Operator Then
                NSP = result(i - 1)
                result(i).Nodes.Add(result(i + 1))
                NSP.Nodes.Add(result(i))
                NewResult.Add(NSP)
            ElseIf result(i).Type = SyntaxPart.SyntaxPartType.AND Or result(i).Type = SyntaxPart.SyntaxPartType.OR Then
                NSP = result(i)
                NewResult.Add(NSP)
            End If
        Next

        Dim res As New StringBuilder
        For i As Integer = 0 To NewResult.Count - 1
            Dim item = NewResult(i)

            If i + 1 < NewResult.Count - 1 Then
                If NewResult(i + 1).Type = SyntaxPart.SyntaxPartType.AND Then
                    res.Append("<AND>" & vbCrLf)
                ElseIf NewResult(i + 1).Type = SyntaxPart.SyntaxPartType.OR Then
                    res.Append("<OR>" & vbCrLf)
                End If
            End If

            If item.Type = SyntaxPart.SyntaxPartType.String Then
                If item.Nodes.Count > 0 Then
                    If item.Nodes(0).Type = SyntaxPart.SyntaxPartType.Operator Then
                        If item.Nodes(0).text = "=" Then
                            res.Append("<PropertyIsEqualTo>" & vbCrLf)
                            res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                            res.Append("<Literal>" & item.Nodes(0).Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                            res.Append("</PropertyIsEqualTo>" & vbCrLf)
                        ElseIf item.Nodes(0).text = ">" Then
                            res.Append("<PropertyIsGreaterThan>" & vbCrLf)
                            res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                            res.Append("<Literal>" & item.Nodes(0).Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                            res.Append("</PropertyIsGreaterThan>" & vbCrLf)
                        ElseIf item.Nodes(0).text = "<" Then
                            res.Append("<PropertyIsLessThan>" & vbCrLf)
                            res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                            res.Append("<Literal>" & item.Nodes(0).Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                            res.Append("</PropertyIsLessThan>" & vbCrLf)
                        ElseIf item.Nodes(0).text = ">=" Then
                            res.Append("<PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                            res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                            res.Append("<Literal>" & item.Nodes(0).Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                            res.Append("</PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                        ElseIf item.Nodes(0).text = "<=" Then
                            res.Append("<PropertyIsLessThanOrEqualTo>" & vbCrLf)
                            res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                            res.Append("<Literal>" & item.Nodes(0).Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                            res.Append("</PropertyIsLessThanOrEqualTo>" & vbCrLf)
                        End If
                    End If
                End If

                If i - 1 > 0 Then
                    If NewResult(i - 1).Type = SyntaxPart.SyntaxPartType.AND Then
                        res.Append("</AND>" & vbCrLf)
                    ElseIf NewResult(i - 1).Type = SyntaxPart.SyntaxPartType.OR Then
                        res.Append("</OR>" & vbCrLf)
                    End If
                End If
            Else

            End If
        Next



        Console.WriteLine(Filter)
        Return res
        'Return JsonConvert.SerializeObject(NewResult, Formatting.Indented)
    End Function
    Public Sub CreateSLD(ResourceId As String)

        Dim SLD As New StringBuilder

        SLD.Append("<?xml version=""1.0"" encoding=""ISO-8859-1""?>" & vbCrLf)
        SLD.Append("<StyledLayerDescriptor version=""1.0.0"" xsi:schemaLocation=""http://www.opengis.net/sld http://schemas.opengis.net/sld/1.0.0/StyledLayerDescriptor.xsd"" xmlns=""://www.opengis.net/sld"" xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        SLD.Append(vbCrLf)
        SLD.Append("<NamedLayer>" & vbCrLf)

        Dim layer = App.GetLayerDefinition(ResourceId)

        SLD.Append("<Name>" & layer.FeatureName.Replace("Default:", "") & "</Name>" & vbCrLf)
        SLD.Append("<UserStyle>" & vbCrLf)
        SLD.Append("<Title>" & layer.FeatureName.Replace("Default:", "") & "</Title>" & vbCrLf)
        SLD.Append("<FeatureTypeStyle>" & vbCrLf)


        For Each item In layer.VectorScaleRange

            For Each item2 In item.Style

                For Each item3 In item2.Rules
                    SLD.Append("<Rule>" & vbCrLf)
                    SLD.Append("<Title>" & item3.LegendLabel & "</Title>" & vbCrLf)
                    If Not item.MaxScale = -1 Then
                        SLD.Append("<MaxScaleDenominator>" & item.MaxScale & "</MaxScaleDenominator>" & vbCrLf)
                    End If
                    If Not item.MinScale = -1 Then
                        SLD.Append("<MinScaleDenominator>" & item.MaxScale & "</MinScaleDenominator>" & vbCrLf)
                    End If

                    If Not String.IsNullOrWhiteSpace(item3.Filter) Then
                        SLD.Append("<ogc:Filter>" & vbCrLf)
                        SLD.Append(GenerateFilter(item3.Filter))
                        SLD.Append("</ogc:Filter>" & vbCrLf)
                    End If

                    If item2.Type = LayerStyle.LayerStyleType.Area Then
                        SLD.Append("<PolygonSymbolizer>" & vbCrLf)

                        If Not String.IsNullOrWhiteSpace(item3.Fill.ForegroundColor) Then
                            SLD.Append("<Fill>" & vbCrLf)
                            SLD.Append("<CssParameter name=""fill"">#" & item3.Fill.ForegroundColor.Substring(2, item3.Fill.ForegroundColor.Length - 2) & "</CssParameter>" & vbCrLf)
                            SLD.Append("</Fill>" & vbCrLf)
                        End If

                        If Not String.IsNullOrWhiteSpace(item3.Stroke.Color) Then
                            SLD.Append("<Stroke>" & vbCrLf)
                            SLD.Append("<CssParameter name=""Stroke"">#" & item3.Stroke.Color.Substring(2, item3.Stroke.Color.Length - 2) & "</CssParameter>" & vbCrLf)
                            Dim Thickness As String = item3.Stroke.Thickness
                            If item3.Stroke.Thickness = 0 Then
                                Thickness = "0.5"
                            End If
                            SLD.Append("<CssParameter name=""stroke-width"">" & Thickness & "</CssParameter>" & vbCrLf)
                            If item3.Stroke.LineStyle = "Dash" Then
                                SLD.Append("<CssParameter name=""stroke-dasharray"">5 2</CssParameter>" & vbCrLf)
                            End If
                            SLD.Append("</Stroke>" & vbCrLf)
                        End If


                        SLD.Append("</PolygonSymbolizer>" & vbCrLf)
                    ElseIf item2.Type = LayerStyle.LayerStyleType.Line Then
                        SLD.Append("<LineSymbolizer>" & vbCrLf)

                        If Not String.IsNullOrWhiteSpace(item3.Stroke.Color) Then
                            SLD.Append("<Stroke>" & vbCrLf)
                            SLD.Append("<CssParameter name=""Stroke"">#" & item3.Stroke.Color.Substring(2, item3.Stroke.Color.Length - 2) & "</CssParameter>" & vbCrLf)
                            Dim Thickness As String = item3.Stroke.Thickness
                            If item3.Stroke.Thickness = 0 Then
                                Thickness = "0.5"
                            End If
                            SLD.Append("<CssParameter name=""stroke-width"">" & Thickness & "</CssParameter>" & vbCrLf)
                            If item3.Stroke.LineStyle = "Dash" Then
                                SLD.Append("<CssParameter name=""stroke-dasharray"">5 2</CssParameter>" & vbCrLf)
                            End If
                            SLD.Append("</Stroke>" & vbCrLf)
                        End If

                        SLD.Append("</LineSymbolizer>" & vbCrLf)
                    End If



                    SLD.Append("</Rule>" & vbCrLf)
                Next
            Next
        Next

        SLD.Append("</FeatureTypeStyle>" & vbCrLf)
        SLD.Append("</UserStyle>" & vbCrLf)
        SLD.Append("</NamedLayer>")
        SLD.Append("</StyledLayerDescriptor>")


        Dim doc As New XmlDocument()
        doc.LoadXml(SLD.ToString)
        ' Save the document to a file and auto-indent the output.
        Using writer As New XmlTextWriter(My.Computer.FileSystem.SpecialDirectories.Temp & "\SLD.xml", Nothing)
            writer.Formatting = Formatting.Indented
            doc.Save(writer)
        End Using


        Dim SLDWin As New SLD_Window
        SLDWin.FileName = My.Computer.FileSystem.SpecialDirectories.Temp & "\SLD.xml"

        SLDWin.Show()



        'My.Computer.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "\SLD.xml", SLD.ToString, False)
        'System.Diagnostics.Process.Start(My.Computer.FileSystem.SpecialDirectories.Temp & "\SLD.xml")
    End Sub
#End Region

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Login()

    End Sub




End Class

Public Class SyntaxPart
    Public Enum SyntaxPartType
        [String] = 1
        [Operator] = 2
        [AND] = 3
        [OR] = 4
    End Enum
    Public Property Type As SyntaxPartType = SyntaxPartType.String
    Public Property text As String = ""
    Public Property Nodes As New List(Of SyntaxPart)
End Class
