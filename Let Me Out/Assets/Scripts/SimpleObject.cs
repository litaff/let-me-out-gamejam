using System;
using Activators;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Controller2D))]
    public class SimpleObject : MonoBehaviour, IActivator
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private bool hostile;
        [SerializeField] private bool simulated;
        private float accelerationTimeGrounded = .1f;
        private float accelerationTimeAirborne = .2f;
        private bool beingPushed;
        private Vector2 direction;
        private Vector2 velocity;
        private Controller2D controller;
        private float velocityXSmoothing;
        

        private void Awake()
        {
            beingPushed = false;
            controller = GetComponent<Controller2D>();
        }

        private void Update()
        {
            if(!simulated) return;
            Simulate();
        }

        private void Simulate()
        {
            var targetVelocityX = -direction.x * moveSpeed * (beingPushed ? 1 : 0);
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
                (controller.Collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

            velocity.y += Character.gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime, false);

            if (controller.Collisions.below)
                if (controller.Collisions.slidingDownMaxSlope)
                    velocity.y += controller.Collisions.slopeNormal.y * -Character.gravity * Time.deltaTime;
                else
                    velocity.y = 0;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            var player = col.GetComponent<Player>();
            if (!player) return;
            direction = Direction(col.transform.position, transform.position);
            print(direction);
            if(hostile)
                player.TakeDamage(direction);
            else
                if(Mathf.Abs(direction.x) > 0.4f)
                    beingPushed = true;
            
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponent<Player>();
            if (!player) return;
            beingPushed = false;
            
        }

        private Vector2 Direction(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y).normalized;
        }
    }
}