using System;
using UnityEngine;

namespace Activators
{
    /// <summary>
    /// Becomes active OnTriggerEnter2D and inactive on the second OnTriggerEnter2D
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Lever : MonoBehaviour
    {
        public static event Action<int> Activate;
        public static event Action<int> Deactivate;

        public bool Active { get; private set; }

        [SerializeField] private bool singleUse = false;
        [SerializeField] private int id;

        private void Awake()
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            Active = false;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            var character = col.GetComponent<Character>();
        
            if(character is null) return;

            if (Active && singleUse) return;
            
            if (Active)
            {
                Deactivate?.Invoke(id);
                Active = false;
                return;
            }
        
            Activate?.Invoke(id);
            Active = true;
        }
    }
}
