Imports System.Text
Imports System.Xml
Imports AIMS

Public Class SLD
    Public Shared Property RemoveÅÄÖ As Boolean = False
    Public Shared Function GenerateFilter(Filter As String) As StringBuilder
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
                        If SP.text.ToUpper = "IN" Then SP.Type = SyntaxPart.SyntaxPartType.IN
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


                If Filter.Substring(i, 1) = "(" Then
                    Dim SP As New SyntaxPart With {.text = word}
                    SP.text = "("
                    SP.Type = SyntaxPart.SyntaxPartType.String
                    result.Add(SP)
                    word = ""
                Else
                    word += Filter.Substring(i, 1)
                End If

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
                            If SP.text.ToUpper = "IN" Then SP.Type = SyntaxPart.SyntaxPartType.IN
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
            If SP.text.ToUpper = "IN" Then SP.Type = SyntaxPart.SyntaxPartType.IN
            If SP.text.ToUpper = "=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = ">" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = "<" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = ">=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator
            If SP.text.ToUpper = "<=" Then SP.Type = SyntaxPart.SyntaxPartType.Operator


            result.Add(SP)
        End If

        'Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented))

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
            ElseIf result(i).Type = SyntaxPart.SyntaxPartType.IN Then
                NSP = result(i - 1)

                For y As Integer = i + 2 To result.Count - 1
                    If result(y).text = ")" Then Exit For
                    If Not result(y).text = "," Then
                        NSP.Nodes.Add(result(y))
                    End If
                Next


                NewResult.Add(NSP)
            End If
        Next

        'Console.WriteLine(JsonConvert.SerializeObject(NewResult, Formatting.Indented))

        Dim res As New StringBuilder
        For i As Integer = 0 To NewResult.Count - 1
            Dim item = NewResult(i)

            If i + 1 < NewResult.Count - 1 Then
                If NewResult(i + 1).Type = SyntaxPart.SyntaxPartType.AND Then
                    res.Append("<AND>" & vbCrLf)
                ElseIf NewResult(i + 1).Type = SyntaxPart.SyntaxPartType.OR Then
                    If i - 1 > 0 Then
                        If Not NewResult(i - 1).Type = SyntaxPart.SyntaxPartType.OR Then
                            res.Append("<OR>" & vbCrLf)
                        End If
                    Else
                        res.Append("<OR>" & vbCrLf)
                    End If
                End If
            End If

            If item.Type = SyntaxPart.SyntaxPartType.String Then
                If item.Nodes.Count >= 2 Then
                    res.Append("<OR>" & vbCrLf)
                End If
            End If

            If item.Type = SyntaxPart.SyntaxPartType.String Then
                If item.Nodes.Count > 0 Then
                    For Each Nodeitem In item.Nodes
                        If Nodeitem.Type = SyntaxPart.SyntaxPartType.Operator Then
                            If Nodeitem.text = "=" Then
                                res.Append("<PropertyIsEqualTo>" & vbCrLf)
                                res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                                res.Append("<Literal>" & Nodeitem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                                res.Append("</PropertyIsEqualTo>" & vbCrLf)
                            ElseIf item.Nodes(0).text = ">" Then
                                res.Append("<PropertyIsGreaterThan>" & vbCrLf)
                                res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                                res.Append("<Literal>" & Nodeitem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                                res.Append("</PropertyIsGreaterThan>" & vbCrLf)
                            ElseIf Nodeitem.text = "<" Then
                                res.Append("<PropertyIsLessThan>" & vbCrLf)
                                res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                                res.Append("<Literal>" & Nodeitem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                                res.Append("</PropertyIsLessThan>" & vbCrLf)
                            ElseIf Nodeitem.text = ">=" Then
                                res.Append("<PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                                res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                                res.Append("<Literal>" & Nodeitem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                                res.Append("</PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                            ElseIf Nodeitem.text = "<=" Then
                                res.Append("<PropertyIsLessThanOrEqualTo>" & vbCrLf)
                                res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                                res.Append("<Literal>" & Nodeitem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                                res.Append("</PropertyIsLessThanOrEqualTo>" & vbCrLf)
                            End If
                        ElseIf Nodeitem.Type = SyntaxPart.SyntaxPartType.String Then
                            res.Append("<PropertyIsEqualTo>" & vbCrLf)
                            res.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                            res.Append("<Literal>" & Nodeitem.text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                            res.Append("</PropertyIsEqualTo>" & vbCrLf)
                        End If
                    Next
                End If

                If item.Type = SyntaxPart.SyntaxPartType.String Then
                    If item.Nodes.Count >= 2 Then
                        res.Append("</OR>" & vbCrLf)
                    End If
                End If

                If i - 1 > 0 Then
                    If NewResult(i - 1).Type = SyntaxPart.SyntaxPartType.AND Then
                        res.Append("</AND>" & vbCrLf)
                    ElseIf NewResult(i - 1).Type = SyntaxPart.SyntaxPartType.OR Then
                        If i + 1 < NewResult.Count - 1 Then
                            If Not NewResult(i + 1).Type = SyntaxPart.SyntaxPartType.OR Then
                                res.Append("</OR>" & vbCrLf)
                            End If
                        Else
                            res.Append("</OR>" & vbCrLf)
                        End If
                    End If
                End If
            Else

            End If
        Next



        'Console.WriteLine(Filter)
        Return res
        'Return JsonConvert.SerializeObject(NewResult, Formatting.Indented)
    End Function
    Private Shared Function ConvertNumberByUnitToPixals(Unit As String, Number As String, Optional ByRef ToString As Boolean = False)
        Dim val As Double = Double.Parse(Number.Replace(".", ","))

        Select Case Unit
            Case "Millimeters"
                If ToString Then Return Math.Round((val / 0.3528), 2).ToString().Replace(",", ".")
                Return Math.Round((val / 0.3528), 2)
            Case "Centimeters"
                If ToString Then Return Math.Round((val * 100) / 0.3528, 2).ToString().Replace(",", ".")
                Return Math.Round((val * 100) / 0.3528, 2)
            Case "Meters"
                If ToString Then Return Math.Round((val * 1000) / 0.3528, 2).ToString().Replace(",", ".")
                Return Math.Round((val * 1000) / 0.3528, 2)
            Case "Points"
                If ToString Then Return Math.Round(val, 2).ToString().Replace(",", ".")
                Return Math.Round(val, 2)
        End Select
        Return Number
    End Function
    Private Shared Function ConvertLineWidthByUnitToPixals(Unit As String, Number As String, Optional ByRef ToString As Boolean = False)
        Dim val As Double = Double.Parse(Number.Replace(".", ","))

        Dim GeoServerDPI As Integer = 90
        Dim meters_per_inch As Double = 0.0254
        Dim metersPerPixel As Double = meters_per_inch / GeoServerDPI


        Select Case Unit
            Case "Millimeters"
                If ToString Then Return Math.Round(val / 0.3528, 2).ToString().Replace(",", ".")
                Return Math.Round((val / 0.3528), 2)
            Case "Centimeters"
                'If ToString Then Return Math.Round((val * 10) / 0.3528, 2).ToString().Replace(",", ".")
                'Return Math.Round((val * 100) / 0.3528, 2)

                Return Math.Round(((val / 100)) / metersPerPixel, 2).ToString().Replace(",", ".")

            Case "Meters"
                If ToString Then Return Math.Round((val * 1000) / 0.3528, 2).ToString().Replace(",", ".")
                Return Math.Round((val * 1000) / 0.3528, 2)
            Case "Points"
                If ToString Then Return Math.Round(val, 2).ToString().Replace(",", ".")
                Return Math.Round(val, 2)
        End Select
        Return Number
    End Function

    Private Shared Sub RenderStroke(SLD As StringBuilder, Strokes As List(Of AIMS.StyleRuleStroke), StyleType As AIMS.LayerStyle.LayerStyleType, Optional ByVal SelectMin As Boolean = False)
        If Not Strokes.Count = 0 Then
            Dim MaxStroke = Strokes(0)

            If SelectMin = True Then
                For Each Stroke In Strokes
                    If MaxStroke.Thickness > Stroke.Thickness Then
                        MaxStroke = Stroke
                    End If
                Next
            Else
                For Each Stroke In Strokes
                    If MaxStroke.Thickness < Stroke.Thickness Then
                        MaxStroke = Stroke
                    End If
                Next
            End If

            If Not String.IsNullOrWhiteSpace(MaxStroke.Color) Then
                SLD.Append("<Stroke>" & vbCrLf)
                SLD.Append("<CssParameter name=""Stroke"">#" & MaxStroke.Color.Substring(2, MaxStroke.Color.Length - 2) & "</CssParameter>" & vbCrLf)

                Dim Thickness As String = 0
                If StyleType = AIMS.LayerStyle.LayerStyleType.Line Then
                    Thickness = ConvertLineWidthByUnitToPixals(MaxStroke.Unit, MaxStroke.Thickness, True)
                Else
                    Thickness = ConvertNumberByUnitToPixals(MaxStroke.Unit, MaxStroke.Thickness, True)
                End If

                If MaxStroke.Thickness = 0 Then
                    Thickness = "0.5"
                End If
                SLD.Append("<CssParameter name=""stroke-width"">" & Thickness & "</CssParameter>" & vbCrLf)
                If MaxStroke.LineStyle = "Dash" Then
                    SLD.Append("<CssParameter name=""stroke-dasharray"">5 2</CssParameter>" & vbCrLf)
                End If
                SLD.Append("</Stroke>" & vbCrLf)
            End If

        End If
    End Sub

    Public Shared Sub CreateSLD(layer As AIMS.LayerDefinition)

        Dim SLD As New StringBuilder

        SLD.Append("<?xml version=""1.0"" encoding=""ISO-8859-1""?>" & vbCrLf)
        SLD.Append("<StyledLayerDescriptor version=""1.0.0"" xsi:schemaLocation=""http://www.opengis.net/sld http://schemas.opengis.net/sld/1.0.0/StyledLayerDescriptor.xsd"" xmlns=""://www.opengis.net/sld"" xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        SLD.Append(vbCrLf)
        SLD.Append("<NamedLayer>" & vbCrLf)

        'Dim layer = App.GetLayerDefinition(ResourceId)
        If RemoveÅÄÖ Then
            layer.FeatureName = layer.FeatureName.Replace("å", "a").Replace("ä", "a").Replace("ö", "o")
        End If


        SLD.Append("<Name>" & layer.FeatureName.Replace("Default:", "") & "</Name>" & vbCrLf)
        SLD.Append("<UserStyle>" & vbCrLf)
        SLD.Append("<Title>" & layer.FeatureName.Replace("Default:", "") & "</Title>" & vbCrLf)
        SLD.Append("<FeatureTypeStyle>" & vbCrLf)


        For Each item In layer.VectorScaleRange

            For Each item2 In item.Style

                For Each item3 In item2.Rules
                    If item3.Fill.FillPattern = "" And item3.Stroke.Count = 0 And item3.Label.FontName = "" Then Continue For


                    SLD.Append("<Rule>" & vbCrLf)

                    If RemoveÅÄÖ Then
                        item3.LegendLabel = item3.LegendLabel.Replace("å", "a").Replace("ä", "a").Replace("ö", "o")
                    End If

                    SLD.Append("<Title>" & item3.LegendLabel & "</Title>" & vbCrLf)
                    If Not item.MaxScale = -1 Then
                        SLD.Append("<MaxScaleDenominator>" & item.MaxScale & "</MaxScaleDenominator>" & vbCrLf)
                    End If
                    If Not item.MinScale = -1 Then
                        SLD.Append("<MinScaleDenominator>" & item.MinScale & "</MinScaleDenominator>" & vbCrLf)
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

                        'For Each Stroke In item3.Stroke
                        '    If Not String.IsNullOrWhiteSpace(Stroke.Color) Then
                        '        SLD.Append("<Stroke>" & vbCrLf)
                        '        SLD.Append("<CssParameter name=""Stroke"">#" & Stroke.Color.Substring(2, Stroke.Color.Length - 2) & "</CssParameter>" & vbCrLf)
                        '        Dim Thickness As String = ConvertNumberByUnitToPixals(Stroke.Unit, Stroke.Thickness, True)
                        '        If Stroke.Thickness = 0 Then
                        '            Thickness = "0.5"
                        '        End If
                        '        SLD.Append("<CssParameter name=""stroke-width"">" & Thickness & "</CssParameter>" & vbCrLf)
                        '        If Stroke.LineStyle = "Dash" Then
                        '            SLD.Append("<CssParameter name=""stroke-dasharray"">5 2</CssParameter>" & vbCrLf)
                        '        End If
                        '        SLD.Append("</Stroke>" & vbCrLf)
                        '    End If
                        'Next

                        RenderStroke(SLD, item3.Stroke, LayerStyle.LayerStyleType.Area)

                        SLD.Append("</PolygonSymbolizer>" & vbCrLf)
                    ElseIf item2.Type = LayerStyle.LayerStyleType.Line Then
                        If Not item3.Fill.FillPattern = "" Or Not item3.Stroke.Count = 0 Then
                            SLD.Append("<LineSymbolizer>" & vbCrLf)


                            RenderStroke(SLD, item3.Stroke, LayerStyle.LayerStyleType.Line)
                            'If Not String.IsNullOrWhiteSpace(item3.Stroke.Color) Then
                            '    SLD.Append("<Stroke>" & vbCrLf)
                            '    SLD.Append("<CssParameter name=""Stroke"">#" & item3.Stroke.Color.Substring(2, item3.Stroke.Color.Length - 2) & "</CssParameter>" & vbCrLf)
                            '    'Dim Thickness As String = ConvertNumberByUnitToPixals(item3.Stroke.Unit, item3.Stroke.Thickness, True)
                            '    Dim Thickness As String = ConvertLineWidthByUnitToPixals(item3.Stroke.Unit, item3.Stroke.Thickness, True)
                            '    If item3.Stroke.Thickness = 0 Then
                            '        Thickness = "0.5"
                            '    End If
                            '    SLD.Append("<CssParameter name=""stroke-width"">" & Thickness & "</CssParameter>" & vbCrLf)
                            '    If item3.Stroke.LineStyle = "Dash" Then
                            '        SLD.Append("<CssParameter name=""stroke-dasharray"">5 2</CssParameter>" & vbCrLf)
                            '    End If
                            '    SLD.Append("</Stroke>" & vbCrLf)
                            'End If

                            SLD.Append("</LineSymbolizer>" & vbCrLf)
                        End If
                    ElseIf item2.Type = LayerStyle.LayerStyleType.Point Then

                        If Not String.IsNullOrWhiteSpace(item3.Label.Text) Then
                            SLD.Append("<TextSymbolizer>" & vbCrLf)
                            SLD.Append("<Label>" & vbCrLf)
                            SLD.Append("<ogc:PropertyName>" & item3.Label.Text.Replace("""", "").Replace("'", "") & "</ogc:PropertyName>" & vbCrLf)
                            SLD.Append("</Label>" & vbCrLf)

                            SLD.Append("<Font>" & vbCrLf)
                            SLD.Append("<CssParameter name=""font-family"">" & item3.Label.FontName & "</CssParameter>" & vbCrLf)

                            Dim FontSize As Double = ConvertNumberByUnitToPixals(item3.Label.Unit, item3.Label.SizeY, True)
                            'Select Case item3.Label.Unit
                            '    Case "Millimeters"
                            '        FontSize = (Double.Parse(item3.Label.SizeY.Replace(".", ",")) / 0.3528)
                            '    Case "Centimeters"
                            '        FontSize = (Double.Parse(item3.Label.SizeY.Replace(".", ",")) / 100) * 0.3528
                            '    Case "Meters"
                            '        FontSize = (Double.Parse(item3.Label.SizeY.Replace(".", ",")) / 1000) * 0.3528
                            '    Case "Points"
                            '        FontSize = Double.Parse(item3.Label.SizeY.Replace(".", ","))
                            'End Select


                            SLD.Append("<CssParameter name=""font-size"">" & FontSize.ToString.Replace(",", ".") & "</CssParameter>" & vbCrLf)

                            If item3.Label.Italic = True Then
                                SLD.Append("<CssParameter name=""font-style"">italic</CssParameter>" & vbCrLf)
                            Else
                                SLD.Append("<CssParameter name=""font-style"">normal</CssParameter>" & vbCrLf)
                            End If

                            If item3.Label.Bold = True Then
                                SLD.Append("<CssParameter name=""font-weight"">bold</CssParameter>" & vbCrLf)
                            End If

                            SLD.Append("</Font>" & vbCrLf)

                            SLD.Append("<Fill>" & vbCrLf)
                            SLD.Append("<CssParameter name=""fill"">#" & item3.Label.ForegroundColor.Substring(2, item3.Label.ForegroundColor.Length - 2) & "</CssParameter>" & vbCrLf)
                            SLD.Append("</Fill>" & vbCrLf)

                            If Not item3.Label.BackgroundStyle = "Ghosted" Then
                                SLD.Append("<Graphic>" & vbCrLf)
                                SLD.Append("<Mark>" & vbCrLf)
                                SLD.Append("<WellKnownName>square</WellKnownName>" & vbCrLf)
                                SLD.Append("<Fill>" & vbCrLf)
                                SLD.Append("<CssParameter name=""fill"">#" & item3.Label.BackgroundColor.Substring(2, item3.Label.BackgroundColor.Length - 2) & "</CssParameter>" & vbCrLf)
                                SLD.Append("</Fill>" & vbCrLf)
                                SLD.Append("</Mark>" & vbCrLf)
                                SLD.Append("</Graphic>" & vbCrLf)
                            End If

                            SLD.Append("</TextSymbolizer>" & vbCrLf)
                        End If
                    End If



                    SLD.Append("</Rule>" & vbCrLf)
                Next
            Next
        Next

        SLD.Append("</FeatureTypeStyle>" & vbCrLf)

        For Each item In layer.VectorScaleRange

            For Each item2 In item.Style

                For Each item3 In item2.Rules
                    If item3.Fill.FillPattern = "" And item3.Stroke.Count = 0 And item3.Label.FontName = "" Then Continue For
                    If Not item2.Type = LayerStyle.LayerStyleType.Line Then Continue For
                    SLD.Append("<FeatureTypeStyle>" & vbCrLf)
                    SLD.Append("<Rule>" & vbCrLf)

                    If RemoveÅÄÖ Then
                        item3.LegendLabel = item3.LegendLabel.Replace("å", "a").Replace("ä", "a").Replace("ö", "o")
                    End If

                    SLD.Append("<Title>" & item3.LegendLabel & "</Title>" & vbCrLf)
                    If Not item.MaxScale = -1 Then
                        SLD.Append("<MaxScaleDenominator>" & item.MaxScale & "</MaxScaleDenominator>" & vbCrLf)
                    End If
                    If Not item.MinScale = -1 Then
                        SLD.Append("<MinScaleDenominator>" & item.MinScale & "</MinScaleDenominator>" & vbCrLf)
                    End If

                    If Not String.IsNullOrWhiteSpace(item3.Filter) Then
                        SLD.Append("<ogc:Filter>" & vbCrLf)
                        SLD.Append(GenerateFilter(item3.Filter))
                        SLD.Append("</ogc:Filter>" & vbCrLf)
                    End If
                    SLD.Append("<LineSymbolizer>" & vbCrLf)
                    RenderStroke(SLD, item3.Stroke, LayerStyle.LayerStyleType.Line, True)
                    SLD.Append("</LineSymbolizer>" & vbCrLf)

                    SLD.Append("</Rule>" & vbCrLf)
                    SLD.Append("</FeatureTypeStyle>" & vbCrLf)
                Next
            Next
        Next



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
End Class
