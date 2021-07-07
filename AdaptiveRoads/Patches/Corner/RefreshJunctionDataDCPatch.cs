namespace AdaptiveRoads.Patches.Corner {
    using AdaptiveRoads.Manager;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System.Reflection;


    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData))]
    static class RefreshJunctionDataDCPatch {
        delegate void RefreshJunctionData(
            ushort nodeID, int segmentIndex, int segmentIndex2,
            NetInfo info, NetInfo info2,
            ushort nodeSegment, ushort nodeSegment2,
            ref uint instanceIndex, ref RenderManager.Instance data);

        static MethodInfo mCaclculateCorner = typeof(NetSegment)
            .GetMethod(nameof(NetSegment.CalculateCorner), BindingFlags.Public | BindingFlags.Instance, throwOnError: true);

        static void Prefix(NetInfo info, NetInfo info2, ushort nodeSegment2 ) {
            var metadata2 = info2.GetMetaData();
            if (metadata2 == null && metadata2.UseSourceShift) {
                var metadata = info.GetMetaData();
                CalculateCornerPatchData.Shift = info.GetMetaData()?.Shift ?? 0;
                CalculateCornerPatchData.TargetSegmentID = nodeSegment2; // target segment uses source shift.
            } else {
                CalculateCornerPatchData.TargetSegmentID = 0; // use target shift.
            }
        }

        static void Postfix() {
            CalculateCornerPatchData.TargetSegmentID = 0;
        }

    }
}
