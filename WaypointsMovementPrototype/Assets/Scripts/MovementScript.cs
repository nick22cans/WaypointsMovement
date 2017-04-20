using UnityEngine;
using System.Collections;

public class MovementScript : MonoBehaviour {
	
	public GameObject[] waypoints;
	//rough amount of movement per frame
	public float speed;
	//distance calculation error amplitude
	public float distance_sqrEpsilon;
	//direction difference error while turning
	public float direction_Epsilon;

	public float lookAheadDistance;
	public float rotationSpeed;


	private int m_currentWaypointIndex;
	private Vector3 m_currentWaypoint;
	private bool m_waypointIsStrict;

	private bool m_isMovingAlongTheRoute = false;
	private bool m_isTurning = false;
	private bool m_thereIsAtLeastOneMoreWaypoint = false;

	private Vector3 m_intersectionPoint;
	private Vector3 m_direction;
	private Vector3 m_lookAheadPoint;

	Vector3 m_directionChangeStep; 

	// Use this for initialization
	void Start () {
		SetUpRoute ();
	}

	public void SetUpRoute()
	{
		if (waypoints.Length > 0)
		{
			m_currentWaypointIndex = -1;
			if (SwitchToTheNextWaypoint ())
			{
				m_isMovingAlongTheRoute = true;
			}
		}
	}

	bool SwitchToTheNextWaypoint()
	{
		m_currentWaypointIndex++;
		if (m_currentWaypointIndex == waypoints.Length)
			return false;
		if (m_currentWaypointIndex != waypoints.Length - 1)
			m_thereIsAtLeastOneMoreWaypoint = true;
		else
			m_thereIsAtLeastOneMoreWaypoint = false;

		m_currentWaypoint = waypoints [m_currentWaypointIndex].transform.position;
		m_waypointIsStrict = (waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().isStrict;
		m_direction = GetDirectionTowardsWaypoint ();

		m_isTurning = false;
		return true;
	}

	void UpdateLookAheadPoint()
	{
		if (m_isTurning && m_thereIsAtLeastOneMoreWaypoint)
		{
			if (GetTwoLinesIntersection (transform.position, PredictPointInCurrentDirection(lookAheadDistance), m_currentWaypoint, waypoints [m_currentWaypointIndex + 1].transform.position))
			{
				float distanceToTheIntersection = GetDistanceToThePoint (m_intersectionPoint);
				Vector3 crossWpDirection = GetDirectionFromOnePointToAnother (m_currentWaypoint, waypoints [m_currentWaypointIndex + 1].transform.position);

				float turnAmplitude = (lookAheadDistance - distanceToTheIntersection);
				turnAmplitude = Mathf.Max(lookAheadDistance, lookAheadDistance * rotationSpeed);

				m_lookAheadPoint = m_intersectionPoint + crossWpDirection * turnAmplitude;
			}
		}
		else
			m_lookAheadPoint = PredictPointInCurrentDirection (lookAheadDistance);
	}

	bool WaypointIsReached()
	{
		if (m_waypointIsStrict && GetDistanceToThePoint (m_currentWaypoint) < distance_sqrEpsilon)
				return true;
		if (m_thereIsAtLeastOneMoreWaypoint)
		{
			Vector3 next_wp_direction = GetDirectionTowardsPoint (waypoints [m_currentWaypointIndex + 1].transform.position);
			if (GetDistanceBetweenPoints (next_wp_direction, m_direction) < direction_Epsilon)
				return true;
			if (GetDistanceToThePoint (m_intersectionPoint) < distance_sqrEpsilon)
				return true;
		} 
		if (GetDistanceToThePoint (m_currentWaypoint) < distance_sqrEpsilon)
			return true;
		return false;
	}

		
	void Move()
	{
		UpdateLookAheadPoint ();

		m_direction = GetDirectionTowardsPoint (m_lookAheadPoint);

		transform.position += m_direction * speed;

		if (!m_isTurning && !m_waypointIsStrict)
		{
			
			if (GetDistanceToThePoint (m_currentWaypoint) < lookAheadDistance)
			{
				m_isTurning = true;
			}
		}
	}


	// Update is called once per frame
	void Update () 
	{
		if (m_isMovingAlongTheRoute) 
		{
			Move();
			if ((m_isTurning && !m_waypointIsStrict) || m_waypointIsStrict) 
			{
				if (WaypointIsReached ()) 
				{
					if (!SwitchToTheNextWaypoint ()) 
					{
						m_isMovingAlongTheRoute = false;
						transform.position = new Vector3 (m_currentWaypoint.x, transform.position.y, m_currentWaypoint.z);

					}
				}
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (m_lookAheadPoint, 1f);
		Gizmos.DrawLine (m_lookAheadPoint, transform.position);

		Gizmos.DrawCube (PredictPointInCurrentDirection (lookAheadDistance), Vector3.one);
	}
		
	float GetDistanceBetweenPoints(Vector3 p1, Vector3 p2){
		Vector2 diff = p2 - p1;
		diff.y = 0;
		return (p2 - p1).magnitude;
	}
		

	float GetDistanceToThePoint(Vector3 point) {
		return ((point - transform.position).magnitude);
	}

	Vector3 GetDirectionFromOnePointToAnother(Vector3 from, Vector3 to)
	{
		Vector3 result = (to - from).normalized;
		result.y = 0;	
		return result;
	}

	Vector3 GetDirectionTowardsPoint(Vector3 point){
		return GetDirectionFromOnePointToAnother(transform.position, point);
	}

	Vector3 GetDirectionTowardsWaypoint(){
		return GetDirectionTowardsPoint(m_currentWaypoint);
	}

	Vector3 PredictPointInCurrentDirection(float distance){
		return(transform.position + m_direction * distance);
	}
		
	bool GetTwoLinesIntersection(Vector3 l1_s, Vector3 l1_e, Vector3 l2_s, Vector3 l2_e)
	{
		if (GetDistanceBetweenPoints (l1_e, l2_s) < distance_sqrEpsilon)
			return false;
		float a1 = l1_e.z - l1_s.z,
		b1 = l1_s.x - l1_e.x,
		c1 = a1 * l1_s.x + b1 * l1_s.z;

		float a2 = l2_e.z - l2_s.z,
		b2 = l2_s.x - l2_e.x,
		c2 = a2 * l2_s.x + b2 * l2_s.z;

		float delta = a1 * b2 - a2 * b1;

		if (delta == 0)
			return false;
		m_intersectionPoint = new Vector3 ((b2 * c1 - b1 * c2) / delta, (l1_e.y + l1_s.y)/2, (a1 * c2 - a2 * c1) / delta);
		return true;
	}
}
