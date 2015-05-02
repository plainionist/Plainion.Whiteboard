using System;
using System.ComponentModel;
using System.Windows.Shapes;

namespace Plainion.WhiteBoard.Designer
{
    public class ItemPropertiesProvider : TypeDescriptionProvider
    {
        private static TypeDescriptionProvider defaultTypeProvider = TypeDescriptor.GetProvider( typeof( ItemPropertiesProvider ) );

        public ItemPropertiesProvider()
            : base( defaultTypeProvider )
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor( Type objectType, object instance )
        {
            if ( instance is DesignerItem )
            {
                var item = (DesignerItem)instance;
                var content = item.Content as ItemContent;

                var descriptor =  new ItemPropertiesDescriptor( content );

                // TODO: this is a workaround to get the "selected object name" set in property grid
                // but for now it works - even after update of a property this method gets called again
                //item.Name = descriptor.GetComponentName();

                return descriptor;
            }

            if ( instance is Connection )
            {
                var connection = (Connection)instance;
                var descriptor = new ItemPropertiesDescriptor( connection );
                
                // TODO: this is a workaround to get the "selected object name" set in property grid
                // but for now it works - even after update of a property this method gets called again
               // connection.Name = descriptor.GetComponentName();

                return descriptor;
            }

            return base.GetTypeDescriptor( objectType, instance );
        }
    }
}
