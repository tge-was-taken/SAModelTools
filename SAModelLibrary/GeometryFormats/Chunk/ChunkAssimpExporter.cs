using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Assimp converter & exporter for chunk geometry.
    /// </summary>
    public class ChunkAssimpExporter : AssimpExporter
    {
        /// <summary>
        /// Instance of the exporter with settings appropriate for animated models.
        /// </summary>
        public static readonly ChunkAssimpExporter Animated = new ChunkAssimpExporter
        {
            AttachMeshesToParentNode = false,
            RemoveNodes              = false
        };

        /// <summary>
        /// Instance of the exporter with settings appropriate for static models.
        /// </summary>
        public static readonly ChunkAssimpExporter Static = new ChunkAssimpExporter
        {
            AttachMeshesToParentNode = false,
            RemoveNodes              = true
        };

        private struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public List<Vector2> UVs;
            public Color Color;
            public List<VertexWeight> Weights;

            public Vertex( Vector3 position, Vector3 normal, Color color, int nodeIndex, float weight )
            {
                Position = position;
                Normal = normal;
                UVs = new List<Vector2>();
                Color = color;
                Weights = new List<VertexWeight> { new VertexWeight( nodeIndex, weight ) };
            }
        }

        private struct VertexWeight
        {
            public int NodeIndex;
            public float Weight;

            public VertexWeight( int nodeId, float weight )
            {
                NodeIndex = nodeId;
                Weight = weight;
            }
        }

        private struct TriangleVertexIndex
        {
            public int VertexIndex;
            public int UVIndex;
            public int MaterialIndex;
        }

        private struct Material
        {
            public Color Ambient;
            public Color Diffuse;
            public Color Specular;
            public float Exponent;
            public SrcAlphaOp SourceAlpha;
            public DstAlphaOp DestinationAlpha;
            public bool ClampU;
            public bool ClampV;
            public bool FlipU;
            public bool FlipV;
            public short TextureId;
            public bool SuperSample;
            public FilterMode FilterMode;
            public MipMapDAdjust MipMapDAdjust;
        }

        private List<Node> mNodes;
        private Dictionary<int, Vertex> mVertexCache;
        private Dictionary<int, (List<Chunk16> List, int Index)> mPolygonCache;
        private List<Material> mMaterials;
        private Dictionary<Material, int> mConvertedMaterialCache;

        public ChunkAssimpExporter()
        {
            Initialize( null );
        }

        protected override void Initialize( Node rootNode )
        {
            mNodes                  = rootNode?.EnumerateAllNodes().ToList();
            mVertexCache            = new Dictionary<int, Vertex>();
            mPolygonCache           = new Dictionary<int, (List<Chunk16> List, int Index)>();
            mMaterials              = new List<Material>();
            mConvertedMaterialCache = new Dictionary<Material, int>();
        }

        protected override void ConvertGeometry( IGeometry iGeometry, ref Matrix4x4 nodeWorldTransform )
        {
            if ( iGeometry.Format != GeometryFormat.Chunk )
                throw new NotSupportedException();

            var geometry = ( Geometry ) iGeometry;

            Matrix4x4.Invert( nodeWorldTransform, out var nodeWorldTransformInv );

            if ( geometry.VertexList != null && geometry.VertexList.Count > 0 )
            {
                // Process vertices
                ProcessVertexList( geometry.VertexList, ref nodeWorldTransform );
            }

            var triangleIndices = new List<TriangleVertexIndex>();
            if ( geometry.PolygonList != null && geometry.PolygonList.Count > 0 )
            {
                // Process polygon list, contains material & triangle data
                ProcessPolygonList( triangleIndices, 0, geometry.PolygonList );
            }

            // Build assimp mesh. Group the triangles by their material index.
            var meshes = triangleIndices.GroupBy( x => x.MaterialIndex );
            foreach ( var mesh in meshes )
            {
                var aiMesh = new Assimp.Mesh();

                // Take the triangles that belong to this material and extract the referenced vertices
                // Obviously the indices won't make sense anymore because we're splitting the meshes up into parts with only 1 material
                // Soo just take it all apart and let Assimp handle regenerating the vertex cache
                var meshTriangles = mesh.ToList();
                var vertexWeights = new List<List<VertexWeight>>();
                for ( int i = 0; i < meshTriangles.Count; i += 3 )
                {
                    var aiFace = new Assimp.Face();

                    for ( int j = 0; j < 3; j++ )
                    {
                        var index = i + j;
                        var cachedVertex = mVertexCache[meshTriangles[index].VertexIndex];
                        var uvIndex = meshTriangles[index].UVIndex;
                        Vector2 uv = new Vector2();
                        if ( uvIndex < cachedVertex.UVs.Count )
                            uv = cachedVertex.UVs[uvIndex];

                        // Need to convert vertex positions and normals back to model space
                        aiMesh.Vertices.Add( ToAssimp( Vector3.Transform( cachedVertex.Position, nodeWorldTransformInv ) ) );
                        aiMesh.Normals.Add( ToAssimp( Vector3.TransformNormal( cachedVertex.Normal, nodeWorldTransformInv ) ) );

                        aiMesh.TextureCoordinateChannels[0].Add( ToAssimp( uv ) );
                        aiMesh.VertexColorChannels[0].Add( ToAssimp( cachedVertex.Color ) );

                        // We need to do some processing on the weights so add them to a seperate list
                        vertexWeights.Add( cachedVertex.Weights );

                        // RIP cache efficiency
                        aiFace.Indices.Add( index );
                    }

                    aiMesh.Faces.Add( aiFace );
                }

                if ( !RemoveNodes )
                {
                    // Convert vertex weights
                    var aiBoneMap = new Dictionary<int, Assimp.Bone>();
                    for ( int i = 0; i < vertexWeights.Count; i++ )
                    {
                        for ( int j = 0; j < vertexWeights[i].Count; j++ )
                        {
                            var vertexWeight = vertexWeights[i][j];

                            if ( !aiBoneMap.TryGetValue( vertexWeight.NodeIndex, out var aiBone ) )
                            {
                                aiBone = aiBoneMap[vertexWeight.NodeIndex] = new Assimp.Bone
                                {
                                    Name = FormatNodeName( vertexWeight.NodeIndex )
                                };

                                // Offset matrix: difference between world transform of weighted bone node and the world transform of the mesh's parent node
                                Matrix4x4.Invert( mNodes[vertexWeight.NodeIndex].WorldTransform * nodeWorldTransformInv, out var offsetMatrix );
                                aiBone.OffsetMatrix = ToAssimp( offsetMatrix );
                            }

                            // Assimps way of storing weights is not very efficient
                            aiBone.VertexWeights.Add( new Assimp.VertexWeight( i, vertexWeight.Weight ) );
                        }
                    }

                    aiMesh.Bones.AddRange( aiBoneMap.Values );
                }

                // Convert material if necessary
                var material = mMaterials[mesh.Key];
                if ( !mConvertedMaterialCache.TryGetValue( material, out var aiMaterialIndex ) )
                {
                    aiMaterialIndex = Scene.Materials.Count;

                    // Create material
                    var textureName = FormatTextureName( material.TextureId );
                    var textureFileName = $"{textureName}.png";
                    var materialName = FormatMaterialName( textureName, Scene.Materials.Count( x => x.TextureDiffuse.FilePath == textureFileName ) );
                    var aiMaterial = new Assimp.Material
                    {
                        Name = materialName,
                        ColorDiffuse = ToAssimp( material.Diffuse ),
                        ColorAmbient = ToAssimp( material.Ambient ),
                        ColorSpecular = ToAssimp( material.Specular ),
                        Shininess = 0,
                        ShininessStrength = 0,
                        Reflectivity = 0,
                        TextureDiffuse = new Assimp.TextureSlot
                        {
                            TextureType = Assimp.TextureType.Diffuse,
                            FilePath = textureFileName,
                            WrapModeU =
                                material.ClampU ? Assimp.TextureWrapMode.Clamp :
                                material.FlipU ? Assimp.TextureWrapMode.Mirror :
                                                  Assimp.TextureWrapMode.Wrap,
                            WrapModeV =
                                material.ClampV ? Assimp.TextureWrapMode.Clamp :
                                material.FlipV ? Assimp.TextureWrapMode.Mirror :
                                                  Assimp.TextureWrapMode.Wrap,
                        }
                    };
                    Scene.Materials.Add( aiMaterial );
                    mConvertedMaterialCache.Add( material, aiMaterialIndex );
                }

                aiMesh.MaterialIndex = aiMaterialIndex;
                Scene.Meshes.Add( aiMesh );
            }
        }

        private void ProcessVertexList( List<VertexChunk> vertexList, ref Matrix4x4 nodeWorldTransform )
        {
            foreach ( var chunk in vertexList )
            {
                switch ( chunk.Type )
                {
                    case ChunkType.VertexXYZ:
                        {
                            var vertexChunk = ( VertexXYZChunk )chunk;
                            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                            {
                                // Transform and store vertex in cache
                                var vertex = vertexChunk.Vertices[i];
                                var position = Vector3.Transform( vertex.Position, nodeWorldTransform );
                                var cacheVertex = new Vertex( position, new Vector3(), Color.White, NodeIndex, 1f );
                                mVertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                            }
                        }
                        break;

                    case ChunkType.VertexN:
                        {
                            var vertexChunk = ( VertexNChunk )chunk;
                            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                            {
                                // Transform and store vertex in cache
                                var vertex = vertexChunk.Vertices[i];
                                var position = Vector3.Transform( vertex.Position, nodeWorldTransform );
                                var normal = Vector3.TransformNormal( vertex.Normal, nodeWorldTransform );
                                var cacheVertex = new Vertex( position, normal, Color.White, NodeIndex, 1f );
                                mVertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                            }
                        }
                        break;

                    case ChunkType.VertexNNF:
                        {
                            var vertexChunk = ( VertexNNFChunk )chunk;
                            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                            {
                                var vertex = vertexChunk.Vertices[i];

                                // Transform vertex
                                var weightByte = vertex.NinjaFlags >> 16;
                                var weight = weightByte * ( 1f / 255f );
                                var position = Vector3.Transform( vertex.Position, nodeWorldTransform ) * weight;
                                var normal = Vector3.TransformNormal( vertex.Normal, nodeWorldTransform ) * weight;

                                // Store vertex in cache
                                var vertexId = vertex.NinjaFlags & 0x0000FFFF;
                                var vertexCacheId = ( int )( vertexChunk.BaseIndex + vertexId );

                                if ( chunk.WeightStatus == WeightStatus.Start || !mVertexCache.ContainsKey( vertexCacheId ) )
                                {
                                    // Add new vertex to cache
                                    var cacheVertex = new Vertex( position, normal, Color.White, NodeIndex, weight );
                                    mVertexCache[vertexCacheId] = cacheVertex;
                                }
                                else
                                {
                                    // Update cached vertex
                                    var cacheVertex = mVertexCache[vertexCacheId];
                                    cacheVertex.Position += position;
                                    cacheVertex.Normal += normal;
                                    cacheVertex.Weights.Add( new VertexWeight( NodeIndex, weight ) );
                                    mVertexCache[vertexCacheId] = cacheVertex;

                                }
                            }
                        }
                        break;

                    case ChunkType.VertexD8888:
                        {
                            var vertexChunk = ( VertexD8888Chunk )chunk;
                            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                            {
                                // Transform and store vertex in cache
                                var vertex = vertexChunk.Vertices[i];
                                var position = Vector3.Transform( vertex.Position, nodeWorldTransform );
                                var cacheVertex = new Vertex( position, Vector3.Zero, vertex.Diffuse, NodeIndex, 1f );
                                mVertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                            }
                        }
                        break;

                    case ChunkType.VertexND8888:
                        {
                            var vertexChunk = ( VertexND8888Chunk )chunk;
                            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                            {
                                // Transform and store vertex in cache
                                var vertex = vertexChunk.Vertices[i];
                                var position = Vector3.Transform( vertex.Position, nodeWorldTransform );
                                var normal = Vector3.TransformNormal( vertex.Normal, nodeWorldTransform );
                                var cacheVertex = new Vertex( position, normal, vertex.Diffuse, NodeIndex, 1f );
                                mVertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                            }
                        }
                        break;

                    case ChunkType.VertexN32:
                        {
                            var vertexChunk = ( VertexN32Chunk )chunk;
                            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                            {
                                // Transform and store vertex in cache
                                var vertex = vertexChunk.Vertices[i];
                                var position = Vector3.Transform( vertex.Position, nodeWorldTransform );
                                var decodedNormal = NormalCodec.Decode( vertex.Normal );
                                var normal = Vector3.TransformNormal( decodedNormal, nodeWorldTransform );
                                var cacheVertex = new Vertex( position, normal, Color.White, NodeIndex, 1f );
                                mVertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                            }
                        }
                        break;

                    default:
                        Debugger.Break();
                        break;
                }
            }
        }

        private void ProcessPolygonList( List<TriangleVertexIndex> triangleIndices, int index, List<Chunk16> list, bool wasCached = false )
        {
            var material = new Material
            {
                SourceAlpha = SrcAlphaOp.Src,
                DestinationAlpha = DstAlphaOp.InverseDst,
                Ambient = Color.Gray,
                Diffuse = Color.Gray,
                Specular = Color.White
            };

            for ( var chunkIndex = index; chunkIndex < list.Count; chunkIndex++ )
            {
                var chunk = list[chunkIndex];

                switch ( chunk.Type )
                {
                    case ChunkType.CachePolygonList:
                        {
                            if ( wasCached )
                                throw new InvalidOperationException( "CachePolygonList in cached polygon list" );

                            var cacheChunk = ( CachePolygonListChunk )chunk;
                            mPolygonCache[cacheChunk.CacheIndex] = (list, chunkIndex + 1);
                        }
                        break;

                    case ChunkType.DrawPolygonList:
                        {
                            if ( wasCached )
                                throw new InvalidOperationException( "DrawPolygonList in cached polygon list" );

                            var drawChunk = ( DrawPolygonListChunk )chunk;
                            if ( !mPolygonCache.ContainsKey( drawChunk.CacheIndex ) )
                                continue;

                            var cachedList = mPolygonCache[drawChunk.CacheIndex];
                            ProcessPolygonList( triangleIndices, cachedList.Index, cachedList.List, true );
                        }
                        break;

                    case ChunkType.MaterialAmbient:
                    case ChunkType.MaterialAmbient2:
                        {
                            var materialChunk = ( MaterialAmbientChunk )chunk;
                            material.Ambient = materialChunk.Ambient;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MaterialAmbientSpecular:
                    case ChunkType.MaterialAmbientSpecular2:
                        {
                            var materialChunk = ( MaterialAmbientSpecularChunk )chunk;
                            material.Ambient = materialChunk.Ambient;
                            material.Specular = materialChunk.Specular;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MaterialBump:
                        break;

                    case ChunkType.MaterialDiffuse:
                    case ChunkType.MaterialDiffuse2:
                        {
                            var materialChunk = ( MaterialDiffuseChunk )chunk;
                            material.Diffuse = materialChunk.Diffuse;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MaterialDiffuseAmbient:
                    case ChunkType.MaterialDiffuseAmbient2:
                        {
                            var materialChunk = ( MaterialDiffuseAmbientChunk )chunk;
                            material.Ambient = materialChunk.Ambient;
                            material.Diffuse = materialChunk.Diffuse;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MaterialDiffuseAmbientSpecular:
                    case ChunkType.MaterialDiffuseAmbientSpecular2:
                        {
                            var materialChunk = ( MaterialDiffuseAmbientSpecularChunk )chunk;
                            material.Diffuse = materialChunk.Diffuse;
                            material.Ambient = materialChunk.Ambient;
                            material.Specular = materialChunk.Specular;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MaterialDiffuseSpecular:
                    case ChunkType.MaterialDiffuseSpecular2:
                        {
                            var materialChunk = ( MaterialDiffuseSpecularChunk )chunk;
                            material.Diffuse = materialChunk.Diffuse;
                            material.Specular = materialChunk.Specular;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MaterialSpecular:
                    case ChunkType.MaterialSpecular2:
                        {
                            var materialChunk = ( MaterialSpecularChunk )chunk;
                            material.Specular = materialChunk.Specular;
                            material.SourceAlpha = materialChunk.SourceAlpha;
                            material.DestinationAlpha = materialChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.MipmapDAdjust:
                        break;

                    case ChunkType.SpecularExponent:
                        {
                            var specularExponentChunk = ( SpecularExponentChunk )chunk;
                            material.Exponent = specularExponentChunk.Exponent;
                        }
                        break;

                    case ChunkType.BlendAlpha:
                        {
                            var blendAlphaChunk = ( BlendAlphaChunk )chunk;
                            material.SourceAlpha = blendAlphaChunk.SourceAlpha;
                            material.DestinationAlpha = blendAlphaChunk.DestinationAlpha;
                        }
                        break;

                    case ChunkType.TextureId:
                    case ChunkType.TextureId2:
                        {
                            var textureIdChunk = ( TextureIdChunk )chunk;
                            material.TextureId = textureIdChunk.Id;
                            material.ClampU = textureIdChunk.ClampU;
                            material.ClampV = textureIdChunk.ClampV;
                            material.FlipU = textureIdChunk.FlipU;
                            material.FlipV = textureIdChunk.FlipV;
                            material.SuperSample = textureIdChunk.SuperSample;
                            material.FilterMode = textureIdChunk.FilterMode;
                            material.MipMapDAdjust = textureIdChunk.MipMapDAdjust;
                        }
                        break;

                    case ChunkType.StripUVN:
                        {
                            var stripChunk = ( StripUVNChunk )chunk;
                            var stripTriangleIndices = stripChunk.ToTriangles();
                            foreach ( var stripIndex in stripTriangleIndices )
                            {
                                if ( !mVertexCache.ContainsKey( stripIndex.Index ) )
                                {
                                    throw new InvalidOperationException( "Strip referenced vertex that is not in the vertex cache" );
                                    //mVertexCache.Add( stripIndex.Index,
                                    //                  new Vertex() { UVs = new List<Vector2>(), Weights = new List<VertexWeight>() } );
                                }

                                var cachedVertex = mVertexCache[stripIndex.Index];
                                var uv = UVCodec.Decode255( stripIndex.UV );
                                int uvIndex = cachedVertex.UVs.IndexOf( uv );
                                if ( uvIndex == -1 )
                                {
                                    uvIndex = cachedVertex.UVs.Count;
                                    cachedVertex.UVs.Add( uv );
                                }

                                var materialIndex = mMaterials.IndexOf( material );
                                if ( materialIndex == -1 )
                                {
                                    materialIndex = mMaterials.Count;
                                    mMaterials.Add( material );
                                }

                                triangleIndices.Add( new TriangleVertexIndex
                                {
                                    VertexIndex = stripIndex.Index,
                                    UVIndex = uvIndex,
                                    MaterialIndex = materialIndex
                                } );
                                mVertexCache[stripIndex.Index] = cachedVertex;
                            }
                        }
                        break;

                    case ChunkType.Strip:
                        {
                            var stripChunk = ( StripChunk )chunk;
                            var stripTriangleIndices = stripChunk.ToTriangles();
                            foreach ( var stripIndex in stripTriangleIndices )
                            {
                                if ( !mVertexCache.ContainsKey( stripIndex.Index ) )
                                    throw new InvalidOperationException( "Strip referenced vertex that is not in the vertex cache" );

                                var materialIndex = mMaterials.IndexOf( material );
                                if ( materialIndex == -1 )
                                {
                                    materialIndex = mMaterials.Count;
                                    mMaterials.Add( material );
                                }

                                triangleIndices.Add( new TriangleVertexIndex
                                {
                                    VertexIndex = stripIndex.Index,
                                    UVIndex = 0,
                                    MaterialIndex = materialIndex
                                } );
                            }
                        }
                        break;

                    default:
                        Debugger.Break();
                        break;
                }

                if ( chunk.Type == ChunkType.CachePolygonList )
                {
                    break;
                }
            }
        }
    }
}
