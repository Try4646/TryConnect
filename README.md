## How to Use TryConnect in Your Mod (WIP!!)

1. **Add TryConnect as a dependency in your mod.**
2. **Register your custom item in the `Awake` method.**


```cs
using System.Collections;
using BepInEx;
using TryConnect;
using UnityEngine;

[BepInDependency(TryConnectApi.PluginGuid)]
[BepInPlugin("com.try-4646.Heroin-Mod", "Heroin", "1.0.0")]
public sealed class HeroinPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        TryConnectRegistrationResult result =
            TryConnectApi.RegisterBundledCustomItem<Drink, HeroinDrink>(
                this,
                new TryConnectBundledItemRegistration
                {
                    Key = "heroin_injector",
                    DisplayName = "Heroin",
                    Description = "Go Crazy.",
                    BundlePath = "heroin.assetbundle",
                    VisualPrefabName = "heroin",

                    VisualLocalScale = Vector3.one * 1f,
                    ApplyVisualColor = true,
                    VisualColor = new Color(0.8f, 0.1f, 0.1f),

                    ReplacementChancePercent = 100,
                    ExtraBasePrice = 1,
                    ExtraFloorPrice = 1

                    // ReplaceColliders = true,
                    // VisualLocalPosition = new Vector3(0f, 0f, 0f),
                    // VisualLocalEulerAngles = new Vector3(0f, 0f, 0f)
                });

        Logger.LogInfo($"TryConnect register result: {result}");
    }
}

public sealed class HeroinDrink : Drink
{
    private bool _extraEffectAppliedThisPress;
    private bool _ragdollEffectActive;

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
            buff.ApplyBuff(PlayerBuffType.TipsyFortune, 1, 15f);
        }

        if (!_ragdollEffectActive)
        {
            StartCoroutine(RandomRagdollEffect(holder));
        }

        Debug.Log("[Heroin] Random ragdoll effect started.");
    }

    private IEnumerator RandomRagdollEffect(PlayerInventory holder)
    {
        _ragdollEffectActive = true;

        PlayerController playerController = holder.GetComponent<PlayerController>();

        float duration = 15f;
        float timer = 0f;

        while (timer < duration)
        {
            float randomDelay = Random.Range(1f, 3.5f);

            yield return new WaitForSeconds(randomDelay);

            if (playerController != null)
            {
                playerController.TriggerRagdoll();

                Debug.Log("[Heroin] Random ragdoll triggered.");
            }

            timer += randomDelay;
        }

        _ragdollEffectActive = false;

        Debug.Log("[Heroin] Random ragdoll effect ended.");
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

