using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.GC
{
    public class GCAssimpExporter : AssimpExporter
    {
        public static readonly GCAssimpExporter Default = new GCAssimpExporter();

        private struct MeshRenderState
        {
            // 0
            public ushort Param0Value1;
            public ushort Param0Value2;

            // 1
            public IndexAttributeFlags IndexFlags;

            // 2
            public ushort LightingParam1;
            public ushort LightingParam2;

            // 3
            public ushort Param3Value1;
            public ushort Param3Value2;

            // 4
            public BlendAlphaFlags BlendAlphaFlags;

            // 5
            public Color AmbientColor;

            // 6
            public ushort Param6Value1;
            public ushort Param6Value2;

            // 7
            public ushort Param7Value1;
            public ushort Param7Value2;

            // 8
            public ushort TextureId;
            public TileMode TileMode;

            // 9
            public ushort Param9Value1;
            public ushort Param9Value2;

            // 10
            public ushort MipMapParam1;
            public ushort MipMapParam2;
        }

        private struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Color Color;
            public Vector2<short> UV;
        }

        private static HashSet<MeshRenderState> sUniqueStates = new HashSet<MeshRenderState>();

        private Dictionary<MeshRenderState, int> mConvertedMaterialCache;

        public GCAssimpExporter()
        {
            Initialize( null );
        }

        protected override void Initialize( Node rootNode )
        {
            mConvertedMaterialCache = new Dictionary<MeshRenderState, int>();
        }

        protected override void ConvertGeometry( IGeometry iGeometry, ref Matrix4x4 nodeWorldTransform )
        {
            if ( iGeometry.Format != GeometryFormat.GC )
                throw new InvalidOperationException();

            var geometry = ( Geometry )iGeometry;
            var positionBuffer = ( VertexPositionBuffer ) geometry.VertexBuffers.FirstOrDefault( x => x.Type == VertexAttributeType.Position );
            var normalBuffer = ( VertexNormalBuffer )geometry.VertexBuffers.FirstOrDefault( x => x.Type == VertexAttributeType.Normal );
            var colorBuffer = ( VertexColorBuffer )geometry.VertexBuffers.FirstOrDefault( x => x.Type == VertexAttributeType.Color );
            var uvBuffer = ( VertexUVBuffer )geometry.VertexBuffers.FirstOrDefault( x => x.Type == VertexAttributeType.UV );

            if ( geometry.OpaqueMeshes != null && geometry.OpaqueMeshes.Count > 0 )
                ConvertMeshes( geometry.OpaqueMeshes, positionBuffer, normalBuffer, colorBuffer, uvBuffer );

            if ( geometry.TranslucentMeshes != null && geometry.TranslucentMeshes.Count > 0 )
                ConvertMeshes( geometry.TranslucentMeshes, positionBuffer, normalBuffer, colorBuffer, uvBuffer );
        }

        private static void ProcessMeshParameters(List<Param> parameters, ref MeshRenderState state)
        {
            foreach ( var param in parameters )
            {
                switch ( param.Type )
                {
                    case 0:
                        {
                            var param0 = ( UnknownParam )param;
                            state.Param0Value1 = param0.Value1;
                            state.Param0Value2 = param0.Value2;
                        }
                        break;

                    case MeshStateParamType.IndexAttributeFlags:
                        state.IndexFlags = ( ( GC.IndexAttributeFlagsParam )param ).Flags;
                        break;

                    case MeshStateParamType.Lighting:
                        {
                            var lightingParams = ( LightingParams )param;
                            state.LightingParam1 = lightingParams.Value1;
                            state.LightingParam2 = lightingParams.Value2;
                        }
                        break;

                    case ( MeshStateParamType )3:
                        {
                            var param3 = ( UnknownParam )param;
                            state.Param3Value1 = param3.Value1;
                            state.Param3Value2 = param3.Value2;
                        }
                        break;


                    case  MeshStateParamType.BlendAlpha:
                        {
                            var blendAlphaParam = ( BlendAlphaParam )param;
                            state.BlendAlphaFlags = blendAlphaParam.Flags;
                        }
                        break;

                    case MeshStateParamType.AmbientColor:
                        {
                            var ambientColorParam = ( AmbientColorParam )param;
                            state.AmbientColor = ambientColorParam.Color;
                        }
                        break;

                    case ( MeshStateParamType )6:
                        {
                            var param6 = ( UnknownParam )param;
                            state.Param6Value1 = param6.Value1;
                            state.Param6Value2 = param6.Value2;
                        }
                        break;

                    case ( MeshStateParamType )7:
                        {
                            var param7 = ( UnknownParam )param;
                            state.Param7Value1 = param7.Value1;
                            state.Param7Value2 = param7.Value2;
                        }
                        break;

                    case MeshStateParamType.Texture:
                        {
                            var textureParams = ( TextureParams )param;
                            state.TextureId = textureParams.TextureId;
                            state.TileMode = textureParams.TileMode;
                        }
                        break;

                    case ( MeshStateParamType )9:
                        {
                            var param9 = ( UnknownParam )param;
                            state.Param9Value1 = param9.Value1;
                            state.Param9Value2 = param9.Value2;
                        }
                        break;

                    case MeshStateParamType.MipMap:
                        {
                            var mipMapParams = ( MipMapParams )param;
                            state.MipMapParam1 = mipMapParams.Value1;
                            state.MipMapParam2 = mipMapParams.Value2;
                        }
                        break;

                    default:
                        Debugger.Break();
                        break;
                }
            }
        }

        private void ConvertMeshes( List<Mesh> meshes, VertexPositionBuffer positionBuffer, VertexNormalBuffer normalBuffer, VertexColorBuffer colorBuffer, VertexUVBuffer uvBuffer )
        {
            var state = new MeshRenderState();

            for ( var i = 0; i < meshes.Count; i++ )
            {
                var mesh = meshes[i];

                if ( mesh.Parameters != null && mesh.Parameters.Count > 0 )
                    ProcessMeshParameters( mesh.Parameters, ref state );

                var stateCopy = state;
                stateCopy.TextureId = 0;
                stateCopy.TileMode = 0;
                //stateCopy.IndexFlags = 0;
                stateCopy.AmbientColor = new Color();
                sUniqueStates.Add( stateCopy );

                var aiMesh      = new Assimp.Mesh();
                var vertexCache = new List<Vertex>();

                Debug.Assert( state.IndexFlags.HasFlag( IndexAttributeFlags.HasPosition ) ? positionBuffer != null : true );
                Debug.Assert( state.IndexFlags.HasFlag( IndexAttributeFlags.HasNormal ) ? normalBuffer != null : true );
                Debug.Assert( state.IndexFlags.HasFlag( IndexAttributeFlags.HasColor ) ? colorBuffer != null : true );
                Debug.Assert( state.IndexFlags.HasFlag( IndexAttributeFlags.HasUV ) ? uvBuffer != null : true );

                // Extract all vertices used by the triangles, and build a new vertex list 
                // with each vertex attribute clumped together
                var aiFace = new Assimp.Face();
                foreach ( var index in mesh.DisplayLists.SelectMany( x => x.ToTriangles() ) )
                {
                    var vertex = new Vertex();

                    if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasPosition ) )
                        vertex.Position = positionBuffer.Elements[index.PositionIndex];

                    if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasNormal ) )
                        vertex.Normal = normalBuffer.Elements[index.NormalIndex];

                    if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasColor ) )
                        vertex.Color = colorBuffer.Elements[index.ColorIndex];

                    if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasUV ) )
                        vertex.UV = uvBuffer.Elements[index.UVIndex];

                    // Find index of this vertex in the list in case it already exists
                    var vertexIndex = vertexCache.IndexOf( vertex );
                    if ( vertexIndex == -1 )
                    {
                        vertexIndex = vertexCache.Count;
                        vertexCache.Add( vertex );
                    }

                    aiFace.Indices.Add( vertexIndex );

                    if ( aiFace.IndexCount == 3 )
                    {
                        // Done with this face, move on to the next one
                        aiMesh.Faces.Add( aiFace );
                        aiFace = new Assimp.Face();
                    }
                }

                // Convert vertices
                aiMesh.Vertices.AddRange( vertexCache.Select( x => ToAssimp( x.Position ) ) );

                if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasNormal ) )
                    aiMesh.Normals.AddRange( vertexCache.Select( x => ToAssimp( x.Normal ) ) );

                if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasUV ) )
                    aiMesh.TextureCoordinateChannels[0].AddRange( vertexCache.Select( x => ToAssimp( UVCodec.Decode1023( x.UV ) ) ) );

                if ( state.IndexFlags.HasFlag( IndexAttributeFlags.HasColor ) )
                    aiMesh.VertexColorChannels[0].AddRange( vertexCache.Select( x => ToAssimp( x.Color ) ) );

                // Convert material
                if ( !mConvertedMaterialCache.TryGetValue( state, out var aiMaterialIndex ) )
                {
                    // Not in cache, so create a new one and add it
                    aiMaterialIndex = Scene.MaterialCount;
                    Scene.Materials.Add( CreateMaterial( Color.Gray, Color.Gray, Color.Gray, FormatTextureName( state.TextureId ), false, false,
                                                         state.TileMode.HasFlag( TileMode.MirrorU ),
                                                         state.TileMode.HasFlag( TileMode.MirrorV ),
                                                         state.BlendAlphaFlags.HasFlag( BlendAlphaFlags.UseAlpha ) ) );

                    mConvertedMaterialCache[ state ] = aiMaterialIndex;
                }

                aiMesh.MaterialIndex = aiMaterialIndex;

                // Add mesh to scene.
                Scene.Meshes.Add( aiMesh );
            }
        }
    }
}
