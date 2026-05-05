namespace TryConnect
{
    public sealed class TryConnectRegisteredItemInfo
    {
        internal TryConnectRegisteredItemInfo(string ownerGuid, string key, int spawnableId, uint assetId, string displayName, string description, SpawnableSO spawnable, bool isRegistered)
        {
            OwnerGuid = ownerGuid;
            Key = key;
            SpawnableId = spawnableId;
            AssetId = assetId;
            DisplayName = displayName;
            Description = description;
            Spawnable = spawnable;
            IsRegistered = isRegistered;
        }

        public string OwnerGuid { get; private set; }
        public string Key { get; private set; }
        public int SpawnableId { get; private set; }
        public uint AssetId { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public SpawnableSO Spawnable { get; private set; }
        public bool IsRegistered { get; private set; }
    }
}
