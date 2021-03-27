using System;
using System.IO;
using System.Numerics;
using CheapLoc;
using ImGuiNET;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace Kapture
{
    public class LogOverlay : WindowBase
    {
        const int eventnumber = 15;
        private readonly IKapturePlugin _plugin;
        private float _uiScale;
        private readonly List<LootEvent> _lootEvent = new List<LootEvent>();
        private readonly object _fileLock = new object();
        private DateTime _lastwrite = DateTime.Now;
        private readonly List<string> _filter = new List<string>();
        private bool[] check = new bool[eventnumber];

        public LogOverlay(IKapturePlugin plugin)
        {
            _plugin = plugin;
        }

        private bool ShowOverlay()
        {
            return (!_plugin.IsInitializing && _plugin.IsLoggedIn() && _plugin.Configuration.Enabled && IsVisible);
        }

        public void ReadFile()
        {
            //_plugin.LogInfo(read.ToString());

            lock (_fileLock)
            {
                string path = Path.Combine(_plugin.DataManager.DataPath, LogFormat.GetFileName(LogFormat.JSON));
                using (StreamReader file = new StreamReader(path))
                {
                    FileInfo fileinfo = new FileInfo(path);
                    if (_lastwrite == fileinfo.LastWriteTime) return;
                    else
                    {
                        _lootEvent.Clear();
                        _filter.Clear();
                        for (int i = 0; i < eventnumber; i++)
                        {
                            check[i] = true;
                        }
                    }

                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        var message = JsonConvert.DeserializeObject<LootEvent>(line);
                        _lootEvent.Add(message);
                        var lootEventTypeName = message.LootEventTypeName;
                        if (!_filter.Contains(lootEventTypeName)) _filter.Add(lootEventTypeName);
                    }

                    _lastwrite = fileinfo.LastWriteTime;
                    file.Close();
                }
            }
        }

        private bool Checkfilter(LootEvent loot)
        {
            var eventname = loot.LootEventTypeName;
            if (!_filter.Contains(eventname)) return false;
            var index = _filter.IndexOf(eventname);
            if (index == -1) return false;
            return check[index];
        }

        public override void DrawView()
        {
            if (!ShowOverlay()) return;
            _uiScale = ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(400 * _uiScale, 300 * _uiScale), ImGuiCond.FirstUseEver);
            bool closeable = true;
            if (ImGui.Begin(Loc.Localize("LogOverlay", "Log"), ref closeable))
            {
                if (_plugin.Configuration.Enabled)
                {
                    var col1 = 120f * Scale;
                    var col2 = 250f * Scale;
                    var col3 = 300f * Scale;

                    //Event filter 
                    if (ImGui.BeginPopup("Event"))
                    {
                        int index = 0;
                        foreach (var filter in _filter)
                        {
                            ImGui.Checkbox(Loc.Localize(filter + "Enabled", filter), ref check[index]);
                            index++;
                        }

                        ImGui.EndPopup();
                    }

                    ImGui.TextColored(UIColor.Violet, Loc.Localize("LootTime", "时间"));
                    ImGui.SameLine(col1);
                    ImGui.TextColored(UIColor.Violet, Loc.Localize("LootItemName", "Item"));
                    ImGui.SameLine(col2);
                    ImGui.PushStyleColor(ImGuiCol.Text, UIColor.Violet);
                    if (ImGui.Button(Loc.Localize("LootEventType", "Event"))) ImGui.OpenPopup("Event");
                    ImGui.PopStyleColor();
                    ImGui.SameLine(col3);
                    ImGui.TextColored(UIColor.Violet, Loc.Localize("LootPlayer", "Player"));
                    ImGui.Separator();

                    lock (_fileLock)
                    {
                        foreach (var loot in _lootEvent)
                        {
                            //ImGui.BeginGroup();
                            if (!Checkfilter(loot)) continue;
                            string time = DateTimeOffset.FromUnixTimeMilliseconds(loot.Timestamp).ToLocalTime()
                                .ToString("yyyy-MM-dd HH:mm");
                            ImGui.Text(time);
                            ImGui.SameLine(col1);
                            string item = loot.ItemName;
                            if (loot.LootMessage.IsHq) item += "";
                            ImGui.Text(item);
                            ImGui.SameLine(col2);
                            string type = Loc.Localize(loot.LootEventTypeName + "Enabled", loot.LootEventTypeName);
                            ImGui.Text(type);
                            ImGui.SameLine(col3);
                            ImGui.Text(loot.PlayerDisplayName);
                            //ImGui.EndGroup();
                        }
                    }
                }

                IsVisible = closeable;
            }

            ImGui.End();
        }
    }
}