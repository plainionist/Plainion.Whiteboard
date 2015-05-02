using System;
using System.Collections.Generic;

namespace Plainion.WhiteBoard.Serialization
{
    public class RewriteIdTransformation : IIdTransformation
    {
        private Dictionary<Guid, Guid> myOldToIdMap;

        public RewriteIdTransformation()
        {
            myOldToIdMap = new Dictionary<Guid, Guid>();
        }

        public Guid GetId( Guid sourceId )
        {
            if ( sourceId == Guid.Empty )
            {
                return Guid.Empty;
            }

            if ( !myOldToIdMap.ContainsKey( sourceId ) )
            {
                myOldToIdMap[ sourceId ] = Guid.NewGuid();
            }

            return myOldToIdMap[ sourceId ];
        }
    }
}
