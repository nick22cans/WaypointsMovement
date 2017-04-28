using UnityEngine;
using System.Collections;

public enum MovementType {path, loop, reverse_loop};

public class MovementScript : MonoBehaviour {
	
	public GameObject[] m_waypoints;

	public MovementType movementType;

	//rough amount of movement per frame
	public float m_speed;
	//distance calculation error amplitude
	private float distance_Epsilon;
	//direction difference error while turning
	private float direction_Epsilon = 0.1f;

	public float lookAheadDistance;

	public bool m_drawGizmos;
	private float m_turn_speed;


	private int m_currentWaypointIndex;
	private Vector3 m_currentWaypoint;
	private Vector3 m_nextWaypoint;
	private Vector3 m_prevWaypoint;
	private Vector3 m_crossWpDirection;
	private bool m_waypointIsStrict;

	private bool m_isMovingAlongTheRoute = false;
	private bool m_isTurning = false;
	private bool m_thereIsAtLeastOneMoreWaypoint = false;

	private Vector3 m_intersectionPoint;
	private Vector3 m_lookAheadPoint;
	private Vector3 m_rawLookAheadPoint;
	private Vector3 m_previousFramePosition;


	//orientation vectors
	private Vector3 m_direction;
	private Vector3 m_wpDirection;
	private Vector3 next_wp_direction;


	private bool arrayReadingDirection = false;
	private int overflowIndex;
	private int lastIndex;

	//debug
	public int m_numberOfLapsComplete;

	// Use this for initialization
	void Start () {
		SetUpRoute ();
		m_turn_speed = m_speed / 5 ;
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
					if (m_currentWaypointIndex == m_waypoints.Length)
						m_currentWaypointIndex = 0;
					else
						m_currentWaypointIndex = -1;
					overflowIndex = m_waypoints.Length;
					lastIndex = overflowIndex - 1;
				}
				else
				{
					m_currentWaypointIndex = m_waypoints.Length - 1;
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

	private Vector3 m_desiredDirection;
	private bool m_isRotatingOnTheSpot;
	private float m_rotationDirection;
	private float m_rotAngleDiff;
	private float m_rotTemp;


	//Pick new waypoint
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



		if (!m_waypointIsStrict)
			m_direction = GetDirectionTowardsWaypoint ();
		else
		{
			m_desiredDirection = GetDirectionTowardsWaypoint ();
			m_rotAngleDiff = Mathf.Abs (GlobalScript.GetAngleBetweenVectors (m_direction, m_desiredDirection));
			m_rotTemp = m_rotAngleDiff;

			m_isRotatingOnTheSpot = true;
		}
		 
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
		
		if ((m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ())
		{
			m_waypointIsStrict = (m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().isStrict;
			distance_Epsilon = (m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().reachingDistance;
		}
		
			return true;
	}

	//Move look ahead point
	void UpdateLookAheadPoint()
	{			
		if (m_isTurning && m_thereIsAtLeastOneMoreWaypoint)
		{
			if (GlobalScript.GetTwoLinesIntersection (transform.position, m_rawLookAheadPoint, m_currentWaypoint, m_nextWaypoint, ref m_intersectionPoint))
			{
				float turnAmplitude = (lookAheadDistance - GetDistanceToThePoint (m_intersectionPoint));
				turnAmplitude = Mathf.Min (turnAmplitude, lookAheadDistance * m_turn_speed);

				m_lookAheadPoint = m_intersectionPoint + m_crossWpDirection * turnAmplitude;
			}
		}
		else
			m_lookAheadPoint = m_rawLookAheadPoint;
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
				next_wp_direction = GetDirectionTowardsPoint (m_nextWaypoint);
				//print (GetAngleBetweenVectors (next_wp_direction, m_direction));
				if (GlobalScript.GetAngleBetweenVectors(next_wp_direction,m_direction) < direction_Epsilon)
					return true;
			}
//			if (GetDistanceToThePoint (m_currentWaypoint) < distance_Epsilon)
//				return true;
		}
		return false;
	}

	public float m_yAngle;
	private Vector3 m_rvoDisposition;	
	private float m_rvoAngle;
	void Move()
	{
		m_rawLookAheadPoint = GlobalScript.PredictPointInDirection (m_previousFramePosition, m_direction, lookAheadDistance);
		UpdateLookAheadPoint ();
		m_direction = GetDirectionTowardsPoint (m_lookAheadPoint);
		m_wpDirection = GetDirectionTowardsWaypoint ();

		// (Vector2.Angle (new Vector2 (m_direction.x, m_direction.z), new Vector2 (m_wpDirection.x, m_wpDirection.z)) > 90 && !m_isTurning)
		if (!m_isTurning)
		{
			m_direction = m_wpDirection;
			m_lookAheadPoint = m_rawLookAheadPoint;
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
		m_yAngle = GlobalScript.GetAngleBetweenVectors (m_direction,Vector3.forward);
		transform.rotation = Quaternion.Euler (new Vector3 (0, m_yAngle,0));

		m_previousFramePosition = transform.position;
	}


	void HandleRouteFinishing()
	{
		m_numberOfLapsComplete++;
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

	public float m_rotationSpeed;

	// Update is called once per frame
	void Update () 
	{
		if (m_isMovingAlongTheRoute) 
		{
			if (m_isRotatingOnTheSpot)
			{
				if (m_rotAngleDiff <= 0)
				{

					m_direction = m_desiredDirection;
					m_isRotatingOnTheSpot = false;
				}
				else
				{
					m_rotAngleDiff -= m_turn_speed;
					//transform.rotation =  Quaternion.Lerp();
					transform.rotation = Quaternion.Slerp (Quaternion.LookRotation (m_direction), Quaternion.LookRotation (m_desiredDirection), 1 - m_rotAngleDiff / m_rotTemp);
				}
			}
			else
			{
				
				Move ();
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
	}

	void OnDrawGizmos() {
		if (m_drawGizmos)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere (m_lookAheadPoint, 1f);
			Gizmos.DrawLine (m_lookAheadPoint, transform.position);

			Gizmos.DrawCube (m_rawLookAheadPoint, Vector3.one);
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


}
