using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using FraGag.Compression;
using NvTriStripDotNet;
using PuyoTools.Modules.Archive;
using PuyoTools.Modules.Texture;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;
using Color = SAModelLibrary.Maths.Color;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    public class ChunkAssimpImporter
    {
        private static readonly NvStripifier sStripifier = new NvStripifier
        {
            StitchStrips = false,
            UseRestart = false,
        };

        public static readonly ChunkAssimpImporter Animated = new ChunkAssimpImporter();

        private class NodeBuildInfo
        {
            public Assimp.Node AiNode;
            public Node Node;
            public int Index;
        }

        private class GeometryBuildInfo
        {
            public NodeBuildInfo TargetNodeInfo;
            public List<Vertex> UnweightedVertices;
            public List<Vertex> WeightedVertices;
            public List<MeshBuildInfo> Meshes;
            public List<Vertex> ExternalWeightedVertices;

            public bool IsSupplementary => Meshes == null && ExternalWeightedVertices.Count > 0;
        }

        private class Vertex : IEquatable<Vertex>
        {
            public int Id;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;
            public List<VertexWeight> Weights;

            public bool Equals( Vertex other )
            {
                return Position.Equals( other.Position ) && Normal.Equals( other.Normal ) && Weights.SequenceEqual( other.Weights ) &&
                       UV.Equals( other.UV );
            }

            public override bool Equals( object obj )
            {
                if ( obj is null )
                {
                    return false;
                }

                return obj is Vertex vertex && Equals( vertex );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Position.GetHashCode();
                    hashCode = ( hashCode * 397 ) ^ Normal.GetHashCode();
                    hashCode = ( hashCode * 397 ) ^ Weights.Count;
                    Weights.ForEach( x => hashCode ^= x.NodeIndex + ( int )( x.Weight * 100f ) );
                    hashCode = ( hashCode * 397 ) ^ UV.GetHashCode();
                    return hashCode;
                }
            }
        }

        private struct VertexWeight
        {
            public float Weight;
            public int NodeIndex;

            public override string ToString()
            {
                return $"{NodeIndex} {Weight}";
            }
        }

        private class MaterialBuildInfo
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

        private class IndexBuildInfo
        {
            public ushort VertexIndex;
            public Vector2 UV;

            public override string ToString()
            {
                return $"{VertexIndex} {UV}";
            }
        }

        private class MeshBuildInfo
        {
            public List<IndexBuildInfo> UnweightedIndices;
            public List<IndexBuildInfo> WeightedIndices;
            public MaterialBuildInfo Material;
        }

        private static bool sDisableWeights = false;

        public ChunkAssimpImporter()
        {
        }

        private static List<Node> sOriginalNodes =
            new SA2.ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" )[16]
                .RootNode.EnumerateAllNodes().ToList();

        public Node Import( string path, string texturePakPath = null )
        {
            // Import scene.
            var aiScene = AssimpHelper.ImportScene( path, true );

            // Find the node root of the animated hierarchy. This assumes that only 1 hierarchy exists, and any other
            // nodes that part of the scene root are mesh nodes.
            var aiRootNode = FindHierarchyRootNode( aiScene.RootNode );

            // Convert the nodes in the animated hierarchy from Assimps format, and also keep track of additional info.
            var convertedNodes = ConvertNodes( aiRootNode );

            // Find the mesh nodes within the scene. A mesh node is a node with 1 or more meshes attached to it.
            var meshNodes = FindMeshNodes( aiScene.RootNode );

            // Map each mesh node to a target node in the animated hierarchy. This is because we can't add extra nodes
            // to the hierarchy, so we must determine the most appropriate node to add the mesh to.
            var mappedMeshes = MapMeshNodesToTargetNodes( meshNodes, aiScene.Meshes, convertedNodes );

            // Convert the materials and textures
            var materialBuildInfos =
                BuildProceduralMaterials( Path.GetDirectoryName( path ), aiScene.Materials, texturePakPath );

            // Take the mapped meshes and merge them into 1 geometry per target node
            // Also builds any supplementary geometries have have to be added to store additional weighted vertex info
            var geometryBuildInfos = BuildProceduralGeometries( mappedMeshes, convertedNodes, materialBuildInfos );

            var weightedVertexIdLookup = new Dictionary<Vertex, int>();
            var nextWeightedVertexId = 4095;

            foreach ( var geometryBuildInfo in geometryBuildInfos )
            {
                // Get target node inverse world transform to transform the vertices the target node's local space
                Matrix4x4.Invert( geometryBuildInfo.TargetNodeInfo.Node.WorldTransform, out var targetNodeInvWorldTransform );

                // Start building geometry
                var geometry = new Geometry();

                if ( !geometryBuildInfo.IsSupplementary && geometryBuildInfo.UnweightedVertices.Count > 0 )
                {
                    // Add unweighted vertices
                    geometry.VertexList.Add( new VertexNChunk( geometryBuildInfo.UnweightedVertices.Select( x => new VertexN
                    {
                        Position = Vector3.Transform( x.Position, targetNodeInvWorldTransform ),
                        Normal = Vector3.TransformNormal( x.Normal, targetNodeInvWorldTransform )
                    } ).ToArray() )
                    { BaseIndex = geometryBuildInfo.UnweightedVertices[0].Id } );
                }

                if ( !sDisableWeights )
                {
                    BuildWeightedVertexChunks( geometry, geometryBuildInfo, ref targetNodeInvWorldTransform, weightedVertexIdLookup, ref nextWeightedVertexId );
                }

                if ( !geometryBuildInfo.IsSupplementary )
                {
                    BuildPolygonList( geometry, geometryBuildInfo, weightedVertexIdLookup );
                }

                geometryBuildInfo.TargetNodeInfo.Node.Geometry = geometry;
            }

            foreach ( var nodeInfo in convertedNodes )
            {
                nodeInfo.Node.OptimizeFlags();
            }

            var rootNode = convertedNodes[0];
            return rootNode.Node;
        }

        private static List<MaterialBuildInfo> BuildProceduralMaterials( string baseDirectory, List<Assimp.Material> aiMaterials, string texturePakPath )
        {
            var textureArchive = new GvmArchive();
            var textureArchiveStream = new MemoryStream();
            var textureArchiveWriter = ( GvmArchiveWriter ) textureArchive.Create( textureArchiveStream );
            var textureIdLookup = new Dictionary<string, int>( StringComparer.InvariantCultureIgnoreCase );

            if ( texturePakPath != null && File.Exists( texturePakPath ) )
            {
                var extension = Path.GetExtension( texturePakPath );
                var fileStream = ( Stream )File.OpenRead( texturePakPath );
                if ( extension.Equals( ".prs", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    try
                    {
                        var decompressedFileStream = new MemoryStream();
                        Prs.Decompress( fileStream, decompressedFileStream );
                        fileStream.Dispose();
                        fileStream = decompressedFileStream;
                    }
                    catch ( Exception )
                    {
                        // Not compressed
                    }

                    fileStream.Position = 0;
                }

                var existingTextureArchive = new GvmArchive();
                var existingTextureArchiveReader = ( GvmArchiveReader ) existingTextureArchive.Open( fileStream );
                for ( var i = 0; i < existingTextureArchiveReader.Entries.Count; i++ )
                {
                    var entry = existingTextureArchiveReader.Entries[i];

                    // Make copy of entry stream
                    var entryStreamCopy = new MemoryStream();
                    entry.Open().CopyTo( entryStreamCopy );
                    entryStreamCopy.Position = 0;

                    var texture = new VrSharp.GvrTexture.GvrTexture( entryStreamCopy );
                    Console.WriteLine( texture.GlobalIndex );
                    entryStreamCopy.Position = 0;

                    // Clean entry name from the added extension
                    var entryName = Path.ChangeExtension( entry.Name, null );

                    textureArchiveWriter.CreateEntry( entryStreamCopy, entryName );
                    textureIdLookup[entryName] = i;
                }
            }

            var materials = new List<MaterialBuildInfo>();

            foreach ( var aiMaterial in aiMaterials )
            {
                var textureName = Path.GetFileNameWithoutExtension( aiMaterial.TextureDiffuse.FilePath ).ToLowerInvariant();
                if ( !textureIdLookup.TryGetValue( textureName, out var textureId ) )
                {
                    textureId = textureIdLookup[textureName] = textureIdLookup.Count;
                    var texturePath = Path.GetFullPath( Path.Combine( baseDirectory, aiMaterial.TextureDiffuse.FilePath ) );
                    if ( File.Exists( texturePath ) )
                    {
                        // Convert texture
                        var texture = new GvrTexture { GlobalIndex = ( uint )( 1 + textureId  ) };

                        var textureStream = new MemoryStream();
                        var textureBitmap = new Bitmap( texturePath );
                        texture.Write( textureBitmap, textureStream );
                        textureStream.Position = 0;

                        // Add it
                        textureArchiveWriter.CreateEntry( textureStream, textureName );
                    }
                }

                var material = new MaterialBuildInfo
                {
                    Ambient = AssimpHelper.FromAssimp( aiMaterial.ColorAmbient ),
                    Diffuse = AssimpHelper.FromAssimp( aiMaterial.ColorDiffuse ),
                    Specular = AssimpHelper.FromAssimp( aiMaterial.ColorSpecular ),
                    ClampU = aiMaterial.TextureDiffuse.WrapModeU == Assimp.TextureWrapMode.Clamp,
                    ClampV = aiMaterial.TextureDiffuse.WrapModeV == Assimp.TextureWrapMode.Clamp,
                    FlipU = aiMaterial.TextureDiffuse.WrapModeU == Assimp.TextureWrapMode.Mirror,
                    FlipV = aiMaterial.TextureDiffuse.WrapModeV == Assimp.TextureWrapMode.Mirror,
                    DestinationAlpha = DstAlphaOp.InverseDst,
                    Exponent = 0,
                    FilterMode = FilterMode.Trilinear,
                    MipMapDAdjust = MipMapDAdjust.D050,
                    SourceAlpha = SrcAlphaOp.Src,
                    SuperSample = false,
                    TextureId = (short)textureId,
                };

                materials.Add( material );
            }

            // Write texture archive to file
            textureArchiveWriter.Flush();
            textureArchiveStream.Position = 0;

            if ( texturePakPath != null )
            {
                // Compress it.
                var textureArchivePrsStream = new MemoryStream();
                Prs.Compress( textureArchiveStream, textureArchivePrsStream );

                // Save compressed file.
                textureArchivePrsStream.Position = 0;
                using ( var outFile = File.Create( texturePakPath ) )
                    textureArchivePrsStream.CopyTo( outFile );

            }

            return materials;
        }

        private static void BuildPolygonList( Geometry geometry, GeometryBuildInfo geometryBuildInfo, Dictionary<Vertex, int> weightedVertexIdLookup )
        {
            var currentMaterial = default( MaterialBuildInfo );
            var geometryWeightedVertexIdLookup = geometryBuildInfo.WeightedVertices.Select( x => weightedVertexIdLookup[x] ).ToList();

            for ( var i = 0; i < geometryBuildInfo.Meshes.Count; i++ )
            {
                var mesh = geometryBuildInfo.Meshes[i];

                // Add material parameters
                AddChangedMaterialParameters( geometry.PolygonList, mesh.Material, currentMaterial, i == 0 );
                currentMaterial = mesh.Material;

                if ( mesh.UnweightedIndices.Count > 0 )
                {
                    var unweightedStrips = GenerateStrips( mesh.UnweightedIndices );

                    geometry.PolygonList.Add( new StripUVNChunk
                    {
                        Strips = unweightedStrips,
                    } );
                }

                if ( mesh.WeightedIndices.Count > 0 )
                {
                    var weightedStrips = GenerateStrips( mesh.WeightedIndices );

                    foreach ( var strip in weightedStrips )
                    {
                        for ( var j = 0; j < strip.Indices.Length; j++ )
                        {
                            var index = strip.Indices[j];
                            strip.Indices[j].Index = ( ushort )geometryWeightedVertexIdLookup[index.Index];
                        }
                    }

                    geometry.PolygonList.Add( new StripUVNChunk
                    {
                        Strips = weightedStrips,
                    } );
                }
            }
        }

        private static void BuildWeightedVertexChunks( Geometry geometry, GeometryBuildInfo geometryBuildInfo, ref Matrix4x4 targetNodeInvWorldTransform, Dictionary<Vertex, int> weightedVertexIdLookup, ref int nextWeightedVertexId )
        {
            Debug.Assert( !sDisableWeights );

            if ( !geometryBuildInfo.IsSupplementary && geometryBuildInfo.WeightedVertices.Count > 0 )
            {
                BuildWeightedVertexChunk( geometry.VertexList, geometryBuildInfo.WeightedVertices, weightedVertexIdLookup,
                                        ref nextWeightedVertexId,
                                        geometryBuildInfo.TargetNodeInfo.Index, ref targetNodeInvWorldTransform, WeightStatus.End );
            }

            if ( geometryBuildInfo.ExternalWeightedVertices.Count > 0 )
            {
                // Add external weighted vertices (vertices that belong to other geometry)

                // Get 'start' weights. These are vertices whose first weight starts at the target node.
                var startWeightedVertices =
                    geometryBuildInfo.ExternalWeightedVertices.Where( x => x.Weights[0].NodeIndex == geometryBuildInfo.TargetNodeInfo.Index )
                                     .ToList();

                if ( startWeightedVertices.Count > 0 )
                {
                    BuildWeightedVertexChunk( geometry.VertexList, startWeightedVertices, weightedVertexIdLookup,
                                            ref nextWeightedVertexId,
                                            geometryBuildInfo.TargetNodeInfo.Index, ref targetNodeInvWorldTransform,
                                            WeightStatus.Start );
                }

                // Get 'middle' weights. That is vertices that have a 3 weights of which the 2nd weight is a weight 
                // to the target node
                var middleWeightedVertices =
                    geometryBuildInfo.ExternalWeightedVertices
                                     .Where( x => x.Weights.Count == 3 && x.Weights[1].NodeIndex ==
                                                  geometryBuildInfo.TargetNodeInfo.Index )
                                     .ToList();

                if ( middleWeightedVertices.Count > 0 )
                {
                    BuildWeightedVertexChunk( geometry.VertexList, middleWeightedVertices, weightedVertexIdLookup,
                                            ref nextWeightedVertexId,
                                            geometryBuildInfo.TargetNodeInfo.Index, ref targetNodeInvWorldTransform,
                                            WeightStatus.Middle );
                }
            }
        }

        private static void BuildWeightedVertexChunk( List<VertexChunk> vertexList, List<Vertex> vertices, Dictionary<Vertex, int> weightedVertexIdLookup,
                                                      ref int nextWeightedVertexId, int targetNodeIndex, ref Matrix4x4 targetNodeInvWorldTransform, WeightStatus weightStatus )
        {
            Debug.Assert( !sDisableWeights );

            var nnfVertices = new VertexNNF[vertices.Count];
            for ( int i = 0; i < vertices.Count; i++ )
            {
                var vertex = vertices[i];
                if ( !vertex.Weights.Any( x => x.NodeIndex == targetNodeIndex ) )
                {
                    // TODO: this vertex was part of the same triangle another weighted vertex was in
                    // to make this work properly I have to know the id of this vertex beforehand
                    //nnfVertices[i].Position = Vector3.Transform( vertex.Position, targetNodeInvWorldTransform );
                    //nnfVertices[i].Normal   = Vector3.TransformNormal( vertex.Normal, targetNodeInvWorldTransform );
                    //var weightByte = 255;
                    //nnfVertices[i].NinjaFlags = ( uint )( weightByte << 16 | nextWeightedVertexId++ & 0xFFFF );
                    //Console.WriteLine( $"{nextWeightedVertexId}" );

                    //if ( !weightedVertexIdLookup.TryGetValue( vertex, out var vertexId ) )
                    //{
                    //    weightedVertexIdLookup[vertex] = nextWeightedVertexId--;
                    //    vertexId = nextWeightedVertexId--;
                    //}

                    if ( !weightedVertexIdLookup.TryGetValue( vertex, out var vertexId ) )
                    {
                        vertexId = nextWeightedVertexId--;
                        weightedVertexIdLookup[vertex] = vertexId;
                    }


                    nnfVertices[i].Position = Vector3.Transform( vertex.Position, targetNodeInvWorldTransform );
                    nnfVertices[i].Normal = Vector3.TransformNormal( vertex.Normal, targetNodeInvWorldTransform );

                    // Pack 'er up
                    nnfVertices[i].NinjaFlags = ( uint )( 0 << 16 | vertexId & 0xFFFF );
                }
                else
                {
                    if ( !weightedVertexIdLookup.TryGetValue( vertex, out var vertexId ) )
                    {
                        vertexId = nextWeightedVertexId--;
                        weightedVertexIdLookup[vertex] = vertexId;
                    }

                    nnfVertices[i].Position = Vector3.Transform( vertex.Position, targetNodeInvWorldTransform );
                    nnfVertices[i].Normal = Vector3.TransformNormal( vertex.Normal, targetNodeInvWorldTransform );

                    // Pack 'er up
                    var targetNodeWeight = vertex.Weights.First( x => x.NodeIndex == targetNodeIndex );
                    var weightByte = ( byte )( targetNodeWeight.Weight * 255f );
                    nnfVertices[i].NinjaFlags = ( uint )( weightByte << 16 | vertexId & 0xFFFF );
                }
            }

            vertexList.Add( new VertexNNFChunk( nnfVertices ) { WeightStatus = weightStatus } );
        }

        private class VertexPositionEqualityComparer : IEqualityComparer<Vector3>
        {
            public const float Threshold = 0.01f;

            public bool Equals( Vector3 x, Vector3 y )
            {
                var delta = x - y;
                return IsDeltaWithinThreshold( delta.X ) && IsDeltaWithinThreshold( delta.Y ) && IsDeltaWithinThreshold( delta.Z );
            }

            private bool IsDeltaWithinThreshold( float value )
            {
                return value > -Threshold && value < Threshold;
            }

            public int GetHashCode( Vector3 obj )
            {
                throw new NotSupportedException();
            }
        }

        private static List<GeometryBuildInfo> BuildProceduralGeometries( List<(Assimp.Node ParentNode, NodeBuildInfo TargetNode, Assimp.Mesh Mesh)> mappedMeshes,
                                                                          List<NodeBuildInfo> convertedNodes, List<MaterialBuildInfo> materials )
        {
            // TODO: maybe weld the vertices first?

            var geometryBuildInfos = new List<GeometryBuildInfo>();
            var vertexId = 0;

            // Process each mapped mesh and start building some info to construct geometries
            foreach ( var targetNodeMeshGroup in mappedMeshes.GroupBy( x => x.TargetNode ) )
            {
                // Build procedural geometry
                var unweightedVertices = new List<Vertex>();
                var weightedVertices = new List<Vertex>();
                var baseVertexId = vertexId;

                // Need to seperate vertices that have 1 weight vs ones that have blend weights
                var weightedVertexLookup = new Dictionary<int, Vertex>();

                // Generate procedural mesh build info
                var meshBuildInfos = new List<MeshBuildInfo>();

                foreach ( var (parentNode, _, mesh) in targetNodeMeshGroup )
                {
                    // Convert the vertex data
                    var vertices = ConvertVertices( mesh, parentNode, convertedNodes, ref vertexId );

                    // Gather the triangle indices
                    var unweightedIndices = new List<IndexBuildInfo>();
                    var weightedIndices = new List<IndexBuildInfo>();
                    var hasWeights = !sDisableWeights && weightedVertexLookup.Count > 0;

                    foreach ( var face in mesh.Faces )
                    {
                        if ( hasWeights && face.Indices.Any( x => weightedVertexLookup.ContainsKey( x ) ) )
                        {
                            RemapFaceIndices( face, vertices, weightedVertices, weightedIndices, baseVertexId  );
                        }
                        else
                        {
                            RemapFaceIndices( face, vertices, unweightedVertices, unweightedIndices, baseVertexId );
                        }
                    }

                    meshBuildInfos.Add( new MeshBuildInfo
                    {
                        UnweightedIndices = unweightedIndices,
                        WeightedIndices = weightedIndices,
                        Material = materials[mesh.MaterialIndex]
                    } );
                }

                geometryBuildInfos.Add( new GeometryBuildInfo
                {
                    TargetNodeInfo = targetNodeMeshGroup.Key,
                    Meshes = meshBuildInfos,
                    UnweightedVertices = unweightedVertices,
                    WeightedVertices = weightedVertices,
                    ExternalWeightedVertices = new List<Vertex>(),
                } );
            }

            if ( !sDisableWeights )
            {
                ResolveExternalWeightedVertices( geometryBuildInfos, convertedNodes );
            }

            var orderedGeometryBuildInfos = geometryBuildInfos.OrderBy( x => x.TargetNodeInfo.Index ).ToList();

            return orderedGeometryBuildInfos;
        }

        private static List<Vertex> ConvertVertices(Assimp.Mesh mesh, Assimp.Node parentNode, List<NodeBuildInfo> convertedNodes, ref int vertexId )
        {
            var parentNodeWorldTransform = CalculateWorldTransform( parentNode );

            var vertices = new List<Vertex>();
            for ( int i = 0; i < mesh.VertexCount; i++ )
            {
                var vertex = new Vertex
                {
                    Id = vertexId++,
                    Position = Vector3.Transform( AssimpHelper.FromAssimp( mesh.Vertices[i] ), parentNodeWorldTransform ),
                    Normal = Vector3.TransformNormal( AssimpHelper.FromAssimp( mesh.Normals[i] ), parentNodeWorldTransform ),
                    UV = mesh.HasTextureCoords( 0 )
                        ? AssimpHelper.FromAssimpAsVector2( mesh.TextureCoordinateChannels[0][i] )
                        : new Vector2(),
                    Weights = new List<VertexWeight>()
                };

                if ( mesh.HasBones )
                {
                    foreach ( var bone in mesh.Bones )
                    {
                        var nodeIndex = convertedNodes.FindIndex( x => x.AiNode.Name == bone.Name );
                        foreach ( var vertexWeight in bone.VertexWeights.Where( x => x.VertexID == i ) )
                            vertex.Weights.Add( new VertexWeight { Weight = vertexWeight.Weight, NodeIndex = nodeIndex } );
                    }

                    // Order the weights ascending by the node index
                    // This is so the weight order corresponds to the order in which it will be processed in the scene.
                    vertex.Weights = vertex.Weights.OrderBy( x => x.NodeIndex ).ToList();
                }
                else
                {
                    Debugger.Break();
                }

                vertices.Add( vertex );
            }

            return vertices;
        }

        private static List<Vertex> FindSeamVertices( List<Vertex> vertices )
        {
            // Seam vertices are vertices that are weighted to node A, but adjacent to another vertex weighted to node B.
            return null;
        }

        private static void WeldWeightedVertices( List<GeometryBuildInfo> geometryBuildInfos)
        {
            Debug.Assert( !sDisableWeights );

            var totalVertexCount = geometryBuildInfos.Sum( x => x.UnweightedVertices.Count );

            var comparer = new VertexPositionEqualityComparer();
            var vertexWelds = new List<(Vector3 Position, List<(GeometryBuildInfo Geometry, Vertex Vertex)> Vertices)>();

            foreach ( var geometry in geometryBuildInfos )
            {
                foreach ( var vertex in geometry.UnweightedVertices )
                {
                    var vertexWeld = vertexWelds.FirstOrDefault( x => comparer.Equals( x.Position, vertex.Position ) );
                    if ( vertexWeld.Vertices == null )
                    {
                        vertexWelds.Add( ( vertex.Position, new List<(GeometryBuildInfo, Vertex)>() { ( geometry, vertex ) } ) );
                    }
                    else
                    {
                        vertexWeld.Vertices.Add( ( geometry, vertex ) );
                    }
                }
            }

            foreach ( var vertexWeld in vertexWelds )
            {
                var uniqueVertexWeights = new HashSet<VertexWeight>();
                foreach ( var vertexWeight in vertexWeld.Vertices.SelectMany( x => x.Vertex.Weights ) )
                    uniqueVertexWeights.Add( vertexWeight );

                foreach ( var uniqueVertexWeight in uniqueVertexWeights )
                {
                    Console.WriteLine( uniqueVertexWeight );
                }

                if ( uniqueVertexWeights.Count < 2 )
                    continue;
            }
        }

        private static void ResolveExternalWeightedVertices( List<GeometryBuildInfo> geometryBuildInfos, List<NodeBuildInfo> convertedNodes )
        {
            Debug.Assert( !sDisableWeights );

            var supplementaryGeometries = new List<GeometryBuildInfo>();
            var geometryBuildInfoLookup = geometryBuildInfos.ToDictionary( x => x.TargetNodeInfo.Index );
            foreach ( var geometryBuildInfo in geometryBuildInfos )
            {
                foreach ( var vertex in geometryBuildInfo.WeightedVertices )
                {
                    foreach ( var vertexWeight in vertex.Weights )
                    {
                        if ( vertexWeight.NodeIndex == geometryBuildInfo.TargetNodeInfo.Index )
                            continue;

                        if ( !geometryBuildInfoLookup.TryGetValue( vertexWeight.NodeIndex, out var otherGeometryBuildInfo ) )
                        {
                            // No geometry associated with this node, we'll have to add a 'dummy' geometry to store the vertex information
                            otherGeometryBuildInfo = new GeometryBuildInfo
                            {
                                TargetNodeInfo = convertedNodes[vertexWeight.NodeIndex],
                                Meshes = null,
                                UnweightedVertices = null,
                                WeightedVertices = null,
                                ExternalWeightedVertices = new List<Vertex>()
                            };

                            supplementaryGeometries.Add( otherGeometryBuildInfo );
                            geometryBuildInfoLookup.Add( otherGeometryBuildInfo.TargetNodeInfo.Index, otherGeometryBuildInfo );
                        }

                        otherGeometryBuildInfo.ExternalWeightedVertices.Add( vertex );
                    }
                }
            }

            geometryBuildInfos.AddRange( supplementaryGeometries );
        }

        private static void RemapFaceIndices( Assimp.Face face, List<Vertex> inVertices, List<Vertex> outVertices, List<IndexBuildInfo> outIndices, int baseIndex = 0 )
        {
            foreach ( int index in face.Indices )
            {
                var vertex = inVertices[index];

                // Include the UVs in the comparison for now as we currently need duplicate vertices for the UVs to be mapped
                // to the strip indices.
                //var newIndex = outVertices.FindIndex( x => x.Position == vertex.Position && x.Normal == vertex.Normal &&
                //                                                x.Weights.SequenceEqual( vertex.Weights ) && x.UV == vertex.UV );

                // Include the UVs in the comparison for now as we currently need duplicate vertices for the UVs to be mapped
                // to the strip indices.
                // Weights don't have to match, as we merge weights for 'duplicate' vertices
                //var newIndex = outVertices.FindIndex( x => x.Position == vertex.Position && x.Normal == vertex.Normal && x.UV == vertex.UV );
                var newIndex = outVertices.FindIndex( x => x.Position == vertex.Position && x.Normal == vertex.Normal &&
                                                                x.Weights.SequenceEqual( vertex.Weights ) && x.UV == vertex.UV );

                if ( newIndex == -1 )
                {
                    newIndex = outVertices.Count;
                    outVertices.Add( vertex );
                }
                //else
                //{
                //    var newVertex = outVertices[ newIndex ];
                //    if ( !newVertex.Weights.SequenceEqual( vertex.Weights ) )
                //    {
                //        // Weights don't match, likely a weight seam
                //        // Just merge the ones that don't match
                //        for ( int i = 0; i < vertex.Weights.Count; i++ )
                //        {
                //            var weight = vertex.Weights[ i ];
                //            if ( !newVertex.Weights.Any( x => x.NodeIndex == weight.NodeIndex ) )
                //            {
                //                // This vertex has a weight to a node that wasn't previously known to be weighted to it
                //                // Update the cached vertex.
                //                newVertex.Weights.Add( weight );
                //            }
                //            else if ( newVertex.Weights.Any( x => x.NodeIndex == weight.NodeIndex && x.Weight != weight.Weight ) )
                //            {
                //                // This vertex is already weighted to this node, but the weights are different?
                //                // Maybe merge?
                //                Debug.Assert( false );
                //            }
                //        }

                //        outVertices[newIndex] = newVertex;
                //    }
                //}

                outIndices.Add( new IndexBuildInfo { VertexIndex = ( ushort )( newIndex + baseIndex ), UV = vertex.UV } );
            }
        }

        private static Strip<StripIndexUVN>[] GenerateStrips( List<IndexBuildInfo> indices )
        {
            // Build vertex index to UV lookup
            // Only works if vertices are duplicated for each unique UV.
            // Alternative solution would be to pass along the UVs into the strip generator
            // But due to the design of the code, that isn't trivial to do right now.
            var uvLookup = new Dictionary<int, Vector2>();
            foreach ( var index in indices )
                uvLookup[index.VertexIndex] = index.UV;

            Trace.Assert( sStripifier.GenerateStrips( indices.Select( x => x.VertexIndex ).ToArray(), out var primitiveGroups ) );

            // Add strips
            var strips = new Strip<StripIndexUVN>[primitiveGroups.Length];
            for ( var i = 0; i < primitiveGroups.Length; i++ )
            {
                var primitiveGroup = primitiveGroups[i];
                Trace.Assert( primitiveGroup.Type == NvTriStripDotNet.PrimitiveType.TriangleStrip );

                var stripIndices = new StripIndexUVN[primitiveGroup.Indices.Length];
                for ( var j = 0; j < primitiveGroup.Indices.Length; j++ )
                {
                    var vertexIndex = primitiveGroup.Indices[j];
                    var uv = uvLookup[vertexIndex];
                    var stripIndex = new StripIndexUVN( vertexIndex, uv );
                    stripIndices[j] = stripIndex;
                }

                var strip = new Strip<StripIndexUVN>( false, stripIndices );
                strips[i] = strip;
            }

            return strips;
        }

        //private static Strip<StripIndexUVN>[] GenerateStripsOptimizedUvs()
        //{
        //    //if ( DUPLICATE_VERTS_FOR_UVS )
        //    //{
        //    //    // TODO: this requires duplicate vertices to exist for the UVs to work properly
        //    //    // only way to fix this would be to pass the uvs along into the strip generator
        //    //    uvLookup = new Dictionary<int, Vector2>();
        //    //    foreach ( var index in indices )
        //    //        uvLookup[index.VertexIndex] = index.UV;

        //    //    Debug.Assert( sStripifier.GenerateStrips( indices.Select( x => x.VertexIndex ).ToArray(), out primitiveGroups ) );
        //    //}
        //    //else
        //    //{
        //    //    //Debug.Assert( sStripifier.GenerateStrips( indices.Select( x => x.VertexIndex ).ToArray(), indices.Select( x => x.UV ).Cast<object>().ToArray(),
        //    //    //                           out var primitiveGroups ) );

        //    //    Debug.Assert( sStripifier.GenerateStrips( indices.Select( x => x.VertexIndex ).ToArray(), out primitiveGroups ) );
        //    //}

        //    //// Add strips to procedural mesh
        //    //var strips = new Strip<StripIndexUVN>[primitiveGroups.Length];
        //    //for ( var i = 0; i < primitiveGroups.Length; i++ )
        //    //{
        //    //    var primitiveGroup = primitiveGroups[ i ];
        //    //    Debug.Assert( primitiveGroup.Type == NvTriStripDotNet.PrimitiveType.TriangleStrip );

        //    //    var stripIndices = new StripIndexUVN[primitiveGroup.Indices.Length];
        //    //    for ( var j = 0; j < primitiveGroup.Indices.Length; j++ )
        //    //    {
        //    //        var vertexIndex = primitiveGroup.Indices[ j ];

        //    //        Vector2 uv;
        //    //        if ( DUPLICATE_VERTS_FOR_UVS )
        //    //        {
        //    //            uv = uvLookup[vertexIndex];
        //    //        }
        //    //        else
        //    //        {
        //    //            //var uv = ( Vector2 ) primitiveGroup.UserData[ j ];

        //    //            uv = new Vector2();
        //    //            var uvs = indices.Where( x => x.VertexIndex == vertexIndex ).Select( x => x.UV ).Distinct().ToList();
        //    //            if ( uvs.Count == 1 )
        //    //            {
        //    //                uv = uvs[ 0 ];
        //    //            }
        //    //            else
        //    //            {
        //    //                Console.WriteLine( uvs.Count );
        //    //            }
        //    //        }

        //    //        var stripIndex  = new StripIndexUVN( ( ushort ) ( vertexIndex ), uv );
        //    //        stripIndices[ j ] = stripIndex;
        //    //    }

        //    //    var strip = new Strip<StripIndexUVN>( false, stripIndices );
        //    //    strips[ i ] = strip;
        //    //}
        //}

        private static void AddChangedMaterialParameters( List<Chunk16> polygonList, MaterialBuildInfo meshMaterial, MaterialBuildInfo currentMaterial, bool force )
        {
            var ambientDifferent = force || meshMaterial.Ambient != currentMaterial.Ambient;
            var diffuseDifferent = force || meshMaterial.Diffuse != currentMaterial.Diffuse;
            var specularDifferent = force || meshMaterial.Specular != currentMaterial.Specular;
            var alphaOpDifferent = force || meshMaterial.SourceAlpha != currentMaterial.SourceAlpha ||
                                   meshMaterial.DestinationAlpha != currentMaterial.DestinationAlpha;

            if ( ambientDifferent && diffuseDifferent && specularDifferent )
            {
                polygonList.Add( new MaterialDiffuseAmbientSpecularChunk
                {
                    Ambient = meshMaterial.Ambient,
                    Diffuse = meshMaterial.Diffuse,
                    Specular = meshMaterial.Specular,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( ambientDifferent && diffuseDifferent )
            {
                polygonList.Add( new MaterialDiffuseAmbientChunk
                {
                    Ambient = meshMaterial.Ambient,
                    Diffuse = meshMaterial.Diffuse,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( ambientDifferent && specularDifferent )
            {
                polygonList.Add( new MaterialAmbientSpecularChunk
                {
                    Ambient = meshMaterial.Ambient,
                    Specular = meshMaterial.Specular,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( ambientDifferent )
            {
                polygonList.Add( new MaterialAmbientChunk
                {
                    Ambient = meshMaterial.Ambient,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( diffuseDifferent && specularDifferent )
            {
                polygonList.Add( new MaterialDiffuseSpecularChunk
                {
                    Diffuse = meshMaterial.Diffuse,
                    Specular = meshMaterial.Specular,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( diffuseDifferent )
            {
                polygonList.Add( new MaterialDiffuseChunk
                {
                    Diffuse = meshMaterial.Diffuse,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( specularDifferent )
            {
                polygonList.Add( new MaterialSpecularChunk
                {
                    Specular = meshMaterial.Specular,
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }
            else if ( alphaOpDifferent )
            {
                polygonList.Add( new BlendAlphaChunk
                {
                    DestinationAlpha = meshMaterial.DestinationAlpha,
                    SourceAlpha = meshMaterial.SourceAlpha,
                } );
            }

            bool mipMapDAdjustDifferent = force || meshMaterial.MipMapDAdjust != currentMaterial.MipMapDAdjust;
            bool textureParamsDifferent = force ||
                                          mipMapDAdjustDifferent ||
                                          meshMaterial.ClampU != currentMaterial.ClampU ||
                                          meshMaterial.ClampV != currentMaterial.ClampV ||
                                          meshMaterial.FilterMode != currentMaterial.FilterMode ||
                                          meshMaterial.FlipU != currentMaterial.FlipU ||
                                          meshMaterial.FlipV != currentMaterial.FlipV ||
                                          meshMaterial.TextureId != currentMaterial.TextureId ||
                                          meshMaterial.SuperSample != currentMaterial.SuperSample;

            if ( textureParamsDifferent )
            {
                // Add texture parameters
                polygonList.Add( new TextureIdChunk
                {
                    ClampU = meshMaterial.ClampU,
                    ClampV = meshMaterial.ClampV,
                    FilterMode = meshMaterial.FilterMode,
                    FlipU = meshMaterial.FlipU,
                    FlipV = meshMaterial.FlipV,
                    Id = meshMaterial.TextureId,
                    MipMapDAdjust = meshMaterial.MipMapDAdjust,
                    SuperSample = meshMaterial.SuperSample,
                } );
            }
            else if ( mipMapDAdjustDifferent )
            {
                polygonList.Add( new MipmapDAdjustChunk
                {
                    DAdjust = meshMaterial.MipMapDAdjust
                } );
            }
        }

        private static Matrix4x4 CalculateWorldTransform( Assimp.Node node )
        {
            Assimp.Matrix4x4 CalculateWorldTransformImpl( Assimp.Node curNode )
            {
                var transform = curNode.Transform;
                if ( curNode.Parent != null )
                    transform *= CalculateWorldTransformImpl( curNode.Parent );

                return transform;
            }

            var worldTransform = CalculateWorldTransformImpl( node );
            return AssimpHelper.FromAssimp( worldTransform );
        }

        private static Assimp.Node FindHierarchyRootNode( Assimp.Node aiSceneRootNode )
        {
            // Pretty naiive for now.
            return aiSceneRootNode.Children.Single( x => !x.HasMeshes );
        }

        private static List<NodeBuildInfo> ConvertNodes( Assimp.Node aiHierarchyRootNode )
        {
            var convertedNodes = new List<NodeBuildInfo>();
            int nodeIndex = 0;

            Node ConvertHierarchyNodeRecursively( Assimp.Node aiNode, ref Node previousSibling, Node parent )
            {
                aiNode.Transform.Decompose( out var scale, out var rotation, out var translation );

                // Create node
                var node = new Node( AssimpHelper.FromAssimp( translation ), AngleVector.FromQuaternion( AssimpHelper.FromAssimp( rotation ) ),
                                     AssimpHelper.FromAssimp( scale ), parent );

                var originalNode = sOriginalNodes[nodeIndex];
                node.Translation = originalNode.Translation;
                node.Rotation = originalNode.Rotation;
                node.Scale = originalNode.Scale;

                convertedNodes.Add( new NodeBuildInfo { AiNode = aiNode, Node = node, Index = nodeIndex++ } );

                // Set sibling (next) reference of previous
                if ( previousSibling != null )
                    previousSibling.Sibling = node;

                previousSibling = node;

                if ( aiNode.HasChildren )
                {
                    Node childPreviousSibling = null;
                    foreach ( var aiChildNode in aiNode.Children )
                    {
                        var childNode = ConvertHierarchyNodeRecursively( aiChildNode, ref childPreviousSibling, node );

                        // Make sure to set the 'first child' reference if we haven't already
                        if ( node.Child == null )
                            node.Child = childNode;
                    }
                }

                return node;
            }

            // Dummy!
            Node dummy = null;
            ConvertHierarchyNodeRecursively( aiHierarchyRootNode, ref dummy, null );

            return convertedNodes;
        }

        private static List<Assimp.Node> FindMeshNodes( Assimp.Node aiSceneRootNode )
        {
            var meshNodes = new List<Assimp.Node>();

            void FindMeshNodesRecursively( Assimp.Node aiParentNode )
            {
                foreach ( var aiNode in aiParentNode.Children )
                {
                    if ( aiNode.HasMeshes )
                        meshNodes.Add( aiNode );
                    else
                        FindMeshNodesRecursively( aiNode );
                }
            }

            FindMeshNodesRecursively( aiSceneRootNode );

            return meshNodes;
        }

        private static List<(Assimp.Node ParentNode, NodeBuildInfo TargetNode, Assimp.Mesh Mesh)> MapMeshNodesToTargetNodes( List<Assimp.Node> meshNodes, List<Assimp.Mesh> meshes, List<NodeBuildInfo> convertedNodes )
        {
            var mappedMeshes = new List<(Assimp.Node ParentNode, NodeBuildInfo TargetNode, Assimp.Mesh Mesh)>();
            foreach ( var aiMeshNode in meshNodes )
            {
                foreach ( int aiMeshIndex in aiMeshNode.MeshIndices )
                {
                    var aiMesh = meshes[aiMeshIndex];
                    Debug.Assert( aiMesh.HasBones );

                    var targetNode = DetermineBestTargetNode( aiMesh, convertedNodes );
                    mappedMeshes.Add( (aiMeshNode, targetNode, aiMesh) );
                }
            }

            return mappedMeshes;
        }

        private static NodeBuildInfo DetermineBestTargetNode( Assimp.Mesh aiMesh, List<NodeBuildInfo> convertedNodes )
        {
            if ( aiMesh.BoneCount > 1 )
            {
                //var boneConveragePercents = CalculateBoneWeightCoveragePercents( aiMesh );
                //var maxCoverage           = boneConveragePercents.Max( x => x.Coverage );
                //return boneConveragePercents.First( x => x.Coverage == maxCoverage ).Bone;

                // Find the bone with the highest node index so that the weighted vertices will have been transformed sufficiently during traversal
                // TODO: maybe split meshes that only have a weight of 1 to multiple bones?
                var boneNodes = aiMesh.Bones.Select( x => convertedNodes.Find( y => y.AiNode.Name == x.Name ) ).ToDictionary( x => x.Index );
                var highestBoneNodeIndex = boneNodes.Max( x => x.Key );
                return boneNodes[highestBoneNodeIndex];
            }
            else
            {
                return convertedNodes.Find( x => x.AiNode.Name == aiMesh.Bones[0].Name );
            }
        }

        private static List<(float Coverage, Assimp.Bone Bone)> CalculateBoneWeightCoveragePercents( Assimp.Mesh aiMesh )
        {
            var boneScores = new List<(float Coverage, Assimp.Bone Bone)>();

            foreach ( var bone in aiMesh.Bones )
            {
                float weightTotal = 0;
                foreach ( var vertexWeight in bone.VertexWeights )
                    weightTotal += vertexWeight.Weight;

                float weightCoverage = ( weightTotal / aiMesh.VertexCount ) * 100f;
                boneScores.Add( (weightCoverage, bone) );
            }

            return boneScores;
        }
    }
}
