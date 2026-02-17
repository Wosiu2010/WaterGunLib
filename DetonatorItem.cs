using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WaterGunLib
{
    public class DetonatorItem : GrabbableObject
    {
        public GameObject ObjectToDetonate;
        public GameObject DestructionObject;
        public string pathToDestructionObject = "Environment/Detonator/Rocks";

        public float minDistance = 5f;

        public string ExplosionText;

        private bool canBeDestroyed = false;

        IEnumerator SetDetonator()
        {
            while (StartOfRound.Instance.inShipPhase)
            {
                yield return null;
            }

            if (ObjectToDetonate == null)
                ObjectToDetonate = GameObject.Find(pathToDestructionObject);


            if (ObjectToDetonate != null)
            {
                canBeDestroyed = true;
            }
        }

        public override void Start()
        {
            base.Start();
            StartCoroutine(SetDetonator());
        }

        public override void Update()
        {
            base.Update();

            if (playerHeldBy != null && StartOfRound.Instance.inShipPhase && canBeDestroyed == true)
            {
                this.DestroyObjectInHand(playerHeldBy);
            }
            else if (playerHeldBy == null && StartOfRound.Instance.inShipPhase && canBeDestroyed == true)
            {
                DespawnObject();
            }
        }
        private void DespawnObject()
        {
            if (NetworkManager.Singleton.IsServer)
                this.GetComponent<NetworkObject>().Despawn();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            
            float distance = Vector3.Distance(transform.position, ObjectToDetonate.transform.position);

            if (distance <= minDistance)
            {
                HUDManager.Instance.DisplayTip(this.itemProperties.itemName, ExplosionText);
                DestroyObject();
            }
            else
            {
                HUDManager.Instance.DisplayTip(this.itemProperties.itemName, "Failed to detonate please get closer to the target");
            }
        }

        private void DestroyObject()
        {
            GameObject instantiatedDestruction = Instantiate(original: DestructionObject, position: ObjectToDetonate.transform.position, rotation: ObjectToDetonate.transform.rotation, parent: RoundManager.Instance.mapPropsContainer.transform);

            Destroy(ObjectToDetonate);
            this.DestroyObjectInHand(playerHeldBy);
        }
    }
}
