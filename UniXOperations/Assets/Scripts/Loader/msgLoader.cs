using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public static class msgLoader
{
    public static string[] LoadEventMessage(string pointPath)
    {
        string messagePath = $"{Path.GetDirectoryName(pointPath)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(pointPath)}.msg";
        if (File.Exists(messagePath))
        {
            //UnityEngine.Debug.Log($"メッセージが見つかりました ({messagePath})");
            
            List<string> messages = new List<string>();
            
            using (var sr = new StreamReader(messagePath, Encoding.GetEncoding("shift_jis")))
            {
                while (true)
                {
                    var buffer = sr.ReadLine();
                    if (buffer == null) break;

                    messages.Add(buffer);
                }
            }

            //foreach(var message in messages)
            //{
            //    UnityEngine.Debug.Log(message);
            //}

            return messages.ToArray();
        }
        else
        {
            //UnityEngine.Debug.Log($"メッセージが見つかりませんでした ({messagePath})");
            return new string[0];
        }
    }
}
