using Auth0TokenGenerator;
using CefSharp.DevTools.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Auth0TokenGenerator.Pages
{
    /// <summary>
    /// Interaction logic for ObjectInTreeView.xaml
    /// </summary>
    public partial class ObjectInTreeView : Page
    {
        private bool Expanded = false;
        public ObjectInTreeView()
        {
            InitializeComponent();
        }

        public object ObjectToVisualize
        {
            get { return (object)GetValue(ObjectToVisualizeProperty); }
            set { SetValue(ObjectToVisualizeProperty, value); }
        }
        public static readonly DependencyProperty ObjectToVisualizeProperty = 
                    DependencyProperty.Register("ObjectToVisualize", 
                                                typeof(object), 
                                                typeof(ObjectInTreeView), 
                                                new PropertyMetadata(null, OnObjectChanged));

        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeNode tree = TreeNode.CreateTree(e.NewValue);
            (d as ObjectInTreeView).TreeNodes = new List<TreeNode>() { tree };
        }

        public List<TreeNode> TreeNodes
        {
            get { return (List<TreeNode>)GetValue(TreeNodesProperty); }
            set { SetValue(TreeNodesProperty, value); }
        }
        public static readonly DependencyProperty TreeNodesProperty =
            DependencyProperty.Register("TreeNodes", typeof(List<TreeNode>), typeof(ObjectInTreeView), new PropertyMetadata(null));

        private void TreeView_PreviewMouseDown(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void Navigate_Back(object sender, RoutedEventArgs e)
        {        
            this.Content = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var test1 = TreeNodesProperty;
            var test2 = TreeNodes.ToList();
            var test3 = ObjectInTreeViewControl;

            //ExpandAllNodes(test2 as TreeViewItem);
        }

        private void ExpandAllNodes(TreeViewItem rootItem)
        {
            foreach (object item in rootItem.Items)
            {
                TreeViewItem treeItem = (TreeViewItem)item;

                if (treeItem != null)
                {
                    ExpandAllNodes(treeItem);
                    treeItem.IsExpanded = true;
                }
            }
        }
    }
}
