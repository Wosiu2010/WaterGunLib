using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using WaterGunLib.Modules;

namespace WaterGunLib
{
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Settings")]
        public bool spawnOnStart = false;
        public Transform spawnPos;

        [Space(5f)]
        [Header("Items")]
        public string[] possibleItemNames;

        void Start()
        {
            if (spawnOnStart)
                SpawnRandomItem(spawnPos.position);
        }

        public GameObject SpawnRandomItem(Vector3 position)
        {
            if (!NetworkManager.Singleton.IsServer) return null;

            if (possibleItemNames.Count() == 0) return null;

            int random = UnityEngine.Random.Range(0, possibleItemNames.Count());
            Item itemProperties = ItemManagement.GetItemFromName(possibleItemNames[random]);
            GameObject item = Instantiate(ItemManagement.GetItemFromName(possibleItemNames[random]).spawnPrefab, position, Quaternion.identity, null);
            Debug.Log($"Spawning item: {item}");
            if (itemProperties.isScrap)
            {
                int randomValue = UnityEngine.Random.Range(itemProperties.minValue, itemProperties.maxValue);
                item.GetComponent<GrabbableObject>().SetScrapValue(Mathf.RoundToInt(randomValue / 2));

                item.GetComponent<NetworkObject>().Spawn();
            }
            else
            {
                item.GetComponent<NetworkObject>().Spawn();
            }

            return item;
        }
    }
}
