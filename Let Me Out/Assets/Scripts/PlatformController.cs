using UnityEngine;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
	[Header("Platform variables")]

	#region Public variables

	[Tooltip("Layer which the platform will carry")]
	public LayerMask passengerMask;

	[Tooltip("Waypoints for the platform")]
	public Vector3[] localWaypoints = new Vector3[1];

	[Tooltip("The speed of the platform")]
	public float Speed = 1;

	[Tooltip("Should the platform go to the first waypoint after the last one")]
	public bool Cyclic;

	[Tooltip("How much time the platform will wait at each waypoint")]
	public float WaitTime;

	[Range(0f, 2f)]
	[Tooltip("How much the platform should ease in on a waypoint")]
	public float EaseAmount;

	#endregion Public variables

	#region Private variables

	private int fromWypointIndex;
	private float percentBetweenWaypoints;
	private float nextMoveTime;
	private Vector3[] globalWaypoints;
	private List<PassengerMovement> passengerMovements = new List<PassengerMovement>();
	private Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	#endregion Private variables

	public override void Start()
	{
		base.Start();

		globalWaypoints = new Vector3[localWaypoints.Length];

		for (int i = 0; i < localWaypoints.Length; i++)
		{
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}
	}

	private void Update()
	{
		UpdateRaycastOrigins();

		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);

		MovePassengers(true);

		transform.Translate(velocity);
		Physics2D.SyncTransforms();

		MovePassengers(false);
	}

	private void MovePassengers(bool beforeMovePlatform)
	{
		foreach (PassengerMovement passenger in passengerMovements)
		{
			if (!passengerDictionary.ContainsKey(passenger.Transform))
				passengerDictionary.Add(passenger.Transform, passenger.Transform.GetComponent<Controller2D>());
			if (passenger.MoveBeforePlatform == beforeMovePlatform)
				passengerDictionary[passenger.Transform].Move(passenger.Velocity, passenger.StandingOnPlatform);
		}
	}

	private Vector3 CalculatePlatformMovement()
	{
		if (Time.time < nextMoveTime)
		{
			return Vector3.zero;
		}
		fromWypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * Speed / distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

		Vector3 newPosition = Vector3.Lerp(globalWaypoints[fromWypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (percentBetweenWaypoints >= 1)
		{
			percentBetweenWaypoints = 0;
			fromWypointIndex++;
			if (!Cyclic)
			{
				if (fromWypointIndex >= globalWaypoints.Length - 1)
				{
					fromWypointIndex = 0;
					System.Array.Reverse(globalWaypoints);
				}
			}
			nextMoveTime = Time.time + WaitTime;
		}

		return newPosition - transform.position;
	}

	private void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovements = new List<PassengerMovement>();

		float directionX = Mathf.Sign(velocity.x);
		float directionY = Mathf.Sign(velocity.y);

		// Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < VerticalRayCount; i++)
			{
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.BottomLeft : raycastOrigins.TopLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (!hit || hit.distance == 0) continue;

				if (movedPassengers.Contains(hit.transform)) continue;

				movedPassengers.Add(hit.transform);
				float pushX = (directionY == 1) ? velocity.x : 0;
				float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

				passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;

			for (int i = 0; i < HorizontalRayCount; i++)
			{
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.BottomLeft : raycastOrigins.BottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (!hit || hit.distance == 0) continue;

				if (movedPassengers.Contains(hit.transform)) continue;

				movedPassengers.Add(hit.transform);
				float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
				float pushY = -skinWidth;

				passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
			}
		}

		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < VerticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.TopLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				if (!hit || hit.distance == 0) continue;

				if (movedPassengers.Contains(hit.transform)) continue;

				movedPassengers.Add(hit.transform);
				float pushX = velocity.x;
				float pushY = velocity.y;

				passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
			}
		}
	}

	private float Ease(float x)
	{
		float a = EaseAmount + 1;
		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
	}

	private void OnDrawGizmos()
	{
		if (localWaypoints != null)
		{
			Gizmos.color = Color.red;
			float size = 0.3f;

			for (int i = 0; i < localWaypoints.Length; i++)
			{
				Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
			}
		}
	}
}

[System.Serializable]
public struct PassengerMovement
{
	public Transform Transform;
	public Vector3 Velocity;
	public bool StandingOnPlatform;
	public bool MoveBeforePlatform;

	public PassengerMovement(Transform transform, Vector3 velocity, bool standingOnPlatform, bool moveBeforePlatform)
	{
		Transform = transform;
		Velocity = velocity;
		StandingOnPlatform = standingOnPlatform;
		MoveBeforePlatform = moveBeforePlatform;
	}
}