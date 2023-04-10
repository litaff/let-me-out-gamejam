using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Controllers
{
    public class RespawnController : MonoBehaviour
    {
        [SerializeField] private List<RespawnUnit> respawnUnits;

        private void ResetAll()
        {
            StartCoroutine(WaitAndResetAll(3));
        }

        private IEnumerator WaitAndResetAll(float delay)
        {
            yield return new WaitForSeconds(delay);
            foreach (var respawnUnit in respawnUnits) 
            { 
                respawnUnit.unit.GetComponent<ISpawnAble>().Spawn(respawnUnit.respawnPosition);
            }
        }

        private void Awake()
        {
            Player.OnDeath += ResetAll;
            
            // go through each game object and init their position for later respawn
            foreach (var respawnUnit in respawnUnits)
            {
                respawnUnit.respawnPosition = respawnUnit.unit.transform.position;
            }
        }
    
        [Serializable]
        public class RespawnUnit
        {
            public GameObject unit;

            public Vector2 respawnPosition;

            public void Init(Vector2 pos)
            {
                respawnPosition = pos;
            }
        }
    }
}
