using System;
using TryConnect.Utils;

namespace TryConnect
{
    public static class TryConnectApi
    {
        public const int ApiVersion = 1;
        public const string PluginGuid = TryConnectPlugin.PluginGuid;

        public static bool IsInitialized
        {
            get { return TryConnectPlugin.Instance != null; }
        }

        public static int GenerateSpawnableId(string ownerGuid, string key)
        {
            ValidateIdentity(ownerGuid, key);
            return ModUtils.GetStableSpawnableId(ownerGuid, key);
        }

        public static TryConnectRegistrationResult RegisterCustomItem(TryConnectItemRegistration registration)
        {
            return RuntimeItemRegistry.RegisterCustomItem(registration);
        }

        public static bool TryGetCustomSpawnable(int spawnableId, out SpawnableSO spawnable)
        {
            spawnable = RuntimeItemRegistry.GetCustomSpawnable(spawnableId);
            return spawnable != null;
        }

        public static bool TryGetRegisteredItem(int spawnableId, out TryConnectRegisteredItemInfo itemInfo)
        {
            return RuntimeItemRegistry.TryGetRegisteredItemInfo(spawnableId, out itemInfo);
        }

        public static TryConnectRegisteredItemInfo[] GetRegisteredItems()
        {
            return RuntimeItemRegistry.GetRegisteredItemInfos();
        }

        private static void ValidateIdentity(string ownerGuid, string key)
        {
            if (string.IsNullOrWhiteSpace(ownerGuid))
            {
                throw new ArgumentException("ownerGuid is required.", "ownerGuid");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("key is required.", "key");
            }
        }
    }
}
