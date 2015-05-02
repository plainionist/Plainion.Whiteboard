using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Plainion.WhiteBoard.Serialization;

namespace Plainion.WhiteBoard.Services
{
    [Export( typeof( PersistenceService ) )]
    class PersistenceService
    {
        public void Save( CanvasContent content, string file )
        {
            using ( var writer = new StreamWriter( file ) )
            {
                var serializer = new CanvasXmlWriter( writer );
                serializer.Location = Path.GetDirectoryName( file );
                serializer.Serialize( content );
            }
        }

        public void SaveToPng( Canvas surface, string file )
        {
            // Save current canvas transform
            var transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            var size = new Size( 1600, 900 );

            // Attentation: Measure and arrange the surface !
            surface.Measure( size );
            surface.Arrange( new Rect( size ) );

            var renderBitmap = new RenderTargetBitmap( (int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32 );

            var bounds = VisualTreeHelper.GetDescendantBounds( surface );
            var dv = new DrawingVisual();
            using ( var ctx = dv.RenderOpen() )
            {
                var vb = new VisualBrush( surface );
                ctx.DrawRectangle( vb, null, new Rect( new Point(), bounds.Size ) );
            }

            renderBitmap.Render( dv );
            using ( var outStream = new FileStream( file, FileMode.OpenOrCreate, FileAccess.Write ) )
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add( BitmapFrame.Create( renderBitmap ) );
                encoder.Save( outStream );
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        public void CopyToClipboard( CanvasContent content )
        {
            using ( var writer = new StringWriter() )
            {
                var settings = new SerializationSettings();
                settings.RewriteIds = true;

                var serializer = new CanvasXmlWriter( writer, settings );
                serializer.Serialize( content );

                Clipboard.Clear();
                Clipboard.SetData( DataFormats.Xaml, writer.ToString() );
            }
        }

        public CanvasContent Load( string file )
        {
            using ( var reader = new StreamReader( file ) )
            {
                var serializer = new CanvasXmlReader( reader );
                serializer.Location = Path.GetDirectoryName( file );
                return serializer.Deserialize();
            }
        }

        public CanvasContent LoadFromClipboard()
        {
            if ( !Clipboard.ContainsData( DataFormats.Xaml ) )
            {
                return null;
            }

            var clipboardData = Clipboard.GetData( DataFormats.Xaml ) as string;
            if ( string.IsNullOrEmpty( clipboardData ) )
            {
                return null;
            }

            using ( var reader = new StringReader( clipboardData ) )
            {
                var serializer = new CanvasXmlReader( reader );
                return serializer.Deserialize();
            }
        }
    }
}
