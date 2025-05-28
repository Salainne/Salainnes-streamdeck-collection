using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Streamdeck_collection.Helpers;
using Streamdeck_collection.Model;
using Streamdeck_collection.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.StreamdeckActions
{
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.InvokeQueuelength")]
    public static class InvokeQueuelength
    {
        private static string _invokeConst = "Invokes" + Environment.NewLine;

        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            if (icon.Value.Data == null)
            {
                icon.Value.Data = new object();
                _ = connection.SetImageAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
            }

            var cnt = -1;
            var tmp = _invokeConst;
            Process[] processes = Process.GetProcesses();
            var invoke = processes.Where(a => a.MainWindowTitle.Contains("Invoke - Community Edition")).ToList();
            foreach (var proc in invoke)
            {
                cnt = 0;
                if (proc.MainWindowTitle.Contains("("))
                {
                    var startIndex = proc.MainWindowTitle.IndexOf("(");
                    var endIndex = proc.MainWindowTitle.IndexOf(")");
                    var sNum = proc.MainWindowTitle.Substring(startIndex + 1, endIndex - 1);

                    int.TryParse(sNum, out cnt);
                }
            }
            if (cnt < 0)
            {
                tmp = tmp + "n/a";
            }
            else
            {
                tmp = tmp + cnt;
            }

            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);

            if (icon.Value.HasHadKeyDown)
            {
                var res = Utilities.Post("http://127.0.0.1:9090/api/v2/models/empty_model_cache", "", out var statuscode);
                if (statuscode >= System.Net.HttpStatusCode.OK)
                {
                    _ = connection.SetTitleAsync($"Cache{Environment.NewLine}cleared", icon.Key, SDKTarget.HardwareAndSoftware, null);
                }
                else
                {
                    _ = connection.SetTitleAsync("Error: " + res, icon.Key, SDKTarget.HardwareAndSoftware, null);
                }
            }

            icon.Value.HasHadKeyDown = false;
        }
    }
}
