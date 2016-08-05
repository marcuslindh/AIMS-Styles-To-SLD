﻿Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Xml

Public Class AIMS

    Private Property UserName As String = ""
    Private Property Password As String = ""
    Private Property Session As String = ""
    Private Property Server As String = ""

    Public ReadOnly Property Site As String
        Get
            Return Server
        End Get
    End Property

    Public Function HTTPGet(URL As String) As String
        Dim Request = WebRequest.Create(URL)
        Tools.SetBasicAuthHeader(Request, UserName, Password)
        Dim Response = Request.GetResponse
        Dim responseStream = Response.GetResponseStream()
        Dim webSteam = New StreamReader(responseStream, Encoding.UTF8)
        Dim text As String = webSteam.ReadToEnd
        webSteam.Close()
        Response.Close()
        Return text
    End Function

    Public Sub Login(User As String, Pass As String, _Server As String)
        UserName = User
        Password = Pass
        Server = _Server
    End Sub
    Public Function GetServerVersion() As String
        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=GETSITEVERSION&VERSION=1.0.0&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601")
        Dim Result As String = ""
        Dim nx As Boolean = False
        Using Xml As XmlReader = XmlReader.Create(New StringReader(text))
            While Xml.Read

                If Xml.NodeType = XmlNodeType.Text And nx = True Then
                    Result = Xml.Value
                End If

                If Xml.NodeType = XmlNodeType.Element And Xml.Name = "Version" Then
                    nx = True
                End If
            End While
            Xml.Dispose()
        End Using


        Return Result
    End Function
    Public Function GetServerInfo() As ServerInfo
        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=GETSITEINFO&VERSION=1.0.0&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601")
        Dim Result As New ServerInfo
        Dim xml = XDocument.Load(New StringReader(text))

        Dim SiteServer = xml.Descendants("SiteServer")(0)

        Result.DisplayName = SiteServer.Element("DisplayName").Value
        Result.Status = SiteServer.Element("Status").Value
        Result.Version = SiteServer.Element("Version").Value

        Dim OperatingSystem = SiteServer.Descendants("OperatingSystem")(0)
        Result.AvailablePhysicalMemory = Tools.StringToLong(OperatingSystem.Element("AvailablePhysicalMemory").Value)
        Result.TotalPhysicalMemory = Tools.StringToLong(OperatingSystem.Element("TotalPhysicalMemory").Value)
        Result.AvailableVirtualMemory = Tools.StringToLong(OperatingSystem.Element("AvailableVirtualMemory").Value)
        Result.TotalVirtualMemory = Tools.StringToLong(OperatingSystem.Element("TotalVirtualMemory").Value)
        Result.OperatingSystemVersion = OperatingSystem.Element("Version").Value

        Dim Statistics = xml.Descendants("Statistics")(0)
        Result.AdminOperationsQueueCount = Tools.IfElementToLong(Statistics.Element("AdminOperationsQueueCount"))
        Result.ClientOperationsQueueCount = Tools.IfElementToLong(Statistics.Element("ClientOperationsQueueCount"))
        Result.SiteOperationsQueueCount = Tools.IfElementToLong(Statistics.Element("SiteOperationsQueueCount"))
        Result.AverageOperationTime = Tools.IfElementToLong(Statistics.Element("AverageOperationTime"))
        Result.CpuUtilization = Tools.IfElementToLong(Statistics.Element("CpuUtilization"))
        Result.TotalOperationTime = Tools.IfElementToLong(Statistics.Element("TotalOperationTime"))
        Result.ActiveConnections = Tools.IfElementToLong(Statistics.Element("ActiveConnections"))
        Result.TotalConnections = Tools.IfElementToLong(Statistics.Element("TotalConnections"))
        Result.TotalOperationsProcessed = Tools.IfElementToLong(Statistics.Element("TotalOperationsProcessed"))
        Result.TotalOperationsReceived = Tools.IfElementToLong(Statistics.Element("TotalOperationsReceived"))
        Result.Uptime = Tools.IfElementToLong(Statistics.Element("Uptime"))

        Return Result
    End Function
    Public Function CreateSession() As String
        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=CREATESESSION&VERSION=1.0.0&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en ")
        Session = text
        Return text
    End Function
    Public Function GetUsers() As List(Of User)
        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=ENUMERATEUSERS&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en")
        Dim Result As New List(Of User)
        Dim xml = XDocument.Load(New StringReader(text))

        Dim node = xml.Descendants("UserList")
        Dim items = node.Descendants("User")

        For Each item In items
            'Result += item.Element("Name").Value

            Result.Add(New User With {
                       .Name = item.Element("Name").Value,
                       .FullName = item.Element("FullName").Value,
                       .Description = item.Element("Description").Value
                   })
        Next


        Return Result
    End Function
    Public Function GetResources(Optional ByVal ResourceId As String = "Library://") As List(Of ResourceItem)
        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=ENUMERATERESOURCES&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en&RESOURCEID=" & ResourceId & "&DEPTH=1&COMPUTECHILDREN=0")
        Dim xml = XDocument.Load(New StringReader(text))

        Dim Result As New List(Of ResourceItem)

        If xml.Descendants("ResourceFolder") IsNot Nothing Then
            Dim items = xml.Descendants("ResourceFolder")

            For Each item In items
                Dim NewItem As New ResourceItem
                NewItem.ResourceId = Tools.IfElementToString(item.Element("ResourceId"))
                NewItem.Depth = Tools.IfElementToInteger(item.Element("Depth"))
                NewItem.Owner = Tools.IfElementToString(item.Element("Owner"))
                NewItem.CreatedDate = Tools.IfElementToString(item.Element("CreatedDate"))
                NewItem.ModifiedDate = Tools.IfElementToString(item.Element("ModifiedDate"))
                NewItem.NumberOfFolders = Tools.IfElementToInteger(item.Element("NumberOfFolders"))
                NewItem.NumberOfDocuments = Tools.IfElementToInteger(item.Element("NumberOfDocuments"))

                If item.Descendants("Security") IsNot Nothing Then
                    If item.Descendants("Security")(0) IsNot Nothing Then
                        Dim Security = item.Descendants("Security")(0)
                        NewItem.Security.Inherited = Tools.IfElementToBoolean(Security.Element("Inherited"))

                        Dim Groups = Security.Descendants("Group")
                        For Each item2 In Groups
                            NewItem.Security.Groups.Add(New ResourceFolderSecurityGroup With {
                                                        .Name = Tools.IfElementToString(item2.Element("Name")),
                                                        .Permissions = Tools.IfElementToString(item2.Element("Permissions"))
                                                    })
                        Next
                    End If
                End If
                Result.Add(NewItem)
            Next
        End If

        If xml.Descendants("ResourceDocument") IsNot Nothing Then
            Dim ResourceDocuments = xml.Descendants("ResourceDocument")

            For Each item In ResourceDocuments
                Dim NewItem As New ResourceItem
                NewItem.ResourceId = Tools.IfElementToString(item.Element("ResourceId"))
                NewItem.Depth = Tools.IfElementToInteger(item.Element("Depth"))
                NewItem.Owner = Tools.IfElementToString(item.Element("Owner"))
                NewItem.CreatedDate = Tools.IfElementToString(item.Element("CreatedDate"))
                NewItem.ModifiedDate = Tools.IfElementToString(item.Element("ModifiedDate"))

                Dim ResourceDocumentHeader = item.Descendants("ResourceDocumentHeader")(0)

                If ResourceDocumentHeader.Descendants("General") IsNot Nothing Then
                    If ResourceDocumentHeader.Descendants("General")(0) IsNot Nothing Then
                        Dim General = ResourceDocumentHeader.Descendants("General")(0)
                        NewItem.ResourceDocumentIconName = Tools.IfElementToString(General.Element("IconName"))
                    End If
                End If

                If ResourceDocumentHeader.Descendants("Security") IsNot Nothing Then
                    If ResourceDocumentHeader.Descendants("Security")(0) IsNot Nothing Then
                        Dim Security = ResourceDocumentHeader.Descendants("Security")(0)
                        NewItem.Security.Inherited = Tools.IfElementToBoolean(Security.Element("Inherited"))

                        Dim Groups = Security.Descendants("Group")
                        For Each item2 In Groups
                            NewItem.Security.Groups.Add(New ResourceFolderSecurityGroup With {
                                                        .Name = Tools.IfElementToString(item2.Element("Name")),
                                                        .Permissions = Tools.IfElementToString(item2.Element("Permissions"))
                                                    })
                        Next
                    End If
                End If

                Result.Add(NewItem)
            Next
        End If

        Dim res = Result.OrderBy(Function(x) x.ResourceId).ToList

        Return res
    End Function
    Public Function GetMapDefinition(ResourceId As String) As MapDefinition
        Try
            Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=GETRESOURCECONTENT&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en&RESOURCEID=" & ResourceId)
            Dim xml = XDocument.Load(New StringReader(text))

            Dim Result As New MapDefinition

            Dim MapDef = xml.Descendants("MapDefinition")(0)

            Result.Name = Tools.IfElementToString(MapDef.Element("Name"))
            Result.CoordinateSystem = Tools.IfElementToString(MapDef.Element("CoordinateSystem"))

            Result.Extents.MinX = Tools.IfElementToString(MapDef.Element("MinX"))
            Result.Extents.MaxX = Tools.IfElementToString(MapDef.Element("MaxX"))
            Result.Extents.MinY = Tools.IfElementToString(MapDef.Element("MinY"))
            Result.Extents.MaxY = Tools.IfElementToString(MapDef.Element("MaxY"))

            Result.BackgroundColor = Tools.IfElementToString(MapDef.Element("BackgroundColor"))
            Result.Metadata = Tools.IfElementToString(MapDef.Element("Metadata"))

            Dim MapLayers = MapDef.Descendants("MapLayer")

            For Each item In MapLayers
                Try
                    Dim NewLayer As New MapLayer
                    NewLayer.Name = Tools.IfElementToString(item.Element("Name"))
                    NewLayer.ResourceId = Tools.IfElementToString(item.Element("ResourceId"))
                    NewLayer.Selectable = Tools.IfElementToBoolean(item.Element("Selectable"))
                    NewLayer.ShowInLegend = Tools.IfElementToBoolean(item.Element("ShowInLegend"))
                    NewLayer.LegendLabel = Tools.IfElementToString(item.Element("LegendLabel"))
                    NewLayer.ExpandInLegend = Tools.IfElementToBoolean(item.Element("ExpandInLegend"))
                    NewLayer.Visible = Tools.IfElementToBoolean(item.Element("Visible"))
                    NewLayer.Group = Tools.IfElementToString(item.Element("Group"))
                    Result.MapLayers.Add(NewLayer)
                Catch ex As Exception
                    Trace.WriteLine("ERROR (GetMapDefinition/MapLayers)>" & ex.Message)
                End Try
            Next
            Return Result
        Catch ex As Exception
            Trace.WriteLine("ERROR (GetMapDefinition)>" & ex.Message)
            Return New MapDefinition
        End Try
    End Function
    Public Function GetLayerDefinition(ResourceId As String) As LayerDefinition
        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=GETRESOURCECONTENT&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en&RESOURCEID=" & ResourceId)
        Dim xml = XDocument.Load(New StringReader(text))

        Dim Result As New LayerDefinition

        For Each VLD In xml.Descendants("VectorLayerDefinition")
            'Dim VLD = xml.Descendants("VectorLayerDefinition")(0)

            Result.ResourceId = Tools.IfElementToString(VLD.Element("ResourceId"))
            Result.FeatureName = Tools.IfElementToString(VLD.Element("FeatureName"))
            Result.FeatureNameType = Tools.IfElementToString(VLD.Element("FeatureNameType"))

            Dim PropertyMappings = VLD.Descendants("PropertyMapping")
            For Each item In PropertyMappings
                Result.PropertyMapping.Add(New PropertyMapping With {
                                           .Name = Tools.IfElementToString(item.Element("Name")),
                                           .Value = Tools.IfElementToString(item.Element("Value"))
                                       })
            Next


            Result.Geometry = Tools.IfElementToString(VLD.Element("Geometry"))
            Result.ToolTip = Tools.IfElementToString(VLD.Element("ToolTip"))

            Dim VectorScaleRange = VLD.Descendants("VectorScaleRange")

            For Each item In VectorScaleRange
                Dim ScaleRange As New VectorScaleRange
                ScaleRange.MaxScale = Tools.IfElementToInteger(item.Element("MaxScale"))
                ScaleRange.MinScale = Tools.IfElementToInteger(item.Element("MinScale"))

                Dim AreaTypeStyle = item.Descendants("AreaTypeStyle")
                For Each AreaTypeStyleItem In AreaTypeStyle
                    Dim Style As New LayerStyle
                    Style.Type = LayerStyle.LayerStyleType.Area

                    Dim AreaRule = AreaTypeStyleItem.Descendants("AreaRule")
                    For Each AreaRuleItem In AreaRule
                        Dim Rule As New LayerStyleRule

                        Rule.LegendLabel = Tools.IfElementToString(AreaRuleItem.Element("LegendLabel"))
                        Rule.Filter = Tools.IfElementToString(AreaRuleItem.Element("Filter"))

                        Dim AreaSymbolization2D = AreaRuleItem.Descendants("AreaSymbolization2D")
                        For Each AreaSymbolization2DItem In AreaSymbolization2D
                            Dim Fill = AreaSymbolization2DItem.Descendants("Fill")
                            For Each FillItem In Fill
                                Rule.Fill.FillPattern = Tools.IfElementToString(FillItem.Element("FillPattern"))
                                Rule.Fill.ForegroundColor = Tools.IfElementToString(FillItem.Element("ForegroundColor"))
                                Rule.Fill.BackgroundColor = Tools.IfElementToString(FillItem.Element("BackgroundColor"))
                            Next

                            Dim Stroke = AreaSymbolization2DItem.Descendants("Stroke")
                            For Each StrokeItem In Stroke
                                Rule.Stroke.Add(New StyleRuleStroke With {
                                                                            .LineStyle = Tools.IfElementToString(StrokeItem.Element("LineStyle")),
                                                                            .Thickness = Tools.IfElementToString(StrokeItem.Element("Thickness")),
                                                                            .Color = Tools.IfElementToString(StrokeItem.Element("Color")),
                                                                            .Unit = Tools.IfElementToString(StrokeItem.Element("Unit")),
                                                                            .SizeContext = Tools.IfElementToString(StrokeItem.Element("SizeContext"))
                                                                        })
                            Next
                        Next


                        Style.Rules.Add(Rule)
                    Next

                    ScaleRange.Style.Add(Style)
                Next

                Dim LineTypeStyle = item.Descendants("LineTypeStyle")
                For Each LineTypeStyleItem In LineTypeStyle
                    Dim Style As New LayerStyle
                    Style.Type = LayerStyle.LayerStyleType.Line
                    Dim LineRule = LineTypeStyleItem.Descendants("LineRule")
                    For Each LineRuleItem In LineRule
                        Dim Rule As New LayerStyleRule

                        Rule.LegendLabel = Tools.IfElementToString(LineRuleItem.Element("LegendLabel"))
                        Rule.Filter = Tools.IfElementToString(LineRuleItem.Element("Filter"))

                        Dim LineSymbolization2D = LineRuleItem.Descendants("LineSymbolization2D")
                        For Each LineSymbolization2DItem In LineSymbolization2D
                            Rule.Stroke.Add(New StyleRuleStroke With {
                                            .LineStyle = Tools.IfElementToString(LineSymbolization2DItem.Element("LineStyle")),
                                            .Thickness = Tools.IfElementToString(LineSymbolization2DItem.Element("Thickness")),
                                            .Color = Tools.IfElementToString(LineSymbolization2DItem.Element("Color")),
                                            .Unit = Tools.IfElementToString(LineSymbolization2DItem.Element("Unit")),
                                            .SizeContext = Tools.IfElementToString(LineSymbolization2DItem.Element("SizeContext"))
                                        })
                        Next
                        Style.Rules.Add(Rule)
                    Next


                    ScaleRange.Style.Add(Style)
                Next

                Dim PointTypeStyle = item.Descendants("PointTypeStyle")
                For Each PointTypeStyleItem In PointTypeStyle
                    Dim Style As New LayerStyle
                    Style.Type = LayerStyle.LayerStyleType.Point
                    Dim PointRule = PointTypeStyleItem.Descendants("PointRule")
                    For Each PointRuleItem In PointRule
                        Dim Rule As New LayerStyleRule
                        Rule.LegendLabel = Tools.IfElementToString(PointRuleItem.Element("LegendLabel"))
                        Rule.Filter = Tools.IfElementToString(PointRuleItem.Element("Filter"))

                        Dim Label = PointRuleItem.Descendants("Label")
                        For Each LabelItem In Label
                            Rule.Label.Unit = Tools.IfElementToString(LabelItem.Element("Unit"))
                            Rule.Label.SizeContext = Tools.IfElementToString(LabelItem.Element("SizeContext"))
                            Rule.Label.SizeX = Tools.IfElementToString(LabelItem.Element("SizeX"))
                            Rule.Label.SizeY = Tools.IfElementToString(LabelItem.Element("SizeY"))
                            Rule.Label.Rotation = Tools.IfElementToString(LabelItem.Element("Rotation"))
                            Rule.Label.Text = Tools.IfElementToString(LabelItem.Element("Text"))
                            Rule.Label.FontName = Tools.IfElementToString(LabelItem.Element("FontName"))
                            Rule.Label.ForegroundColor = Tools.IfElementToString(LabelItem.Element("ForegroundColor"))
                            Rule.Label.BackgroundStyle = Tools.IfElementToString(LabelItem.Element("BackgroundStyle"))
                            Rule.Label.BackgroundColor = Tools.IfElementToString(LabelItem.Element("BackgroundColor"))
                            Rule.Label.HorizontalAlignment = Tools.IfElementToString(LabelItem.Element("HorizontalAlignment"))
                            Rule.Label.Italic = Tools.IfElementToBoolean(LabelItem.Element("Italic"))
                            Rule.Label.Bold = Tools.IfElementToBoolean(LabelItem.Element("Bold"))
                            Rule.Label.Underlined = Tools.IfElementToBoolean(LabelItem.Element("Underlined"))

                            Dim AdvancedPlacement = LabelItem.Descendants("AdvancedPlacement")
                            For Each AdvancedPlacementItem In AdvancedPlacement
                                Rule.Label.AdvancedPlacement.ScaleLimit = Tools.IfElementToString(AdvancedPlacementItem.Element("ScaleLimit"))
                            Next
                        Next


                        Style.Rules.Add(Rule)
                    Next
                    ScaleRange.Style.Add(Style)
                Next


                Result.VectorScaleRange.Add(ScaleRange)
            Next

        Next

        Return Result
    End Function
    Public Function GetFeatureSourceInfo(ResourceId As String) As List(Of ResourceData)

        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=ENUMERATERESOURCEDATA&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en&RESOURCEID=" & ResourceId)
        Dim xml = XDocument.Load(New StringReader(text))
        Dim Result As New List(Of ResourceData)
        Dim ResourceData = xml.Descendants("ResourceData")

        For Each item In ResourceData
            Dim n As New ResourceData
            n.Name = Tools.IfElementToString(item.Element("Name"))
            n.Type = Tools.IfElementToString(item.Element("Type"))
            Result.Add(n)
        Next
        Return Result
    End Function
    Public Function GetClasses(ResourceId As String) As List(Of String)

        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=GETCLASSES&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en&RESOURCEID=" & ResourceId)
        Dim xml = XDocument.Load(New StringReader(text))

        Dim Result As New List(Of String)

        Dim Items = xml.Descendants("Item")
        For Each item In Items
            Result.Add(item.Value)
        Next


        Return Result
    End Function

    Public Function GetFeatureSchema(ResourceId As String, Schema As String, ClassName As String) As FeatureSchema

        Dim text As String = HTTPGet(Server & "/mapagent/mapagent.fcgi?OPERATION=DESCRIBEFEATURESCHEMA&VERSION=1.0.0&SESSION=" & Session & "&CLIENTAGENT=Autodesk+MapGuide+Studio+v2.6.1.9601&LOCALE=en&RESOURCEID=" & ResourceId & "&SCHEMA=" & Schema & "&CLASSNAMES=" & ClassName)
        Dim xml = XDocument.Load(New StringReader(text))
        Dim Result As New FeatureSchema
        Dim doc = xml.Descendants()

        Dim Prefix As String = doc(0).GetNamespaceOfPrefix("xs").NamespaceName

        Dim xs = doc.Descendants("{" & Prefix & "}element")

        Result.Name = xs.Attributes("name")(0).Value
        Result.Type = xs.Attributes("type")(0).Value

        If xs.Attributes("abstract")(0).Value = "true" Then
            Result.abstract = True
        Else
            Result.abstract = False
        End If

        Result.substitutionGroup = xs.Attributes("substitutionGroup")(0).Value

        Return Result
    End Function


End Class


Public Class User
    Public Property Name As String = ""
    Public Property FullName As String = ""
    Public Property Description As String = ""
End Class

Public Class ServerInfo
    Public Property DisplayName As String = ""
    Public Property Status As String = ""
    Public Property Version As String = ""
    Public Property AvailablePhysicalMemory As Long = 0
    Public Property TotalPhysicalMemory As Long = 0
    Public Property AvailableVirtualMemory As Long = 0
    Public Property TotalVirtualMemory As Long = 0
    Public Property OperatingSystemVersion As String = ""

    Public Property AdminOperationsQueueCount As Long = 0
    Public Property ClientOperationsQueueCount As Long = 0
    Public Property SiteOperationsQueueCount As Long = 0
    Public Property AverageOperationTime As Long = 0
    Public Property CpuUtilization As Long = 0
    Public Property TotalOperationTime As Long = 0
    Public Property ActiveConnections As Long = 0
    Public Property TotalConnections As Long = 0
    Public Property TotalOperationsProcessed As Long = 0
    Public Property TotalOperationsReceived As Long = 0
    Public Property Uptime As Long = 0

End Class

Public Class ResourceItem
    Public Property ResourceId As String = ""
    Public Property Depth As Integer = 1
    Public Property Owner As String = ""
    Public Property CreatedDate As String = ""
    Public Property ModifiedDate As String = ""
    Public Property NumberOfFolders As Integer = 0
    Public Property NumberOfDocuments As Integer = 0
    Public Property ResourceDocumentIconName As String = ""
    Public Property Security As New ResourceFolderSecurity

End Class

Public Class ResourceFolderSecurity
    Public Property Inherited As Boolean = False
    Public Property Groups As New List(Of ResourceFolderSecurityGroup)
End Class

Public Class ResourceFolderSecurityGroup
    Public Property Name As String = ""
    Public Property Permissions As String = ""
End Class

Public Class MapDefinition
    Public Property Name As String = ""
    Public Property CoordinateSystem As String = ""
    Public Property Extents As New MapDefinitionExtents
    Public Property BackgroundColor As String = ""
    Public Property Metadata As String = ""
    Public Property MapLayers As New List(Of MapLayer)
End Class

Public Class MapDefinitionExtents
    Public Property MinX As String = ""
    Public Property MaxX As String = ""
    Public Property MinY As String = ""
    Public Property MaxY As String = ""
End Class

Public Class MapLayer
    Public Property Name As String = ""
    Public Property ResourceId As String = ""
    Public Property Selectable As Boolean = False
    Public Property ShowInLegend As Boolean = False
    Public Property LegendLabel As String = ""
    Public Property ExpandInLegend As Boolean = False
    Public Property Visible As Boolean = False
    Public Property Group As String = ""
End Class

Public Class LayerDefinition
    Public Property ResourceId As String = ""
    Public Property FeatureName As String = ""
    Public Property FeatureNameType As String = ""
    Public Property PropertyMapping As New List(Of PropertyMapping)
    Public Property Geometry As String = ""
    Public Property ToolTip As String = ""

    Public Property VectorScaleRange As New List(Of VectorScaleRange)

End Class

Public Class PropertyMapping
    Public Property Name As String = ""
    Public Property Value As String = ""
End Class

Public Class ResourceData
    Public Property Name As String = ""
    Public Property Type As String = ""
End Class

Public Class FeatureSchema
    Public Property Name As String = ""
    Public Property Type As String = ""
    Public Property abstract As Boolean = False
    Public Property substitutionGroup As String = ""
End Class

Public Class VectorScaleRange
    Public Property MaxScale As Integer = -1
    Public Property MinScale As Integer = -1
    Public Property Style As New List(Of LayerStyle)

End Class

Public Class LayerStyle
    Public Enum LayerStyleType
        Area = 1
        Line = 2
        Point = 3
    End Enum

    Public Property Type As LayerStyleType
    Public Property Rules As New List(Of LayerStyleRule)



End Class

Public Class LayerStyleRule
    Public Property LegendLabel As String = ""
    Public Property Filter As String = ""
    Public Property Fill As New StyleRuleFill
    Public Property Stroke As New List(Of StyleRuleStroke)
    Public Property Label As New StyleRuleLabel
End Class

Public Class StyleRuleFill
    Public Property FillPattern As String = ""
    Public Property ForegroundColor As String = ""
    Public Property BackgroundColor As String = ""

End Class

Public Class StyleRuleStroke
    Public Property LineStyle As String = ""
    Public Property Thickness As String = ""
    Public Property Color As String = ""
    Public Property Unit As String = ""
    Public Property SizeContext As String = ""
End Class

Public Class StyleRuleLabel
    Public Property Unit As String = ""
    Public Property SizeContext As String = ""
    Public Property SizeX As String = ""
    Public Property SizeY As String = ""
    Public Property Rotation As String = ""
    Public Property Text As String = ""
    Public Property FontName As String = ""
    Public Property ForegroundColor As String = ""
    Public Property BackgroundColor As String = ""
    Public Property BackgroundStyle As String = ""
    Public Property HorizontalAlignment As String = ""
    Public Property Italic As Boolean = False
    Public Property Bold As Boolean = False
    Public Property Underlined As Boolean = False
    Public Property AdvancedPlacement As New StyleRuleLabelAdvancedPlacement
End Class

Public Class StyleRuleLabelAdvancedPlacement
    Public Property ScaleLimit As String = ""
End Class