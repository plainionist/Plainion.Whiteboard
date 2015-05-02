using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;
using Plainion.WhiteBoard.Designer;
using Plainion;

namespace Plainion.WhiteBoard.Serialization
{
    public class CanvasXmlWriter
    {
        private TextWriter myWriter;
        private IIdTransformation myIdTransformation;

        private const int Version = 1;

        public CanvasXmlWriter( TextWriter writer )
            : this( writer, new SerializationSettings() )
        {
        }

        public CanvasXmlWriter( TextWriter writer, SerializationSettings settings )
        {
            Contract.RequiresNotNull( writer, "writer" );
            Contract.RequiresNotNull( settings, "settings" );

            myWriter = writer;
            myIdTransformation = CreateIdTransformation( settings );
        }

        private static IIdTransformation CreateIdTransformation( SerializationSettings settings )
        {
            if( settings.RewriteIds )
            {
                return new RewriteIdTransformation();
            }
            else
            {
                return new IdentityIdTransformation();
            }
        }

        public void Serialize( CanvasContent content )
        {
            var root = new XElement( "Root",
                new XAttribute( "OffsetX", content.Offset.X ),
                new XAttribute( "OffsetY", content.Offset.X ),
                new XAttribute( "Version", Version ),
                Serialize( content.Items ),
                Serialize( content.Connections ) );

            root.Save( myWriter );
        }

        private XElement Serialize( IEnumerable<DesignerItem> items )
        {
            return new XElement( "DesignerItems",
                items.Select( item => Serialize( item ) ) );
        }

        private XElement Serialize( DesignerItem item )
        {
            var contentXaml = XElement.Parse( XamlWriter.Save( item.Content ) );

            RewriteImageSource( contentXaml );

            return new XElement( "DesignerItem",
                new XElement( "Left", Canvas.GetLeft( item ) ),
                new XElement( "Top", Canvas.GetTop( item ) ),
                new XElement( "Width", item.Width ),
                new XElement( "Height", item.Height ),
                new XElement( "ID", myIdTransformation.GetId( item.ID ) ),
                new XElement( "zIndex", Canvas.GetZIndex( item ) ),
                new XElement( "IsGroup", item.IsGroup ),
                new XElement( "ParentID", myIdTransformation.GetId( item.ParentID ) ),
                new XElement( "Content",
                    new XAttribute( "Version", "1" ),
                    contentXaml.ToString() )
                );
        }

        private void RewriteImageSource( XElement root )
        {
            if( Location == null )
            {
                return;
            }

            if( root.Name == XName.Get( "Image", "http://schemas.microsoft.com/winfx/2006/xaml/presentation" ) )
            {
                var source = new Uri( root.Attribute( "Source" ).Value ).LocalPath;
                if( source.StartsWith( Location, StringComparison.OrdinalIgnoreCase ) )
                {
                    source = source.Substring( Location.Length );
                    if( source.StartsWith( "\\" ) )
                    {
                        source = source.Substring( 1 );
                    }
                    root.Attribute( "Source" ).Value = source;
                }
            }

            foreach( var child in root.Elements() )
            {
                RewriteImageSource( child );
            }
        }

        private XElement Serialize( IEnumerable<Connection> connections )
        {
            return new XElement( "Connections",
                connections.Select( c => Serialize( c ) ) );
        }

        private XElement Serialize( Connection connection )
        {
            var propertiesXaml = XamlWriter.Save( connection.Properties );

            return new XElement( "Connection",
                new XElement( "SourceID", myIdTransformation.GetId( connection.Source.ParentDesignerItem.ID ) ),
                new XElement( "SinkID", myIdTransformation.GetId( connection.Sink.ParentDesignerItem.ID ) ),
                new XElement( "SourceConnectorName", connection.Source.Name ),
                new XElement( "SinkConnectorName", connection.Sink.Name ),
                new XElement( "SourceArrowSymbol", connection.SourceArrowSymbol ),
                new XElement( "SinkArrowSymbol", connection.SinkArrowSymbol ),
                new XElement( "IsDotted", connection.IsDotted ),
                new XElement( "zIndex", Canvas.GetZIndex( connection ) ),
                new XElement( "Properties", propertiesXaml ),
                new XElement( "Caption", connection.Caption )
                );
        }

        public string Location { get; set; }
    }
}
