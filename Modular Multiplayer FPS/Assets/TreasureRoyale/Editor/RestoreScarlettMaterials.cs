using UnityEditor;
using UnityEngine;

namespace MarsFPSKit
{
    static class RestoreScarlettMaterials
    {
        [MenuItem("TreasureRoyale/Restore Scarlett Materials (Reimport FBX)")]
        static void Run()
        {
            string[] fbxPaths = {
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Outfit1_Hair1.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Outfit1_Hair2.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Outfit2_Hair1.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Outfit2_Hair2.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Hairstyle1.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Hairstyle2.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Hairstyle3.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Raygun.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Stage.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Tool_Communicator.fbx",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Models/Tool_Detector.fbx",
            };

            int count = 0;
            foreach (var path in fbxPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    count++;
                    Debug.Log($"[TR] Reimported: {path}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[TR] Reimported {count} FBX files. Now run Fix Scarlett Materials.");
        }
    }
}
