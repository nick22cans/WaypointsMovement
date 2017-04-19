using UnityEngine;
using System.Collections;

public class MovementScript : MonoBehaviour {
	
	public GameObject[] waypoints;
	//rough amount of movement per frame
	public float movementStep;
	//distance calculation error amplitude
	public float epsilon;
	//units = frames number
	public float m_directionTransitionDuration;

	#region waypoints
	private int m_currentWaypointIndex;
	private float m_currentWpReachingDistance;
	private GameObject m_currentWaypoint;
	private bool m_waypointIsStrict;
	#endregion

	private bool m_isMovingAlongTheRoute = false;

	#region direction interpolation
	private bool m_isInterpolatingDirection;
	private Vector3 m_interpolationStep;
	private Vector3 m_fromDirection;
	private Vector3 m_toDirection;
	private Vector3 m_currentDirection;
	#endregion


	// Use this for initialization
	void Start () {
		SetUpRoute ();
	}

	public void SetUpRoute()
	{
		if (waypoints.Length > 0) {
			m_currentWaypointIndex = -1;
			if (PickNewWaypoint()) {
				m_isMovingAlongTheRoute = true;
			}
		}
	}

	bool PickNewWaypoint()
	{
		m_currentWaypointIndex++;
		if (m_currentWaypointIndex == waypoints.Length)
			return false;
		
		m_currentWaypoint = waypoints [m_currentWaypointIndex];
		m_currentWpReachingDistance = (m_currentWaypoint).GetComponent<WaypointScript> ().reachingDistance;

		if (m_isInterpolatingDirection) {
			m_toDirection = (m_currentWaypoint.transform.position - waypoints [m_currentWaypointIndex - 1].transform).normalized;
			m_interpolationStep = (m_toDirection - m_fromDirection)/
		}

		if (m_currentWpReachingDistance == 0)
			m_waypointIsStrict = true;
		else
			m_waypointIsStrict = false;
		m_currentDirection = GetDirectionTowardsWaypoint ();
		return true;
	}

	bool IsWaypointReached()
	{
		float distance = GetDistanceToTheWaypoint ();
		if (distance < m_currentWpReachingDistance && !m_waypointIsStrict) {
			m_isInterpolatingDirection = true;
			m_fromDirection = m_currentDirection;
			return true;
		}
		if (m_waypointIsStrict && distance <= epsilon) {
			m_isInterpolatingDirection = false;
			return true;
		}
		return false;
	}

	void PerformMovementStep()
	{
		if (m_isInterpolatingDirection)
			
		transform.position += m_currentDirection * movementStep;
	}

	float GetDistanceToTheWaypoint() {
		return ((m_currentWaypoint.transform.position - transform.position).magnitude);
	}

	Vector3 GetDirectionTowardsWaypoint(){
		return ((m_currentWaypoint.transform.position - transform.position).normalized);
	}


	// Update is called once per frame
	void Update () {
		if (m_isMovingAlongTheRoute) {
			if (IsWaypointReached ()) {
				if (!PickNewWaypoint ())
					m_isMovingAlongTheRoute = false;
			} else
				PerformMovementStep ();
		}
	}
}
