using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using Plainion.WhiteBoard.Designer;
using Plainion;

namespace Plainion.WhiteBoard.Serialization
{
    public class CanvasXmlReader
    {
        private TextReader myReader;

        public CanvasXmlReader( TextReader reader )
        {
            Contract.RequiresNotNull( reader, "reader" );

            myReader = reader;
        }

        public CanvasContent Deserialize()
        {
            var root = XElement.Load( myReader );

            double offsetX = Double.Parse( root.Attribute( "OffsetX" ).Value, CultureInfo.InvariantCulture );
            double offsetY = Double.Parse( root.Attribute( "OffsetY" ).Value, CultureInfo.InvariantCulture );

            var items = new List<DesignerItem>();

            var itemsXML = root.Elements( "DesignerItems" ).Elements( "DesignerItem" );
            foreach( var itemXML in itemsXML )
            {
                var id = new Guid( itemXML.Element( "ID" ).Value );
                var item = DeserializeDesignerItem( itemXML, id );

                item.SetConnectorDecoratorTemplate();

                items.Add( item );
            }

            var connections = new List<Connection>();

            var connectionsXML = root.Elements( "Connections" ).Elements( "Connection" );
            foreach( var connectionXML in connectionsXML )
            {
                var sourceID = new Guid( connectionXML.Element( "SourceID" ).Value );
                var sinkID = new Guid( connectionXML.Element( "SinkID" ).Value );

                var sourceConnectorName = connectionXML.Element( "SourceConnectorName" ).Value;
                var sinkConnectorName = connectionXML.Element( "SinkConnectorName" ).Value;

                var sourceConnector = GetConnector( items, sourceID, sourceConnectorName );
                var sinkConnector = GetConnector( items, sinkID, sinkConnectorName );

                var connection = new Connection( sourceConnector, sinkConnector );

                connection.SourceArrowSymbol = ( ArrowSymbol )Enum.Parse( typeof( ArrowSymbol ), connectionXML.Element( "SourceArrowSymbol" ).Value );
                connection.SinkArrowSymbol = ( ArrowSymbol )Enum.Parse( typeof( ArrowSymbol ), connectionXML.Element( "SinkArrowSymbol" ).Value );
                connection.IsDotted = connectionXML.Element( "IsDotted" ) != null ? bool.Parse( connectionXML.Element( "IsDotted" ).Value ) : false;

                Canvas.SetZIndex( connection, Int32.Parse( connectionXML.Element( "zIndex" ).Value ) );

                var properties = ( ItemPropertyCollection )XamlReader.Load( XmlReader.Create( new StringReader( connectionXML.Element( "Properties" ).Value ) ) );
                connection.Properties = properties;

                connection.Caption = connectionXML.Element( "Caption" ).Value;

                connections.Add( connection );
            }

            var canvas = new CanvasContent( items, connections );
            canvas.AddOffset( offsetX, offsetY );

            return canvas;
        }

        private DesignerItem DeserializeDesignerItem( XElement itemXML, Guid id )
        {
            var item = new DesignerItem( id );

            item.Width = Double.Parse( itemXML.Element( "Width" ).Value, CultureInfo.InvariantCulture );
            item.Height = Double.Parse( itemXML.Element( "Height" ).Value, CultureInfo.InvariantCulture );
            item.ParentID = new Guid( itemXML.Element( "ParentID" ).Value );
            item.IsGroup = Boolean.Parse( itemXML.Element( "IsGroup" ).Value );
            Canvas.SetLeft( item, Double.Parse( itemXML.Element( "Left" ).Value, CultureInfo.InvariantCulture ) );
            Canvas.SetTop( item, Double.Parse( itemXML.Element( "Top" ).Value, CultureInfo.InvariantCulture ) );
            Canvas.SetZIndex( item, Int32.Parse( itemXML.Element( "zIndex" ).Value ) );

            DeserializeItemContent( itemXML, item );

            return item;
        }

        private void DeserializeItemContent( XElement itemXML, DesignerItem item )
        {
            var contentContainerElement = itemXML.Element( "Content" );
            var versionAttribute = contentContainerElement.Attribute( "Version" );

            var contentXml = XElement.Load( XmlReader.Create( new StringReader( contentContainerElement.Value ) ) );

            RewriteImageSource( contentXml );

            var content = XamlReader.Load( contentXml.CreateReader() );

            if( versionAttribute == null )
            {
                UpgradeItemContent( ( ItemContent )content );
            }

            item.Content = content;
        }

        private void RewriteImageSource( XElement root )
        {
            if( Location == null )
            {
                return;
            }

            if( root.Name == XName.Get( "Image", "http://schemas.microsoft.com/winfx/2006/xaml/presentation" ) )
            {
                var source = root.Attribute( "Source" ).Value;
                if( !Path.IsPathRooted( source ) )
                {
                    root.Attribute( "Source" ).Value = Path.GetFullPath( Path.Combine( Location, source ) );
                }
            }

            foreach( var child in root.Elements() )
            {
                RewriteImageSource( child );
            }
        }

        // backward compatibility code
        private void UpgradeItemContent( ItemContent content )
        {
            if( content.Properties.Any( p => p.ElementName == "ClassName" ) )
            {
                ( ( Border )content.Content ).Name = "Body";
                content.Properties.Add( new ItemProperty { ElementName = "Body", ElementProperty = "Background", DisplayName = "Fill", IsComponentName = false } );

                return;
            }

            if( content.Properties.Any( p => p.ElementName == "ObjectName" ) )
            {
                ( ( Border )content.Content ).Name = "Body";
                content.Properties.Add( new ItemProperty { ElementName = "Body", ElementProperty = "Background", DisplayName = "Fill", IsComponentName = false } );

                return;
            }

            if( content.Properties.Any( p => p.ElementName == "Question" ) )
            {
                var grid = ( Grid )content.Content;
                var path = grid.Children.OfType<System.Windows.Shapes.Path>().Single();
                path.Name = "Body";

                content.Properties.Add( new ItemProperty { ElementName = "Body", ElementProperty = "Fill", DisplayName = "Fill", IsComponentName = false } );

                return;
            }
        }

        private Connector GetConnector( IEnumerable<DesignerItem> items, Guid itemID, String connectorName )
        {
            var designerItem = items.FirstOrDefault( item => item.ID == itemID );

            var connectorDecorator = ( Control )designerItem.Template.FindName( "PART_ConnectorDecorator", designerItem );
            connectorDecorator.ApplyTemplate();

            return connectorDecorator.Template.FindName( connectorName, connectorDecorator ) as Connector;
        }

        public string Location { get; set; }
    }
}
