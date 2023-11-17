using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

public class PointData
{
    #region .ctor

    public PointData(Vector3 position, Quaternion rotation, sbyte data1, sbyte data2, sbyte data3, sbyte data4)
    {
        Position = position;
        Rotation = rotation;
        Data1 = data1;
        Data2 = data2;
        Data3 = data3;
        Data4 = data4;
    }

    #endregion

    #region Public Property

    public Vector3 Position { get; }

    public Quaternion Rotation { get; }

    public sbyte Data1 { get; }

    public sbyte Data2 { get; }

    public sbyte Data3 { get; }

    public sbyte Data4 { get; }

    #endregion

    #region Public Method

    public override string ToString()
    {
        switch(Data1)
        {
            case 2:
                return string.Format(_formatMapper[Data1], Data4, _weaponKindMapper[Data2], Data3);
            case 3:
                return string.Format(_formatMapper[Data1], Data4, _pathKindMapper[Data2], Data3);
            case 4:
                return string.Format(_formatMapper[Data1], Data4, _characterKindMapper[Data2], Data3);
            case 5:
                return string.Format(_formatMapper[Data1], Data4, _objectKindMapper[Data2], Data3);
            case 7:
                return string.Format(_formatMapper[Data1], Data4, _weaponKindMapper[Data2], _weaponKindMapper[Data3]);
            case 10:
            case 11:
                return string.Format(_formatMapper[Data1], Data4);
            case 1:
            case 6:
            case 8:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
                return string.Format(_formatMapper[Data1], Data4, Data2, Data3);
            default:
                return "不明なポイント情報";
        }
    }

    #endregion

    #region Private Field

    private static Dictionary<sbyte, string> _formatMapper = new Dictionary<sbyte, string>()
    {
        { 1, "[{0}] 人\r\n人情報ID:{1}, 最初のパス:{2}" },
        { 2, "[{0}] 武器({1})\r\n弾薬数:{2}" },
        { 3, "[{0}] パス({1})\r\n次のパス:{2}" },
        { 4, "[{0}] 人情報({1})\r\nチーム番号:{2}" },
        { 5, "[{0}] 小物({1})\r\n位置修正フラグ:{2}" },
        { 6, "[{0}] 人(手ぶら)\r\n人情報ID:{1}, 最初のパス:{2}" },
        { 7, "[{0}] ランダム武器\r\n{1} または {2}" },
        { 8, "[{0}] ランダムパス\r\n{1} または {2}" },

        { 10, "[{0}] イベント 任務達成"},
        { 11, "[{0}] イベント 任務失敗"},
        { 12, "[{0}] イベント 死亡待ち\r\n対象ID:{1}, 次イベントID:{2}" },
        { 13, "[{0}] イベント 到着待ち\r\n対象ID:{1}, 次イベントID:{2}" },
        { 14, "[{0}] イベント 歩きに変更\r\n対象ID:{1}, 次イベントID:{2}" },
        { 15, "[{0}] イベント 小物破壊待ち\r\n対象ID:{1}, 次イベントID:{2}" },
        { 16, "[{0}] イベント ケース待ち\r\n対象ID:{1}, 次イベントID:{2}" },
        { 17, "[{0}] イベント 時間待ち\r\n{1}秒, 次イベントID:{2}" },
        { 18, "[{0}] イベント メッセージ\r\nメッセージ番号:{1}, 次イベントID:{2}" },
        { 19, "[{0}] イベント チーム変更\r\n対象ID:{1}, 次イベントID:{2}" },
    };

    private static Dictionary<sbyte, string> _pathKindMapper = new Dictionary<sbyte, string>()
    {
        { 0, "歩き" },
        { 1, "走り" },
        { 2, "待機" },
        { 3, "追尾" },
        { 4, "警戒待ち" },
        { 5, "時間待ち" },
        { 6, "手榴弾投げ" },
        { 7, "優先的な走り" },
    };

    private static Dictionary<sbyte, string> _characterKindMapper = new Dictionary<sbyte, string>()
    {
        { 0, "特殊 黒 A" },
        { 1, "特殊 黒 B" },
        { 2, "特殊 緑 A" },
        { 3, "特殊 緑 B" },
        { 4, "特殊 緑 C" },
        { 5, "特殊 白 A" },
        { 6, "特殊 白 B" },
        { 7, "ハゲ" },
        { 8, "特殊 紫" },
        { 9, "特殊 青" },
        { 10, "戦闘用ロボット" },
        { 11, "スーツ 黒 SG" },
        { 12, "スーツ 黒" },
        { 13, "スーツ 灰" },
        { 14, "警察官" },
        { 15, "スーツ 茶" },
        { 16, "シャツ男 1" },
        { 17, "中東兵" },
        { 18, "女" },
        { 19, "金髪男" },
        { 20, "市民 1" },
        { 21, "市民 2" },
        { 22, "シャツ男 1 SG" },
        { 23, "金髪男 SG" },
        { 24, "市民 1 SG" },
        { 25, "市民 2 SG" },
        { 26, "兵士 1 A" },
        { 27, "兵士 1 B" },
        { 28, "兵士 2" },
        { 29, "ゾンビ 1" },
        { 30, "ゾンビ 2" },
        { 31, "ゾンビ 3" },
        { 32, "ゾンビ 4" },
        { 33, "スーツ 紺" },
        { 34, "スーツ 紺 SG" },
        { 35, "将軍" },
        { 36, "スーツ 青" },
        { 37, "スーツ 青 SG" },
        { 38, "シャツ男 2 SG" },
        { 39, "兵士 3" },
        { 40, "兵士 3 SG" },
        { 41, "ゲイツ" },
        { 42, "ゲイツ SG" },
    };

    private static Dictionary<sbyte, string> _weaponKindMapper = new Dictionary<sbyte, string>()
    {
        { 0, "NONE" },
        { 1, "MP5" },
        { 2, "PSG1" },
        { 3, "M92F" },
        { 4, "GLOCK18S" },
        { 5, "DESERT EAGLE" },
        { 6, "MAC10" },
        { 7, "UMP" },
        { 8, "P90" },
        { 9, "M4" },
        { 10, "AK47" },
        { 11, "AUG" },
        { 12, "M249" },
        { 13, "GRENADE" },
        { 14, "MP5SD" },
        { 15, "CASE" },
        { 16, "GLOCK18F" },
        { 17, "M1911" },
        { 18, "GLOCK17" },
        { 19, "M1" },
        { 20, "FAMAS" },
        { 21, "MK23" },
        { 22, "MK23 SD" },
    };

    private static Dictionary<sbyte, string> _objectKindMapper = new Dictionary<sbyte, string>()
    {
        { 0, "缶" },
        { 1, "パソコン" },
        { 2, "パソコン キーボード上" },
        { 3, "パソコン 本体逆" },
        { 4, "パソコン ワイド" },
        { 5, "パソコン ワイド キーボード上" },
        { 6, "パソコン ワイド 本体逆" },
        { 7, "椅子" },
        { 8, "ダンボール" },
        { 9, "パソコン 起動中" },
        { 10, "パソコン 起動中 暗" },
        { 11, "パイロン" },
        { 12, "<追加小物>" },
    };

    #endregion
}

public static class pd1loader
{
    #region Public Method

    public static PointData[] LoadPD1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("ファイルが見つかりません。", path);

        return LoadPD1Enum(path).ToArray();
    }

    #endregion

    #region Private Method

    private static IEnumerable<PointData> LoadPD1Enum(string path)
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
                    new Vector3(floatInfo[0], floatInfo[1], floatInfo[2]),
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
