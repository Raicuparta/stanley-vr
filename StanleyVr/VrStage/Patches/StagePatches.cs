using HarmonyLib;

namespace StanleyVr.VrStage.Patches;

[HarmonyPatch]
public static class StagePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.Start))]
    private static void CreateStage(StanleyController __instance)
    {
        VrStageController.Create(__instance);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainCamera), nameof(MainCamera.Start))]
    private static void SetUpStageMainCamera(MainCamera __instance)
    {
        if (!VrStageController.Instance) return;

        VrStageController.Instance.SetUp(__instance);
    }
}