#if DEBUG
namespace AdaptiveRoads.LifeCycle
{
    using JetBrains.Annotations;
    using ICities;
    using System;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using KianCommons.Serialization;

    [UsedImplicitly]
    public class SerializableDataExtension
        : SerializableDataExtensionBase
    {
        private const string DATA_ID = "AdaptiveRoads";
        static Version DataVersion => typeof(SerializableDataExtensionBase).VersionOf();

        public override void OnLoadData()
        {
            try {
                Log.Info("SerializableDataExtension.OnLoadData() ...", true);
                byte[] data = serializableDataManager.LoadData(DATA_ID);
                if (data == null) {
                    NetworkExtensionManager.Deserialize(null);
                } else {
                    var s = SimpleDataSerializer.Reader(data);
                    NetworkExtensionManager.Deserialize(s);
                }

                Log.Info("SerializableDataExtension.OnLoadData() was successful!", true);
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public override void OnSaveData()
        {
            try {
                Log.Info("OnSaveData() ...", true);
                var man = NetworkExtensionManager.Instance;
                Assertion.AssertNotNull(DataVersion, "DataVersion");
                var s = SimpleDataSerializer.Writer(DataVersion, man.SerializationCapacity);
                man.Serialize(s);
                serializableDataManager.SaveData(DATA_ID, s.GetBytes());
                Log.Info("OnSaveData() was successful!", true);
            } catch (Exception ex) {
                ex.Log();
            }
        }
    }
}
#endif
