using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to any world container (barrel, crate, carcass, clay pot etc.)
    // Extends Kit_InteractableObject — kit weapon manager handles F-key and prompt.
    public class SurvivalLootContainer : Kit_InteractableObject
    {
        [SerializeField] List<SurvivalLootTable.WeightedItem> lootTable;
        [SerializeField] int minItems = 1;
        [SerializeField] int maxItems = 3;
        [SerializeField] float respawnTime = 90f;
        [SerializeField] GameObject closedVisual;
        [SerializeField] GameObject openVisual;

        bool _looted;
        List<SurvivalInventory.SurvivalItem> _currentLoot = new List<SurvivalInventory.SurvivalItem>();

        void Start()
        {
            interactionText = "Loot";
            GenerateLoot();
            SetVisual(false);
        }

        public override bool CanInteract(Kit_PlayerBehaviour who)
        {
            return !_looted;
        }

        public override void Interact(Kit_PlayerBehaviour who)
        {
            if (_looted) return;
            var inv = who.GetComponent<SurvivalInventory>();
            if (inv == null) return;

            bool tookAny = false;
            foreach (var item in _currentLoot)
                if (inv.TryPickup(item)) tookAny = true;

            if (!tookAny) return;

            _looted = true;
            _currentLoot.Clear();
            SetVisual(true);
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnTime);
            GenerateLoot();
            _looted = false;
            SetVisual(false);
        }

        void GenerateLoot()
        {
            _currentLoot.Clear();
            int count = Random.Range(minItems, maxItems + 1);
            for (int i = 0; i < count; i++)
            {
                var item = SurvivalLootTable.Roll(lootTable);
                if (item != null) _currentLoot.Add(item);
            }
        }

        void SetVisual(bool isOpen)
        {
            if (closedVisual) closedVisual.SetActive(!isOpen);
            if (openVisual) openVisual.SetActive(isOpen);
        }
    }
}
