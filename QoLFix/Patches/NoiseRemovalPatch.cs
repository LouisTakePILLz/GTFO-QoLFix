﻿using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace QoLFix.Patches
{
    public class NoiseRemovalPatch : IPatch
    {
        private static readonly string PatchName = nameof(NoiseRemovalPatch);
        private static readonly ConfigDefinition ConfigEnabled = new ConfigDefinition(PatchName, "Enabled");

        public static IPatch Instance { get; private set; }

        public void Initialize()
        {
            Instance = this;
            QoLFixPlugin.Instance.Config.Bind(ConfigEnabled, false, new ConfigDescription("Disables the blue noise shader. This makes the game look clearer, although some areas might look a lot darker than normal."));
        }

        public string Name { get; } = PatchName;

        public bool Enabled => QoLFixPlugin.Instance.Config.GetConfigEntry<bool>(ConfigEnabled).Value;

        public void Patch(Harmony harmony)
        {
            var methodInfo = typeof(PE_BlueNoise).GetMethod(nameof(PE_BlueNoise.Update));
            harmony.Patch(methodInfo, prefix: new HarmonyMethod(AccessTools.Method(typeof(NoiseRemovalPatch), nameof(PE_BlueNoise__Update))));
        }

        private static Texture EmptyTexture;

        private static bool PE_BlueNoise__Update()
        {
            if (PE_BlueNoise.s_computeShader == null) return false;
            if (EmptyTexture == null)
            {
                EmptyTexture = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            }
            Shader.SetGlobalTexture("_PE_BlueNoise", EmptyTexture);
            PE_BlueNoise.s_computeShader.SetTexture(0, "_PE_BlueNoise", EmptyTexture);
            return false;
        }
    }
}
