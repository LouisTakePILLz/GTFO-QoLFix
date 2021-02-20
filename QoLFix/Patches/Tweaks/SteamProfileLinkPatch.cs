﻿using System;
using BepInEx.Configuration;
using CellMenu;
using HarmonyLib;
using QoLFix.Patches.Common;
using SNetwork;
using Steamworks;
using UnityEngine;

namespace QoLFix.Patches.Tweaks
{
    public class SteamProfileLinkPatch : IPatch
    {
        private static readonly string PatchName = nameof(SteamProfileLinkPatch);
        private static readonly ConfigDefinition ConfigEnabled = new ConfigDefinition(PatchName, "Enabled");

        public static IPatch Instance { get; private set; }

        public void Initialize()
        {
            Instance = this;
            QoLFixPlugin.Instance.Config.Bind(ConfigEnabled, true, new ConfigDescription("Lets you open the steam profile of your teammates by clicking on their name."));
        }

        public string Name => PatchName;

        public bool Enabled => QoLFixPlugin.Instance.Config.GetConfigEntry<bool>(ConfigEnabled).Value;

        public Harmony Harmony { get; set; }

        public void Patch()
        {
            PlayerNameExtPatch.CursorUpdate += OnCursorUpdate;
        }

        private void OnCursorUpdate(CM_PageBase page, Vector2 pos, ref RaycastHit2D rayHit, bool hovering, bool clicked, Lazy<SNet_Player> player)
        {
            page.SetCursorHovering(hovering);
            if (!clicked) return;

            Instance.LogInfo($"Opening steam profile for {player.Value.NickName} ({player.Value.Lookup})");

            var url = $"https://steamcommunity.com/profiles/{player.Value.Lookup}";
            if (SteamUtils.IsOverlayEnabled())
            {
                SteamFriends.ActivateGameOverlayToWebPage(url);
            }
            else
            {
                Application.OpenURL(url);
            }
        }
    }
}
