using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Character : MonoBehaviour
{
	public static float gravity;
	public float MaxJumpHeight = 4;
	public float MinJumpHeight = 1;
	public float TimeToJumpApex = 0.4f;
	public float MoveSpeed = 6;
	public float WallSlideSpeedMax = 3;
	public float WallStickTime = 0.25f;
	public Vector2 WallJumpClimb;
	public Vector2 WallJumpOff;
	public Vector2 WallLeap;
	
	protected float MaxJumpVelocity;
	protected float MinJumpVelocity;
	protected float velocityXSmoothing;
	protected float accelerationTimeAirborne = 0.2f;
	protected float accelerationTimeGrounded = 0.1f;
	protected float TimeToWallUnstick;
	protected Controller2D controller;
	protected Vector3 velocity;

	protected virtual void Start()
	{
		controller = GetComponent<Controller2D>();

		gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
		MaxJumpVelocity = Mathf.Abs(gravity) * TimeToJumpApex;
		MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);
	}
}