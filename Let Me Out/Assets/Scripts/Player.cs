using System;
using System.Collections;
using System.Collections.Generic;
using Activators;
using Core;
using UnityEngine;

public class Player : Character, ISpawnAble, IActivator
{
	[SerializeField] private int maxHitPoints;
	[SerializeField] private float maxInvulnerabilityTime;
	private int hitPoints;
	private float invulnerabilityTime;
	private Vector2 input;

	public static event Action<Vector2> OnDamage;
	public static event Action OnDeath;

	public void Spawn(Vector2 pos)
	{
		transform.position = pos;
		
		hitPoints = maxHitPoints;
		invulnerabilityTime = 0;
	}
	
	// always 1 hit point
	public void TakeDamage(Vector2 direction)
	{
		if (invulnerabilityTime > 0) return;

		hitPoints--;
		invulnerabilityTime = maxInvulnerabilityTime;
		velocity = direction*30;
		
		OnDamage?.Invoke(transform.position);
		
		if (hitPoints > 0) return;
		
		Die();
	}

	protected override void Start()
	{
		base.Start();
		
		input = Vector2.zero;
		
		Spawn(transform.position);
	}

	private void Update()
	{
		if (hitPoints <= 0) return;
		// ^ stops all movement for this character isolate movement and move this there is needed

		if(invulnerabilityTime > 0)
			invulnerabilityTime -= Time.deltaTime;
		
		MoveWithInput();
	}

	private void Die()
	{
		OnDeath?.Invoke();
	}

	private bool Stunned()
	{
		return invulnerabilityTime > maxInvulnerabilityTime / 2;
	}
	
	private void MoveWithInput()
	{
		input = !Stunned() ? 
			new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : 
			Vector2.zero;

		int wallDirectionX = controller.Collisions.left ? -1 : 1;

		float targetVelocityX = input.x * MoveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
			(controller.Collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

		bool wallSliding = false;

		if ((controller.Collisions.left || controller.Collisions.right) && !controller.Collisions.below && velocity.y < 0)
		{
			wallSliding = true;

			if (velocity.y < -WallSlideSpeedMax)
				velocity.y = -WallSlideSpeedMax;

			if (TimeToWallUnstick > 0)
			{
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (input.x != wallDirectionX && input.x != 0)
				{
					TimeToWallUnstick -= Time.deltaTime;
				}
				else
				{
					TimeToWallUnstick = WallStickTime;
				}
			}
			else
				TimeToWallUnstick = WallStickTime;
		}

		if (Input.GetKeyDown(KeyCode.Space) && !Stunned())
		{
			if (wallSliding)
			{
				if (wallDirectionX == input.x)
				{
					velocity.x = -wallDirectionX * WallJumpClimb.x;
					velocity.y = WallJumpClimb.y;
				}
				else if (input.x == 0)
				{
					velocity.x = -wallDirectionX * WallJumpOff.x;
					velocity.y = WallJumpOff.y;
				}
				else
				{
					velocity.x = -wallDirectionX * WallLeap.x;
					velocity.y = WallLeap.y;
				}
			}

			if (controller.Collisions.below)
				if (controller.Collisions.slidingDownMaxSlope)
				{
					if (input.x != -Mathf.Sign(controller.Collisions.slopeNormal.x)) // not jump on max slope
					{
						velocity.y = MaxJumpVelocity * controller.Collisions.slopeNormal.y;
						velocity.x = MaxJumpVelocity * controller.Collisions.slopeNormal.x;
					}
				}
				else
					velocity.y = MaxJumpVelocity;
		}

		if (Input.GetKeyUp(KeyCode.Space) && !Stunned())
		{
			if (velocity.y > MinJumpVelocity)
				velocity.y = MinJumpVelocity;
		}

		velocity.y += gravity * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime, input);

		if (controller.Collisions.above || controller.Collisions.below)
			if (controller.Collisions.slidingDownMaxSlope)
				velocity.y += controller.Collisions.slopeNormal.y * -gravity * Time.deltaTime;
			else
				velocity.y = 0;
	}
}
