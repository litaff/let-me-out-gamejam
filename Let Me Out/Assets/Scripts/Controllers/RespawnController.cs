using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;


namespace Controllers
{
    public class RespawnController : MonoBehaviour
    {
        [SerializeField] private List<RespawnFloor> floors;

        public void ResetFloor(FloorType type)
        {
            foreach (var floor in floors.Where(floor => floor.floorType == type))
            {
                foreach (var respawnUnit in floor.respawnUnits)
                {
                    respawnUnit.unit.GetComponent<ISpawnAble>().Spawn(respawnUnit.respawnPosition);
                }
            }
        }

        private void Awake()
        {
            // go through each game object and init their position for later respawn
            foreach (var respawnUnit in floors.SelectMany(floor => floor.respawnUnits))
            {
                respawnUnit.respawnPosition = respawnUnit.unit.transform.position;
            }
        }
    
        // this has to be a class
        [Serializable]
        public class RespawnFloor
        {
            public FloorType floorType;
            public List<RespawnUnit> respawnUnits;

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
}
