using UnityEngine;

namespace KillstreaksPlugin
{
    [System.Serializable]
    public class KillstreakVoices
    {
        /// <summary>
        /// Which killstreak this is for
        /// </summary>
        public Killstreak_KillstreakBase killstreak;
        /// <summary>
        /// Your own killstreak is ready
        /// </summary>
        public AudioClip[] readyForDeployment;
        /// <summary>
        /// Friendly killstreak is online
        /// </summary>
        public AudioClip[] friendlyOnline;
        /// <summary>
        /// Enemy killstreak is online
        /// </summary>
        public AudioClip[] enemyOnline;
    }

    public class Killstreaks_Commander : MonoBehaviour
    {
        /// <summary>
        /// Source
        /// </summary>
        public AudioSource source;

        /// <summary>
        /// Voices for all killstreaks
        /// </summary>
        public KillstreakVoices[] killstreakVoices;

        public void ReadyForDeploy(Killstreak_KillstreakBase killstreak)
        {
            if (!source.isPlaying)
            {
                int id = -1;
                for (int i = 0; i < killstreakVoices.Length; i++)
                {
                    if (killstreakVoices[i].killstreak == killstreak)
                    {
                        id = i;
                        break;
                    }
                }

                if (id >= 0)
                {
                    if (killstreakVoices[id].readyForDeployment.Length > 0)
                    {
                        source.clip = killstreakVoices[id].readyForDeployment[Random.Range(0, killstreakVoices[id].readyForDeployment.Length)];
                        source.Play();
                    }
                }
            }
        }

        public void FriendlyOnline(Killstreak_KillstreakBase killstreak)
        {
            if (!source.isPlaying)
            {
                int id = -1;
                for (int i = 0; i < killstreakVoices.Length; i++)
                {
                    if (killstreakVoices[i].killstreak == killstreak)
                    {
                        id = i;
                        break;
                    }
                }

                if (id >= 0)
                {
                    if (killstreakVoices[id].friendlyOnline.Length > 0)
                    {
                        source.clip = killstreakVoices[id].friendlyOnline[Random.Range(0, killstreakVoices[id].friendlyOnline.Length)];
                        source.Play();
                    }
                }
            }
        }

        public void EnemyOnline(Killstreak_KillstreakBase killstreak)
        {
            if (!source.isPlaying)
            {
                int id = -1;
                for (int i = 0; i < killstreakVoices.Length; i++)
                {
                    if (killstreakVoices[i].killstreak == killstreak)
                    {
                        id = i;
                        break;
                    }
                }

                if (id >= 0)
                {
                    if (killstreakVoices[id].enemyOnline.Length > 0)
                    {
                        source.clip = killstreakVoices[id].enemyOnline[Random.Range(0, killstreakVoices[id].enemyOnline.Length)];
                        source.Play();
                    }
                }
            }
        }
    }
}