﻿using System;
using System.Collections.Generic;
using System.Linq;
using DailyDuty.ConfigurationSystem;
using DailyDuty.Data;
using DailyDuty.System.Modules;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace DailyDuty.DisplaySystem.DisplayModules
{
    internal class TreasureMap : DisplayModule
    {
        protected Daily.TreasureMapSettings Settings => Service.Configuration.CharacterSettingsMap[Service.Configuration.CurrentCharacter].TreasureMapSettings;
        protected override GenericSettings GenericSettings => Settings;

        private readonly HashSet<int> mapLevels;

        private int SelectedMinimumMapLevel
        {
            get
            {
                if (Settings.MinimumMapLevel == 0)
                {
                    Settings.MinimumMapLevel = mapLevels.First();
                }

                return Settings.MinimumMapLevel;
            }
            set => Settings.MinimumMapLevel = value;
        }

        public TreasureMap()
        {
            CategoryString = "Treasure Map";

            mapLevels = DataObjects.MapList.Select(m => m.Level).ToHashSet();
        }

        protected override void DisplayData()
        {
            DisplayLastMapCollectedTime();
            TimeUntilNextMap();
        }

        protected override void DisplayOptions()
        {
        }

        protected override void EditModeOptions()
        {

            ImGui.Text("Manually Reset Map Timer");

            if (ImGui.Button($"Reset##{CategoryString}", ImGuiHelpers.ScaledVector2(75, 25)))
            {
                Settings.LastMapGathered = DateTime.Now;
                Service.Configuration.Save();
            }
        }

        protected override void NotificationOptions()
        {
            DrawPersistentNotificationCheckBox();
            DrawNotifyOnMapCollectionCheckBox();
            DrawHarvestableMapNotificationCheckbox();
            DrawMinimumMapLevelComboBox();
        }

        private void DrawPersistentNotificationCheckBox()
        {
            ImGui.Checkbox($"Persistent Reminders##{CategoryString}", ref Settings.PersistentReminders);
            ImGuiComponents.HelpMarker("Send a chat notification on non-duty area change.");
            ImGui.Spacing();
        }

        private void DrawHarvestableMapNotificationCheckbox()
        {
            ImGui.Checkbox("Harvestable Map Notification", ref Settings.HarvestableMapNotification);
            ImGuiComponents.HelpMarker("Show a notification in chat when there are harvestable Treasure Maps available in the current area.");
            ImGui.Spacing();
        }

        private void DrawNotifyOnMapCollectionCheckBox()
        {
            ImGui.Checkbox("Map Acquisition Notification", ref Settings.NotifyOnAcquisition);
            ImGuiComponents.HelpMarker("Confirm Map Acquisition with a chat message.");
            ImGui.Spacing();
        }

        private static void TimeUntilNextMap()
        {
            var timeSpan = TreasureMapModule.TimeUntilNextMap();
            ImGui.Text("Time Until Next Map:");
            ImGui.SameLine();

            if (timeSpan == TimeSpan.Zero)
            {
                ImGui.TextColored(new(0, 255, 0, 255), $" {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
            }
            else
            {
                ImGui.Text($" {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
            }
        }

        private void DisplayLastMapCollectedTime()
        {
            ImGui.Text("Last Map Collected:");
            ImGui.SameLine();

            ImGui.Text(Settings.LastMapGathered == new DateTime() ? "Never" : $"{Settings.LastMapGathered}");

            ImGui.Spacing();
        }

        private void DrawMinimumMapLevelComboBox()
        {
            if (Settings.HarvestableMapNotification == false) return;

            ImGui.Indent(15 *ImGuiHelpers.GlobalScale);

            ImGui.PushItemWidth(50 * ImGuiHelpers.GlobalScale);

            if (ImGui.BeginCombo("Minimum Map Level", SelectedMinimumMapLevel.ToString(), ImGuiComboFlags.PopupAlignLeft))
            {
                foreach (var element in mapLevels)
                {
                    bool isSelected = element == SelectedMinimumMapLevel;
                    if (ImGui.Selectable(element.ToString(), isSelected))
                    {
                        SelectedMinimumMapLevel = element;
                        Settings.MinimumMapLevel = SelectedMinimumMapLevel;
                        Service.Configuration.Save();
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            ImGuiComponents.HelpMarker("Only show notifications that a map is available if the map is at least this level.");

            ImGui.PopItemWidth();

            ImGui.Indent(-15 * ImGuiHelpers.GlobalScale);
        }

        public override void Dispose()
        {

        }
    }
}
