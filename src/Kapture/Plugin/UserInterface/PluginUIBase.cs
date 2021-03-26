// ReSharper disable InconsistentNaming

using System;

namespace Kapture
{
    public class PluginUIBase : IDisposable
    {
        private readonly IKapturePlugin KapturePlugin;
        public LootOverlayWindow LootOverlayWindow;
        public RollMonitorOverlayWindow RollMonitorOverlayWindow;
        public SettingsWindow SettingsWindow;
        public LogOverlay LogOverlay;

        protected PluginUIBase(IKapturePlugin kapturePlugin)
        {
            KapturePlugin = kapturePlugin;
            BuildWindows();
            SetWindowVisibility();
            AddEventHandlers();
        }

        public void Dispose()
        {
        }

        private void BuildWindows()
        {
            LootOverlayWindow = new LootOverlayWindow(KapturePlugin);
            RollMonitorOverlayWindow = new RollMonitorOverlayWindow(KapturePlugin);
            SettingsWindow = new SettingsWindow(KapturePlugin);
            LogOverlay = new LogOverlay(KapturePlugin);
        }

        private void SetWindowVisibility()
        {
            LootOverlayWindow.IsVisible = KapturePlugin.Configuration.ShowLootOverlay;
            RollMonitorOverlayWindow.IsVisible = KapturePlugin.Configuration.ShowRollMonitorOverlay;
            SettingsWindow.IsVisible = false;
            LogOverlay.IsVisible = false;
        }

        private void AddEventHandlers()
        {
            SettingsWindow.LootOverlayVisibilityUpdated += UpdateLootOverlayVisibility;
            SettingsWindow.RollMonitorOverlayVisibilityUpdated += UpdateRollMonitorOverlayVisibility;
            SettingsWindow.LogOverlayVisibilityUpdated += UpdateLogOverlayVisibility;
        }

        private void UpdateLootOverlayVisibility(object sender, bool e)
        {
            LootOverlayWindow.IsVisible = e;
        }

        private void UpdateRollMonitorOverlayVisibility(object sender, bool e)
        {
            RollMonitorOverlayWindow.IsVisible = e;
        }

        private void UpdateLogOverlayVisibility(object sender, bool e)
        {
            LogOverlay.IsVisible = e;
            if (e == true) LogOverlay.ReadFile();
        }

        public void Draw()
        {
            LootOverlayWindow.DrawView();
            RollMonitorOverlayWindow.DrawView();
            SettingsWindow.DrawView();
            LogOverlay.DrawView();
        }
    }
}