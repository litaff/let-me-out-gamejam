using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Point : MonoBehaviour
{
    public static event Action OnPickUp;

    private CircleCollider2D hitBox;
    private ParticleSystem particles;


    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        hitBox = GetComponent<CircleCollider2D>();
        hitBox.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.GetComponent<Player>();
        if(!player) return;
        player.Heal();
        PickUp();
    }

    private void PickUp()
    {
        OnPickUp?.Invoke();
        particles.Play();
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        hitBox.enabled = false;
    }
}
