using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class TeleporterPad : MonoBehaviour
{
    [SerializeField] private TeleporterPad linkedPad;
    [SerializeField] private float teleportOffset;
    private List<Character> characters;

    private void Awake()
    {
        characters = new List<Character>();
        GetComponent<CircleCollider2D>().isTrigger = true; // to make sure it's a trigger
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var character = col.GetComponent<Character>();

        if (character is null) return;

        if (characters.Contains(character)) return;
        
        linkedPad.Teleport(character);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var character = other.GetComponent<Character>();

        characters.Remove(character);
    }

    private void Teleport(Character character)
    {
        characters.Add(character);
        var position = transform.position;
        character.transform.position = new Vector2(position.x, position.y + teleportOffset);
    }
}
