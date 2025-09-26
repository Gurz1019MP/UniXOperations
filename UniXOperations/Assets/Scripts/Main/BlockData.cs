using B83.Image.BMP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace XOPS
{
    public class BlockData
    {
        #region .ctor

        public BlockData(string[] textures, Block[] blocks, string path)
        {
            _textures = textures;
            _blocks = blocks;
            _path = path;
        }

        #endregion

        #region Property

        private Mesh _mesh;
        public Mesh Mesh
        {
            get
            {
                if (_mesh == null)
                {
                    _mesh = GenerateMesh();
                }
                return _mesh;
            }
        }

        private Material[] _materials;
        public Material[] Materials
        {
            get
            {
                if (_materials == null)
                {
                    _materials = GenerateMaterials();
                }
                return _materials;
            }
        }

        #endregion

        #region Private Method

        private Mesh GenerateMesh()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<Triangle>();
            var uvs = new List<Vector2>();

            foreach (var block in _blocks)
            {
                var blockVertices = GenerateVertices(block.FloatInfo);
                vertices.AddRange(blockVertices);
                triangles.AddRange(GenerateTriangles(block.Id * 24, block.IntInfo));
                for (int j = 0; j < 6; j++)
                {
                    uvs.Add(new Vector2(block.FloatInfo[j * 4 + 27], block.FloatInfo[j * 4 + 51]));    // 0, 1
                    uvs.Add(new Vector2(block.FloatInfo[j * 4 + 24], block.FloatInfo[j * 4 + 48]));    // 0, 0
                    uvs.Add(new Vector2(block.FloatInfo[j * 4 + 25], block.FloatInfo[j * 4 + 49]));    // 1, 0
                    uvs.Add(new Vector2(block.FloatInfo[j * 4 + 26], block.FloatInfo[j * 4 + 50]));    // 1, 1
                }
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = vertices.ToArray();
            newMesh.uv = uvs.ToArray();
            newMesh.subMeshCount = 10;
            foreach (var subTriangleGroup in triangles.GroupBy(t => t.TextureId))
            {
                newMesh.SetTriangles(subTriangleGroup.SelectMany(g => g.Indexes).ToArray(), subTriangleGroup.Key);
            }

            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();

            newMesh.Optimize();

            return newMesh;
        }

        private Material[] GenerateMaterials()
        {
            return _textures.Select(t =>
            {
                var material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Mode", 1f);
                material.SetFloat("_Glossiness", 0.0f);
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;

                string filepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_path), t);
                if (string.IsNullOrEmpty(t))
                {
                    Debug.Log("テクスチャ名が空白でした");
                    material.color = Color.green;
                }
                else if (File.Exists(filepath))
                {
                    if (System.IO.Path.GetExtension(filepath).ToLower() == ".bmp")
                    {
                        try
                        {
                            material.mainTexture = new BMPLoader().LoadBMP(filepath).ToTexture2D();
                        }
                        catch
                        {
                            material.mainTexture = null;
                        }
                    }
                    else
                    {
                        Texture2D temp = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                        temp.LoadImage(File.ReadAllBytes(filepath));
                        material.mainTexture = temp;
                    }
                    material.mainTextureScale = new Vector2(1, -1);
                }
                else
                {
                    Debug.Log($"テクスチャが見つかりませんでした ({t})");
                    material.color = Color.cyan;
                }

                return material;
            }).ToArray();
        }

        private Vector3[] GenerateVertices(IReadOnlyList<float> main)
        {
            return new Vector3[]
            {
            // 0
            GenerateVertex(main, 0),
            GenerateVertex(main, 3),
            GenerateVertex(main, 2),
            GenerateVertex(main, 1),
            
            // 1
            GenerateVertex(main, 7),
            GenerateVertex(main, 4),
            GenerateVertex(main, 5),
            GenerateVertex(main, 6),

            // 2
            GenerateVertex(main, 4),
            GenerateVertex(main, 0),
            GenerateVertex(main, 1),
            GenerateVertex(main, 5),

            // 3
            GenerateVertex(main, 5),
            GenerateVertex(main, 1),
            GenerateVertex(main, 2),
            GenerateVertex(main, 6),

            // 4
            GenerateVertex(main, 6),
            GenerateVertex(main, 2),
            GenerateVertex(main, 3),
            GenerateVertex(main, 7),

            // 5
            GenerateVertex(main, 7),
            GenerateVertex(main, 3),
            GenerateVertex(main, 0),
            GenerateVertex(main, 4),
            };
        }

        private Vector3 GenerateVertex(IReadOnlyList<float> main, int index)
        {
            return new Vector3(main[index], main[index + 8], main[index + 16]);
        }

        private Triangle[] GenerateTriangles(int offset, int[] textures)
        {
            var triangles = new List<Triangle>();
            triangles.AddRange(GenerateFaceTriangles(offset, 0, textures));
            triangles.AddRange(GenerateFaceTriangles(offset, 1, textures));
            triangles.AddRange(GenerateFaceTriangles(offset, 2, textures));
            triangles.AddRange(GenerateFaceTriangles(offset, 3, textures));
            triangles.AddRange(GenerateFaceTriangles(offset, 4, textures));
            triangles.AddRange(GenerateFaceTriangles(offset, 5, textures));

            return triangles.ToArray();
        }

        private Triangle[] GenerateFaceTriangles(int blockNumber, int faceNumber, int[] textures)
        {
            int offset = blockNumber + faceNumber * 4;
            return new Triangle[]
            {
            new Triangle(offset, offset + 1, offset + 2, textures[faceNumber]),
            new Triangle(offset, offset + 2, offset + 3, textures[faceNumber]),
            };
        }

        #endregion

        #region Private Field

        private string[] _textures;

        private Block[] _blocks;

        private string _path;

        #endregion
    }
}
