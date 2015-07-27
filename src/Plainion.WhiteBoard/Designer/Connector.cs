using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Model;

namespace Plainion.WhiteBoard.Designer
{
    public class Connector : Control, INotifyPropertyChanged
    {
        // drag start point, relative to the DesignerCanvas
        private Point? myDragStartPoint;
        private Point myPosition;
        private DesignerItem myOwner;
        private List<Connection> myConnections;

        public Connector()
        {
            LayoutUpdated += OnLayoutUpdated;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ConnectorOrientation Orientation
        {
            get;
            set;
        }

        /// <summary>
        /// center position of this Connector relative to the DesignerCanvas
        /// </summary>
        public Point Position
        {
            get
            {
                return myPosition;
            }
            set
            {
                if ( myPosition != value )
                {
                    myPosition = value;
                    OnPropertyChanged( "Position" );
                }
            }
        }

        /// <summary>
        /// the DesignerItem this Connector belongs to;
        /// retrieved from DataContext, which is set in the
        /// DesignerItem template
        /// </summary>
        public DesignerItem ParentDesignerItem
        {
            get
            {
                if ( myOwner == null )
                {
                    myOwner = DataContext as DesignerItem;
                }

                return myOwner;
            }
        }

        /// <summary>
        /// keep track of connections that link to this connector
        /// </summary>
        public List<Connection> Connections
        {
            get
            {
                if ( myConnections == null )
                {
                    myConnections = new List<Connection>();
                }

                return myConnections;
            }
        }

        // when the layout changes we update the position property
        void OnLayoutUpdated( object sender, EventArgs eventArgs )
        {
            var designer = GetDesignerCanvas( this );

            if ( designer != null )
            {
                //get centre position of this Connector relative to the DesignerCanvas
                Position = TransformToAncestor( designer ).Transform( new Point( Width / 2, Height / 2 ) );
            }
        }

        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs eventArgs )
        {
            base.OnMouseLeftButtonDown( eventArgs );

            var canvas = GetDesignerCanvas( this );
            if ( canvas != null )
            {
                // position relative to DesignerCanvas
                myDragStartPoint = new Point?( eventArgs.GetPosition( canvas ) );

                eventArgs.Handled = true;
            }
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            base.OnMouseMove( e );

            // if mouse button is not pressed we have no drag operation, ...
            if ( e.LeftButton != MouseButtonState.Pressed )
            {
                myDragStartPoint = null;
            }

            // but if mouse button is pressed and start point value is set we do have one
            if ( myDragStartPoint.HasValue )
            {
                // create connection adorner 
                DesignerCanvas canvas = GetDesignerCanvas( this );
                if ( canvas != null )
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( canvas );
                    if ( adornerLayer != null )
                    {
                        ConnectorAdorner adorner = new ConnectorAdorner( canvas, this );
                        if ( adorner != null )
                        {
                            adornerLayer.Add( adorner );
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        internal ConnectorModel GetInfo()
        {
            var owner = new DesignerItemModel();
            owner.Left = DesignerCanvas.GetLeft( ParentDesignerItem );
            owner.Top = DesignerCanvas.GetTop( ParentDesignerItem );
            owner.Size = new Size( ParentDesignerItem.ActualWidth, ParentDesignerItem.ActualHeight );

            ConnectorModel info = new ConnectorModel( owner );
            info.Orientation = Orientation;
            info.Position = Position;

            return info;
        }

        // iterate through visual tree to get parent DesignerCanvas
        private DesignerCanvas GetDesignerCanvas( DependencyObject element )
        {
            while ( element != null && !(element is DesignerCanvas) )
            {
                element = VisualTreeHelper.GetParent( element );
            }

            return element as DesignerCanvas;
        }

        protected void OnPropertyChanged( string name )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if ( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( name ) );
            }
        }
    }
}
