using System;
using System.Collections.Generic;
using UnityEngine;

namespace Activators
{
    /// <summary>
    /// Becomes active OnTriggerEnter2D and inactive OnTriggerExit2D
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class PressurePlate : MonoBehaviour
    {
        public static event Action<int> Activate;
        public static event Action<int> Deactivate;
    
        [SerializeField] protected int id;

        private List<IActivator> activators;

        private void Awake()
        {
            activators = new List<IActivator>();
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            var activator = col.GetComponent<IActivator>();
        
            if(activator is null) return;

            activators.Add(activator);
            Activate?.Invoke(id);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var activator = other.GetComponent<IActivator>();
        
            if(activator is null) return;

            activators.Remove(activator);
            if(activators.Count == 0)
                Deactivate?.Invoke(id);
        }
    }
}
