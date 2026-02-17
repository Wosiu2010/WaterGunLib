using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaterGunLib.Modules
{
    [CreateAssetMenu(menuName = "WaterGunLib/Enemies/EnemyTypeRefrence")]
    public class EnemyTypeRef : ScriptableObject
    {
        [Header("Settings")]
        public bool networkPrefab;
        public int rarity;
        public TerminalNode infoNode;
        public WaterGunLib.Modules.Enemies.SpawnType spawnType;
        public List<string> PlanetNames;
        public EnemyType enemyType;
    }


    public class Enemies
    {
        public static Dictionary<RegisteredEnemy, List<string>> registeredEnemies = new Dictionary<RegisteredEnemy, List<string>>();

        public enum SpawnType
        {
            Outdoor,
            Indoor,
            DayTime
        }

        public class RegisteredEnemy
        {
            public EnemyType enemyType;
            public string modGUID;
            public SpawnType spawnType;
            public int rarity;
            public TerminalNode infoNode;
        };

        /// <summary>
        /// Registers enemy
        /// </summary>
        public static void RegisterEnemy(EnemyTypeRef enemyTypeRef, string modGUID)
        {
            RegisteredEnemy enemy = new RegisteredEnemy
            {
                enemyType = enemyTypeRef.enemyType,
                modGUID = modGUID,
                spawnType = enemyTypeRef.spawnType,
                rarity = enemyTypeRef.rarity,
                infoNode = enemyTypeRef.infoNode
            };

            if (registeredEnemies.ContainsKey(enemy))
            {
                Debug.LogError($"[WaterGunLib]: {enemy.enemyType.enemyName} is already registered. modName: {enemy.modGUID}");
            }
            else
            {
                if (enemyTypeRef.networkPrefab) Plugin.prefabsToNetwork.Add(enemy.enemyType.enemyPrefab);
                registeredEnemies.Add(enemy, enemyTypeRef.PlanetNames); 
                Debug.Log($"[WaterGunLib]: Registered {enemy.enemyType.enemyName}. modGUID: {enemy.modGUID}");
            }
        }
    }
}
