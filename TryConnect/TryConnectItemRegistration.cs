using System;
using UnityEngine;

namespace TryConnect
{
    public sealed class TryConnectItemRegistration
    {
        public string OwnerGuid { get; set; }
        public string Key { get; set; }
        public int SpawnableId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public SpawnableSO BaseSpawnable { get; set; }
        public int BaseSpawnableId { get; set; }
        public Type BaseItemComponentType { get; set; }
        public Color Tint { get; set; } = Color.white;
        public Vector3 ModelScaleMultiplier { get; set; } = Vector3.one;
        public int ReplacementChancePercent { get; set; }
        public int ExtraBasePrice { get; set; }
        public int ExtraFloorPrice { get; set; }
    }
}
