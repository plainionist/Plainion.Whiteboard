using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Plainion.WhiteBoard.Designer
{
    public class ItemPropertyDescriptor : PropertyDescriptor
    {
        private DependencyObject myRoot;
        private DependencyObject myOwner;
        private ItemProperty myProperty;
        private PropertyInfo myPropertyInfo;

        public ItemPropertyDescriptor( DependencyObject root, ItemProperty property )
            : base( property.DisplayName, null )
        {
            myRoot = root;
            myProperty = property;
        }

        private DependencyObject Owner
        {
            get
            {
                if( myOwner == null )
                {
                    myOwner = GetOwner();
                }

                return myOwner;
            }
        }

        private DependencyObject GetOwner()
        {
            if( myProperty.ElementName == null )
            {
                // property directly on the owner
                return myRoot;
            }

            var owner = LogicalTreeHelper.FindLogicalNode( myRoot, myProperty.ElementName );
            if( owner != null )
            {
                return owner;
            }

            throw new InvalidOperationException( "Owner not found: " + myProperty.ElementName );
        }

        private PropertyInfo PropertyInfo
        {
            get
            {
                if( myPropertyInfo == null )
                {
                    myPropertyInfo = Owner.GetType().GetProperty( myProperty.ElementProperty );
                }

                return myPropertyInfo;
            }
        }

        public override Type ComponentType
        {
            get { return typeof( ItemContent ); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return PropertyInfo.PropertyType; }
        }

        public override bool CanResetValue( object component )
        {
            return true;
        }

        public override object GetValue( object component )
        {
            return PropertyInfo.GetValue( Owner, null );
        }

        public override void ResetValue( object component )
        {
            if( PropertyType.IsValueType )
            {
                myPropertyInfo.SetValue( Owner, Activator.CreateInstance( PropertyType ), null );
            }
            else
            {
                myPropertyInfo.SetValue( Owner, null, null );
            }
        }

        public override void SetValue( object component, object value )
        {
            myPropertyInfo.SetValue( Owner, value, null );
        }

        public override bool ShouldSerializeValue( object component )
        {
            return false;
        }

        public override bool IsBrowsable
        {
            get { return true; }
        }
    }
}
