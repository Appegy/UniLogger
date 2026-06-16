using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

/// <summary>
/// Rewrites every project asset and the in-repo package in the current serialization
/// format. Run it after bumping the Unity editor version to normalize asset/.meta files
/// in one pass: it covers the project (Assets + ProjectSettings) and the embedded /
/// in-repo local package together.
/// </summary>
public static class ReserializeAssetsTool
{
    // Importers that flood meta with default fields when reserialized. For these we
    // reserialize the asset only, to avoid noisy .meta churn (verified on Unity 6000.4.x):
    // MonoImporter adds executionOrder/icon/defaultReferences, PluginImporter the full
    // platformData matrix.
    private static readonly HashSet<Type> _lazyMetaImporters = new()
    {
        typeof(MonoImporter),
        typeof(PluginImporter),
    };

    [MenuItem("Appegy/Reserialize Assets")]
    public static void ReserializeAll()
    {
        var projectRoot = Path.GetFullPath(Directory.GetCurrentDirectory());

        var withMetadata = new List<string>();
        var assetsOnly = new List<string>();

        foreach (var path in AssetDatabase.GetAllAssetPaths())
        {
            if (!ShouldProcess(path, projectRoot))
            {
                continue;
            }

            if (!File.Exists(path + ".meta"))
            {
                // ProjectSettings/*.asset have no .meta - reserialize the asset only.
                assetsOnly.Add(path);
                continue;
            }

            var importer = AssetImporter.GetAtPath(path);
            if (importer != null && _lazyMetaImporters.Contains(importer.GetType()))
            {
                assetsOnly.Add(path);
            }
            else
            {
                withMetadata.Add(path);
            }
        }

        Debug.Log($"[Reserialize] {withMetadata.Count} with metadata + {assetsOnly.Count} assets only");

        try
        {
            AssetDatabase.ForceReserializeAssets(withMetadata, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            AssetDatabase.ForceReserializeAssets(assetsOnly, ForceReserializeAssetsOptions.ReserializeAssets);
        }
        finally
        {
            AssetDatabase.SaveAssets();
        }
    }

    // Project assets + the in-repo package; skips packages that resolve outside this repo.
    private static bool ShouldProcess(string path, string projectRoot)
    {
        if (path.StartsWith("Assets/") || path.StartsWith("ProjectSettings/"))
        {
            return true;
        }

        if (!path.StartsWith("Packages/"))
        {
            return false;
        }

        var info = PackageInfo.FindForAssetPath(path);
        if (info == null)
        {
            return false;
        }

        // Embedded packages live inside the project's Packages/ folder - always include.
        if (info.source == PackageSource.Embedded)
        {
            return true;
        }

        // Local (file:) packages: include ones that resolve inside this repository (the repo
        // root is the parent of the Unity project, e.g. <repo>/src next to <repo>/lab).
        // Skip local packages resolving elsewhere (shared SDK submodules in another repo).
        if (info.source != PackageSource.Local)
        {
            return false;
        }

        var repoRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
        var resolved = Path.GetFullPath(info.resolvedPath);
        return resolved.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase);
    }
}
