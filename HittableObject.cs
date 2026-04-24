using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace WaterGunLib
{
    public class HittableObject : NetworkBehaviour, IHittable
    {
        public UnityEvent onObjectBreak;
        public UnityEvent onObjectHit;

        public bool syncObjectBreak = false;
        public bool syncObjectHit = false;

        public int objectHp = 5;


        bool IHittable.Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            if (syncObjectHit)
            {
                DamageObjectRpc();
            }
            else
                DamageObject();


            return true;
        }

        private void DamageObject()
        {
            objectHp -= 1;
            if (objectHp <= 0)
            {
                onObjectBreak.Invoke();
            }
            else
            {
                onObjectHit.Invoke();
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void DamageObjectRpc()
        {
            objectHp -= 1;
            if (objectHp <= 0)
            {
                InvokeBreakRpc();
            }
            else
            {
                InvokeHitRpc();
            }
        }

        [Rpc(SendTo.Everyone)]
        private void InvokeBreakRpc()
        {
            onObjectBreak.Invoke();
        }

        [Rpc(SendTo.Everyone)]
        private void InvokeHitRpc()
        {
            onObjectHit.Invoke();
        }
    }
}
