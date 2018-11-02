using System;
using System.Runtime.Serialization;

namespace SAModelLibrary.Exceptions
{
    [Serializable]
    internal class InvalidGeometryDataException : Exception
    {
        public InvalidGeometryDataException()
        {
        }

        public InvalidGeometryDataException( string message ) : base( message )
        {
        }

        public InvalidGeometryDataException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected InvalidGeometryDataException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }

    public class InvalidFileFormatException : Exception
    {
        public InvalidFileFormatException( string message ) : base( message )
        {
        }
    }
}