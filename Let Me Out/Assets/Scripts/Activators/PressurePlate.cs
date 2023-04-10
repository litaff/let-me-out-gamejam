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
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite onSprite;

        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private List<IActivator> activators;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            activators = new List<IActivator>();
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            var activator = col.GetComponent<IActivator>();
        
            if(activator is null) return;

            activators.Add(activator);
            
            Activate?.Invoke(id);
            audioSource.Play();
            spriteRenderer.sprite = onSprite;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var activator = other.GetComponent<IActivator>();
        
            if(activator is null) return;

            activators.Remove(activator);
            
            if (activators.Count != 0) return;
            
            Deactivate?.Invoke(id);
            audioSource.Play();
            spriteRenderer.sprite = offSprite;
        }
    }
}
