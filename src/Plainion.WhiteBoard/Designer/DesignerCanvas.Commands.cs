using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Plainion.WhiteBoard.Model;
using Plainion.WhiteBoard.Serialization;
using Microsoft.Win32;

namespace Plainion.WhiteBoard.Designer
{
    public partial class DesignerCanvas
    {
        public static RoutedCommand Group = new RoutedCommand();
        public static RoutedCommand Ungroup = new RoutedCommand();
        public static RoutedCommand BringForward = new RoutedCommand();
        public static RoutedCommand BringToFront = new RoutedCommand();
        public static RoutedCommand SendBackward = new RoutedCommand();
        public static RoutedCommand SendToBack = new RoutedCommand();
        public static RoutedCommand AlignTop = new RoutedCommand();
        public static RoutedCommand AlignVerticalCenters = new RoutedCommand();
        public static RoutedCommand AlignBottom = new RoutedCommand();
        public static RoutedCommand AlignLeft = new RoutedCommand();
        public static RoutedCommand AlignHorizontalCenters = new RoutedCommand();
        public static RoutedCommand AlignRight = new RoutedCommand();
        public static RoutedCommand DistributeHorizontal = new RoutedCommand();
        public static RoutedCommand DistributeVertical = new RoutedCommand();
        public static RoutedCommand SelectAll = new RoutedCommand();
        public static RoutedCommand ConnectionStyleChanged = new RoutedCommand();

        public DesignerCanvas()
        {
            CommandBindings.Add( new CommandBinding( ApplicationCommands.New, New_Executed ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Open, Open_Executed ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Save, Save_Executed ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Print, Print_Executed ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Cut, Cut_Executed, Cut_Enabled ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Copy, Copy_Executed, Copy_Enabled ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Paste, Paste_Executed, Paste_Enabled ) );
            CommandBindings.Add( new CommandBinding( ApplicationCommands.Delete, Delete_Executed, Delete_Enabled ) );

            CommandBindings.Add( new CommandBinding( Commands.DiscardAndClose, OnDiscardAndClose ) );
            CommandBindings.Add( new CommandBinding( Commands.SaveAndClose, OnSaveAndClose ) );

            CommandBindings.Add( new CommandBinding( DesignerCanvas.Group, Group_Executed, Group_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.Ungroup, Ungroup_Executed, Ungroup_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.BringForward, BringForward_Executed, Order_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.BringToFront, BringToFront_Executed, Order_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.SendBackward, SendBackward_Executed, Order_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.SendToBack, SendToBack_Executed, Order_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.AlignTop, AlignTop_Executed, Align_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.AlignVerticalCenters, AlignVerticalCenters_Executed, Align_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.AlignBottom, AlignBottom_Executed, Align_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.AlignLeft, AlignLeft_Executed, Align_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.AlignHorizontalCenters, AlignHorizontalCenters_Executed, Align_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.AlignRight, AlignRight_Executed, Align_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.DistributeHorizontal, DistributeHorizontal_Executed, Distribute_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.DistributeVertical, DistributeVertical_Executed, Distribute_Enabled ) );
            CommandBindings.Add( new CommandBinding( DesignerCanvas.SelectAll, SelectAll_Executed ) );
            SelectAll.InputGestures.Add( new KeyGesture( Key.A, ModifierKeys.Control ) );

            CommandBindings.Add( new CommandBinding( DesignerCanvas.ConnectionStyleChanged, ConnectionStyleChanged_Executed ) );

            // Attention: needs to be in sync with combobox for now
            ArrowStyle = ArrowSymbol.Arrow;
            AllowDrop = true;
            Clipboard.Clear();
            Focusable = true;
            IsEnabled = true;
        }

        private void OnSaveAndClose( object sender, ExecutedRoutedEventArgs e )
        {
            if( Save() )
            {
                App.Current.Shutdown();
            }
        }

        private void OnDiscardAndClose( object sender, ExecutedRoutedEventArgs e )
        {
            App.Current.Shutdown();
        }

        private void New_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            this.Children.Clear();
            this.SelectionService.ClearSelection();
        }

        private void Open_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "WhiteBoard Files (*.bwb)|*.bwb";

            var ret = openFile.ShowDialog();
            if( ret == false )
            {
                return;
            }

            Open( openFile.FileName );
        }

        public void Open( string file )
        {
            Filename = file;

            Children.Clear();
            SelectionService.ClearSelection();

            try
            {
                var content = PersistenceService.Load( Filename );

                foreach( var item in content.Items )
                {
                    Children.Add( item );
                }

                this.InvalidateVisual();

                foreach( var connection in content.Connections )
                {
                    Children.Add( connection );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error );
            }
        }

        private Adorner mySavedAdorner;

        private void Save_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            if( Save() )
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer( this );

                mySavedAdorner = new SavedFeedbackAdorner( ( UIElement )VisualTreeHelper.GetParent( this ) );
                adornerLayer.Add( mySavedAdorner );

                var dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += OnElapsed;
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds( 500 );
                dispatcherTimer.Start();
            }
        }


        private void OnElapsed( object sender, EventArgs e )
        {
            var timer = ( DispatcherTimer )sender;
            timer.Tick -= OnElapsed;
            timer.Stop();

            var adornerLayer = AdornerLayer.GetAdornerLayer( this );
            adornerLayer.Remove( mySavedAdorner );
        }

        private bool Save()
        {
            if( Filename == null )
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.Filter = "WhiteBoard Files (*.bwb)|*.bwb|PNG (*.png)|*.png";

                var ret = saveFile.ShowDialog();
                if( ret == false )
                {
                    return false;
                }

                try
                {
                    if( saveFile.FilterIndex == 2 )
                    {
                        SelectionService.ClearSelection();

                        PersistenceService.SaveToPng( this, saveFile.FileName );
                        return false;
                    }
                    else
                    {
                        Filename = saveFile.FileName;
                    }
                }
                catch( Exception ex )
                {
                    MessageBox.Show( ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error );
                }
            }

            try
            {
                var content = new CanvasContent( Children.OfType<DesignerItem>(), Children.OfType<Connection>() );
                PersistenceService.Save( content, Filename );

                return true;
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error );
            }

            return false;
        }

        private void Print_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            SelectionService.ClearSelection();

            var printDialog = new PrintDialog();

            if( true == printDialog.ShowDialog() )
            {
                // http://stackoverflow.com/questions/7931961/wpf-printing-to-fit-page
                // http://www.a2zdotnet.com/View.aspx?id=66

                //store original scale
                var originalScale = LayoutTransform;

                //get selected printer capabilities
                var capabilities = printDialog.PrintQueue.GetPrintCapabilities( printDialog.PrintTicket );

                //get scale of the print wrt to screen of WPF visual
                double scale = Math.Min( capabilities.PageImageableArea.ExtentWidth / ActualWidth, capabilities.PageImageableArea.ExtentHeight /
                               ActualHeight );

                //Transform the Visual to scale
                LayoutTransform = new ScaleTransform( scale, scale );

                //get the size of the printer page
                System.Windows.Size sz = new System.Windows.Size( capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight );

                //update the layout of the visual to the printer page size.
                Measure( sz );
                Arrange( new System.Windows.Rect( new System.Windows.Point( capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight ), sz ) );

                //now print the visual to printer to fit on the one page.
                printDialog.PrintVisual( this, "WPF Diagram" );

                //apply the original transform.
                LayoutTransform = originalScale;
            }
        }

        private void Copy_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            CopyCurrentSelection();
        }

        private void Copy_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            if( SelectionService == null )
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = SelectionService.CurrentSelection.Count() > 0;
        }

        private void Paste_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            try
            {
                var content = PersistenceService.LoadFromClipboard();

                var currentOffset = content.Offset;

                content.AddOffset( 10, 10 );
                PersistenceService.CopyToClipboard( content );

                foreach( var item in content.Items )
                {
                    // apply offset
                    Canvas.SetLeft( item, Canvas.GetLeft( item ) + currentOffset.X );
                    Canvas.SetTop( item, Canvas.GetTop( item ) + currentOffset.Y );

                    Children.Add( item );
                }

                SelectionService.ClearSelection();

                foreach( DesignerItem item in content.Items )
                {
                    if( item.ParentID == Guid.Empty )
                    {
                        SelectionService.AddToSelection( this, item );
                    }
                }

                foreach( var connection in content.Connections )
                {
                    Children.Add( connection );

                    SelectionService.AddToSelection( this, connection );
                }

                DesignerCanvas.BringToFront.Execute( null, this );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error );
            }
        }

        private void Paste_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            e.CanExecute = Clipboard.ContainsData( DataFormats.Xaml );
        }

        private void Delete_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            DeleteCurrentSelection();
        }

        private void Delete_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            if( SelectionService == null )
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = SelectionService.CurrentSelection.Count() > 0;
        }

        private void Cut_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            CopyCurrentSelection();
            DeleteCurrentSelection();
        }

        private void Cut_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            if( SelectionService == null )
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = this.SelectionService.CurrentSelection.Count() > 0;
        }

        private void Group_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var items = from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
                        where item.ParentID == Guid.Empty
                        select item;

            Rect rect = GetBoundingRectangle( items );

            var groupItem = new DesignerItem();
            groupItem.IsGroup = true;
            groupItem.Width = rect.Width;
            groupItem.Height = rect.Height;

            Canvas.SetLeft( groupItem, rect.Left );
            Canvas.SetTop( groupItem, rect.Top );

            var itemContent = new ItemContent();
            // TODO: copy does NOT work
            // itemContent.Properties = new ItemPropertyCollection( items.SelectMany( item => ((ItemContent)item.Content).Properties ) );
            groupItem.Content = itemContent;

            Canvas.SetZIndex( groupItem, Children.Count );

            Children.Add( groupItem );

            foreach( var item in items )
            {
                item.ParentID = groupItem.ID;
            }

            SelectionService.SelectItem( this, groupItem );
        }

        private void Group_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            if( SelectionService == null )
            {
                e.CanExecute = false;
                return;
            }

            int count = ( from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                          where item.ParentID == Guid.Empty
                          select item ).Count();

            e.CanExecute = count > 1;
        }

        private void Ungroup_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var groups = ( from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                           where item.IsGroup && item.ParentID == Guid.Empty
                           select item ).ToArray();

            foreach( var groupRoot in groups )
            {
                var children = from child in SelectionService.CurrentSelection.OfType<DesignerItem>()
                               where child.ParentID == groupRoot.ID
                               select child;

                foreach( var child in children )
                {
                    child.ParentID = Guid.Empty;
                }

                SelectionService.RemoveFromSelection( this, groupRoot );
                Children.Remove( groupRoot );

                UpdateZIndex();
            }
        }

        private void Ungroup_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            if( SelectionService == null )
            {
                e.CanExecute = false;
                return;
            }

            var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                              where item.ParentID != Guid.Empty
                              select item;


            e.CanExecute = groupedItem.Count() > 0;
        }

        private void BringForward_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            List<UIElement> ordered = ( from item in SelectionService.CurrentSelection
                                        orderby Canvas.GetZIndex( item as UIElement ) descending
                                        select item as UIElement ).ToList();

            int count = this.Children.Count;

            for( int i = 0; i < ordered.Count; i++ )
            {
                int currentIndex = Canvas.GetZIndex( ordered[ i ] );
                int newIndex = Math.Min( count - 1 - i, currentIndex + 1 );
                if( currentIndex != newIndex )
                {
                    Canvas.SetZIndex( ordered[ i ], newIndex );
                    IEnumerable<UIElement> it = this.Children.OfType<UIElement>().Where( item => Canvas.GetZIndex( item ) == newIndex );

                    foreach( UIElement elm in it )
                    {
                        if( elm != ordered[ i ] )
                        {
                            Canvas.SetZIndex( elm, currentIndex );
                            break;
                        }
                    }
                }
            }
        }

        private void Order_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            //e.CanExecute = SelectionService.CurrentSelection.Count() > 0;
            e.CanExecute = true;
        }

        private void BringToFront_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            List<UIElement> selectionSorted = ( from item in SelectionService.CurrentSelection
                                                orderby Canvas.GetZIndex( item as UIElement ) ascending
                                                select item as UIElement ).ToList();

            List<UIElement> childrenSorted = ( from UIElement item in this.Children
                                               orderby Canvas.GetZIndex( item as UIElement ) ascending
                                               select item as UIElement ).ToList();

            int i = 0;
            int j = 0;
            foreach( UIElement item in childrenSorted )
            {
                if( selectionSorted.Contains( item ) )
                {
                    int idx = Canvas.GetZIndex( item );
                    Canvas.SetZIndex( item, childrenSorted.Count - selectionSorted.Count + j++ );
                }
                else
                {
                    Canvas.SetZIndex( item, i++ );
                }
            }
        }

        private void SendBackward_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            List<UIElement> ordered = ( from item in SelectionService.CurrentSelection
                                        orderby Canvas.GetZIndex( item as UIElement ) ascending
                                        select item as UIElement ).ToList();

            int count = this.Children.Count;

            for( int i = 0; i < ordered.Count; i++ )
            {
                int currentIndex = Canvas.GetZIndex( ordered[ i ] );
                int newIndex = Math.Max( i, currentIndex - 1 );
                if( currentIndex != newIndex )
                {
                    Canvas.SetZIndex( ordered[ i ], newIndex );
                    IEnumerable<UIElement> it = this.Children.OfType<UIElement>().Where( item => Canvas.GetZIndex( item ) == newIndex );

                    foreach( UIElement elm in it )
                    {
                        if( elm != ordered[ i ] )
                        {
                            Canvas.SetZIndex( elm, currentIndex );
                            break;
                        }
                    }
                }
            }
        }

        private void SendToBack_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            List<UIElement> selectionSorted = ( from item in SelectionService.CurrentSelection
                                                orderby Canvas.GetZIndex( item as UIElement ) ascending
                                                select item as UIElement ).ToList();

            List<UIElement> childrenSorted = ( from UIElement item in this.Children
                                               orderby Canvas.GetZIndex( item as UIElement ) ascending
                                               select item as UIElement ).ToList();
            int i = 0;
            int j = 0;
            foreach( UIElement item in childrenSorted )
            {
                if( selectionSorted.Contains( item ) )
                {
                    int idx = Canvas.GetZIndex( item );
                    Canvas.SetZIndex( item, j++ );

                }
                else
                {
                    Canvas.SetZIndex( item, selectionSorted.Count + i++ );
                }
            }
        }

        private void AlignTop_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double top = Canvas.GetTop( selectedItems.First() );

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = top - Canvas.GetTop( item );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetTop( di, Canvas.GetTop( di ) + delta );
                    }
                }
            }
        }

        private void Align_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
            //                  where item.ParentID == Guid.Empty
            //                  select item;


            //e.CanExecute = groupedItem.Count() > 1;
            e.CanExecute = true;
        }

        private void AlignVerticalCenters_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double bottom = Canvas.GetTop( selectedItems.First() ) + selectedItems.First().Height / 2;

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = bottom - ( Canvas.GetTop( item ) + item.Height / 2 );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetTop( di, Canvas.GetTop( di ) + delta );
                    }
                }
            }
        }

        private void AlignBottom_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double bottom = Canvas.GetTop( selectedItems.First() ) + selectedItems.First().Height;

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = bottom - ( Canvas.GetTop( item ) + item.Height );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetTop( di, Canvas.GetTop( di ) + delta );
                    }
                }
            }
        }

        private void AlignLeft_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double left = Canvas.GetLeft( selectedItems.First() );

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = left - Canvas.GetLeft( item );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetLeft( di, Canvas.GetLeft( di ) + delta );
                    }
                }
            }
        }

        private void AlignHorizontalCenters_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double center = Canvas.GetLeft( selectedItems.First() ) + selectedItems.First().Width / 2;

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = center - ( Canvas.GetLeft( item ) + item.Width / 2 );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetLeft( di, Canvas.GetLeft( di ) + delta );
                    }
                }
            }
        }

        private void AlignRight_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double right = Canvas.GetLeft( selectedItems.First() ) + selectedItems.First().Width;

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = right - ( Canvas.GetLeft( item ) + item.Width );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetLeft( di, Canvas.GetLeft( di ) + delta );
                    }
                }
            }
        }

        private void DistributeHorizontal_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                let itemLeft = Canvas.GetLeft( item )
                                orderby itemLeft
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double left = Double.MaxValue;
                double right = Double.MinValue;
                double sumWidth = 0;
                foreach( DesignerItem item in selectedItems )
                {
                    left = Math.Min( left, Canvas.GetLeft( item ) );
                    right = Math.Max( right, Canvas.GetLeft( item ) + item.Width );
                    sumWidth += item.Width;
                }

                double distance = Math.Max( 0, ( right - left - sumWidth ) / ( selectedItems.Count() - 1 ) );
                double offset = Canvas.GetLeft( selectedItems.First() );

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = offset - Canvas.GetLeft( item );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetLeft( di, Canvas.GetLeft( di ) + delta );
                    }
                    offset = offset + item.Width + distance;
                }
            }
        }

        private void Distribute_Enabled( object sender, CanExecuteRoutedEventArgs e )
        {
            //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
            //                  where item.ParentID == Guid.Empty
            //                  select item;


            //e.CanExecute = groupedItem.Count() > 1;
            e.CanExecute = true;
        }

        private void DistributeVertical_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                let itemTop = Canvas.GetTop( item )
                                orderby itemTop
                                select item;

            if( selectedItems.Count() > 1 )
            {
                double top = Double.MaxValue;
                double bottom = Double.MinValue;
                double sumHeight = 0;
                foreach( DesignerItem item in selectedItems )
                {
                    top = Math.Min( top, Canvas.GetTop( item ) );
                    bottom = Math.Max( bottom, Canvas.GetTop( item ) + item.Height );
                    sumHeight += item.Height;
                }

                double distance = Math.Max( 0, ( bottom - top - sumHeight ) / ( selectedItems.Count() - 1 ) );
                double offset = Canvas.GetTop( selectedItems.First() );

                foreach( DesignerItem item in selectedItems )
                {
                    double delta = offset - Canvas.GetTop( item );
                    foreach( DesignerItem di in SelectionService.GetGroupMembers( this, item ) )
                    {
                        Canvas.SetTop( di, Canvas.GetTop( di ) + delta );
                    }
                    offset = offset + item.Height + distance;
                }
            }
        }

        private void SelectAll_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            SelectionService.SelectAll( this );
        }

        private void CopyCurrentSelection()
        {
            var selectedDesignerItems = SelectionService.CurrentSelection.OfType<DesignerItem>();
            var selectedConnections = SelectionService.CurrentSelection.OfType<Connection>().ToList();

            foreach( Connection connection in this.Children.OfType<Connection>() )
            {
                if( !selectedConnections.Contains( connection ) )
                {
                    DesignerItem sourceItem = ( from item in selectedDesignerItems
                                                where item.ID == connection.Source.ParentDesignerItem.ID
                                                select item ).FirstOrDefault();

                    DesignerItem sinkItem = ( from item in selectedDesignerItems
                                              where item.ID == connection.Sink.ParentDesignerItem.ID
                                              select item ).FirstOrDefault();

                    if( sourceItem != null &&
                        sinkItem != null &&
                        BelongToSameGroup( sourceItem, sinkItem ) )
                    {
                        selectedConnections.Add( connection );
                    }
                }
            }

            var content = new CanvasContent( selectedDesignerItems, selectedConnections );
            content.AddOffset( 10, 10 );

            PersistenceService.CopyToClipboard( content );
        }

        private void DeleteCurrentSelection()
        {
            foreach( Connection connection in SelectionService.CurrentSelection.OfType<Connection>() )
            {
                this.Children.Remove( connection );
            }

            foreach( DesignerItem item in SelectionService.CurrentSelection.OfType<DesignerItem>() )
            {
                Control cd = item.Template.FindName( "PART_ConnectorDecorator", item ) as Control;

                List<Connector> connectors = new List<Connector>();
                GetConnectors( cd, connectors );

                foreach( Connector connector in connectors )
                {
                    foreach( Connection con in connector.Connections )
                    {
                        this.Children.Remove( con );
                    }
                }
                this.Children.Remove( item );
            }

            SelectionService.ClearSelection();
            UpdateZIndex();
        }

        private void UpdateZIndex()
        {
            List<UIElement> ordered = ( from UIElement item in this.Children
                                        orderby Canvas.GetZIndex( item as UIElement )
                                        select item as UIElement ).ToList();

            for( int i = 0; i < ordered.Count; i++ )
            {
                Canvas.SetZIndex( ordered[ i ], i );
            }
        }

        private static Rect GetBoundingRectangle( IEnumerable<DesignerItem> items )
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach( DesignerItem item in items )
            {
                x1 = Math.Min( Canvas.GetLeft( item ), x1 );
                y1 = Math.Min( Canvas.GetTop( item ), y1 );

                x2 = Math.Max( Canvas.GetLeft( item ) + item.Width, x2 );
                y2 = Math.Max( Canvas.GetTop( item ) + item.Height, y2 );
            }

            return new Rect( new Point( x1, y1 ), new Point( x2, y2 ) );
        }

        private void GetConnectors( DependencyObject parent, List<Connector> connectors )
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount( parent );
            for( int i = 0; i < childrenCount; i++ )
            {
                DependencyObject child = VisualTreeHelper.GetChild( parent, i );
                if( child is Connector )
                {
                    connectors.Add( child as Connector );
                }
                else
                {
                    GetConnectors( child, connectors );
                }
            }
        }

        private bool BelongToSameGroup( IGroupable item1, IGroupable item2 )
        {
            IGroupable root1 = SelectionService.GetGroupRoot( this, item1 );
            IGroupable root2 = SelectionService.GetGroupRoot( this, item2 );

            return ( root1.ID == root2.ID );
        }

        private void ConnectionStyleChanged_Executed( object sender, ExecutedRoutedEventArgs e )
        {
            var selectedItem = ( FrameworkElement )e.Parameter;
            ArrowStyle = ( ArrowSymbol )selectedItem.Tag;
        }

        public ArrowSymbol ArrowStyle
        {
            get;
            private set;
        }

        public string Filename
        {
            get;
            private set;
        }
    }
}
