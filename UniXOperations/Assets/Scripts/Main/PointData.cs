using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        switch (Data1)
        {
            case 2:
                return string.Format(_formatMapper[Data1], Data4, _weaponKindMapper[Data2], Data3);
            case 3:
                return string.Format(_formatMapper[Data1], Data4, _pathKindMapper[Data2], Data3);
            case 4:
                return string.Format(_formatMapper[Data1], Data4, CharacterSpec.GetCharacterSpec(new CharacterKind(Data2)).Name, Data3);
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
