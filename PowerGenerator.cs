
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;



namespace WaterGunLib
{
    public class PowerGenerator : NetworkBehaviour
    {
        [Space]
        [Header("Refrences")]
        public List<Light> LightsToDisable;
        public AudioClip FailSound;
        public AudioClip ToggleSound;

        public AudioSource GeneratorAudioSource;

        [Space]
        [Header("Settings")]
        public bool IsAbleToFail = true;

        [Range(0f, 100f)]
        public float ChanceToFail = 50f;

        public int FailDamage = 50;

        [Space]
        [Header("Events")]
        public UnityEvent OnGeneratorFail;
        public UnityEvent OnGeneratorSuccess;
    

        public void ToggleAllLights(PlayerControllerB player)
        {
            ulong clientid = player.playerClientId;
            ToggleLightsServerRpc(clientid);
        }


        [ServerRpc(RequireOwnership = false)]
        public void ToggleLightsServerRpc(ulong clientid)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == clientid)
                {

                    if (Random.value <= ChanceToFail / 100f)
                    {
                        ToggleLightsClientRpc(true, clientid);
                    }
                    else
                    {
                        ToggleLightsClientRpc(false, clientid);
                    }
                }
            }
        
        }

        [ClientRpc]
        public void ToggleLightsClientRpc(bool IsFailed, ulong clientid)
        {
            if (IsFailed == false)
            {
                if (GeneratorAudioSource != null)
                {
                    GeneratorAudioSource.PlayOneShot(ToggleSound);
                }
                foreach (Light light in LightsToDisable)
                {
                    if (light.enabled == false)
                    {
                        light.enabled = true;
                    }
                    else
                    {
                        light.enabled = false;
                    }

                }
                OnGeneratorSuccess.Invoke();
            }
            else if (IsFailed && IsAbleToFail)
            {
                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player.playerClientId == clientid)
                    {
                        player.DamagePlayer(FailDamage, causeOfDeath: CauseOfDeath.Electrocution);
                    }
                }
                if (GeneratorAudioSource != null)
                {
                    GeneratorAudioSource.PlayOneShot(FailSound);
                }
                OnGeneratorFail.Invoke();
            }
        
        }



    }
}
