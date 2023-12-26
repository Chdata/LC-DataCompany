#define PUBLIC_RELEASE

#if !PUBLIC_RELEASE
using GameNetcodeStuff;
using HarmonyLib;
#endif

namespace DataCompanyMod.Patches
{
    internal class Cheats
    {
#if !PUBLIC_RELEASE
        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        [HarmonyPrefix]
        private static void PreBeginUsingTerminal(Terminal __instance)
        {
            __instance.groupCredits = 90000;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        [HarmonyPrefix]
        public static void BeforeDamagerPlayer(ref int ___health)
        {
            ___health = 10000;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void InfiniteSprintPatch_Update(ref float ___sprintMeter)
        {
            ___sprintMeter = 1f;
        }
#endif
    }
}
