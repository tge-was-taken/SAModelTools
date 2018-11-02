using System.Numerics;

namespace SAModelLibrary.Maths
{
    /// <summary>
    /// Describes the layout of an angle rotation vector.
    /// </summary>
    public struct AngleVector
    {
        /// <summary>
        /// An angle value with all of its components set to 0.
        /// </summary>
        public static readonly AngleVector Zero = new AngleVector( 0, 0, 0 );

        /// <summary>
        /// X component of the <see cref="AngleVector"/>
        /// </summary>
        public int X;

        /// <summary>
        /// Y component of the <see cref="AngleVector"/>
        /// </summary>
        public int Y;

        /// <summary>
        /// Z component of the <see cref="AngleVector"/>
        /// </summary>
        public int Z;

        /// <summary>
        /// Create a new angle vector with all components initialized.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public AngleVector( int x, int y, int z )
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(AngleVector l, AngleVector r)
        {
            return l.X == r.X && l.Y == r.Y && l.Z == r.Z;
        }

        public static bool operator !=( AngleVector l, AngleVector r )
        {
            return l.X != r.X && l.Y != r.Y && l.Z != r.Z;
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is AngleVector other ) )
                return false;

            return this == other;
        }

        /// <summary>
        /// Converts the angle vector to euler rotation in degrees.
        /// </summary>
        /// <returns></returns>
        public Vector3 ToDegrees()
        {
            return new Vector3( RotationConverter.AngleToDegrees( X ),
                                RotationConverter.AngleToDegrees( Y ),
                                RotationConverter.AngleToDegrees( Z ) );
        }

        /// <summary>
        /// Converts the angle vector to euler rotation in radians.
        /// </summary>
        /// <returns></returns>
        public Vector3 ToRadians()
        {
            return new Vector3( RotationConverter.AngleToRadians( X ),
                                RotationConverter.AngleToRadians( Y ),
                                RotationConverter.AngleToRadians( Z ) );
        }

        /// <summary>
        /// Converts the angle vector into a rotation matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ToRotationMatrix()
        {
            return Matrix4x4.CreateRotationX( RotationConverter.AngleToRadians( X ) ) *
                   Matrix4x4.CreateRotationY( RotationConverter.AngleToRadians( Y ) ) *
                   Matrix4x4.CreateRotationZ( RotationConverter.AngleToRadians( Z ) );
        }

        /// <summary>
        /// Converts the angle vector into a rotation matrix around an origin.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public Matrix4x4 ToRotationMatrix( Vector3 origin )
        {
            return Matrix4x4.CreateRotationX( RotationConverter.AngleToRadians( X ), origin ) *
                   Matrix4x4.CreateRotationY( RotationConverter.AngleToRadians( Y ), origin ) *
                   Matrix4x4.CreateRotationZ( RotationConverter.AngleToRadians( Z ), origin );
        }

        /// <summary>
        /// Converts the angle vector into a rotation matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ToRotationMatrixZXY()
        {
            return Matrix4x4.CreateRotationZ( RotationConverter.AngleToRadians( X ) ) *
                   Matrix4x4.CreateRotationX( RotationConverter.AngleToRadians( Y ) ) *
                   Matrix4x4.CreateRotationY( RotationConverter.AngleToRadians( Z ) );
        }

        /// <summary>
        /// Converts the angle vector into a rotation matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ToRotationMatrixZYX()
        {
            return Matrix4x4.CreateRotationZ( RotationConverter.AngleToRadians( X ) ) *
                   Matrix4x4.CreateRotationY( RotationConverter.AngleToRadians( Y ) ) *
                   Matrix4x4.CreateRotationX( RotationConverter.AngleToRadians( Z ) );
        }

        /// <summary>
        /// Converts the angle vector into a rotation matrix around an origin.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public Matrix4x4 ToRotationMatrixZXY( Vector3 origin )
        {
            return Matrix4x4.CreateRotationZ( RotationConverter.AngleToRadians( X ), origin ) *
                   Matrix4x4.CreateRotationX( RotationConverter.AngleToRadians( Y ), origin ) *
                   Matrix4x4.CreateRotationY( RotationConverter.AngleToRadians( Z ), origin );
        }

        /// <summary>
        /// Create an angle vector from euler rotation in degrees.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static AngleVector FromDegrees( Vector3 vector )
        {
            return new AngleVector( RotationConverter.DegreesToAngle( vector.X ),
                                    RotationConverter.DegreesToAngle( vector.Y ),
                                    RotationConverter.DegreesToAngle( vector.Z ) );
        }

        /// <summary>
        /// Create an angle vector from euler rotation in radians.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static AngleVector FromRadians( Vector3 vector )
        {
            return new AngleVector( RotationConverter.RadiansToAngle( vector.X ),
                                    RotationConverter.RadiansToAngle( vector.Y ),
                                    RotationConverter.RadiansToAngle( vector.Z ) );
        }

        /// <summary>
        /// Create an angle vector from a quaternion rotation vector.
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static AngleVector FromQuaternion( Quaternion quaternion )
        {
            var degrees = quaternion.ToEulerAngles();
            return FromDegrees( degrees );
        }
    }
}