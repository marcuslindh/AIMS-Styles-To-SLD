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

        SLD.CreateSLD(App.GetLayerDefinition(item.ResourceId))

        'CreateSLD(item.ResourceId)


    End Sub
#End Region

#Region "SLD"

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
        [IN] = 5
    End Enum
    Public Property Type As SyntaxPartType = SyntaxPartType.String
    Public Property text As String = ""
    Public Property Nodes As New List(Of SyntaxPart)
End Class
