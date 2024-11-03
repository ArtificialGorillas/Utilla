﻿using HarmonyLib;

namespace Utilla.HarmonyPatches
{
    [HarmonyPatch(typeof(GorillaTagger))]
    [HarmonyPatch("Start", MethodType.Normal)]
    static class PostInitializedPatch
    {
        public static Events events;

        private static void Postfix()
        {
            // await Task.Yield();
            events.TriggerGameInitialized();
        }
    }
}
