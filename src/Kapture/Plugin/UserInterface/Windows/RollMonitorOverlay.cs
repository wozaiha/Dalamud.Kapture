﻿using System.Linq;
using System.Numerics;
using CheapLoc;
using ImGuiNET;

namespace Kapture
{
    public class RollMonitorOverlayWindow : WindowBase
    {
        private readonly IKapturePlugin _plugin;
        private float _uiScale;

        public RollMonitorOverlayWindow(IKapturePlugin plugin)
        {
            _plugin = plugin;
        }

        private bool ShowOverlay()
        {
            return !_plugin.IsInitializing && _plugin.IsLoggedIn() && _plugin.Configuration.Enabled && IsVisible &&
                   (_plugin.Configuration.RollDisplayMode == DisplayMode.AlwaysOn.Code ||
                    _plugin.Configuration.RollDisplayMode == DisplayMode.ContentOnly.Code && _plugin.InContent ||
                    _plugin.Configuration.RollDisplayMode == DisplayMode.DuringRollsOnly.Code && _plugin.IsRolling);
        }

        public override void DrawView()
        {
            if (!ShowOverlay()) return;
            var isVisible = IsVisible;
            _uiScale = ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(300 * _uiScale, 150 * _uiScale), ImGuiCond.FirstUseEver);
            if (ImGui.Begin(Loc.Localize("RollMonitorOverlayWindow", "Roll Monitor") + "###Kapture_RollMonitor_Window"))
            {
                if (_plugin.ClientLanguage() != 1)
                {
                    ImGui.Text(Loc.Localize("LanguageNotSupported1",
                        "Sorry, only English is supported for this test release."));
                    ImGui.Text(Loc.Localize("LanguageNotSupported2",
                        "I'll be adding support for other client languages soon."));
                }
                else if (_plugin.Configuration.Enabled)
                {
                    var lootRolls = _plugin.LootRollsDisplay;

                    if (lootRolls.Count > 0)
                    {
                        ImGui.Columns(3);
                        ImGui.TextColored(UIColor.Violet, Loc.Localize("MonitorItemName", "Item"));
                        ImGui.NextColumn();
                        ImGui.TextColored(UIColor.Violet, Loc.Localize("MonitorRollers", "Rollers"));
                        ImGui.NextColumn();
                        ImGui.TextColored(UIColor.Violet, Loc.Localize("MonitorWinner", "Winner"));
                        ImGui.NextColumn();
                        ImGui.Separator();

                        foreach (var lootRoll in lootRolls.ToList())
                        {
                            ImGui.Text(lootRoll.ItemName);
                            ImGui.NextColumn();
                            ImGui.Text(lootRoll.RollersDisplay);
                            ImGui.NextColumn();
                            ImGui.Text(lootRoll.Winner);
                            ImGui.NextColumn();
                        }
                    }
                    else
                    {
                        ImGui.Text(Loc.Localize("WaitingForItems", "Waiting for items."));
                    }
                }

                IsVisible = isVisible;
            }

            ImGui.End();
        }
    }
}