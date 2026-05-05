using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Extensions;
using HarmonyLib;
using Mirror;
using MoreMountains.Tools;
using TryConnect.Utils;
using UnityEngine;

namespace TryConnect
{
    internal static class RuntimeItemRegistry
    {
        private const int TicketFizzSpawnableId = 900001;
        private const int LoadedChipSpawnableId = 900002;

        private static readonly Dictionary<int, CustomItemRequest> RequestsBySpawnableId = new Dictionary<int, CustomItemRequest>();
        private static readonly Dictionary<string, CustomItemRequest> RequestsByUniqueKey = new Dictionary<string, CustomItemRequest>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<int, CustomItemDefinition> DefinitionsBySpawnableId = new Dictionary<int, CustomItemDefinition>();
        private static readonly Dictionary<uint, CustomItemDefinition> DefinitionsByAssetId = new Dictionary<uint, CustomItemDefinition>();
        private static readonly Dictionary<GameObject, CustomItemDefinition> DefinitionsByMarkerPrefab = new Dictionary<GameObject, CustomItemDefinition>();
        private static readonly Dictionary<GameObject, List<CustomItemDefinition>> DefinitionsByBasePrefab = new Dictionary<GameObject, List<CustomItemDefinition>>();
        private static readonly MethodInfo SetParentHierarchyAfterDelayMethod = AccessTools.Method(typeof(ItemStamp), "SetParentHierarchyAfterDelay");
        private static readonly FieldInfo AssetIdField = AccessTools.Field(typeof(NetworkIdentity), "_assetId");
        private static readonly FieldInfo SpawnHandlersField = AccessTools.Field(typeof(NetworkClient), "spawnHandlers");

        private static TryConnectPlugin _plugin;
        private static bool _builtInRegistrationsAdded;

        internal static void Initialize(TryConnectPlugin plugin)
        {
            _plugin = plugin;
            EnsureBuiltInRegistrations();
        }

        internal static void Dispose()
        {
            foreach (CustomItemDefinition definition in DefinitionsBySpawnableId.Values)
            {
                NetworkClient.UnregisterSpawnHandler(definition.AssetId);
                if (definition.MarkerPrefab != null)
                {
                    UnityEngine.Object.Destroy(definition.MarkerPrefab);
                }
                if (definition.Spawnable != null)
                {
                    UnityEngine.Object.Destroy(definition.Spawnable);
                }
            }
            DefinitionsBySpawnableId.Clear();
            DefinitionsByAssetId.Clear();
            DefinitionsByMarkerPrefab.Clear();
            DefinitionsByBasePrefab.Clear();
            _plugin = null;
        }

        internal static TryConnectRegistrationResult RegisterCustomItem(TryConnectItemRegistration registration)
        {
            EnsureBuiltInRegistrations();
            if (!TryNormalizeRegistration(registration))
            {
                return TryConnectRegistrationResult.InvalidRegistration;
            }

            string uniqueKey = BuildUniqueKey(registration.OwnerGuid, registration.Key);
            if (RequestsByUniqueKey.ContainsKey(uniqueKey) || RequestsBySpawnableId.ContainsKey(registration.SpawnableId))
            {
                return TryConnectRegistrationResult.AlreadyRegistered;
            }

            AddRequest(CreateRequest(registration));
            TryRegisterRuntimeContent();
            return TryConnectRegistrationResult.Accepted;
        }

        internal static void TryRegisterRuntimeContent()
        {
            EnsureBuiltInRegistrations();
            if (_plugin == null)
            {
                return;
            }

            SpawnableSettings spawnableSettings = Resources.Load<SpawnableSettings>("SpawnableSettings");
            if (spawnableSettings == null || !spawnableSettings.isEnabled)
            {
                return;
            }

            ItemPriceSettings priceSettings = Resources.Load<ItemPriceSettings>("ItemPriceSettings");
            bool changed = false;

            foreach (CustomItemRequest request in RequestsBySpawnableId.Values)
            {
                changed |= EnsureDefinition(spawnableSettings, priceSettings, request);
            }
            TryEnsureAllClientRegistrations();

            if (changed)
            {
                spawnableSettings.NotifyChanged();
                TryConnectPlugin.Log.LogInfo(string.Format("Registered {0} runtime custom item(s).", DefinitionsBySpawnableId.Count));
            }
        }

        internal static void TryEnsureAllClientRegistrations()
        {
            foreach (CustomItemDefinition definition in DefinitionsBySpawnableId.Values)
            {
                EnsureClientSpawnHandler(definition);
            }
        }

        internal static void TryEnsureClientRegistration(uint assetId)
        {
            CustomItemDefinition definition;
            if (DefinitionsByAssetId.TryGetValue(assetId, out definition))
            {
                EnsureClientSpawnHandler(definition);
            }
        }

        internal static SpawnableSO GetCustomSpawnable(int id)
        {
            CustomItemDefinition definition;
            if (DefinitionsBySpawnableId.TryGetValue(id, out definition))
            {
                return definition.Spawnable;
            }
            return null;
        }

        internal static bool TryGetRegisteredItemInfo(int spawnableId, out TryConnectRegisteredItemInfo itemInfo)
        {
            itemInfo = null;

            CustomItemRequest request;
            if (!RequestsBySpawnableId.TryGetValue(spawnableId, out request))
            {
                return false;
            }

            CustomItemDefinition definition;
            bool isRegistered = DefinitionsBySpawnableId.TryGetValue(spawnableId, out definition);
            itemInfo = new TryConnectRegisteredItemInfo(
                request.OwnerGuid,
                request.Key,
                request.SpawnableId,
                request.AssetId,
                request.DisplayName,
                request.Description,
                isRegistered ? definition.Spawnable : null,
                isRegistered);
            return true;
        }

        internal static TryConnectRegisteredItemInfo[] GetRegisteredItemInfos()
        {
            List<TryConnectRegisteredItemInfo> items = new List<TryConnectRegisteredItemInfo>(RequestsBySpawnableId.Count);
            foreach (CustomItemRequest request in RequestsBySpawnableId.Values)
            {
                CustomItemDefinition definition;
                bool isRegistered = DefinitionsBySpawnableId.TryGetValue(request.SpawnableId, out definition);
                items.Add(new TryConnectRegisteredItemInfo(
                    request.OwnerGuid,
                    request.Key,
                    request.SpawnableId,
                    request.AssetId,
                    request.DisplayName,
                    request.Description,
                    isRegistered ? definition.Spawnable : null,
                    isRegistered));
            }
            items.Sort(delegate(TryConnectRegisteredItemInfo left, TryConnectRegisteredItemInfo right)
            {
                return left.SpawnableId.CompareTo(right.SpawnableId);
            });
            return items.ToArray();
        }

        internal static bool TryGetCustomBasePrice(SpawnableSO spawnableSO, ref int basePrice)
        {
            CustomItemDefinition definition;
            if (!TryGetDefinition(spawnableSO, out definition))
            {
                return false;
            }
            basePrice = definition.BasePrice;
            return true;
        }

        internal static bool TryGetCustomFloorPrice(SpawnableSO spawnableSO, ref int priceIncreasePerFloor)
        {
            CustomItemDefinition definition;
            if (!TryGetDefinition(spawnableSO, out definition))
            {
                return false;
            }
            priceIncreasePerFloor = definition.PriceIncreasePerFloor;
            return true;
        }

        internal static bool TryGetCustomDescription(SpawnableSO spawnableSO, ref string description)
        {
            CustomItemDefinition definition;
            if (!TryGetDefinition(spawnableSO, out definition))
            {
                return false;
            }
            description = definition.Description;
            return true;
        }

        internal static GameObject GetShopReplacement(MMLootTableGameObjectSO lootTable, Vector3 stampPosition, GameObject fallbackPrefab)
        {
            if (fallbackPrefab == null)
            {
                return null;
            }

            List<CustomItemDefinition> candidates;
            if (!DefinitionsByBasePrefab.TryGetValue(fallbackPrefab, out candidates) || candidates.Count == 0)
            {
                return fallbackPrefab;
            }

            int seed = 0;
            if (NetworkSingleton<SeededRandomManager>.Instance != null)
            {
                seed = NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
            }
            string lootTableName = (lootTable != null) ? lootTable.name : "ItemStamp";
            int roll = ModUtils.GetDeterministicPercent(stampPosition, seed, lootTableName + "|" + fallbackPrefab.name);

            int cursor = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                cursor += candidates[i].ReplacementChancePercent;
                if (roll < cursor)
                {
                    return candidates[i].MarkerPrefab;
                }
            }

            return fallbackPrefab;
        }

        internal static bool TrySpawnCustomItemStampPrefab(ItemStamp stamp, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (stamp == null)
            {
                return false;
            }
            return TrySpawnRegisteredPrefab(prefab, position, rotation, scale, stamp);
        }

        internal static bool TrySpawnConsolePrefab(GameObject prefab, Vector3 position)
        {
            return TrySpawnRegisteredPrefab(prefab, position, Quaternion.identity, Vector3.one, null);
        }

        private static bool TrySpawnRegisteredPrefab(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, ItemStamp stamp)
        {
            CustomItemDefinition definition;
            if (prefab == null || !DefinitionsByMarkerPrefab.TryGetValue(prefab, out definition))
            {
                return false;
            }

            GameObject spawnedObject = UnityEngine.Object.Instantiate(definition.BasePrefab, position, rotation);
            ApplyDefinitionToInstance(definition, spawnedObject, scale);
            NetworkServer.Spawn(spawnedObject, definition.AssetId, null);
            TryConnectPlugin.Log.LogInfo(string.Format("Spawned custom item '{0}'.", definition.DisplayName));

            if (stamp != null && NetworkSingleton<ItemStampManager>.Instance != null)
            {
                NetworkSingleton<ItemStampManager>.Instance.RegisterSpawnedInstance(spawnedObject, stamp);
                StartParentHierarchyCoroutine(stamp, spawnedObject);
            }

            return true;
        }

        private static void StartParentHierarchyCoroutine(ItemStamp stamp, GameObject spawnedObject)
        {
            if (SetParentHierarchyAfterDelayMethod == null)
            {
                return;
            }

            IEnumerator routine = SetParentHierarchyAfterDelayMethod.Invoke(stamp, new object[]
            {
                spawnedObject
            }) as IEnumerator;
            if (routine != null)
            {
                stamp.StartCoroutine(routine);
            }
        }

        private static bool EnsureDefinition(SpawnableSettings spawnableSettings, ItemPriceSettings priceSettings, CustomItemRequest request)
        {
            if (request == null)
            {
                return false;
            }

            CustomItemDefinition definition;
            if (!DefinitionsBySpawnableId.TryGetValue(request.SpawnableId, out definition))
            {
                definition = BuildDefinition(spawnableSettings, priceSettings, request);
                if (definition == null)
                {
                    return false;
                }
                CacheDefinition(definition);
            }

            return EnsureDefinition(spawnableSettings, definition);
        }

        private static bool EnsureDefinition(SpawnableSettings spawnableSettings, CustomItemDefinition definition)
        {
            if (definition == null || definition.Spawnable == null)
            {
                return false;
            }

            bool alreadyPresent = false;
            for (int i = 0; i < spawnableSettings.spawnables.Count; i++)
            {
                SpawnableSO existing = spawnableSettings.spawnables[i];
                if (existing != null && existing.spawnableID == definition.Spawnable.spawnableID)
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (!alreadyPresent)
            {
                spawnableSettings.spawnables.Add(definition.Spawnable);
                return true;
            }

            return false;
        }

        private static void EnsureBuiltInRegistrations()
        {
            if (_builtInRegistrationsAdded || _plugin == null || TryConnectPlugin.EnableCustomItems == null || !TryConnectPlugin.EnableCustomItems.Value)
            {
                return;
            }

            _builtInRegistrationsAdded = true;
            AddRequest(new CustomItemRequest
            {
                OwnerGuid = TryConnectPlugin.PluginGuid,
                Key = "ticket_fizz",
                SpawnableId = TicketFizzSpawnableId,
                AssetId = ModUtils.GetStableAssetId(TryConnectPlugin.PluginGuid, "ticket_fizz"),
                DisplayName = "Ticket Fizz",
                Description = "A TryConnect house drink. Behaves like the vanilla Drink, but shows up as injected custom shop loot.",
                BaseItemComponentType = typeof(Drink),
                Tint = new Color(0.22f, 0.95f, 0.92f),
                ModelScaleMultiplier = new Vector3(1.08f, 1.08f, 1.08f),
                ReplacementChancePercent = ModUtils.ClampPercent(TryConnectPlugin.TicketFizzReplacementChance.Value),
                ExtraBasePrice = 2,
                ExtraFloorPrice = 1
            });

            AddRequest(new CustomItemRequest
            {
                OwnerGuid = TryConnectPlugin.PluginGuid,
                Key = "loaded_chip",
                SpawnableId = LoadedChipSpawnableId,
                AssetId = ModUtils.GetStableAssetId(TryConnectPlugin.PluginGuid, "loaded_chip"),
                DisplayName = "Loaded Chip",
                Description = "A counterfeit high-roller token. Behaves like the vanilla Golden Chip when applied to a game.",
                BaseItemComponentType = typeof(GoldenChip),
                Tint = new Color(1f, 0.72f, 0.12f),
                ModelScaleMultiplier = new Vector3(1.12f, 1.12f, 1.12f),
                ReplacementChancePercent = ModUtils.ClampPercent(TryConnectPlugin.LoadedChipReplacementChance.Value),
                ExtraBasePrice = 3,
                ExtraFloorPrice = 1
            });
        }

        private static void AddRequest(CustomItemRequest request)
        {
            RequestsBySpawnableId.Add(request.SpawnableId, request);
            RequestsByUniqueKey.Add(BuildUniqueKey(request.OwnerGuid, request.Key), request);
        }

        private static CustomItemRequest CreateRequest(TryConnectItemRegistration registration)
        {
            return new CustomItemRequest
            {
                OwnerGuid = registration.OwnerGuid,
                Key = registration.Key,
                SpawnableId = registration.SpawnableId,
                AssetId = ModUtils.GetStableAssetId(registration.OwnerGuid, registration.Key),
                DisplayName = registration.DisplayName,
                Description = registration.Description ?? string.Empty,
                BaseSpawnable = registration.BaseSpawnable,
                BaseSpawnableId = registration.BaseSpawnableId,
                BaseItemComponentType = registration.BaseItemComponentType,
                Tint = registration.Tint,
                ModelScaleMultiplier = registration.ModelScaleMultiplier,
                ReplacementChancePercent = ModUtils.ClampPercent(registration.ReplacementChancePercent),
                ExtraBasePrice = registration.ExtraBasePrice,
                ExtraFloorPrice = registration.ExtraFloorPrice
            };
        }

        private static bool TryNormalizeRegistration(TryConnectItemRegistration registration)
        {
            if (registration == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(registration.OwnerGuid) || string.IsNullOrWhiteSpace(registration.Key) || string.IsNullOrWhiteSpace(registration.DisplayName))
            {
                return false;
            }
            if (registration.BaseSpawnable == null && registration.BaseSpawnableId == 0 && registration.BaseItemComponentType == null)
            {
                return false;
            }
            if (registration.BaseItemComponentType != null && !typeof(Component).IsAssignableFrom(registration.BaseItemComponentType))
            {
                return false;
            }

            registration.OwnerGuid = registration.OwnerGuid.Trim();
            registration.Key = registration.Key.Trim();
            registration.DisplayName = registration.DisplayName.Trim();
            registration.Description = registration.Description ?? string.Empty;
            if (registration.SpawnableId == 0)
            {
                registration.SpawnableId = ModUtils.GetStableSpawnableId(registration.OwnerGuid, registration.Key);
            }
            return true;
        }

        private static string BuildUniqueKey(string ownerGuid, string key)
        {
            return ownerGuid + ":" + key;
        }

        private static void CacheDefinition(CustomItemDefinition definition)
        {
            DefinitionsBySpawnableId.Add(definition.Spawnable.spawnableID, definition);
            DefinitionsByAssetId.Add(definition.AssetId, definition);
            DefinitionsByMarkerPrefab.Add(definition.MarkerPrefab, definition);

            List<CustomItemDefinition> definitionsForBasePrefab;
            if (!DefinitionsByBasePrefab.TryGetValue(definition.BasePrefab, out definitionsForBasePrefab))
            {
                definitionsForBasePrefab = new List<CustomItemDefinition>();
                DefinitionsByBasePrefab.Add(definition.BasePrefab, definitionsForBasePrefab);
            }
            definitionsForBasePrefab.Add(definition);
            definitionsForBasePrefab.Sort(delegate(CustomItemDefinition left, CustomItemDefinition right)
            {
                return left.Spawnable.spawnableID.CompareTo(right.Spawnable.spawnableID);
            });
        }

        private static CustomItemDefinition BuildDefinition(SpawnableSettings spawnableSettings, ItemPriceSettings priceSettings, CustomItemRequest request)
        {
            SpawnableSO baseSpawnable = ResolveBaseSpawnable(spawnableSettings, request);
            if (baseSpawnable == null || baseSpawnable.prefab == null)
            {
                LogBuildFailureOnce(request, "Could not resolve a base spawnable.");
                return null;
            }

            GameObject markerPrefab = UnityEngine.Object.Instantiate(baseSpawnable.prefab);
            markerPrefab.name = request.Key + "_Marker";
            markerPrefab.hideFlags = HideFlags.HideAndDontSave;
            markerPrefab.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(markerPrefab);

            Item markerItem = markerPrefab.GetComponent<Item>();
            if (markerItem == null)
            {
                UnityEngine.Object.Destroy(markerPrefab);
                LogBuildFailureOnce(request, string.Format("Failed to create custom marker for '{0}' because the base prefab is missing Item.", request.DisplayName));
                return null;
            }

            SpawnableSO spawnable = ScriptableObject.CreateInstance<SpawnableSO>();
            spawnable.hideFlags = HideFlags.HideAndDontSave;
            spawnable.name = request.Key + "_SpawnableSO";
            spawnable.spawnableID = request.SpawnableId;
            spawnable.spawnableName = request.DisplayName;
            spawnable.spawnableDescription = request.Description;
            spawnable.prefab = markerPrefab;
            markerItem.spawnableSo = spawnable;

            int basePrice = 4;
            int priceIncreasePerFloor = 1;
            if (priceSettings != null)
            {
                basePrice = Math.Max(1, priceSettings.GetBasePrice(baseSpawnable) + request.ExtraBasePrice);
                priceIncreasePerFloor = Math.Max(0, priceSettings.GetPriceIncreasePerFloor(baseSpawnable) + request.ExtraFloorPrice);
            }

            CustomItemDefinition definition = new CustomItemDefinition();
            definition.OwnerGuid = request.OwnerGuid;
            definition.Key = request.Key;
            definition.AssetId = request.AssetId;
            definition.BasePrefab = baseSpawnable.prefab;
            definition.BasePrice = basePrice;
            definition.Description = request.Description;
            definition.DisplayName = request.DisplayName;
            definition.MarkerPrefab = markerPrefab;
            definition.ModelScaleMultiplier = request.ModelScaleMultiplier;
            definition.PriceIncreasePerFloor = priceIncreasePerFloor;
            definition.ReplacementChancePercent = request.ReplacementChancePercent;
            definition.Spawnable = spawnable;
            definition.Tint = request.Tint;

            ApplyDefinitionToMarker(definition, markerPrefab);
            EnsureClientSpawnHandler(definition);
            return definition;
        }

        private static void LogBuildFailureOnce(CustomItemRequest request, string message)
        {
            if (request == null || request.HasLoggedBuildFailure)
            {
                return;
            }

            request.HasLoggedBuildFailure = true;
            if (TryConnectPlugin.Log != null)
            {
                TryConnectPlugin.Log.LogWarning(string.Format("Custom item '{0}' ({1}) could not be registered yet. {2}", request.DisplayName, request.OwnerGuid, message));
            }
        }

        private static SpawnableSO ResolveBaseSpawnable(SpawnableSettings spawnableSettings, CustomItemRequest request)
        {
            if (request.BaseSpawnable != null)
            {
                return request.BaseSpawnable;
            }
            if (request.BaseSpawnableId != 0)
            {
                return FindBaseSpawnableById(spawnableSettings, request.BaseSpawnableId);
            }
            if (request.BaseItemComponentType != null)
            {
                return FindBaseSpawnable(spawnableSettings, request.BaseItemComponentType);
            }
            return null;
        }

        private static SpawnableSO FindBaseSpawnableById(SpawnableSettings spawnableSettings, int spawnableId)
        {
            for (int i = 0; i < spawnableSettings.spawnables.Count; i++)
            {
                SpawnableSO candidate = spawnableSettings.spawnables[i];
                if (candidate != null && candidate.spawnableID == spawnableId)
                {
                    return candidate;
                }
            }
            return null;
        }

        private static SpawnableSO FindBaseSpawnable(SpawnableSettings spawnableSettings, Type componentType)
        {
            for (int i = 0; i < spawnableSettings.spawnables.Count; i++)
            {
                SpawnableSO candidate = spawnableSettings.spawnables[i];
                if (candidate != null && candidate.prefab != null && candidate.prefab.GetComponent(componentType) != null)
                {
                    return candidate;
                }
            }
            return null;
        }

        private static void ApplyDefinitionToMarker(CustomItemDefinition definition, GameObject markerPrefab)
        {
            if (markerPrefab == null)
            {
                return;
            }
            markerPrefab.name = definition.DisplayName;
            ApplyDefinitionVisuals(definition, markerPrefab.transform);
            Item item = markerPrefab.GetComponent<Item>();
            if (item != null)
            {
                item.spawnableSo = definition.Spawnable;
            }
        }

        private static void ApplyDefinitionToInstance(CustomItemDefinition definition, GameObject spawnedObject, Vector3 scale)
        {
            spawnedObject.name = definition.DisplayName;
            spawnedObject.transform.localScale = scale;
            spawnedObject.SetActive(true);

            NetworkIdentity networkIdentity = spawnedObject.GetComponent<NetworkIdentity>();
            if (networkIdentity != null)
            {
                SetNetworkAssetId(networkIdentity, definition.AssetId);
            }

            Item item = spawnedObject.GetComponent<Item>();
            if (item != null)
            {
                item.spawnableSo = definition.Spawnable;
            }

            ApplyDefinitionVisuals(definition, spawnedObject.transform);
        }

        private static void ApplyDefinitionVisuals(CustomItemDefinition definition, Transform root)
        {
            Item itemComponent = root.GetComponent<Item>();
            Transform modelTransform = (itemComponent != null) ? itemComponent.modelTransform : root.Find("Model");
            if (modelTransform == null)
            {
                modelTransform = root;
            }
            modelTransform.localScale = Vector3.Scale(Vector3.one, definition.ModelScaleMultiplier);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }
                Material[] materials = renderer.materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material material = materials[j];
                    if (ModUtils.TrySetMaterialTint(material, definition.Tint) && material.HasProperty("_Metallic"))
                    {
                        material.SetFloat("_Metallic", Mathf.Clamp01(material.GetFloat("_Metallic") + 0.15f));
                    }
                }
                renderer.materials = materials;
            }
        }

        private static void EnsureClientSpawnHandler(CustomItemDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            IDictionary spawnHandlers = SpawnHandlersField != null ? SpawnHandlersField.GetValue(null) as IDictionary : null;
            if (spawnHandlers != null && spawnHandlers.Contains(definition.AssetId))
            {
                return;
            }

            NetworkClient.RegisterSpawnHandler(definition.AssetId, SpawnCustomPrefab, UnspawnCustomPrefab);
            TryConnectPlugin.Log.LogInfo(string.Format("Registered custom spawn handler for '{0}' ({1}).", definition.DisplayName, definition.AssetId));
        }

        private static void SetNetworkAssetId(NetworkIdentity networkIdentity, uint assetId)
        {
            if (AssetIdField != null)
            {
                AssetIdField.SetValue(networkIdentity, assetId);
            }
        }

        private static bool TryGetDefinition(SpawnableSO spawnableSO, out CustomItemDefinition definition)
        {
            definition = null;
            if (spawnableSO == null)
            {
                return false;
            }
            return DefinitionsBySpawnableId.TryGetValue(spawnableSO.spawnableID, out definition);
        }

        private static GameObject SpawnCustomPrefab(SpawnMessage message)
        {
            CustomItemDefinition definition;
            if (!DefinitionsByAssetId.TryGetValue(message.assetId, out definition))
            {
                return null;
            }

            GameObject spawnedObject = UnityEngine.Object.Instantiate(definition.BasePrefab, message.position, message.rotation);
            ApplyDefinitionToInstance(definition, spawnedObject, message.scale);
            return spawnedObject;
        }

        private static void UnspawnCustomPrefab(GameObject spawnedObject)
        {
            if (spawnedObject != null)
            {
                UnityEngine.Object.Destroy(spawnedObject);
            }
        }

        private sealed class CustomItemDefinition
        {
            internal string OwnerGuid;
            internal string Key;
            internal uint AssetId;
            internal GameObject BasePrefab;
            internal int BasePrice;
            internal string Description;
            internal string DisplayName;
            internal GameObject MarkerPrefab;
            internal Vector3 ModelScaleMultiplier;
            internal int PriceIncreasePerFloor;
            internal int ReplacementChancePercent;
            internal SpawnableSO Spawnable;
            internal Color Tint;
        }

        private sealed class CustomItemRequest
        {
            internal string OwnerGuid;
            internal string Key;
            internal int SpawnableId;
            internal uint AssetId;
            internal string DisplayName;
            internal string Description;
            internal SpawnableSO BaseSpawnable;
            internal int BaseSpawnableId;
            internal Type BaseItemComponentType;
            internal Color Tint;
            internal Vector3 ModelScaleMultiplier;
            internal int ReplacementChancePercent;
            internal int ExtraBasePrice;
            internal int ExtraFloorPrice;
            internal bool HasLoggedBuildFailure;
        }
    }
}
