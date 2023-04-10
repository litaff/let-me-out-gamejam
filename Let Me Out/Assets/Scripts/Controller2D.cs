using UnityEngine;

public class Controller2D : RaycastController
{
	[Header("Controller variables")]
	public float MaxSlopeAngle = 80;

	public CollisionInfo Collisions;

	private Vector2 characterInput;

	public override void Start()
	{
		base.Start();
		Collisions.faceDirection = 1;
	}

	public void Move(Vector3 velocity, bool standingOnPlatform)
	{
		Move(velocity, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
	{
		UpdateRaycastOrigins();

		Collisions.Reset();

		Collisions.velocityOld = velocity;

		characterInput = input;

		if (velocity.y < 0)
			velocity = DescendSlope(velocity);

		if (velocity.x != 0)
			Collisions.faceDirection = (int)Mathf.Sign(velocity.x);

		velocity = HorizontalCollisions(velocity);

		if (velocity.y != 0)
			velocity = VerticalCollisions(velocity);

		transform.Translate(velocity);
		Physics2D.SyncTransforms();

		if (!standingOnPlatform) return;

		Collisions.below = true;
	}

	private Vector3 VerticalCollisions(Vector3 velocity)
	{
		float directionY = Mathf.Sign(velocity.y);
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;

		for (int i = 0; i < VerticalRayCount; i++)
		{
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.BottomLeft : raycastOrigins.TopLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (!hit) continue;

			if (hit.collider.CompareTag("Through"))
			{
				if (directionY == 1 || hit.distance == 0) continue;
				if (Collisions.fallingThroughPlatform) continue;
				if (characterInput.y == -1)
				{
					Collisions.fallingThroughPlatform = true;
					Invoke("ResetFallingThroughPlatform", 0.5f);
					continue;
				}
			}

			velocity.y = (hit.distance - skinWidth) * directionY;
			rayLength = hit.distance;

			if (Collisions.climbingSlope)
				velocity.x = velocity.y / Mathf.Tan(Collisions.slopeAngle * Mathf.Deg2Rad * Mathf.Sign(velocity.x));

			Collisions.below = directionY == -1;
			Collisions.above = directionY == 1;
		}

		if (Collisions.climbingSlope)
		{
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.BottomLeft : raycastOrigins.BottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

			if (!hit) return velocity;

			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeAngle == Collisions.slopeAngle) return velocity;

			velocity.x = (hit.distance - skinWidth) * directionX;
			Collisions.slopeAngle = slopeAngle;
			Collisions.slopeNormal = hit.normal;
		}

		return velocity;
	}

	private Vector3 HorizontalCollisions(Vector3 velocity)
	{
		float directionX = Collisions.faceDirection;
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth)
			rayLength = 2 * skinWidth;

		for (int i = 0; i < HorizontalRayCount; i++)
		{
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.BottomLeft : raycastOrigins.BottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (!hit) continue;

			if (hit.distance == 0) continue;

			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (i == 0 && slopeAngle <= MaxSlopeAngle)
			{
				if (Collisions.descendingSlope)
				{
					Collisions.descendingSlope = false;
					velocity = Collisions.velocityOld;
				}
				float distanceToSlopeStart = 0;
				if (slopeAngle != Collisions.slopeAngleOld)
				{
					distanceToSlopeStart = hit.distance - skinWidth;
					velocity.x -= distanceToSlopeStart * directionX;
				}
				velocity = ClimbSlope(velocity, slopeAngle, hit.normal);
				velocity.x += distanceToSlopeStart * directionX;
			}

			if (!Collisions.climbingSlope || slopeAngle > MaxSlopeAngle)
			{
				velocity.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;

				Collisions.left = directionX == -1;
				Collisions.right = directionX == 1;

				if (!Collisions.climbingSlope) return velocity;

				velocity.y = Mathf.Tan(Collisions.slopeAngle * Mathf.Deg2Rad * Mathf.Abs(velocity.x));
			}
		}

		return velocity;
	}

	private Vector3 ClimbSlope(Vector3 velocity, float angle, Vector2 slopeNormal)
	{
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin(angle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y > climbVelocityY) return velocity;

		velocity.y = climbVelocityY;
		velocity.x = Mathf.Cos(angle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
		Collisions.below = true;
		Collisions.climbingSlope = true;
		Collisions.slopeAngle = angle;
		Collisions.slopeNormal = slopeNormal;
		return velocity;
	}

	private Vector3 DescendSlope(Vector3 velocity)
	{
		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.BottomLeft, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, CollisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.BottomRight, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, CollisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight)
		{
			velocity = SlideDownMaxSlope(maxSlopeHitRight, velocity);
			velocity = SlideDownMaxSlope(maxSlopeHitLeft, velocity);
		}

		if (Collisions.slidingDownMaxSlope) return velocity;

		float directionX = Mathf.Sign(velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.BottomRight : raycastOrigins.BottomLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, CollisionMask);

		if (!hit) return velocity;

		float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

		if (slopeAngle == 0 || slopeAngle > MaxSlopeAngle) return velocity;

		if (Mathf.Sign(hit.normal.x) != directionX) return velocity;

		if (hit.distance - skinWidth > Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) return velocity;

		float moveDistance = Mathf.Abs(velocity.x);
		float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
		velocity.y -= descendVelocityY;

		Collisions.slopeAngle = slopeAngle;
		Collisions.descendingSlope = true;
		Collisions.below = true;
		Collisions.slopeNormal = hit.normal;

		return velocity;
	}

	private Vector3 SlideDownMaxSlope(RaycastHit2D hit, Vector3 velocity)
	{
		if (!hit) return velocity;

		float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

		if (slopeAngle <= MaxSlopeAngle) return velocity;

		velocity.x = hit.normal.x * (Mathf.Abs(velocity.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

		Collisions.slopeAngle = slopeAngle;
		Collisions.slidingDownMaxSlope = true;
		Collisions.slopeNormal = hit.normal;

		return velocity;
	}

	private void ResetFallingThroughPlatform()
	{
		Collisions.fallingThroughPlatform = false;
	}
}

[System.Serializable]
public struct CollisionInfo
{
	public bool above, below;
	public bool left, right;
	public bool climbingSlope;
	public bool descendingSlope;
	public bool fallingThroughPlatform;
	public bool slidingDownMaxSlope;
	public float slopeAngle, slopeAngleOld;
	public int faceDirection;
	public Vector2 slopeNormal;
	public Vector3 velocityOld;

	public void Reset()
	{
		above = below = false;
		left = right = false;
		climbingSlope = false;
		descendingSlope = false;
		slidingDownMaxSlope = false;
		slopeNormal = Vector2.zero;

		slopeAngleOld = slopeAngle;
		slopeAngle = 0;
	}
}