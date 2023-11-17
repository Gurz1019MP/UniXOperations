using B83.Image.BMP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

                string filepath = Path.Combine(Path.GetDirectoryName(_path), t);
                if (string.IsNullOrEmpty(t))
                {
                    Debug.Log("テクスチャ名が空白でした");
                    material.color = Color.green;
                }
                else if (File.Exists(filepath))
                {
                    if (Path.GetExtension(filepath).ToLower() == ".bmp")
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

    public class Block
    {
        #region .ctor

        public Block(short id, float[] floatInfo, int[] intInfo)
        {
            Id = id;
            FloatInfo = floatInfo;
            IntInfo = intInfo;
        }

        #endregion

        #region Property

        public short Id { get; set; }

        public float[] FloatInfo { get; private set; }

        public int[] IntInfo { get; private set; }

        #endregion
    }

    public class Triangle
    {
        #region .ctor

        public Triangle(int index1, int index2, int index3, int textureId)
        {
            Indexes = new int[] { index1, index2, index3 };
            TextureId = textureId;
        }

        #endregion

        #region Property

        public int[] Indexes { get; private set; }

        public int TextureId { get; private set; }

        #endregion
    }

    public static class bd1loader
    {
        #region Public Method

        public static BlockData LoadBD1(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"ファイルが見つかりません。({path})", path);

            List<string> textures = new List<string>();
            List<Block> blocks = new List<Block>();

            using (FileStream fs = new FileStream(path, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // テクスチャ読み込み
                for (int i = 0; i < _maxTextureCount; i++)
                {
                    char[] chars = br.ReadChars(_textureContentLength);
                    string buffer = string.Empty;
                    foreach (var c in chars)
                    {
                        if (c == '\0') break;
                        else buffer += c;

                        if (buffer.EndsWith(".bmp")) break;
                    }
                    textures.Add(buffer);
                }

                // データ数読み込み
                var dataCount = br.ReadInt16();

                // ブロック読み込み
                for (short i = 0; i < dataCount; i++)
                {
                    List<float> floatInfo = new List<float>();
                    for (int j = 0; j < 24 + 48; j++)
                    {
                        floatInfo.Add(BitConverter.ToSingle(br.ReadBytes(4), 0));
                    }

                    List<int> intInfo = new List<int>();
                    for (int j = 0; j <= 6; j++)
                    {
                        intInfo.Add(BitConverter.ToInt32(br.ReadBytes(4), 0));
                    }

                    blocks.Add(new Block(i, floatInfo.ToArray(), intInfo.ToArray()));
                }
            }

            return new BlockData(textures.ToArray(), blocks.ToArray(), path);
        }

        #endregion

        #region Field

        private readonly static int _maxTextureCount = 10;
        private readonly static int _textureContentLength = 31;

        #endregion
    }
}
