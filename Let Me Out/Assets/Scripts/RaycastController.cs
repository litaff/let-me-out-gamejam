using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
	[Header("Raycast variables")]

	#region Public variables

	[Tooltip("Used to determin the target layer for a raycast")]
	public LayerMask CollisionMask;

	[Tooltip("The density of raycastes")]
	public float DistanceBetweenRays = 0.25f;

	#endregion Public variables

	#region Protected variables

	protected RaycastOrigins raycastOrigins;
	protected const float skinWidth = 0.015f;
	protected float horizontalRaySpacing;
	protected float verticalRaySpacing;
	protected int HorizontalRayCount;
	protected int VerticalRayCount;

	#endregion Protected variables

	private BoxCollider2D boxCollider;

	public virtual void Start()
	{
		boxCollider = GetComponent<BoxCollider2D>();

		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand(skinWidth * -2);

		raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	private void CalculateRaySpacing()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand(skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		HorizontalRayCount = Mathf.RoundToInt(boundsHeight / DistanceBetweenRays);
		VerticalRayCount = Mathf.RoundToInt(boundsWidth / DistanceBetweenRays);

		HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
		VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
	}
}

[System.Serializable]
public struct RaycastOrigins
{
	public Vector2 TopLeft, TopRight;
	public Vector2 BottomLeft, BottomRight;
}