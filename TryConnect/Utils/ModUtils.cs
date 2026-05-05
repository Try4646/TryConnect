using UnityEngine;

namespace TryConnect.Utils
{
    internal static class ModUtils
    {
        internal static uint GetStableAssetId(string key)
        {
            return GetStableAssetId(TryConnectPlugin.MyGUID, key);
        }

        internal static uint GetStableAssetId(string ownerGuid, string key)
        {
            uint hash = GetStableHash(ownerGuid + ":" + key);
            if (hash == 0u)
            {
                hash = 1u;
            }
            return hash;
        }

        internal static int GetStableSpawnableId(string ownerGuid, string key)
        {
            uint hash = GetStableHash(ownerGuid + ":" + key);
            return (int)(1073741824u | hash & 1073741823u);
        }

        internal static int ClampPercent(int value)
        {
            return Mathf.Clamp(value, 0, 100);
        }

        internal static int GetDeterministicPercent(Vector3 position, int seed, string key)
        {
            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ Mathf.RoundToInt(position.x * 100f);
                hash = hash * 397 ^ Mathf.RoundToInt(position.y * 100f);
                hash = hash * 397 ^ Mathf.RoundToInt(position.z * 100f);
                for (int i = 0; i < key.Length; i++)
                {
                    hash = hash * 397 ^ key[i];
                }
                if (hash == int.MinValue)
                {
                    hash = int.MaxValue;
                }
                return Mathf.Abs(hash) % 100;
            }
        }

        internal static bool TrySetMaterialTint(Material material, Color tint)
        {
            if (material == null)
            {
                return false;
            }
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
                return true;
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
                return true;
            }
            return false;
        }

        private static uint GetStableHash(string text)
        {
            uint hash = 2166136261u;
            for (int i = 0; i < text.Length; i++)
            {
                hash ^= text[i];
                hash *= 16777619u;
            }
            return hash;
        }
    }
}
