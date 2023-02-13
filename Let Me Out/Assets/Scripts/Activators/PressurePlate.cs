using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PressurePlate : MonoBehaviour
{
    public static event Action<int> Activate;
    public static event Action<int> Deactivate;
    [SerializeField] private int id;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var character = col.GetComponent<Character>();
        
        if(character is null) return;
        
        Activate?.Invoke(id);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var character = other.GetComponent<Character>();
        
        if(character is null) return;
        
        Deactivate?.Invoke(id);
    }
}
