using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace WaterGunLib
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Space]
        [Header("Refrences")]
        public Transform SpawnTransform;

        [Space]
        [Header("Settings")]
        public bool IsDynamic = false;

        [Space]
        [Header("Single Enemy Spawn Settings")]
        private string EnemyName;

        [Space]
        [Header("Mutiple Enemy Spawn Settings")]
        public string[] EnemyNames;
        public float TimeBetweenSpawn = 5f;
        public int SpawnCap = -1;


        private int Spawns = 0;
        private bool IsSpawningEnabled;

        public void SpawnEnemy()
        {
            SpawnEnemyServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void SpawnEnemyServerRpc()
        {

            if (IsDynamic == false)
            {
                EnemyType EnemyToSpawn = Resources.FindObjectsOfTypeAll<EnemyType>().First(enemytype => enemytype.enemyName == EnemyName);
                GameObject SpawnedEnemy = Instantiate(EnemyToSpawn.enemyPrefab, position: SpawnTransform.position, rotation: SpawnTransform.rotation);
                SpawnedEnemy.GetComponent<NetworkObject>().Spawn();
                if (SpawnCap != -1)
                    Spawns++;
            }
            else
            {
                if (Spawns <= SpawnCap)
                {
                    EnemyType[] enemies = Resources.FindObjectsOfTypeAll<EnemyType>();
                    List<EnemyType> EnemiesToSpawn = new List<EnemyType>();
                    foreach (EnemyType enemy in enemies)
                    {
                        if (EnemyNames.Contains(enemy.enemyName))
                        {
                            EnemiesToSpawn.Add(enemy);
                        }
                    }

                    int RandomEnemy = UnityEngine.Random.Range(0, EnemiesToSpawn.Count);
                    GameObject SpawnedEnemy = Instantiate(EnemiesToSpawn[RandomEnemy].enemyPrefab, position: SpawnTransform.position, rotation: SpawnTransform.rotation);
                    SpawnedEnemy.GetComponent<NetworkObject>().Spawn();
                    if (SpawnCap != -1)
                        Spawns++;
                    StartCoroutine(SpawnEnemyAfterDelay());
                }
            }
        }

        public void ToggleSpawning()
        {
            if (IsSpawningEnabled == false)
            {
                IsSpawningEnabled = true;
            
                StartCoroutine(SpawnEnemyAfterDelay());
            }
            else
            {
                IsSpawningEnabled = false;
            }
        }

        IEnumerator SpawnEnemyAfterDelay()
        {
            yield return new WaitForSeconds(TimeBetweenSpawn);
            if (IsSpawningEnabled == true)
            {
                SpawnEnemy();
            }
        }
    }
}
