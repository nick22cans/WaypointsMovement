using UnityEngine;
using System.Collections;

public enum MovementType {path, loop, reverse_loop};

public class MovementScript : MonoBehaviour {
	
	public GameObject[] m_waypoints;

	public MovementType movementType;

	//rough amount of movement per frame
	public float m_speed;
	//distance calculation error amplitude
	public float distance_Epsilon;
	//direction difference error while turning
	private float direction_Epsilon;

	public float lookAheadDistance;

	public bool m_drawGizmos;
	private float rotationm_speed;


	private int m_currentWaypointIndex;
	private Vector3 m_currentWaypoint;
	private Vector3 m_nextWaypoint;
	private Vector3 m_crossWpDirection;
	private bool m_waypointIsStrict;

	private bool m_isMovingAlongTheRoute = false;
	private bool m_isTurning = false;
	private bool m_thereIsAtLeastOneMoreWaypoint = false;

	private Vector3 m_intersectionPoint;
	private Vector3 m_direction;
	private Vector3 m_wpDirection;
	public Vector3 m_lookAheadPoint;

	Vector3 m_directionChangeStep; 


	private bool arrayReadingDirection = false;
	private int overflowIndex;
	private int lastIndex;


	// Use this for initialization
	void Start () {
		SetUpRoute ();
		rotationm_speed = m_speed / 10;
		direction_Epsilon = m_speed / 3;
	}

	public void SetUpRoute()
	{
		if (m_waypoints.Length > 0)
		{
			if (movementType == MovementType.path 
				|| movementType == MovementType.loop)
			{
				arrayReadingDirection = true;
				m_currentWaypointIndex = -1;
				overflowIndex = m_waypoints.Length;
				lastIndex = overflowIndex - 1;
			}
				
			if (movementType == MovementType.reverse_loop)
			{
				arrayReadingDirection = !arrayReadingDirection;

				if (arrayReadingDirection)
				{
					m_currentWaypointIndex = -1;
					overflowIndex = m_waypoints.Length;
					lastIndex = overflowIndex - 1;
				}
				else
				{
					m_currentWaypointIndex = m_waypoints.Length;
					overflowIndex = -1;
					lastIndex = overflowIndex + 1;
				}
			}
			if (SwitchToTheNextm_waypointsSegment ())
			{
				m_isMovingAlongTheRoute = true;
			}
		}
	}

	bool SwitchToTheNextm_waypointsSegment()
	{
		if (arrayReadingDirection)
			m_currentWaypointIndex++;
		else
			m_currentWaypointIndex--;
		
		if (m_currentWaypointIndex == overflowIndex)
			return false;
		//
		m_currentWaypoint = m_waypoints [m_currentWaypointIndex].transform.position;
		if ((m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ())
			m_waypointIsStrict = (m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().isStrict;
		m_direction = GetDirectionTowardsWaypoint ();
		 
		m_isTurning = false;

		//next waypoint
		if (m_currentWaypointIndex != lastIndex)
		{
			m_thereIsAtLeastOneMoreWaypoint = true;
			if (arrayReadingDirection)
				m_nextWaypoint = m_waypoints [m_currentWaypointIndex + 1].transform.position;
			else
				m_nextWaypoint = m_waypoints [m_currentWaypointIndex - 1].transform.position;

			m_crossWpDirection = GetDirectionFromOnePointToAnother (m_currentWaypoint, m_nextWaypoint);
		} 
		else
		{
			m_thereIsAtLeastOneMoreWaypoint = false;
			if (movementType == MovementType.path)
				m_waypointIsStrict = true;
		}
		
		return true;
	}

	void UpdateLookAheadPoint()
	{
		if (m_isTurning && m_thereIsAtLeastOneMoreWaypoint)
		{
			if (GetTwoLinesIntersection (transform.position, PredictPointInCurrentDirection(lookAheadDistance),
				m_currentWaypoint, m_nextWaypoint))
			{
				float turnAmplitude = (lookAheadDistance - GetDistanceToThePoint (m_intersectionPoint));
				turnAmplitude = Mathf.Min(turnAmplitude, lookAheadDistance * rotationm_speed);

				m_lookAheadPoint = m_intersectionPoint + m_crossWpDirection * turnAmplitude;
			}
		}
		else
			m_lookAheadPoint = PredictPointInCurrentDirection (lookAheadDistance);
	}

	bool CheckWaypointReaching()
	{
		if (m_waypointIsStrict)
		{
			if (GetDistanceToThePoint (m_currentWaypoint) < distance_Epsilon)
				return true;
		}
		else if (m_isTurning)
		{
			if (m_thereIsAtLeastOneMoreWaypoint ||
				movementType == MovementType.loop || 
				movementType == MovementType.reverse_loop)
			{
				Vector3 next_wp_direction = GetDirectionTowardsPoint (m_nextWaypoint);
				if (GetDistanceBetweenPoints (next_wp_direction, m_direction) < direction_Epsilon)
					return true;
			}
//			if (GetDistanceToThePoint (m_currentWaypoint) < distance_Epsilon)
//				return true;
		}
		return false;
	}

		
	void Move()
	{
		UpdateLookAheadPoint ();
		m_direction = GetDirectionTowardsPoint (m_lookAheadPoint);
		m_wpDirection = GetDirectionTowardsWaypoint ();

		// (Vector2.Angle (new Vector2 (m_direction.x, m_direction.z), new Vector2 (m_wpDirection.x, m_wpDirection.z)) > 90 && !m_isTurning)
		if (!m_isTurning)
		{
			m_direction = m_wpDirection;
			m_lookAheadPoint = m_direction * lookAheadDistance;
		}

		transform.position += m_direction * m_speed;

		if (!m_isTurning)
		{
			if (!m_waypointIsStrict)
			{			
				if (GetDistanceToThePoint (m_currentWaypoint) < lookAheadDistance)
				{
					m_isTurning = true;
					//update lookahead for the current waypoint
					m_intersectionPoint = m_currentWaypoint;
					m_lookAheadPoint = m_intersectionPoint + m_crossWpDirection * m_speed;
					m_direction = GetDirectionTowardsPoint (m_lookAheadPoint);
				}
			}
		}
	}

	void HandleRouteFinishing()
	{
		switch (movementType)
		{
		case MovementType.path:
			{
				m_isMovingAlongTheRoute = false;
			}
			break;
		case MovementType.loop:
			{
				SetUpRoute ();
			}
			break;
		case MovementType.reverse_loop:
			{
				SetUpRoute ();
			}
			break;
		default:
			break;
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
				if (CheckWaypointReaching ()) 
				{
					if (!SwitchToTheNextm_waypointsSegment ()) 
					{
						HandleRouteFinishing ();
					}
				}
			}
		}
	}

	void OnDrawGizmos() {
		if (m_drawGizmos)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere (m_lookAheadPoint, 1f);
			Gizmos.DrawLine (m_lookAheadPoint, transform.position);

			Gizmos.DrawCube (PredictPointInCurrentDirection (lookAheadDistance), Vector3.one);
		}
	}
		
	float GetDistanceBetweenPoints(Vector3 p1, Vector3 p2){
		Vector2 diff = p2 - p1;
		diff.y = 0;
		return (p2 - p1).magnitude;
	}
		

	float GetDistanceToThePoint(Vector3 point) {
		return (GetDistanceBetweenPoints (transform.position, point));
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
		
	bool GetTwoLinesIntersection(Vector3 dir_line_s, Vector3 dir_line_e, Vector3 wp_line_s, Vector3 wp_line_e)
	{

		float a1 = dir_line_e.z - dir_line_s.z,
		b1 = dir_line_s.x - dir_line_e.x,
		c1 = a1 * dir_line_s.x + b1 * dir_line_s.z;

		float a2 = wp_line_e.z - wp_line_s.z,
		b2 = wp_line_s.x - wp_line_e.x,
		c2 = a2 * wp_line_s.x + b2 * wp_line_s.z;

		float delta = a1 * b2 - a2 * b1;

		if (delta == 0)
			return false;

		float x = (b2 * c1 - b1 * c2) / delta,
		y = (dir_line_e.y + dir_line_s.y) / 2,
		z = (a1 * c2 - a2 * c1) / delta;

		if (Mathf.Abs (x) < 0.0001)
			x += wp_line_s.x;
		if (Mathf.Abs (z) < 0.0001)
			z += wp_line_s.z;

//		if (x >= Mathf.Min (wp_line_s.x, wp_line_e.x) && x <= Mathf.Max (wp_line_s.x, wp_line_e.x)
//			&& z >= Mathf.Min (wp_line_s.z, wp_line_e.z) && z <= Mathf.Max (wp_line_s.z, wp_line_e.z))
//		{
//			m_intersectionPoint = new Vector3 (x, y, z);
//			return true;
//		}

		x = Mathf.Max (x, Mathf.Min (wp_line_s.x, wp_line_e.x));
		x = Mathf.Min (x, Mathf.Max (wp_line_s.x, wp_line_e.x));
		z = Mathf.Max (z, Mathf.Min (wp_line_s.z, wp_line_e.z));
		z = Mathf.Min (z, Mathf.Max (wp_line_s.z, wp_line_e.z));

		m_intersectionPoint = new Vector3 (x, y, z);
		return true;
	}
}
