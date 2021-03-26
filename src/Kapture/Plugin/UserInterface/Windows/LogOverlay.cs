using System;
using System.IO;
using System.Numerics;
using CheapLoc;
using DalamudPluginCommon;
using ImGuiNET;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace Kapture
{
    public class LogOverlay : WindowBase
    {
        private readonly IKapturePlugin _plugin;
        private float _uiScale;
        private readonly List<LootEvent> LootEvent = new List<LootEvent>();
        private readonly object fileLock = new object();
        private long length;

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

            lock (fileLock)
            {
                string path = Path.Combine(_plugin.DataManager.DataPath, LogFormat.GetFileName(LogFormat.JSON));
                using (StreamReader file = new StreamReader(path))
                {
                    if (length == file.BaseStream.Length) return;
                    else LootEvent.Clear();
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        LootEvent.Add(JsonConvert.DeserializeObject<LootEvent>(line));
                    }
                    length = file.BaseStream.Length;
                    file.Close();
                }
            }

        }

        public override void DrawView()
        {

            if (!ShowOverlay()) return;
            _uiScale = ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(400 * _uiScale, 300 * _uiScale), ImGuiCond.FirstUseEver);
            bool closeable = true;
            if (ImGui.Begin(Loc.Localize("LogOverlay", "Log"),ref closeable))
            {
                if (_plugin.Configuration.Enabled)
                {

                    var col1 = 120f * Scale;
                    var col2 = 250f * Scale;
                    var col3 = 300f * Scale;

                    ImGui.TextColored(UIColor.Violet, Loc.Localize("Time", "时间"));
                    ImGui.SameLine(col1);
                    ImGui.TextColored(UIColor.Violet, Loc.Localize("LootItemName", "Item"));
                    ImGui.SameLine(col2);
                    ImGui.TextColored(UIColor.Violet, Loc.Localize("LootEventType", "Event"));
                    ImGui.SameLine(col3);
                    ImGui.TextColored(UIColor.Violet, Loc.Localize("LootPlayer", "Player"));
                    ImGui.Separator();

                    lock (fileLock)
                    {
                        foreach (var loot in LootEvent)
                        {
                            //ImGui.BeginGroup();
                            string time = DateTimeOffset.FromUnixTimeMilliseconds(loot.Timestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                            ImGui.Text(time);
                            ImGui.SameLine(col1);
                            string item = loot.ItemName;
                            if (loot.LootMessage.IsHq) item += "";
                            ImGui.Text(item);
                            ImGui.SameLine(col2);
                            string type = Loc.Localize(loot.LootEventTypeName+ "Enabled", loot.LootEventTypeName);
                            ImGui.Text(loot.LootEventTypeName);
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