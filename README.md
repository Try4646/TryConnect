## How to Use TryConnect in Your Mod (WIP!!)

1. **Add TryConnect as a dependency in your mod.**
2. **Register your custom item in the `Awake` method.**


```cs
using System;
using BepInEx;
using TryConnect;
using UnityEngine;

[BepInDependency(TryConnectApi.PluginGuid)]
[BepInPlugin("com.example.house-special", "House Special", "1.0.0")]
public sealed class HouseSpecialPlugin : BaseUnityPlugin
{
    private const string BundlePath = "sphere.assetbundle";
    private const string VisualPrefabName = "sphere";

    private void Awake()
    {
        SpawnableSO drinkSpawnable = TryConnectApi.FindVanillaSpawnable<Drink>();
        if (drinkSpawnable == null)
        {
            Logger.LogError("Could not resolve a vanilla Drink spawnable.");
            return;
        }

        GameObject customPrefab = TryConnectApi.CreatePrefabTemplate(drinkSpawnable, "HouseSpecialPrefab");
        GameObject markerPrefab = TryConnectApi.CreatePrefabTemplate(drinkSpawnable, "HouseSpecialMarker");

        TryConnectApi.SwapPrefabComponent<Drink, HouseSpecialDrink>(customPrefab);

        AssetBundle bundle = null;
        try
        {
            bundle = TryConnectAssetBundles.LoadRelativeToPlugin(this, BundlePath);
            GameObject visualPrefab = TryConnectAssetBundles.LoadAsset<GameObject>(bundle, VisualPrefabName);

            TryConnectApi.ReplaceVisualsWithPrefab(
                customPrefab,
                visualPrefab,
                Vector3.one * 0.5f,
                replaceColliders: true);

            TryConnectApi.ReplaceVisualsWithPrefab(
                markerPrefab,
                visualPrefab,
                Vector3.one * 0.5f,
                replaceColliders: true);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load sphere bundle content: {ex}");
            return;
        }
        finally
        {
            if (bundle != null)
            {
                bundle.Unload(false);
            }
        }

        TryConnectRegistrationResult result = TryConnectApi.RegisterCustomItem(
            new TryConnectItemRegistration
            {
                OwnerGuid = Info.Metadata.GUID,
                Key = "house_special",
                DisplayName = "House Special",
                Description = "TryConnect test custom drink.",
                BaseSpawnable = drinkSpawnable,
                CustomPrefab = customPrefab,
                MarkerPrefab = markerPrefab,
                ApplyTint = true,
                Tint = Color.magenta,
                ReplacementChancePercent = 100,
                ExtraBasePrice = 1,
                ExtraFloorPrice = 1
            });

        Logger.LogInfo($"TryConnect register result: {result}");
    }
}

public sealed class HouseSpecialDrink : Drink
{
    private bool _extraEffectAppliedThisPress;

    protected override void OnUseItem(bool isPressed)
    {
        base.OnUseItem(isPressed);

        if (!isPressed)
        {
            _extraEffectAppliedThisPress = false;
            return;
        }

        if (_extraEffectAppliedThisPress || !isServer)
        {
            return;
        }

        _extraEffectAppliedThisPress = true;

        PlayerInventory holder = NetworkHolder;
        if (holder == null)
        {
            return;
        }

        PlayerBuff buff = holder.GetComponent<PlayerBuff>();
        if (buff != null)
        {
            buff.ApplyBuff(PlayerBuffType.TipsyFortune, 1, 5f);
        }

        Debug.Log("[House Special] Extra server-side effect applied.");
    }
}

```

**Explanation:**
- Add `[BepInDependency(TryConnectApi.PluginGuid)]` to make sure TryConnect is loaded first.
- Create a `TryConnectItemRegistration` object with your custom item details.
- Call `TryConnectApi.RegisterCustomItem(registration)` to register your item.
- Log the result for debugging.

**Result:**  
Your custom item will now appear in the shop with the specified chance and properties, managed by TryConnect.

