using UnityEngine;
using System.Collections;

public enum MovementType {path, loop, reverse_loop};

public class MovementScript : MonoBehaviour {

	#region Variables
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
	private float m_maxTurnSpeed;

	//Waypoints
	private Vector3 m_currentWaypoint;
	private Vector3 m_prevWaypoint;
	private Vector3 m_crossWpDirection;
	private bool m_waypointIsStrict;
	private float m_wpDistance;

	//Custom points
	private Vector3 m_intersectionPoint;
	private Vector3 m_lookAheadPoint;
	private Vector3 m_rawLookAheadPoint;
	//private Vector3 m_previousFramePosition;


	//orientation vectors
	private Vector3 m_direction;
	private Vector3 m_desiredDirection;
	private Vector3 m_nextWpDirection;

	//Pointers
	private int m_currentWaypointIndex;
	private int m_arrayOverflowIndex;
	private int m_arrayLastIndex;

	//debug
	public int m_numberOfLapsComplete;


	//Boolean checks
	private bool m_routeIsFinished = true;
	private bool m_thereIsAtLeastOneMoreWaypoint = false;
	private bool m_arrayReadingDirection = false;

	//Rotation properties
	private float m_rotationDirection;
	private float m_rotAngleDiff;
	private float m_rotTemp;
	#endregion

	#region Initialization
	// Use this for initialization
	void Start () {
		SetUpRoute ();
		m_prevWaypoint = m_currentWaypoint;
		m_maxTurnSpeed = m_speed / 10 ;
		m_direction = GlobalScript.GetDirection (transform.position, m_currentWaypoint);
	}

	public void SetUpRoute()
	{
		if (m_waypoints.Length > 0)
		{
			if (movementType == MovementType.path || movementType == MovementType.loop)
			{
				SetUpRouteValues (true, -1, m_waypoints.Length - 1, m_waypoints.Length);
			}				
			else if (movementType == MovementType.reverse_loop)
			{
				if (m_arrayReadingDirection)
				{
					SetUpRouteValues (false, m_waypoints.Length - 1, 0, -1);

				}
				else
				{
					if (m_currentWaypointIndex == m_waypoints.Length)
						SetUpRouteValues (true, 0, m_waypoints.Length - 1, m_waypoints.Length);
					
					else
						SetUpRouteValues (true, -1, m_waypoints.Length - 1, m_waypoints.Length);
				}
			}
			if (TryPickNewWaypoint ())
			{
				m_routeIsFinished = false;
			}
		}
	}

	private void SetUpRouteValues(bool arrayReadingDirection, int start, int last, int end)
	{
		m_arrayReadingDirection = arrayReadingDirection;
		m_currentWaypointIndex = start;
		m_arrayLastIndex = last;
		m_arrayOverflowIndex = end;
	}
	#endregion

	// Update is called once per frame
	void Update ()
	{
		if (!m_routeIsFinished)
		{
			Move ();
			if (CheckWaypointReaching ())
			{
				if (!TryPickNewWaypoint ())
				{
					HandleRouteFinishing ();
				}
			}
		}
	}

	private Vector3 m_rvoDisposition;	
	private float m_rvoAngle;
	void Move()
	{
		UpdateLookAheadPoint ();
		Rotate ();
		transform.position += m_direction * m_speed;
	}

	private float m_desiredYRotation;
	private float m_angleDiff = 5f;
	void Rotate()
	{
		m_desiredDirection = GlobalScript.GetDirection (transform.position, m_lookAheadPoint);
		if (GlobalScript.GetAngle (m_desiredDirection, m_direction) > 1f)
		{
			//m_angleDiff = Quaternion.Angle (Quaternion.LookRotation (m_desiredDirection), Quaternion.LookRotation (m_direction));
			m_direction = Vector3.Slerp (m_direction, m_desiredDirection, m_angleDiff / (GlobalScript.GetAngle (m_direction, m_desiredDirection)));
			transform.rotation = Quaternion.LookRotation (m_direction);
		}
	}


	//Move look ahead point
	void UpdateLookAheadPoint()
	{	
		m_rawLookAheadPoint = GlobalScript.PredictPointInDirection (transform.position, m_direction, lookAheadDistance);

		if (m_thereIsAtLeastOneMoreWaypoint)
		{
			if (GlobalScript.GetTwoLinesIntersection (transform.position, m_rawLookAheadPoint, m_prevWaypoint, m_currentWaypoint, ref m_intersectionPoint))
			{
				float overflowAmount = lookAheadDistance - GlobalScript.GetDistance (transform.position, m_intersectionPoint);

				if (overflowAmount > 0)
				{
					overflowAmount = Mathf.Min (overflowAmount, lookAheadDistance * m_maxTurnSpeed);
					m_lookAheadPoint = m_intersectionPoint + m_crossWpDirection * overflowAmount;
					return;
				}
			}
		}
		m_lookAheadPoint = m_rawLookAheadPoint;
	}

	//Pick new waypoint
	bool TryPickNewWaypoint()
	{
		if (m_arrayReadingDirection)
			m_currentWaypointIndex++;
		else
			m_currentWaypointIndex--;
		
		if (m_currentWaypointIndex == m_arrayOverflowIndex)
			return false;
		//
		 
		m_prevWaypoint = m_currentWaypoint;
		m_currentWaypoint = m_waypoints [m_currentWaypointIndex].transform.position;

		//next waypoint
		if (m_currentWaypointIndex != m_arrayOverflowIndex)
		{
			m_thereIsAtLeastOneMoreWaypoint = true;

			m_crossWpDirection = GlobalScript.GetDirection (m_prevWaypoint, m_currentWaypoint);
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

	bool CheckWaypointReaching()
	{
		if (GlobalScript.GetDistance (transform.position, m_currentWaypoint) < lookAheadDistance)
			return true;
		return false;
	}



	void HandleRouteFinishing()
	{
		m_numberOfLapsComplete++;
		switch (movementType)
		{
		case MovementType.path:
			{
					m_routeIsFinished = true;
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


	void OnDrawGizmos() {
		if (m_drawGizmos)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere (m_lookAheadPoint, 1f);
			Gizmos.DrawLine (m_lookAheadPoint, transform.position);
			Gizmos.DrawCube (m_rawLookAheadPoint, Vector3.one);

			Gizmos.color = Color.red;
			Gizmos.DrawSphere (m_intersectionPoint, 2f);
			Gizmos.DrawLine (m_intersectionPoint, transform.position);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (m_prevWaypoint, m_currentWaypoint);
		}
	}
}
