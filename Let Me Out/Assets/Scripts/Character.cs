using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Character : MonoBehaviour
{
	public float MaxJumpHeight = 4;
	public float MinJumpHeight = 1;
	public float TimeToJumpApex = 0.4f;
	public float MoveSpeed = 6;
	public float WallSlideSpeedMax = 3;
	public float WallStickTime = 0.25f;
	public Vector2 WallJumpClimb;
	public Vector2 WallJumpOff;
	public Vector2 WallLeap;

	private float gravity;
	private float MaxJumpVelocity;
	private float MinJumpVelocity;
	private float velocityXSmoothing;
	private float accelerationTimeAirborne = 0.2f;
	private float accelerationTimeGrounded = 0.1f;
	private float TimeToWallUnstick;
	private Controller2D controller;
	private Vector3 velocity;

	private void Start()
	{
		controller = GetComponent<Controller2D>();

		gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
		MaxJumpVelocity = Mathf.Abs(gravity) * TimeToJumpApex;
		MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);
	}

	private void Update()
	{
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		int wallDirectionX = controller.Collisions.left ? -1 : 1;

		float tragetVelocityX = input.x * MoveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, tragetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

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

		if (Input.GetKeyDown(KeyCode.Space))
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
		if (Input.GetKeyUp(KeyCode.Space))
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