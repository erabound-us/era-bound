#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace MarsFPSKit
{
    public static class TR_HubThemeCreator
    {
        [MenuItem("Tools/Treasure Royale/0 - Create Default Theme")]
        static void CreateDefaultTheme()
        {
            const string dir  = "Assets/TreasureRoyale/Themes";
            const string path = dir + "/TR_HubTheme_Default.asset";

            if (!AssetDatabase.IsValidFolder("Assets/TreasureRoyale"))
                AssetDatabase.CreateFolder("Assets", "TreasureRoyale");
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets/TreasureRoyale", "Themes");

            var existing = AssetDatabase.LoadAssetAtPath<TR_HubTheme>(path);
            if (existing != null) { Debug.Log("[TR] Default theme already exists."); return; }

            var theme = ScriptableObject.CreateInstance<TR_HubTheme>();

            // Load LiberationSans SDF from TMP Resources
            const string fontPath = "Assets/314 Arts/MarsFPSKit/Required/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            theme.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (theme.font == null) Debug.LogWarning("[TR] Font not found at " + fontPath);

            AssetDatabase.CreateAsset(theme, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[TR] Default theme created at " + path);
            Selection.activeObject = theme;
        }
    }
}
#endif
