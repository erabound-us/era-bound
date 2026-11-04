using UnityEngine;

namespace MarsFPSKit
{
    // Stubs for types referenced by TR scripts but not yet implemented.
    // Replace each with a real implementation when ready.

    public class ExtractionHUD : MonoBehaviour
    {
        public static ExtractionHUD Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }
        public void OnBeaconStateChanged(ExtractionBeacon.BeaconState state, string playerName) { }
        public void OnProgressChanged(float progress, ExtractionBeacon.BeaconState state) { }
        public void OnExtractionComplete(int actorNumber, bool isLocal) { }
        public void OnChannelCancelled(string reason) { }
        public void OnRelicPickedUp(string relicId) { }
        public void OnRelicAwarded(string playerName, string relicId, bool isLocal) { }
        public void OnSoloRelicBroadcast(string playerName) { }
        public void OnArtifactReset(string playerName, int count) { }
        public void ShowMessage(string msg) { }
    }

    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory LocalInstance { get; private set; }
        void Awake() { if (LocalInstance == null) LocalInstance = this; }
        public bool HasArtifacts() => false;
        public void CommitExtraction() { }
        public void CommitRelicExtraction(string relicId) { }
        public void CommitConsolationReward(string relicId) { }
        public void SaveArtifactsOnAfkKick() { }
    }

    public class PlayerStatusEffects : MonoBehaviour
    {
        public void ApplySlow(float amount, float duration) { }
        public void ApplyBlur(float duration) { }
        public void ApplyVulnerability(float duration) { }
    }

    public class ArtifactPickup : MonoBehaviour
    {
        public void InitWithBumpedRarity(string artifactId, string originalRarity) { }
    }

    public class GuardianController : MonoBehaviour
    {
        public void SetHealth(float health) { }
        public void SetDamage(float damage) { }
    }

    public class SurvivalHUD : MonoBehaviour
    {
        public static SurvivalHUD Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }
        public void ShowConsumeProgress(string itemName, float duration) { }
        public void UpdateArmorDisplay(float percent) { }
        public void RefreshInventory() { }
    }
}
