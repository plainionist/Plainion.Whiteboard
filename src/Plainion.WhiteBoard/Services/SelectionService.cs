using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Plainion.WhiteBoard.Model;

namespace Plainion.WhiteBoard.Services
{
    [Export( typeof( SelectionService ) )]
    internal class SelectionService : INotifyPropertyChanged
    {
        private List<ISelectable> myCurrentSelection;

        public SelectionService()
        {
            myCurrentSelection = new List<ISelectable>();
        }

        public IEnumerable<ISelectable> CurrentSelection
        {
            get
            {
                return myCurrentSelection;
            }
        }

        /// <summary>
        /// FirstOrDefault on CurrentSelection
        /// </summary>
        public ISelectable SelectedItem
        {
            get
            {
                return CurrentSelection.FirstOrDefault();
            }
        }
        
        internal void SelectItem( ICanvasModel model, ISelectable item )
        {
            ClearSelection();
            AddToSelection( model, item );
        }

        internal void AddToSelection( ICanvasModel model, ISelectable item )
        {
            if ( item is IGroupable )
            {
                var groupItems = GetGroupMembers( model, item as IGroupable );

                foreach ( ISelectable groupItem in groupItems )
                {
                    groupItem.IsSelected = true;
                    myCurrentSelection.Add( groupItem );
                }
            }
            else
            {
                item.IsSelected = true;
                myCurrentSelection.Add( item );
            }

            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( "CurrentSelection" ) );
                PropertyChanged( this, new PropertyChangedEventArgs( "SelectedItem" ) );
            }
        }

        internal void RemoveFromSelection( ICanvasModel model, ISelectable item )
        {
            if ( item is IGroupable )
            {
                var groupItems = GetGroupMembers( model, item as IGroupable );

                foreach ( ISelectable groupItem in groupItems )
                {
                    groupItem.IsSelected = false;
                    myCurrentSelection.Remove( groupItem );
                }
            }
            else
            {
                item.IsSelected = false;
                myCurrentSelection.Remove( item );
            }

            OnSelectionChanged();
        }

        internal void ClearSelection()
        {
            myCurrentSelection.ForEach( item => item.IsSelected = false );
            myCurrentSelection.Clear();

            OnSelectionChanged();
        }

        internal void SelectAll( ICanvasModel model )
        {
            ClearSelection();
            myCurrentSelection.AddRange( model.GetItems<ISelectable>() );
            myCurrentSelection.ForEach( item => item.IsSelected = true );

            OnSelectionChanged();
        }

        internal List<IGroupable> GetGroupMembers( ICanvasModel model, IGroupable item )
        {
            var list = model.GetItems<IGroupable>();
            var rootItem = GetRoot( list, item );
            return GetGroupMembers( list, rootItem );
        }

        internal IGroupable GetGroupRoot( ICanvasModel model, IGroupable item )
        {
            var list = model.GetItems<IGroupable>();
            return GetRoot( list, item );
        }

        private IGroupable GetRoot( IEnumerable<IGroupable> list, IGroupable node )
        {
            if ( node == null || node.ParentID == Guid.Empty )
            {
                return node;
            }

            foreach ( var item in list )
            {
                if ( item.ID == node.ParentID )
                {
                    return GetRoot( list, item );
                }
            }

            return null;
        }

        private List<IGroupable> GetGroupMembers( IEnumerable<IGroupable> list, IGroupable parent )
        {
            List<IGroupable> groupMembers = new List<IGroupable>();
            groupMembers.Add( parent );

            var children = list.Where( node => node.ParentID == parent.ID );

            foreach ( IGroupable child in children )
            {
                groupMembers.AddRange( GetGroupMembers( list, child ) );
            }

            return groupMembers;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
