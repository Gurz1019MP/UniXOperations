using System;
using System.Collections.Generic;
using System.IO;

namespace XOPS
{
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
