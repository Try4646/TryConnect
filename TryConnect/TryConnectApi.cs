using System;
using System.Collections.Generic;
using System.Reflection;
using Mirror;
using MoreMountains.Feedbacks;
using TryConnect.Utils;
using UnityEngine;

namespace TryConnect
{
    public static class TryConnectApi
    {
        public const int ApiVersion = 3;
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
        private static readonly FieldInfo ItemColliderCacheField = typeof(Item).GetField("_allColliders", BindingFlags.Instance | BindingFlags.NonPublic);

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

        public static bool TryFindVanillaSpawnable(Type componentType, out SpawnableSO spawnable)
        {
            spawnable = null;
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }
            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                throw new ArgumentException("componentType must derive from UnityEngine.Component.", "componentType");
            }

            SpawnableSettings settings = Resources.Load<SpawnableSettings>("SpawnableSettings");
            if (settings == null || settings.spawnables == null)
            {
                return false;
            }

            for (int i = 0; i < settings.spawnables.Count; i++)
            {
                SpawnableSO candidate = settings.spawnables[i];
                if (candidate != null && candidate.prefab != null && candidate.prefab.GetComponent(componentType) != null)
                {
                    spawnable = candidate;
                    return true;
                }
            }

            return false;
        }

        public static bool TryFindVanillaSpawnable<TComponent>(out SpawnableSO spawnable)
            where TComponent : Component
        {
            return TryFindVanillaSpawnable(typeof(TComponent), out spawnable);
        }

        public static SpawnableSO FindVanillaSpawnable<TComponent>()
            where TComponent : Component
        {
            SpawnableSO spawnable;
            return TryFindVanillaSpawnable<TComponent>(out spawnable) ? spawnable : null;
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

        public static int RemoveNonTriggerColliders(GameObject root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            List<Collider> colliders = CollectNonTriggerColliders(root);
            for (int i = 0; i < colliders.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }

            RefreshItemPrefab(root);
            return colliders.Count;
        }

        public static void RefreshItemPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            Item[] items = prefab.GetComponentsInChildren<Item>(true);
            for (int i = 0; i < items.Length; i++)
            {
                RefreshItemState(items[i]);
            }
        }

        public static Transform GetPrefabVisualRoot(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            return ResolveVisualRoot(prefab);
        }

        public static GameObject ReplaceVisualsWithPrimitive(GameObject prefab, PrimitiveType primitiveType, Color color, Vector3 localScale, bool replaceColliders = true)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            Transform visualRoot = ResolveVisualRoot(prefab);
            int layer = visualRoot.gameObject.layer != 0 ? visualRoot.gameObject.layer : prefab.layer;

            SetRenderersEnabled(visualRoot, false);
            if (replaceColliders)
            {
                RemoveNonTriggerColliders(prefab);
            }

            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = primitiveType + "Visual";
            primitive.layer = layer;
            primitive.transform.SetParent(visualRoot, false);
            primitive.transform.localPosition = Vector3.zero;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                Material material = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
                material.color = color;
                renderer.sharedMaterial = material;
            }

            RefreshItemPrefab(prefab);
            return primitive;
        }

        public static GameObject ReplaceVisualsWithPrefab(GameObject prefab, GameObject visualPrefab, Vector3 localScale, bool replaceColliders = true)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }
            if (visualPrefab == null)
            {
                throw new ArgumentNullException("visualPrefab");
            }

            Transform visualRoot = ResolveVisualRoot(prefab);
            int layer = visualRoot.gameObject.layer != 0 ? visualRoot.gameObject.layer : prefab.layer;

            SetRenderersEnabled(visualRoot, false);
            if (replaceColliders)
            {
                RemoveNonTriggerColliders(prefab);
            }

            GameObject visualInstance = UnityEngine.Object.Instantiate(visualPrefab);
            visualInstance.name = string.IsNullOrWhiteSpace(visualPrefab.name) ? "CustomVisual" : visualPrefab.name;
            visualInstance.transform.SetParent(visualRoot, false);
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = localScale;
            SetLayerRecursively(visualInstance, layer);

            RefreshItemPrefab(prefab);
            return visualInstance;
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

        private static Transform ResolveVisualRoot(GameObject prefab)
        {
            Item item = prefab != null ? prefab.GetComponent<Item>() : null;
            Transform modelTransform = item != null ? item.modelTransform : null;
            if (modelTransform == null && prefab != null)
            {
                modelTransform = prefab.transform.Find("Model");
            }

            return modelTransform ?? (prefab != null ? prefab.transform : null);
        }

        private static void SetRenderersEnabled(Transform root, bool isEnabled)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = isEnabled;
                }
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null)
            {
                return;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null)
                {
                    transforms[i].gameObject.layer = layer;
                }
            }
        }

        private static void RefreshItemState(Item item)
        {
            if (item == null)
            {
                return;
            }

            Transform modelTransform = item.modelTransform;
            if (modelTransform == null)
            {
                modelTransform = item.transform.Find("Model");
                item.modelTransform = modelTransform;
            }

            item.handRig = ResolveHandRig(modelTransform);
            item.handMesh = ResolveHandMesh(item.handRig);
            item.onHandFb = ResolveFeedbackPlayer(item.transform, "OnHand");
            item.onDropFb = ResolveFeedbackPlayer(item.transform, "OnDrop");
            item.onThrowVfx = ResolveThrowVfx(item.transform);

            if (ItemColliderCacheField != null)
            {
                ItemColliderCacheField.SetValue(item, CollectNonTriggerColliders(item));
            }
        }

        private static GameObject ResolveHandRig(Transform modelTransform)
        {
            if (modelTransform == null)
            {
                return null;
            }

            Transform handRigTransform = modelTransform.Find("HandRig");
            return handRigTransform != null ? handRigTransform.gameObject : null;
        }

        private static SkinnedMeshRenderer ResolveHandMesh(GameObject handRig)
        {
            if (handRig == null)
            {
                return null;
            }

            SkinnedMeshRenderer[] renderers = handRig.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            return renderers.Length > 0 ? renderers[0] : null;
        }

        private static MMF_Player ResolveFeedbackPlayer(Transform itemTransform, string feedbackName)
        {
            Transform feedbacksTransform = itemTransform != null ? itemTransform.Find("Feedbacks") : null;
            if (feedbacksTransform == null)
            {
                return null;
            }

            Transform feedbackTransform = feedbacksTransform.Find(feedbackName);
            return feedbackTransform != null ? feedbackTransform.GetComponent<MMF_Player>() : null;
        }

        private static ParticleSystem ResolveThrowVfx(Transform itemTransform)
        {
            Transform feedbacksTransform = itemTransform != null ? itemTransform.Find("Feedbacks") : null;
            if (feedbacksTransform == null)
            {
                return null;
            }

            Transform throwVfxTransform = feedbacksTransform.Find("OnThrowVFX");
            return throwVfxTransform != null ? throwVfxTransform.GetComponent<ParticleSystem>() : null;
        }

        private static List<Collider> CollectNonTriggerColliders(Item item)
        {
            return item != null ? CollectNonTriggerColliders(item.gameObject) : new List<Collider>();
        }

        private static List<Collider> CollectNonTriggerColliders(GameObject root)
        {
            List<Collider> colliders = new List<Collider>();
            Collider[] allColliders = root != null ? root.GetComponentsInChildren<Collider>(true) : new Collider[0];
            for (int i = 0; i < allColliders.Length; i++)
            {
                Collider collider = allColliders[i];
                if (collider != null && !collider.isTrigger)
                {
                    colliders.Add(collider);
                }
            }
            return colliders;
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
