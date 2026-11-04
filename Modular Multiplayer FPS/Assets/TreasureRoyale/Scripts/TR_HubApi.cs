using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MarsFPSKit
{
    // Attach once to a persistent object in the Hub scene.
    // All hub panels call TR_HubApi.Instance.Get/Post.
    public class TR_HubApi : MonoBehaviour
    {
        public static TR_HubApi Instance { get; private set; }

        [SerializeField] int[] probePorts = { 8787, 8788, 8789, 8790 };

        string _baseUrl = "http://127.0.0.1:8788";
        bool _ready;
        public static bool IsReady => Instance != null && Instance._ready;
        public static string PlayerId { get; set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => StartCoroutine(ProbeWorker());

        IEnumerator ProbeWorker()
        {
            if (string.IsNullOrEmpty(PlayerId)) PlayerId = "test_player";
            foreach (int port in probePorts)
            {
                string url = $"http://127.0.0.1:{port}/health";
                using var req = UnityWebRequest.Get(url);
                req.timeout = 1;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success || req.responseCode > 0)
                {
                    _baseUrl = $"http://127.0.0.1:{port}";
                    _ready   = true;
                    Debug.Log($"[TR_HubApi] Worker found at {_baseUrl}");
                    yield break;
                }
            }
            Debug.LogWarning("[TR_HubApi] Worker not found on any probe port.");
        }

        public void Get(string endpoint, Action<string, string> onDone)
            => StartCoroutine(Request("GET", endpoint, null, onDone));

        public void Post(string endpoint, object body, Action<string, string> onDone)
            => StartCoroutine(Request("POST", endpoint, JsonUtility.ToJson(body), onDone));

        IEnumerator Request(string method, string endpoint, string bodyJson, Action<string, string> onDone)
        {
            string url = _baseUrl + endpoint;
            using var req = new UnityWebRequest(url, method);
            if (bodyJson != null)
            {
                var bytes = Encoding.UTF8.GetBytes(bodyJson);
                req.uploadHandler   = new UploadHandlerRaw(bytes);
                req.SetRequestHeader("Content-Type", "application/json");
            }
            req.downloadHandler = new DownloadHandlerBuffer();
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                onDone(req.downloadHandler.text, null);
            else
                onDone(null, req.error);
        }

        // ── Typed helpers ─────────────────────────────────────────────────────

        public void GetPlayer(Action<PlayerProfile> cb)
            => Get($"/player/{PlayerId}", (json, err) =>
            {
                if (err != null) { Debug.LogWarning($"[TR_HubApi] {err}"); return; }
                cb(JsonUtility.FromJson<PlayerProfile>(json));
            });

        public void GetInventory(Action<InventoryResponse> cb)
            => Get($"/player/{PlayerId}/inventory", (json, err) =>
            {
                if (err != null) { Debug.LogWarning($"[TR_HubApi] {err}"); return; }
                cb(JsonUtility.FromJson<InventoryResponse>(json));
            });

        public void GetCatalog(string era, Action<CatalogItem[]> cb)
        {
            string q = string.IsNullOrEmpty(era) ? "" : $"?era={era}";
            Get($"/shop/catalog{q}", (json, err) =>
            {
                if (err != null) { Debug.LogWarning($"[TR_HubApi] {err}"); return; }
                cb(JsonHelper.FromJsonArray<CatalogItem>(json));
            });
        }

        public void GetAuctionListings(Action<AuctionListing[]> cb)
            => Get($"/auction/listings?player_id={PlayerId}", (json, err) =>
            {
                if (err != null) { Debug.LogWarning($"[TR_HubApi] {err}"); return; }
                cb(JsonHelper.FromJsonArray<AuctionListing>(json));
            });
    }

    // ── Data models ───────────────────────────────────────────────────────────

    [Serializable]
    public class PlayerProfile
    {
        public string player_id;
        public string display_name;
        public int    chrono_coins;
    }

    [Serializable]
    public class InventoryItem
    {
        public int    id;
        public string item_id;
        public string name;
        public string category;
        public string subcategory;
        public string slot_type;
        public int    slot_size;
        public int    quantity;
        public int    durability;
        public int    max_durability;
        public int    is_consumable;
        public string description;
        public string era_tags;
        public int    slot_index;
    }

    [Serializable]
    public class ValuableItem
    {
        public int    id;
        public string item_id;
        public string name;
        public string era;
        public string rarity;
        public int    is_legendary;
        public string historical_figure;
        public int    base_value;
        public string passive_ability;
    }

    [Serializable]
    public class ContainerItem
    {
        public int    id;
        public string container_id;
        public string name;
        public int    extra_slots;
        public int    is_equipped;
    }

    [Serializable]
    public class PetItem
    {
        public int    id;
        public string pet_type;
        public string name;
        public int    extra_slots;
        public int    is_active;
        public string skin_id;
    }

    [Serializable]
    public class VehicleItem
    {
        public int    id;
        public string vehicle_type;
        public int    extra_slots;
        public int    is_active;
        public string skin_id;
    }

    [Serializable]
    public class UpgradeItem
    {
        public string upgrade_id;
        public int    level;
    }

    [Serializable]
    public class InventoryResponse
    {
        public InventoryItem[] gear;
        public ValuableItem[]  valuables;
        public ContainerItem[] containers;
        public PetItem[]       pets;
        public VehicleItem[]   vehicles;
        public UpgradeItem[]   upgrades;
        public int             totalSlots;
        public int             usedSlots;
    }

    [Serializable]
    public class CatalogItem
    {
        public string item_id;
        public string name;
        public string category;
        public string subcategory;
        public string slot_type;
        public int    slot_size;
        public int    shop_price;
        public int    is_consumable;
        public int    max_durability;
        public string description;
        public string era_tags;
    }

    [Serializable]
    public class AuctionListing
    {
        public string listing_id;
        public string seller_id;
        public string seller_name;
        public string item_type;
        public string item_snapshot;
        public int    asking_price;
        public int    is_private;
        public string allowed_buyers;
        public string status;
        public int    listing_fee;
        public long   expires_at;
    }

    public static class JsonHelper
    {
        [Serializable] class Wrapper<T> { public T[] items; }
        public static T[] FromJsonArray<T>(string json)
            => JsonUtility.FromJson<Wrapper<T>>("{\"items\":" + json + "}")?.items ?? new T[0];
    }
}
