using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;


namespace WaterGunLib
{
    public class ToxicGas : MonoBehaviour
    {
        [Space]
        [Header("Settings")]
        public int Delay = 3;
        public int Damage = 20;

        public CauseOfDeath DeathType;
        public int DeathAnimation = 0;
        public bool DamageSFX = false;

        private bool IsAbleToDealDamage = true;

        void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<PlayerControllerB>())
            {
                StartCoroutine(DamagePlayerAfterDelay(other.GetComponent<PlayerControllerB>()));
            }

        }

        IEnumerator DamagePlayerAfterDelay(PlayerControllerB Player)
        {
            if (IsAbleToDealDamage == true)
            {
                IsAbleToDealDamage = false;
                Player.DamagePlayer(damageNumber: Damage, causeOfDeath: DeathType, deathAnimation: DeathAnimation);
                Debug.Log($"Damaged Player: {Player.playerUsername}");
                yield return new WaitForSeconds(Delay);
                IsAbleToDealDamage = true;
            }

        }
    }
}