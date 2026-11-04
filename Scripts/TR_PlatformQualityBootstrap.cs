using UnityEngine;

namespace MarsFPSKit
{
    // Attach once to the BootScene or the MarsFPSKit_IngamePrefab.
    // Detects platform and applies quality settings that affect all era maps.
    // Works alongside Kit_GameInformation.enableTouchscreenInput.
    [DefaultExecutionOrder(-100)]
    public class TR_PlatformQualityBootstrap : MonoBehaviour
    {
        [Header("Desktop")]
        [SerializeField] int desktopShadowDistance  = 150;
        [SerializeField] float desktopLodBias       = 1.0f;
        [SerializeField] int desktopTextureQuality  = 0; // 0=Full

        [Header("Mobile")]
        [SerializeField] int mobileShadowDistance   = 50;
        [SerializeField] float mobileLodBias        = 0.5f;
        [SerializeField] int mobileTextureQuality   = 1; // 1=Half
        [SerializeField] int mobileTargetFrameRate  = 60;

        void Awake()
        {
            bool isMobile = Application.isMobilePlatform;

            if (isMobile)
            {
                QualitySettings.shadowDistance  = mobileShadowDistance;
                QualitySettings.lodBias         = mobileLodBias;
                QualitySettings.masterTextureLimit = mobileTextureQuality;
                Application.targetFrameRate     = mobileTargetFrameRate;
                QualitySettings.vSyncCount      = 0;
            }
            else
            {
                QualitySettings.shadowDistance  = desktopShadowDistance;
                QualitySettings.lodBias         = desktopLodBias;
                QualitySettings.masterTextureLimit = desktopTextureQuality;
            }
        }
    }
}
