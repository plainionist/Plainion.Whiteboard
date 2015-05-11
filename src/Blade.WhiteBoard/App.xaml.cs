using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Plainion.Windows;

namespace Plainion.WhiteBoard
{
    public partial class App : Application
    {
        private CompositionContainer myContainer;

        protected override void OnStartup( StartupEventArgs e )
        {
            base.OnStartup( e );

            new UnhandledExceptionHook( this );

            Application.Current.Exit += OnShutdown;

            var catalog = new AssemblyCatalog( GetType().Assembly );
            myContainer = new CompositionContainer( catalog, CompositionOptions.DisableSilentRejection );

            myContainer.Compose( new CompositionBatch() );

            var shell = myContainer.GetExportedValue<Shell>();

            myContainer.SatisfyImportsOnce( shell.myDesigner );

            ( ( Shell )MainWindow ).myProperties.DataContext = shell.myDesigner.SelectionService;

            Application.Current.MainWindow = shell;
            Application.Current.MainWindow.Show();

            var args = Environment.GetCommandLineArgs();
            if( args.Length == 2 )
            {
                shell.myDesigner.Open( args[ 1 ] );
            }
        }

        private void OnShutdown( object sender, ExitEventArgs e )
        {
            myContainer.Dispose();
        }
    }
}
