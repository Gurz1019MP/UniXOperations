using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

public static class pd1loader
{
    #region Public Method

    public static PointData[] LoadPD1(string path, float fieldScale)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("ファイルが見つかりません。", path);

        return LoadPD1Enum(path, fieldScale).ToArray();
    }

    #endregion

    #region Private Method

    private static IEnumerable<PointData> LoadPD1Enum(string path, float fieldScale)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("ファイルが見つかりません。", path);

        using (FileStream fs = new FileStream(path, FileMode.Open))
        using (BinaryReader br = new BinaryReader(fs))
        {
            // データ数読み込み
            var dataCount = br.ReadInt16();

            // ブロック読み込み
            for (short i = 0; i < dataCount; i++)
            {
                List<float> floatInfo = new List<float>();
                for (int j = 0; j < 4; j++)
                {
                    floatInfo.Add(BitConverter.ToSingle(br.ReadBytes(4), 0));
                }

                List<sbyte> byteInfo = new List<sbyte>();
                for (int j = 0; j < 4; j++)
                {
                    byteInfo.Add(br.ReadSByte());
                }

                yield return new PointData(
                    new Vector3(floatInfo[0], floatInfo[1], floatInfo[2]) * fieldScale,
                    Quaternion.Euler(0, floatInfo[3] / Mathf.PI * 180, 0),
                    byteInfo[0],
                    byteInfo[1],
                    byteInfo[2],
                    byteInfo[3]);
            }
        }
    }

    #endregion
}
