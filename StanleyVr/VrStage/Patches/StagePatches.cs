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
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.EnableCamera))]
    private static void RecenterOnEnableCamera(StanleyController __instance)
    {
        if (!VrStageController.Instance) return;

        // Camera gets enabled and disabled after loading a scene.
        // We renceter it immediately after.
        // We also recenter it again after a short delay, in case the first recenter wasn't good.
        // Kind of a shitty solution but I don't know what else to do here.
        VrStageController.Instance.Recenter();
        VrStageController.Instance.RecenterDelayed();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainCamera), nameof(MainCamera.Start))]
    private static void SetUpStageMainCamera(MainCamera __instance)
    {
        if (!VrStageController.Instance) return;

        VrStageController.Instance.SetUp(__instance);
    }
}