using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

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



    public void TeleportPlayer(bool WithRotation)
    {
        PlayerControllerB Controller = StartOfRound.Instance.localPlayerController;
        Controller.TeleportPlayer(withRotation: WithRotation, pos: Target.position);    
        PlayEndSoundOnEveryClientRpc();
        PlayStartSoundOnEveryClientRpc();
        Debug.Log("TeleportStarted");
    }


    [ClientRpc]
    void PlayStartSoundOnEveryClientRpc()
    {
        if (StartTeleport != null && StartAudioSource != null)
        {
            StartAudioSource.PlayOneShot(StartTeleport);
        }
    }

    [ClientRpc]
    void PlayEndSoundOnEveryClientRpc()
    {
        if (EndTeleport != null && EndAudioSource != null)
        {
            EndAudioSource.PlayOneShot(EndTeleport);
        }
    }
}