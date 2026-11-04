using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MarsFPSKit
{
    public static class TR_WaterWorksSetup
    {
        [MenuItem("TreasureRoyale/Setup WaterWorks URP")]
        public static void Setup()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("[TR] WaterWorks Setup: No URP asset found in Graphics Settings.");
                return;
            }

            var so = new SerializedObject(urpAsset);
            var depthProp  = so.FindProperty("m_RequireDepthTexture");
            var opaqueProp = so.FindProperty("m_RequireOpaqueTexture");

            bool changed = false;
            // Keep both OFF — they cause scene darkening with Unlit shaders
            if (depthProp  != null && depthProp.boolValue)  { depthProp.boolValue  = false; changed = true; Debug.Log("[TR] WaterWorks Setup: Disabled Depth Texture."); }
            if (opaqueProp != null && opaqueProp.boolValue) { opaqueProp.boolValue = false; changed = true; Debug.Log("[TR] WaterWorks Setup: Disabled Opaque Texture."); }
            if (changed) { so.ApplyModifiedProperties(); EditorUtility.SetDirty(urpAsset); AssetDatabase.SaveAssets(); }

            // Remove Water_Volume renderer feature if present — it causes pitch black screen
            var rendererListProp = so.FindProperty("m_RendererDataList");
            if (rendererListProp != null && rendererListProp.arraySize > 0)
            {
                var rendererDataRef = rendererListProp.GetArrayElementAtIndex(0).objectReferenceValue as ScriptableRendererData;
                if (rendererDataRef != null)
                {
                    var rdSo = new SerializedObject(rendererDataRef);
                    var featuresProp = rdSo.FindProperty("m_RendererFeatures");
                    if (featuresProp != null)
                    {
                        bool removed = false;
                        for (int i = featuresProp.arraySize - 1; i >= 0; i--)
                        {
                            var elem = featuresProp.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (elem != null && elem.GetType().Name == "Water_Volume")
                            {
                                featuresProp.DeleteArrayElementAtIndex(i);
                                removed = true;
                            }
                        }
                        if (removed)
                        {
                            rdSo.ApplyModifiedProperties();
                            EditorUtility.SetDirty(rendererDataRef);
                            AssetDatabase.SaveAssets();
                            Debug.Log("[TR] WaterWorks Setup: Removed Water_Volume renderer feature (was causing pitch black).");
                        }
                    }
                }
            }

            Debug.Log("[TR] WaterWorks Setup: Complete. Scene should be bright. Water uses Unlit/Color planes.");
        }
    }
}
