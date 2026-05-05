using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Mirror;
using MoreMountains.Tools;
using UnityEngine;

namespace TryConnect.Patches
{
    [HarmonyPatch(typeof(SpawnableSettings), nameof(SpawnableSettings.GetSpawnableSoById))]
    internal static class SpawnableSettingsPatches
    {
        private static void Postfix(int id, ref SpawnableSO __result)
        {
            if (__result == null)
            {
                __result = RuntimeItemRegistry.GetCustomSpawnable(id);
            }
        }
    }

    [HarmonyPatch(typeof(ItemPriceSettings), nameof(ItemPriceSettings.GetBasePrice))]
    internal static class ItemPriceSettingsBasePricePatches
    {
        private static bool Prefix(SpawnableSO spawnableSO, ref int __result)
        {
            return !RuntimeItemRegistry.TryGetCustomBasePrice(spawnableSO, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemPriceSettings), nameof(ItemPriceSettings.GetPriceIncreasePerFloor))]
    internal static class ItemPriceSettingsFloorPricePatches
    {
        private static bool Prefix(SpawnableSO spawnableSO, ref int __result)
        {
            return !RuntimeItemRegistry.TryGetCustomFloorPrice(spawnableSO, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDescriptionSettings), nameof(ItemDescriptionSettings.GetDescription))]
    internal static class ItemDescriptionSettingsPatches
    {
        private static bool Prefix(SpawnableSO spawnableSO, ref string __result)
        {
            return !RuntimeItemRegistry.TryGetCustomDescription(spawnableSO, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemStampManager), nameof(ItemStampManager.GetUniqueLoot), typeof(MMLootTableGameObjectSO), typeof(Vector3))]
    internal static class ItemStampManagerLootPatches
    {
        private static void Postfix(MMLootTableGameObjectSO lootTable, Vector3 stampPosition, ref GameObject __result)
        {
            __result = RuntimeItemRegistry.GetShopReplacement(lootTable, stampPosition, __result);
        }
    }

    [HarmonyPatch(typeof(ItemStamp), "SpawnItem")]
    internal static class ItemStampSpawnPatches
    {
        private static bool Prefix(ItemStamp __instance, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return !RuntimeItemRegistry.TrySpawnCustomItemStampPrefab(__instance, prefab, position, rotation, scale);
        }
    }

    [HarmonyPatch(typeof(NewConsole), "SpawnPrefabServer")]
    internal static class NewConsoleSpawnPatches
    {
        private static bool Prefix(GameObject prefab, Vector3 position)
        {
            return !RuntimeItemRegistry.TrySpawnConsolePrefab(prefab, position);
        }
    }

    [HarmonyPatch]
    internal static class CustomNetworkManagerPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnStartServer));
            yield return AccessTools.Method(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnClientConnect));
            yield return AccessTools.Method(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnClientSceneChanged));
        }

        private static void Postfix()
        {
            RuntimeItemRegistry.TryRegisterRuntimeContent();
        }
    }

    [HarmonyPatch]
    internal static class NetworkClientSpawnLifecyclePatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(NetworkClient), "ClearSpawners");
        }

        private static void Postfix()
        {
            RuntimeItemRegistry.TryEnsureAllClientRegistrations();
        }
    }

    [HarmonyPatch(typeof(NetworkClient), "OnSpawn")]
    internal static class NetworkClientOnSpawnPatches
    {
        private static void Prefix(SpawnMessage message)
        {
            RuntimeItemRegistry.TryEnsureClientRegistration(message.assetId);
        }
    }
}
