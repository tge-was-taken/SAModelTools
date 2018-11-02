using System;
using System.Collections.Generic;
using System.Numerics;

namespace SAModelLibrary
{
    /// <summary>
    /// Represents a sphere consisting of a center vector and a sphere radius used for calculations.
    /// </summary>
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        /// <summary>
        /// The center vector of the sphere.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Creates a new <see cref="BoundingSphere"/> whose values are set to the specified values.
        /// </summary>
        /// <param name="center">The value to set the center vector to.</param>
        /// <param name="radius">The value to set the sphere radius to.</param>
        public BoundingSphere( Vector3 center, float radius )
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Calculates and creates a new <see cref="BoundingSphere"/> from vertices.
        /// </summary>
        /// <param name="vertices">The vertices used to calculate the components.</param>
        /// <returns>A new <see cref="BoundingSphere"/> calculated from the vertices.</returns>
        public static BoundingSphere Calculate(IEnumerable<Vector3> vertices )
        {
            var min = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            var max = new Vector3( float.MinValue, float.MinValue, float.MinValue );
            foreach ( var vertex in vertices )
            {
                min.X = Math.Min( min.X, vertex.X );
                min.Y = Math.Min( min.Y, vertex.Y );
                min.Z = Math.Min( min.Z, vertex.Z );

                max.X = Math.Max( max.X, vertex.X );
                max.Y = Math.Max( max.Y, vertex.Y );
                max.Z = Math.Max( max.Z, vertex.Z );
            }

            var center = min + max / 2f;

            var maxDistSq = 0.0f;
            foreach ( var vertex in vertices )
            {
                var distanceFromCenter = vertex - center;
                maxDistSq = Math.Max( maxDistSq, distanceFromCenter.LengthSquared() );
            }

            var sphereRadius = ( float )Math.Sqrt( maxDistSq );

            return new BoundingSphere( center, sphereRadius );
        }

        public static BoundingSphere Calculate( Vector3 min, Vector3 max )
        {
            var center = min + max / 2f;
            var sizeX = max.X - min.X;
            var sizeY = max.Y - min.Y;
            var sizeZ = max.Z - min.Z;
            var radius = Math.Max( sizeX, Math.Max( sizeY, sizeZ ) ) / 2f;
            return new BoundingSphere( center, radius );
        }

        public override bool Equals( object obj )
        {
            return obj is BoundingSphere sphere && Equals( sphere );
        }

        public bool Equals( BoundingSphere other )
        {
            return Center == other.Center && Radius == other.Radius;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 11;
                hash = hash * 33 + Center.GetHashCode();
                hash = hash * 33 + Radius.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"[{Center.X}, {Center.Y}, {Center.Z}] {Radius}";
        }
    }
}