using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Plainion.Windows;

namespace Plainion.WhiteBoard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CompositionContainer myContainer;

        private void Application_Activated( object sender, EventArgs e )
        {
            if ( myContainer != null )
            {
                return;
            }

            new UnhandledExceptionHook( this );

            var catalog = new AssemblyCatalog( GetType().Assembly );

            myContainer = new CompositionContainer( catalog );

            var designer = ((MainWindow)MainWindow).myDesigner;

            myContainer.SatisfyImportsOnce( designer );

            ((MainWindow)MainWindow).myProperties.DataContext = designer.SelectionService;

            var args = Environment.GetCommandLineArgs();
            if( args.Length == 2 )
            {
                designer.Open( args[ 1 ] );
            }

        }
    }
}
