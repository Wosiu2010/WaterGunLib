using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;



namespace WaterGunLib
{
    public class TeleportTarget : NetworkBehaviour
    {
        [Space]
        [Header("Refrences")]
        public Transform Target;
        public AudioClip StartTeleport;
        public AudioClip EndTeleport;
        [Space]
        [Header("AudioSource")]
        public AudioSource EndAudioSource;
        public AudioSource StartAudioSource;

        public void TeleportPlayer()
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(Target.position, withRotation: true, Target.eulerAngles.y);
            GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            TeleportPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
            Debug.Log("TeleportStarted");
        }

        [ServerRpc(RequireOwnership = false)]
        void TeleportPlayerServerRpc(int PlayerObj)
        {
            TeleportPlayerClientRpc(PlayerObj);
        }

        [ClientRpc]
        void TeleportPlayerClientRpc(int PlayerObj)
        {
            StartOfRound.Instance.allPlayerScripts[PlayerObj].TeleportPlayer(Target.position, withRotation: true, Target.eulerAngles.y);
            StartOfRound.Instance.allPlayerScripts[PlayerObj].isInElevator = false;
            StartOfRound.Instance.allPlayerScripts[PlayerObj].isInHangarShipRoom = false;
            if (EndTeleport != null && EndAudioSource != null)
            {
                EndAudioSource.PlayOneShot(EndTeleport);
            }
            if (StartTeleport != null && StartAudioSource != null)
            {
                StartAudioSource.PlayOneShot(StartTeleport);
            }
        }
    }
}
