﻿Imports System.Text
Imports System.Xml
Imports AIMS
Imports System.IO

Public Class SLD
    Public Shared Property RemoveÅÄÖ As Boolean = False

    Private Shared Property LogFile As String = Environment.CurrentDirectory & "\SLD_Log.txt"
    Private Shared Writer As StreamWriter
    Private Shared WriterOpen As Boolean = False


    Private Shared Sub ResetLog()
        If Not WriterOpen Then
            Writer = New StreamWriter(LogFile, False, System.Text.Encoding.UTF8)
            WriterOpen = True
        End If

        'My.Computer.FileSystem.WriteAllText(LogFile, "", False)
    End Sub

    Private Shared Sub Log(text As String, Optional ByVal Header As Boolean = False)
        If Not WriterOpen Then
            Writer = New StreamWriter(LogFile, False, System.Text.Encoding.UTF8)
            WriterOpen = True
        End If
        Dim str As String = ""
        If Header = False Then
            str = String.Format("   {0}", text)
        Else
            str = String.Format("## {0} ##", text)
        End If
        Writer.WriteLine(str)
        'My.Computer.FileSystem.WriteAllText(LogFile, str, True)
    End Sub

    Public Shared Function ConvertFieldsToSLDFields(Style As LayerStyle) As LayerStyle
        For Each Rule In Style.Rules
            If Not Rule.Fill.BackgroundColor = "" Then
                Rule.Fill.BackgroundColor = "#" & Rule.Fill.BackgroundColor.Substring(2, Rule.Fill.BackgroundColor.Length - 2).ToUpper
            End If
            If Not Rule.Fill.ForegroundColor = "" Then
                Rule.Fill.ForegroundColor = "#" & Rule.Fill.ForegroundColor.Substring(2, Rule.Fill.ForegroundColor.Length - 2).ToUpper
            End If

            If Not Rule.Label.SizeY = "" Then
                Rule.Label.SizeY = ConvertNumberByUnitToPixals(Rule.Label.Unit, Rule.Label.SizeY, True)
            End If

            Rule.Label.Text = Rule.Label.Text.Replace("""", "").Replace("'", "")

            If Not Rule.Label.ForegroundColor = "" Then
                Rule.Label.ForegroundColor = "#" & Rule.Label.ForegroundColor.Substring(2, Rule.Label.ForegroundColor.Length - 2).ToUpper
            End If

            If Not Rule.Label.BackgroundColor = "" Then
                Rule.Label.BackgroundColor = "#" & Rule.Label.BackgroundColor.Substring(2, Rule.Label.BackgroundColor.Length - 2).ToUpper
            End If

            Rule.Label.SizeY = ConvertNumberByUnitToPixals(Rule.Label.Unit, Rule.Label.SizeY)

            For Each item In Rule.Stroke
                If Not item.Color = "" Then
                    item.Color = "#" & item.Color.Substring(2, item.Color.Length - 2)
                End If

                Dim Thickness As String = 0
                If Style.Type = LayerStyle.LayerStyleType.Line Then
                    Thickness = ConvertLineWidthByUnitToPixals(item.Unit, item.Thickness, True)
                Else
                    Thickness = ConvertNumberByUnitToPixals(item.Unit, item.Thickness, True)
                End If
                If Thickness = 0 Then
                    Thickness = "0.5"
                End If

                item.Thickness = Thickness
            Next
        Next




        Return Style
    End Function
    Public Shared Function RemoveÅÄÖFromLayerDefinition(layer As AIMS.LayerDefinition)
        layer.FeatureName = Tools.RemoveÅÄÖ(layer.FeatureName)

        For Each Scale In layer.VectorScaleRange
            For Each item In Scale.Style
                For Each Rule In item.Rules
                    Rule.LegendLabel = Tools.RemoveÅÄÖ(Rule.LegendLabel)
                Next
            Next
        Next
        Return layer
    End Function
    Public Shared Function ConvertLayerDefinitionFieldsToSLDFields(layer As AIMS.LayerDefinition)

        If RemoveÅÄÖ Then
            layer.FeatureName = Tools.RemoveÅÄÖ(layer.FeatureName)
        End If

        layer.FeatureName = layer.FeatureName.Replace("Default:", "")

        For Each Scale In layer.VectorScaleRange
            For Each item In Scale.Style
                item = ConvertFieldsToSLDFields(item)

                For Each Rule In item.Rules
                    If RemoveÅÄÖ Then
                        Rule.LegendLabel = Tools.RemoveÅÄÖ(Rule.LegendLabel)
                    End If
                Next
            Next
        Next
        Return layer
    End Function

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
        If String.IsNullOrEmpty(Number) Then Return "0"
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

            Log("Stroke", True)
            Log("Count:" & Strokes.Count)
            For Each Stroke In Strokes
                Log(String.Format("{0} {1}", Stroke.Thickness, Stroke.Color))
            Next

            Try
                If SelectMin = True Then
                    For Each Stroke In Strokes
                        If CDbl(MaxStroke.Thickness.Replace(".", ",")) > CDbl(Stroke.Thickness.Replace(".", ",")) Then
                            MaxStroke = Stroke
                        End If
                    Next
                Else
                    For Each Stroke In Strokes
                        If CDbl(MaxStroke.Thickness.Replace(".", ",")) < CDbl(Stroke.Thickness.Replace(".", ",")) Then
                            MaxStroke = Stroke
                        End If
                    Next
                End If
                Log("Selected Stroke", True)
                Log(String.Format("{0} {1}", MaxStroke.Thickness, MaxStroke.Color))

            Catch ex As Exception
                MaxStroke = Strokes(0)
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

                Log("Selected Stroke", True)
                Log(String.Format("{0} {1}", MaxStroke.Thickness, MaxStroke.Color))
            End Try

            If Not String.IsNullOrWhiteSpace(MaxStroke.Color) Then
                SLD.Append("<Stroke>" & vbCrLf)
                SLD.Append("<CssParameter name=""Stroke"">" & MaxStroke.Color & "</CssParameter>" & vbCrLf)
                Log("Stroke Color: " & MaxStroke.Color)
                'Dim Thickness As String = 0
                'If StyleType = AIMS.LayerStyle.LayerStyleType.Line Then
                '    Thickness = ConvertLineWidthByUnitToPixals(MaxStroke.Unit, MaxStroke.Thickness, True)
                'Else
                '    Thickness = ConvertNumberByUnitToPixals(MaxStroke.Unit, MaxStroke.Thickness, True)
                'End If

                'If MaxStroke.Thickness = 0 Then
                '    Thickness = "0.5"
                'End If
                'SLD.Append("<CssParameter name=""stroke-width"">" & Thickness & "</CssParameter>" & vbCrLf)
                SLD.Append("<CssParameter name=""stroke-width"">" & MaxStroke.Thickness & "</CssParameter>" & vbCrLf)
                Log("stroke width: " & MaxStroke.Thickness)

                If MaxStroke.LineStyle = "Dash" Then
                    SLD.Append("<CssParameter name=""stroke-dasharray"">5 2</CssParameter>" & vbCrLf)
                End If
                SLD.Append("</Stroke>" & vbCrLf)
            End If

        End If
    End Sub

    Public Shared Sub CreateSLD(layer As AIMS.LayerDefinition, Optional ByVal FieldsToSLDFields As Boolean = False)
        If FieldsToSLDFields Then
            layer = ConvertLayerDefinitionFieldsToSLDFields(layer)
        End If
        ResetLog()

        Log("Start", True)



        Dim SLD As New StringBuilder

        SLD.Append("<?xml version=""1.0"" encoding=""ISO-8859-1""?>" & vbCrLf)
        SLD.Append("<StyledLayerDescriptor version=""1.0.0"" xsi:schemaLocation=""http://www.opengis.net/sld http://schemas.opengis.net/sld/1.0.0/StyledLayerDescriptor.xsd"" xmlns=""://www.opengis.net/sld"" xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        SLD.Append(vbCrLf)
        SLD.Append("<NamedLayer>" & vbCrLf)

        Log("Name: " & layer.FeatureName)

        SLD.Append("<Name>" & layer.FeatureName & "</Name>" & vbCrLf)
        SLD.Append("<UserStyle>" & vbCrLf)
        SLD.Append("<Title>" & layer.FeatureName & "</Title>" & vbCrLf)
        SLD.Append("<FeatureTypeStyle>" & vbCrLf)


        For Each item In layer.VectorScaleRange

            For Each item2 In item.Style

                'For Each item3 In item2.Rules
                For Each item3 In item2.Rules
                    If item3.Fill.FillPattern = "" And item3.Stroke.Count = 0 And item3.Label.FontName = "" Then Continue For


                    SLD.Append("<Rule>" & vbCrLf)

                    Log("Rule", True)
                    Log("Title: " & item3.LegendLabel)

                    SLD.Append("<Title>" & item3.LegendLabel & "</Title>" & vbCrLf)
                    If Not item.MaxScale = -1 Then
                        SLD.Append("<MaxScaleDenominator>" & item.MaxScale & "</MaxScaleDenominator>" & vbCrLf)
                        Log("MaxScaleDenominator: " & item.MaxScale)
                    End If
                    If Not item.MinScale = -1 Then
                        SLD.Append("<MinScaleDenominator>" & item.MinScale & "</MinScaleDenominator>" & vbCrLf)
                        Log("MinScaleDenominator: " & item.MinScale)
                    End If

                    If Not String.IsNullOrWhiteSpace(item3.Filter) Then
                        Log("Convert Filter", True)
                        Log("Filter: " & item3.Filter)

                        SLD.Append("<ogc:Filter>" & vbCrLf)
                        'SLD.Append(GenerateFilter(item3.Filter))
                        SLD.Append(SLDFilter.GenerateFilter(item3.Filter))
                        SLD.Append("</ogc:Filter>" & vbCrLf)
                    End If

                    If item2.Type = LayerStyle.LayerStyleType.Area Then
                        Log("Style Type: Area", True)

                        SLD.Append("<PolygonSymbolizer>" & vbCrLf)

                        If Not String.IsNullOrWhiteSpace(item3.Fill.ForegroundColor) Then
                            Log("Fill: " & item3.Fill.ForegroundColor)

                            SLD.Append("<Fill>" & vbCrLf)
                            SLD.Append("<CssParameter name=""fill"">" & item3.Fill.ForegroundColor & "</CssParameter>" & vbCrLf)
                            SLD.Append("</Fill>" & vbCrLf)
                        End If

                        RenderStroke(SLD, item3.Stroke, LayerStyle.LayerStyleType.Area)

                        SLD.Append("</PolygonSymbolizer>" & vbCrLf)
                    ElseIf item2.Type = LayerStyle.LayerStyleType.Line Then
                        Log("Style Type: Line", True)

                        If Not item3.Fill.FillPattern = "" Or Not item3.Stroke.Count = 0 Then
                            SLD.Append("<LineSymbolizer>" & vbCrLf)

                            RenderStroke(SLD, item3.Stroke, LayerStyle.LayerStyleType.Line)

                            SLD.Append("</LineSymbolizer>" & vbCrLf)
                        End If
                    ElseIf item2.Type = LayerStyle.LayerStyleType.Point Then
                        Log("Style Type: Point", True)
                        If Not String.IsNullOrWhiteSpace(item3.Label.Text) Then
                            Log("Label", True)
                            Log("Text: " & item3.Label.Text)

                            SLD.Append("<TextSymbolizer>" & vbCrLf)
                            SLD.Append("<Label>" & vbCrLf)
                            SLD.Append("<ogc:PropertyName>" & item3.Label.Text & "</ogc:PropertyName>" & vbCrLf)
                            SLD.Append("</Label>" & vbCrLf)


                            If Not String.IsNullOrEmpty(item3.Label.Rotation) Then

                                SLD.Append("<LabelPlacement>" & vbCrLf)
                                SLD.Append("<PointPlacement>" & vbCrLf)

                                SLD.Append("<AnchorPoint>" & vbCrLf)
                                SLD.Append("<AnchorPointX>0</AnchorPointX>" & vbCrLf)
                                SLD.Append("<AnchorPointY>0</AnchorPointY>" & vbCrLf)
                                SLD.Append("</AnchorPoint>" & vbCrLf)

                                SLD.Append("<Displacement>" & vbCrLf)
                                SLD.Append("<DisplacementX>0</DisplacementX>" & vbCrLf)
                                SLD.Append("<DisplacementY>0</DisplacementY>" & vbCrLf)
                                SLD.Append("</Displacement>" & vbCrLf)

                                SLD.Append("<Rotation>" & vbCrLf)
                                'SLD.Append("-40" & vbCrLf)

                                Dim d As Double
                                If Double.TryParse(item3.Label.Rotation, d) Then
                                    SLD.Append(d & vbCrLf)
                                    Log("Rotation: " & d)
                                Else
                                    Log("Rotation: " & item3.Label.Rotation)

                                    SLD.Append("<Sub>" & vbCrLf)
                                    SLD.Append("<Literal>360</Literal>" & vbCrLf)
                                    SLD.Append("<PropertyName>" & item3.Label.Rotation & "</PropertyName>" & vbCrLf)
                                    SLD.Append("</Sub>" & vbCrLf)
                                End If
                                SLD.Append("</Rotation>" & vbCrLf)

                                SLD.Append("</PointPlacement>" & vbCrLf)
                                SLD.Append("</LabelPlacement>" & vbCrLf)
                            End If

                            Log("Font", True)

                            SLD.Append("<Font>" & vbCrLf)
                            SLD.Append("<CssParameter name=""font-family"">" & item3.Label.FontName & "</CssParameter>" & vbCrLf)
                            Log("font-family: " & item3.Label.FontName)

                            SLD.Append("<CssParameter name=""font-size"">" & item3.Label.SizeY.Replace(",", ".") & "</CssParameter>" & vbCrLf)
                            Log("font-size: " & item3.Label.SizeY.Replace(",", "."))

                            'SLD.Append("<CssParameter name=""font-size"">" & FontSize.ToString.Replace(",", ".") & "</CssParameter>" & vbCrLf)

                            If item3.Label.Italic = True Then
                                SLD.Append("<CssParameter name=""font-style"">italic</CssParameter>" & vbCrLf)
                                Log("font-style: italic")
                            Else
                                SLD.Append("<CssParameter name=""font-style"">normal</CssParameter>" & vbCrLf)
                                Log("font-style: normal")
                            End If

                            If item3.Label.Bold = True Then
                                SLD.Append("<CssParameter name=""font-weight"">bold</CssParameter>" & vbCrLf)
                                Log("font-weight: bold")
                            End If

                            SLD.Append("</Font>" & vbCrLf)

                            Log("Fill: " & item3.Label.ForegroundColor)

                            SLD.Append("<Fill>" & vbCrLf)
                            SLD.Append("<CssParameter name=""fill"">#" & item3.Label.ForegroundColor & "</CssParameter>" & vbCrLf)
                            SLD.Append("</Fill>" & vbCrLf)

                            If Not item3.Label.BackgroundStyle = "Ghosted" Then
                                Log("BackgroundColor: " & item3.Label.BackgroundColor)

                                SLD.Append("<Graphic>" & vbCrLf)
                                SLD.Append("<Mark>" & vbCrLf)
                                SLD.Append("<WellKnownName>square</WellKnownName>" & vbCrLf)
                                SLD.Append("<Fill>" & vbCrLf)
                                SLD.Append("<CssParameter name=""fill"">#" & item3.Label.BackgroundColor & "</CssParameter>" & vbCrLf)
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

                    Log("Rule", True)
                    Log("Title: " & item3.LegendLabel)

                    SLD.Append("<Title>" & item3.LegendLabel & "</Title>" & vbCrLf)
                    If Not item.MaxScale = -1 Then
                        SLD.Append("<MaxScaleDenominator>" & item.MaxScale & "</MaxScaleDenominator>" & vbCrLf)
                        Log("MaxScaleDenominator: " & item.MaxScale)
                    End If
                    If Not item.MinScale = -1 Then
                        SLD.Append("<MinScaleDenominator>" & item.MinScale & "</MinScaleDenominator>" & vbCrLf)
                        Log("MinScaleDenominator: " & item.MinScale)
                    End If

                    If Not String.IsNullOrWhiteSpace(item3.Filter) Then
                        Log("Convert Filter", True)
                        Log("Filter: " & item3.Filter)

                        SLD.Append("<ogc:Filter>" & vbCrLf)
                        SLD.Append(SLDFilter.GenerateFilter(item3.Filter))
                        SLD.Append("</ogc:Filter>" & vbCrLf)
                    End If

                    If item2.Type = LayerStyle.LayerStyleType.Line Then
                        Log("Style Type: Line", True)

                        SLD.Append("<LineSymbolizer>" & vbCrLf)
                        RenderStroke(SLD, item3.Stroke, LayerStyle.LayerStyleType.Line, True)
                        SLD.Append("</LineSymbolizer>" & vbCrLf)
                    End If

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

        Writer.Flush()
        Writer.Close()
        WriterOpen = False
        'My.Computer.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "\SLD.xml", SLD.ToString, False)
        'System.Diagnostics.Process.Start(My.Computer.FileSystem.SpecialDirectories.Temp & "\SLD.xml")
    End Sub
End Class
