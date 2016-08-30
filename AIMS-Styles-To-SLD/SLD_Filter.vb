Imports System.Text

Public Class SLDFilter
    'Version 2 of SLD Filter
    Public Sub New()

    End Sub
    Public Shared Function SplitByStrings(str As String) As ArrayList
        Dim Result As New ArrayList
        Dim Pos As String = ""
        Dim word As String = ""
        For i As Integer = 0 To str.Length - 1
            Pos = str.Substring(i, 1)

            If Pos = "'" Or Pos = """" Then
                If Not word = "" Then
                    If word.Substring(0, 1) = "'" Or word.Substring(0, 1) = """" Then
                        word += Pos
                        Result.Add(New SyntaxPartV2 With {.text = word, .Type = SyntaxPartV2.SyntaxPartType.String})
                        word = ""
                    Else
                        Result.Add(New SyntaxPartV2 With {.text = word, .Type = SyntaxPartV2.SyntaxPartType.NotSpecified})
                        word = "" & Pos
                    End If
                End If
            Else
                word += Pos
            End If
        Next
        If word.Length > 0 Then
            Result.Add(New SyntaxPartV2 With {.text = word, .Type = SyntaxPartV2.SyntaxPartType.NotSpecified})
        End If

        Return Result
    End Function
    Private Shared Function SplitBySymbols_Operators(word As String)
        Dim Result As New ArrayList
        Dim Operators() As String = New String() {"IN", "=", ">=", "<=", "(", ")", ",", "<", ">"}
        Dim OperatorExist As Boolean = False
        For Each oper In Operators
            If word = oper Then
                Result.Add(New SyntaxPartV2 With {.text = word, .Type = SyntaxPartV2.SyntaxPartType.Operator})
                OperatorExist = True
                word = ""
                Exit For
            End If
        Next
        If OperatorExist = False Then
            Dim Operators2() As String = New String() {">=", "<=", "(", ")", ",", "=", "<", ">"}
            Dim Exist As Boolean = False
            For Each item In Operators2
                Dim pos As Integer = word.IndexOf(item)
                If pos > -1 Then
                    'Dim s() As String = word.Split(item)
                    Dim part1 As String = ""
                    Dim part2 As String = ""

                    If pos = 0 Then
                        part2 = word.Substring(item.Length, word.Length - (item.Length))
                    Else
                        part1 = word.Substring(0, pos)
                        part2 = word.Substring(pos + item.Length, word.Length - 1)
                    End If


                    If Not String.IsNullOrEmpty(part1) Then
                        Result.Add(New SyntaxPartV2 With {.text = part1, .Type = SyntaxPartV2.SyntaxPartType.NotSpecified})
                    End If
                    Result.Add(New SyntaxPartV2 With {.text = item, .Type = SyntaxPartV2.SyntaxPartType.Operator})
                    If Not String.IsNullOrEmpty(part2) Then
                        Result.Add(New SyntaxPartV2 With {.text = part2, .Type = SyntaxPartV2.SyntaxPartType.NotSpecified})
                    End If


                    Exist = True
                    Exit For
                End If
            Next
            If Exist = False Then
                Result.Add(New SyntaxPartV2 With {.text = word, .Type = SyntaxPartV2.SyntaxPartType.NotSpecified})
                word = ""
            End If
        End If
        Return Result
    End Function
    Public Shared Function SplitBySymbols(input As ArrayList) As ArrayList
        Dim ReservedNames() As String = New String() {"NOT", "AND", "OR"}
        Dim Result As New ArrayList
        For inputIndex As Integer = 0 To input.Count - 1
            Dim item As SyntaxPartV2 = input(inputIndex)

            If item.Type = SyntaxPartV2.SyntaxPartType.NotSpecified Then
                Dim word As String = ""
                Dim pos As String = ""
                For i As Integer = 0 To item.text.Length - 1
                    pos = item.text.Substring(i, 1)

                    If pos = " " Then
                        Dim ReservedExist As Boolean = False
                        For Each Reserved In ReservedNames
                            If word.ToUpper = Reserved Then
                                Result.Add(New SyntaxPartV2 With {.text = word, .Type = SyntaxPartV2.SyntaxPartType.Logic})
                                word = ""
                                ReservedExist = True
                                Exit For
                            End If
                        Next
                        If ReservedExist = False Then
                            If word.Length > 0 Then
                                Result.AddRange(SplitBySymbols_Operators(word))
                                word = ""
                            End If
                        End If
                    Else
                        word += pos
                    End If

                Next
                If word.Length > 0 Then
                    Result.AddRange(SplitBySymbols_Operators(word))
                End If
            Else
                Result.Add(item)
            End If
        Next

        For ResultIndex As Integer = 0 To Result.Count - 1
            If Not ResultIndex + 1 > Result.Count - 1 Then
                Dim item As SyntaxPartV2 = Result(ResultIndex)
                Dim item2 As SyntaxPartV2 = Result(ResultIndex + 1)
                If item.Type = SyntaxPartV2.SyntaxPartType.NotSpecified Then
                    If item2.Type = SyntaxPartV2.SyntaxPartType.Operator Then
                        item.Type = SyntaxPartV2.SyntaxPartType.Variable
                    End If
                End If
            End If
        Next

        For ResultIndex As Integer = 0 To Result.Count - 1
            Dim item As SyntaxPartV2 = Result(ResultIndex)
            If item.Type = SyntaxPartV2.SyntaxPartType.NotSpecified Then
                Dim IntNumber As Integer = 0
                If Integer.TryParse(item.text, IntNumber) Then
                    item.Type = SyntaxPartV2.SyntaxPartType.Number
                End If
                Dim DoubleNumber As Double = 0.0
                If Double.TryParse(item.text, DoubleNumber) Then
                    item.Type = SyntaxPartV2.SyntaxPartType.Number
                End If
            End If
        Next


        Return Result
    End Function
    Public Shared Function CreateTreeBySymbols(input As ArrayList) As ArrayList
        'Dim Operators() As String = New String() {"IN", "=", ">=", "<=", "(", ")", ","}
        Dim Result As New ArrayList
        Dim jump As Integer = -1
        For i As Integer = 0 To input.Count - 1
            Dim item As SyntaxPartV2 = input(i)

            If item.Type = SyntaxPartV2.SyntaxPartType.Operator Then
                Dim NewSyntaxPartV2 = CType(input(i - 1), SyntaxPartV2)

                If item.text.ToUpper = "IN" Then
                    For y As Integer = i + 2 To input.Count - 1
                        If CType(input(y), SyntaxPartV2).text = ")" Then
                            jump = y
                            Exit For
                        Else
                            If Not CType(input(y), SyntaxPartV2).text = "," Then
                                NewSyntaxPartV2.Nodes.Add(input(y))
                            End If
                        End If
                    Next
                Else
                    Dim op = item
                    op.Nodes.Add(input(i + 1))
                    NewSyntaxPartV2.Nodes.Add(op)
                End If
                Result.Add(NewSyntaxPartV2)
                If Not jump = -1 Then
                    i = jump
                    jump = -1
                End If
            ElseIf item.Type = SyntaxPartV2.SyntaxPartType.Logic Then
                Result.Add(item)
            End If

        Next

        For i As Integer = 0 To Result.Count - 1
            If i <= Result.Count - 1 Then
                Dim item As SyntaxPartV2 = Result(i)

                If item.Type = SyntaxPartV2.SyntaxPartType.Logic Then

                    If item.text.ToUpper = "OR" Or item.text.ToUpper = "AND" Then

                        item.Nodes.Add(Result(i - 1))
                        item.Nodes.Add(Result(i + 1))

                        Result.Remove(Result(i + 1))
                        Result.Remove(Result(i - 1))
                        i -= 1
                    End If


                End If
            End If
        Next

        Dim NewResult As New ArrayList

        Dim Count As Integer = Result.Count - 1
        For i As Integer = 0 To Count
            'If Not i >= Count Then
            Dim item As SyntaxPartV2 = CType(Result(i), SyntaxPartV2)

            If item.Type = SyntaxPartV2.SyntaxPartType.Logic And item.text.ToUpper = "NOT" Then
                Dim item2 As SyntaxPartV2 = CType(Result(i + 1), SyntaxPartV2)


                Dim NewNodes As New ArrayList
                For Each node In item2.Nodes
                    Dim NotItem As New SyntaxPartV2 With {
                           .Type = SyntaxPartV2.SyntaxPartType.Logic,
                           .text = "NOT"}

                    NotItem.Nodes.Add(node)

                    NewNodes.Add(NotItem)
                Next

                Dim newNode As New SyntaxPartV2 With {.text = item2.text, .Type = item2.Type}
                newNode.Nodes = NewNodes
                NewResult.Add(newNode)
                i += 1
            Else
                NewResult.Add(item)
            End If

            'If item.Type = SyntaxPartV2.SyntaxPartType.Logic And item.text.ToUpper = "NOT" Then
            '    Dim node As SyntaxPartV2 = Result(i + 1)

            '    If node.Type = SyntaxPartV2.SyntaxPartType.Logic Then
            '        Dim newNodes As New ArrayList
            '        For Each nodeitem In node.Nodes
            '            Dim NotItem As New SyntaxPartV2 With {
            '                .Type = SyntaxPartV2.SyntaxPartType.Logic,
            '                .text = "NOT"}

            '            NotItem.Nodes.Add(nodeitem)
            '            newNodes.Add(NotItem)
            '            'nodeitem = NotItem
            '        Next
            '        CType(Result(i + 1), SyntaxPartV2).Nodes.Clear()
            '        CType(Result(i + 1), SyntaxPartV2).Nodes.AddRange(newNodes)
            '    End If

            'item.Nodes.Add(Result(i + 1))
            'Result.Remove(Result(i))
            'Count -= 1
            'i += 1
            'End If

            'End If


        Next

        'My.Computer.FileSystem.WriteAllText("C:\Temp\SLD_Filter.json", Newtonsoft.Json.JsonConvert.SerializeObject(NewResult, Newtonsoft.Json.Formatting.Indented) & vbCrLf, True)



        Return NewResult
    End Function
    Private Shared Sub GenerateSLDFilter_Variable(item As SyntaxPartV2, ByRef SLD As StringBuilder)
        If item.Nodes.Count > 1 Then
            SLD.Append("<OR>" & vbCrLf)
            For Each INitem As SyntaxPartV2 In item.Nodes
                If INitem.Type = SyntaxPartV2.SyntaxPartType.Logic Then
                    SLD.Append("<NOT>" & vbCrLf)
                    SLD.Append("<PropertyIsEqualTo>" & vbCrLf)
                    SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                    SLD.Append("<Literal>" & INitem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                    SLD.Append("</PropertyIsEqualTo>" & vbCrLf)
                    SLD.Append("</NOT>" & vbCrLf)
                Else
                    SLD.Append("<PropertyIsEqualTo>" & vbCrLf)
                    SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                    SLD.Append("<Literal>" & INitem.text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                    SLD.Append("</PropertyIsEqualTo>" & vbCrLf)
                End If
            Next
            SLD.Append("</OR>" & vbCrLf)
        Else
            Dim NodeItem As SyntaxPartV2 = item.Nodes(0)
            If NodeItem.text = "=" Then
                SLD.Append("<PropertyIsEqualTo>" & vbCrLf)
                SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                SLD.Append("</PropertyIsEqualTo>" & vbCrLf)
            ElseIf NodeItem.text = ">" Then
                SLD.Append("<PropertyIsGreaterThan>" & vbCrLf)
                SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                SLD.Append("</PropertyIsGreaterThan>" & vbCrLf)
            ElseIf NodeItem.text = "<" Then
                SLD.Append("<PropertyIsLessThan>" & vbCrLf)
                SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                SLD.Append("</PropertyIsLessThan>" & vbCrLf)
            ElseIf NodeItem.text = ">=" Then
                SLD.Append("<PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                SLD.Append("</PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
            ElseIf NodeItem.text = "<=" Then
                SLD.Append("<PropertyIsLessThanOrEqualTo>" & vbCrLf)
                SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                SLD.Append("</PropertyIsLessThanOrEqualTo>" & vbCrLf)
            End If
        End If
    End Sub
    Public Shared Function GenerateSLDFilter(SyntaxTree As ArrayList) As String
        Dim SLD As New StringBuilder

        For i As Integer = 0 To SyntaxTree.Count - 1
            Dim item As SyntaxPartV2 = SyntaxTree(i)

            If item.Type = SyntaxPartV2.SyntaxPartType.Logic Then
                Select Case item.text.ToUpper
                    Case "NOT"
                        SLD.Append("<NOT>" & vbCrLf)
                        SLD.Append(GenerateSLDFilter(item.Nodes))
                        SLD.Append("</NOT>" & vbCrLf)
                    Case "AND"
                        SLD.Append("<AND>" & vbCrLf)
                        For Each NodeItem As SyntaxPartV2 In item.Nodes
                            If NodeItem.Type = SyntaxPartV2.SyntaxPartType.Logic And NodeItem.text = "AND" Then
                                SLD.Append(GenerateSLDFilter(NodeItem.Nodes))
                            ElseIf NodeItem.Type = SyntaxPartV2.SyntaxPartType.Variable Then
                                GenerateSLDFilter_Variable(NodeItem, SLD)
                            Else

                                SLD.Append(GenerateSLDFilter(NodeItem.Nodes))
                            End If
                        Next
                        SLD.Append("</AND>" & vbCrLf)
                    Case "OR"
                        SLD.Append("<OR>" & vbCrLf)
                        For Each NodeItem As SyntaxPartV2 In item.Nodes
                            If NodeItem.Type = SyntaxPartV2.SyntaxPartType.Logic And NodeItem.text = "OR" Then
                                SLD.Append(GenerateSLDFilter(NodeItem.Nodes))
                            ElseIf NodeItem.Type = SyntaxPartV2.SyntaxPartType.Variable Then
                                GenerateSLDFilter_Variable(NodeItem, SLD)
                            Else
                                SLD.Append(GenerateSLDFilter(NodeItem.Nodes))
                            End If
                        Next
                        SLD.Append("</OR>" & vbCrLf)
                End Select
            ElseIf item.Type = SyntaxPartV2.SyntaxPartType.Variable Then
                GenerateSLDFilter_Variable(item, SLD)

                'If item.Nodes.Count > 1 Then
                '    SLD.Append("<OR>" & vbCrLf)
                '    For Each INitem As SyntaxPartV2 In item.Nodes
                '        SLD.Append("<PropertyIsEqualTo>" & vbCrLf)
                '        SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                '        SLD.Append("<Literal>" & INitem.text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                '        SLD.Append("</PropertyIsEqualTo>" & vbCrLf)
                '    Next
                '    SLD.Append("</OR>" & vbCrLf)
                'Else
                '    Dim NodeItem As SyntaxPartV2 = item.Nodes(0)
                '    If NodeItem.text = "=" Then
                '        SLD.Append("<PropertyIsEqualTo>" & vbCrLf)
                '        SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                '        SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                '        SLD.Append("</PropertyIsEqualTo>" & vbCrLf)
                '    ElseIf NodeItem.text = ">" Then
                '        SLD.Append("<PropertyIsGreaterThan>" & vbCrLf)
                '        SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                '        SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                '        SLD.Append("</PropertyIsGreaterThan>" & vbCrLf)
                '    ElseIf NodeItem.text = "<" Then
                '        SLD.Append("<PropertyIsLessThan>" & vbCrLf)
                '        SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                '        SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                '        SLD.Append("</PropertyIsLessThan>" & vbCrLf)
                '    ElseIf NodeItem.text = ">=" Then
                '        SLD.Append("<PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                '        SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                '        SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                '        SLD.Append("</PropertyIsGreaterThanOrEqualTo>" & vbCrLf)
                '    ElseIf NodeItem.text = "<=" Then
                '        SLD.Append("<PropertyIsLessThanOrEqualTo>" & vbCrLf)
                '        SLD.Append("<PropertyName>" & item.text.Replace("'", "").Replace("""", "") & "</PropertyName>" & vbCrLf)
                '        SLD.Append("<Literal>" & NodeItem.Nodes(0).text.Replace("'", "").Replace("""", "") & "</Literal>" & vbCrLf)
                '        SLD.Append("</PropertyIsLessThanOrEqualTo>" & vbCrLf)
                '    End If
                'End If

            End If

        Next




        Return SLD.ToString
    End Function
    Public Shared Function GenerateFilter(Filter As String) As String
        Return GenerateSLDFilter(CreateTreeBySymbols(SplitBySymbols(SplitByStrings(Filter))))
    End Function
End Class

Public Class SyntaxPartV2
    Public Enum SyntaxPartType
        [String] = 1
        [Number] = 2
        [Operator] = 3
        [Variable] = 4
        Logic = 5
        NotSpecified = 6
    End Enum
    Public Property Type As SyntaxPartType = SyntaxPartType.String
    Public Property text As String = ""
    'Public Property Nodes As New List(Of SyntaxPartV2)
    Public Property Nodes As New ArrayList
End Class
