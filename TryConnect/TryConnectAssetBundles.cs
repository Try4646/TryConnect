using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace TryConnect
{
    public static class TryConnectAssetBundles
    {
        public static AssetBundle Load(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw new ArgumentException("fullPath is required.", "fullPath");
            }

            string resolvedPath = Path.GetFullPath(fullPath.Trim());
            if (!File.Exists(resolvedPath))
            {
                throw new FileNotFoundException("Asset bundle file was not found.", resolvedPath);
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(resolvedPath);
            if (bundle == null)
            {
                throw new InvalidOperationException(string.Format("Failed to load asset bundle '{0}'.", resolvedPath));
            }

            return bundle;
        }

        public static AssetBundle LoadRelativeToPlugin(BaseUnityPlugin plugin, string relativePath)
        {
            return Load(ResolvePluginRelativePath(plugin, relativePath));
        }

        public static T LoadAsset<T>(AssetBundle bundle, string assetName) where T : UnityEngine.Object
        {
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }
            if (string.IsNullOrWhiteSpace(assetName))
            {
                throw new ArgumentException("assetName is required.", "assetName");
            }

            T asset = bundle.LoadAsset<T>(assetName.Trim());
            if (asset == null)
            {
                throw new InvalidOperationException(string.Format("Asset '{0}' of type '{1}' was not found in bundle '{2}'.", assetName, typeof(T).FullName, bundle.name));
            }

            return asset;
        }

        public static GameObject CreatePrefabTemplateFromBundle(AssetBundle bundle, string prefabName, string templateName = null)
        {
            GameObject prefab = LoadAsset<GameObject>(bundle, prefabName);
            return TryConnectApi.CreatePrefabTemplate(prefab, templateName);
        }

        public static GameObject CreatePrefabTemplateFromBundle(BaseUnityPlugin plugin, string bundlePath, string prefabName, string templateName = null)
        {
            AssetBundle bundle = LoadRelativeToPlugin(plugin, bundlePath);
            try
            {
                return CreatePrefabTemplateFromBundle(bundle, prefabName, templateName);
            }
            finally
            {
                bundle.Unload(false);
            }
        }

        public static string ResolvePluginRelativePath(BaseUnityPlugin plugin, string relativePath)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException("plugin");
            }
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("relativePath is required.", "relativePath");
            }

            string assemblyLocation = plugin.GetType().Assembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                throw new InvalidOperationException(string.Format("Could not resolve assembly location for plugin '{0}'.", plugin.GetType().FullName));
            }

            string pluginDirectory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrWhiteSpace(pluginDirectory))
            {
                throw new InvalidOperationException(string.Format("Could not resolve plugin directory for '{0}'.", assemblyLocation));
            }

            return Path.GetFullPath(Path.Combine(pluginDirectory, relativePath.Trim()));
        }
    }
}