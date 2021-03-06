namespace AdaptiveRoads.Patches {
    using KianCommons.Plugins;
    using System.Reflection;
    using HarmonyLib;

    [HarmonyPatch]
    [PreloadPatch]
    public static class HideCrosswalksPatch {
        public static MethodBase TargetMethod() {
            return PluginUtil.GetHideCrossings().GetMainAssembly()
                .GetType("HideCrosswalks.Utils.RoadUtils", throwOnError: true)
                .GetMethod("CalculateCanHideCrossingsRaw", BindingFlags.NonPublic | BindingFlags.Static);
        }

        static bool Prepare(MethodBase original) {
            if (original == null)
                return PluginUtil.GetHideCrossings().IsActive();
            return true;
        }
        public static bool Prefix(NetInfo info, ref bool __result) {
            //Log.Debug("IsNormalSymetricalTwoWay.Prefix() was called for info:" + info);
            if(info.IsAdaptive()) {
                __result = false;
                return false;
            }
            return true;
        }
    }

}
