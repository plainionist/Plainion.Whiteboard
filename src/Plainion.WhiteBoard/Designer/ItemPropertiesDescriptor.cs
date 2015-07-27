using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Plainion.WhiteBoard.Designer
{
    public class ItemPropertiesDescriptor : ICustomTypeDescriptor
    {
        private ItemContent myOwner;
        private ItemPropertyCollection myProperties;

        public ItemPropertiesDescriptor( ItemContent content )
        {
            myOwner = content;

            if ( content.Properties != null )
            {
                myProperties = content.Properties;
            }
            else
            {
                myProperties = new ItemPropertyCollection();
            }
        }

        public AttributeCollection GetAttributes()
        {
            return new AttributeCollection( null );
        }

        public string GetClassName()
        {
            return "Item";
        }

        public string GetComponentName()
        {
            var compNameProp = myProperties.SingleOrDefault( p => p.IsComponentName );
            if ( compNameProp == null )
            {
                return null;
            }

            var descriptor = new ItemPropertyDescriptor( myOwner, compNameProp );

            return (string)descriptor.GetValue( myOwner );
        }

        public System.ComponentModel.TypeConverter GetConverter()
        {
            return new ExpandableObjectConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor( Type editorBaseType )
        {
            return null;
        }

        public EventDescriptorCollection GetEvents( Attribute[] attributes )
        {
            return new EventDescriptorCollection( null );
        }

        public EventDescriptorCollection GetEvents()
        {
            return new EventDescriptorCollection( null );
        }

        public PropertyDescriptorCollection GetProperties( Attribute[] attributes )
        {
            var properties = new List<PropertyDescriptor>();

            foreach ( var prop in myProperties )
            {
                // TODO: caching?
                properties.Add( new ItemPropertyDescriptor( myOwner, prop ) );
            }

            return new PropertyDescriptorCollection( properties.ToArray() );
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties( null );
        }

        public object GetPropertyOwner( PropertyDescriptor pd )
        {
            return this;
        }
    }
}
