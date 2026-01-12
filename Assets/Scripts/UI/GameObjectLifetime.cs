using System.Collections;
using UnityEngine;

namespace Text2ImageAI
{

    public class GameObjectLifetime : MonoBehaviour
    {

        [SerializeField]
        private float lifetimeInSeconds;

        [SerializeField]
        private bool shouldDestroy;

        private WaitForSeconds lifetime;

        private void Awake()
        {
            lifetime = new WaitForSeconds(lifetimeInSeconds);    
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(C_Countdown());
        }

        private IEnumerator C_Countdown()
        {
            yield return lifetime;

            if (shouldDestroy)
            {
                DestroyImmediate(gameObject);
            }
            else 
            { 
                gameObject.SetActive(false);
            }
        }

    }

}