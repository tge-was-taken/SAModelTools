using System;
using System.Collections;
using System.Collections.Generic;
using SAModelLibrary.IO;

namespace SAModelLibrary
{
    /// <summary>
    /// Represents a list of death zones associated with a stage.
    /// </summary>
    public class DeathZoneList : ISerializableObject, IList<DeathZone>
    {
        private readonly List<DeathZone> mList;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        public DeathZoneList()
        {
            mList = new List<DeathZone>();
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            while ( true )
            {
                try
                {
                    var deathZone = reader.ReadObject<DeathZone>();
                    if ( deathZone.Flags == 0 && deathZone.RootNode == null )
                        break;

                    mList.Add( deathZone );
                }
                catch ( Exception )
                {
                    break;
                }
            }
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            foreach ( var deathZone in mList )
            {
                writer.WriteObject( deathZone );
            }

            // terminator
            writer.Write( 0 );
            writer.Write( 0 );
        }

        #region IList implementation

        /// <inheritdoc />
        public IEnumerator<DeathZone> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( IEnumerable ) mList ).GetEnumerator();
        }

        /// <inheritdoc />
        public void Add( DeathZone item )
        {
            mList.Add( item );
        }

        /// <inheritdoc />
        public void Clear()
        {
            mList.Clear();
        }

        /// <inheritdoc />
        public bool Contains( DeathZone item )
        {
            return mList.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( DeathZone[] array, int arrayIndex )
        {
            mList.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public bool Remove( DeathZone item )
        {
            return mList.Remove( item );
        }

        /// <inheritdoc />
        public int Count => mList.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf( DeathZone item )
        {
            return mList.IndexOf( item );
        }

        /// <inheritdoc />
        public void Insert( int index, DeathZone item )
        {
            mList.Insert( index, item );
        }

        /// <inheritdoc />
        public void RemoveAt( int index )
        {
            mList.RemoveAt( index );
        }

        /// <inheritdoc />
        public DeathZone this[ int index ]
        {
            get => mList[ index ];
            set => mList[ index ] = value;
        }
        #endregion
    }
}