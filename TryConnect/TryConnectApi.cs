using System;
using System.Reflection;
using Mirror;
using TryConnect.Utils;
using UnityEngine;

namespace TryConnect
{
    public static class TryConnectApi
    {
        public const int ApiVersion = 1;
        public const string PluginGuid = TryConnectPlugin.PluginGuid;
        private static readonly FieldInfo SceneIdField = typeof(NetworkIdentity).GetField("sceneId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo HasSpawnedField = typeof(NetworkIdentity).GetField("hasSpawned", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo DestroyCalledField = typeof(NetworkIdentity).GetField("destroyCalled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo NetIdField = typeof(NetworkIdentity).GetField("<netId>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo AssetIdBackingField = typeof(NetworkIdentity).GetField("_assetId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo IsClientField = typeof(NetworkIdentity).GetField("<isClient>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo IsServerField = typeof(NetworkIdentity).GetField("<isServer>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo IsLocalPlayerField = typeof(NetworkIdentity).GetField("<isLocalPlayer>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo IsOwnedField = typeof(NetworkIdentity).GetField("<isOwned>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo SpawnedFromInstantiateField = typeof(NetworkIdentity).GetField("<SpawnedFromInstantiate>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo ConnectionToServerField = typeof(NetworkIdentity).GetField("<connectionToServer>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo ConnectionToClientField = typeof(NetworkIdentity).GetField("_connectionToClient", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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

        public static bool TryGetVanillaSpawnable(int spawnableId, out SpawnableSO spawnable)
        {
            return RuntimeItemRegistry.TryGetVanillaSpawnable(spawnableId, out spawnable);
        }

        public static bool TryFindVanillaSpawnable(string searchTerm, out SpawnableSO spawnable)
        {
            return RuntimeItemRegistry.TryFindVanillaSpawnable(searchTerm, out spawnable);
        }

        public static GameObject CreatePrefabTemplate(SpawnableSO baseSpawnable, string templateName = null)
        {
            if (baseSpawnable == null || baseSpawnable.prefab == null)
            {
                throw new ArgumentException("baseSpawnable and its prefab are required.", "baseSpawnable");
            }
            return CreatePrefabTemplate(baseSpawnable.prefab, templateName);
        }

        public static GameObject CreatePrefabTemplate(GameObject sourcePrefab, string templateName = null)
        {
            if (sourcePrefab == null)
            {
                throw new ArgumentNullException("sourcePrefab");
            }

            GameObject clone = UnityEngine.Object.Instantiate(sourcePrefab);
            clone.name = string.IsNullOrWhiteSpace(templateName) ? sourcePrefab.name + "_TryConnectTemplate" : templateName.Trim();
            clone.hideFlags = HideFlags.HideAndDontSave;
            clone.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(clone);
            PrepareRuntimeNetworkTemplate(clone);
            return clone;
        }

        public static TTarget SwapPrefabComponent<TSource, TTarget>(GameObject prefab)
            where TSource : Component
            where TTarget : TSource
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            TSource source = prefab.GetComponent<TSource>();
            if (source == null)
            {
                throw new InvalidOperationException(string.Format("Prefab '{0}' is missing component '{1}'.", prefab.name, typeof(TSource).FullName));
            }

            TTarget target = prefab.AddComponent<TTarget>();
            CopyPrefabComponentState(source, target);

            Behaviour sourceBehaviour = source as Behaviour;
            Behaviour targetBehaviour = target as Behaviour;
            if (sourceBehaviour != null && targetBehaviour != null)
            {
                targetBehaviour.enabled = sourceBehaviour.enabled;
            }

            UnityEngine.Object.DestroyImmediate(source);
            return target;
        }

        public static TryConnectRegisteredItemInfo[] GetRegisteredItems()
        {
            return RuntimeItemRegistry.GetRegisteredItemInfos();
        }

        internal static void PrepareRuntimeNetworkTemplate(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            NetworkIdentity[] identities = prefab.GetComponentsInChildren<NetworkIdentity>(true);
            for (int i = 0; i < identities.Length; i++)
            {
                NetworkIdentity identity = identities[i];
                if (identity == null)
                {
                    continue;
                }

                if (SceneIdField != null)
                {
                    SceneIdField.SetValue(identity, 0ul);
                }
                if (HasSpawnedField != null)
                {
                    HasSpawnedField.SetValue(identity, false);
                }
                if (DestroyCalledField != null)
                {
                    DestroyCalledField.SetValue(identity, false);
                }
                if (NetIdField != null)
                {
                    NetIdField.SetValue(identity, 0u);
                }
                if (AssetIdBackingField != null)
                {
                    AssetIdBackingField.SetValue(identity, 0u);
                }
                if (IsClientField != null)
                {
                    IsClientField.SetValue(identity, false);
                }
                if (IsServerField != null)
                {
                    IsServerField.SetValue(identity, false);
                }
                if (IsLocalPlayerField != null)
                {
                    IsLocalPlayerField.SetValue(identity, false);
                }
                if (IsOwnedField != null)
                {
                    IsOwnedField.SetValue(identity, false);
                }
                if (SpawnedFromInstantiateField != null)
                {
                    SpawnedFromInstantiateField.SetValue(identity, false);
                }
                if (ConnectionToClientField != null)
                {
                    ConnectionToClientField.SetValue(identity, null);
                }
                if (ConnectionToServerField != null)
                {
                    ConnectionToServerField.SetValue(identity, null);
                }
            }
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

        private static void CopyPrefabComponentState(Component source, Component target)
        {
            Type type = source.GetType();
            while (type != null && type != typeof(NetworkBehaviour))
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (!field.IsStatic && !field.IsInitOnly)
                    {
                        field.SetValue(target, field.GetValue(source));
                    }
                }
                type = type.BaseType;
            }
        }
    }
}
