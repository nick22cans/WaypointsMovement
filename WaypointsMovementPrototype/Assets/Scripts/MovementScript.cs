using UnityEngine;
using System.Collections;

public enum MovementType {path, loop, reverse_loop};

public class MovementScript : MonoBehaviour {

	#region Variables
	public GameObject[] m_waypoints;
	public MovementType movementType;

	//rough amount of movement per frame
	public float m_speed = 0.5f;
	//maximum degrees amount at which the object can rotate per frame
	public float m_maxAngularRotationSpeed = 2f;
	public float lookAheadDistance = 20f;
	public bool m_drawGizmos;

	//Waypoints
	private Vector3 m_currentWaypoint;
	private Vector3 m_prevWaypoint;
	private Vector3 m_crossWpDirection;
	private bool m_waypointIsStrict;
	private float m_strictWpReachingDistance;
	private float m_overflowAmount;

	//Custom points
	private Vector3 m_intersectionPoint;
	private Vector3 m_lookAheadPoint;
	private Vector3 m_rawLookAheadPoint;
	private Vector3 m_prevFrameLocation;


	//orientation vectors
	private Vector3 m_direction;
	private Vector3 m_desiredDirection;

	//Pointers
	private int m_currentWaypointIndex;
	private int m_arrayOverflowIndex = -1;
	private int m_arrayLastIndex;

	//Boolean checks
	private bool m_routeIsFinished = true;
	private bool m_arrayReadingDirection = false;
	private bool m_isTurningAround = false;
	private bool m_isMoving = true;
	#endregion

	private WNS_AnimationControllerScript m_animationScript;

	#region Initialization
	// Use this for initialization
	void Start () {
		SetUpRoute ();
		m_prevWaypoint = m_currentWaypoint;
		m_desiredDirection = m_direction = GlobalScript.GetDirection (transform.position, m_currentWaypoint);
		m_animationScript = GetComponent<WNS_AnimationControllerScript> ();
		if (m_animationScript == null)
			print ("Animation is not linked");
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
					if (m_currentWaypointIndex == m_arrayOverflowIndex)
						SetUpRouteValues (true, 0, m_waypoints.Length - 1, m_waypoints.Length);
					
					else
						SetUpRouteValues (true, -1, m_waypoints.Length - 1, m_waypoints.Length);
				}
			}
			if (!TryPickNewWaypoint ())
				m_routeIsFinished = true;
			else
				m_routeIsFinished = false;


			if (movementType == MovementType.reverse_loop)
				m_isTurningAround = true;
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

	bool dir = true;
	public bool m_dynamicSpeedChange;
	// Update is called once per frame
	void Update ()
	{
		if (m_dynamicSpeedChange)
		{
			if (dir)
			{
				m_speed += 0.01f;
				if (m_speed > 2f)
					dir = !dir;
			}
			else
			{
				m_speed -= 0.01f;
				if (m_speed < 0f)
					dir = !dir;
			}
		}
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
		
	void Move()
	{
		UpdateLookAheadPoint ();
		Rotate ();
		m_prevFrameLocation = transform.position;
		if (m_isMoving)
		{
			transform.position += m_direction * m_speed;
			if (m_animationScript)
					m_animationScript.HandleSpeedChange (GlobalScript.GetDistance (transform.position, m_prevFrameLocation));

		}
	}
		
	void Rotate()
	{
		if (m_waypointIsStrict || m_isTurningAround)
			m_desiredDirection = GlobalScript.GetDirection (transform.position, m_currentWaypoint);
		else
			m_desiredDirection = GlobalScript.GetDirection (transform.position, m_lookAheadPoint);

//		if (GlobalScript.GetAngle (m_desiredDirection, GlobalScript.GetDirection (transform.position, m_currentWaypoint)) > 120 &&
//			GlobalScript.GetDistance(m_currentWaypoint,transform.position) > GlobalScript.GetDistance(m_currentWaypoint,m_prevFrameLocation))
//		{
//			m_desiredDirection = GlobalScript.GetDirection (transform.position, m_currentWaypoint);
//			m_isMoving = false;
//		}

		if (!m_isMoving)
			if (GlobalScript.GetAngle (m_desiredDirection, m_direction) < m_maxAngularRotationSpeed)
				m_isMoving = true;
			

		if (GlobalScript.GetAngle (m_desiredDirection, m_direction) > 1f)
			m_direction = Vector3.Slerp (m_direction, m_desiredDirection, m_maxAngularRotationSpeed / (GlobalScript.GetAngle (m_direction, m_desiredDirection)));
		transform.rotation = Quaternion.LookRotation (m_direction);
	}

	//Move look ahead point
	void UpdateLookAheadPoint()
	{	
		m_rawLookAheadPoint = GlobalScript.PredictPointInDirection (transform.position, m_direction, lookAheadDistance);

		if (!m_waypointIsStrict)
			if (GlobalScript.GetTwoLinesIntersection (transform.position, m_rawLookAheadPoint, m_prevWaypoint, m_currentWaypoint, ref m_intersectionPoint))
			{
				m_overflowAmount = lookAheadDistance - GlobalScript.GetDistance (transform.position, m_intersectionPoint);
				if (m_overflowAmount > 0)
				{
					m_overflowAmount = Mathf.Min (m_overflowAmount, m_speed * lookAheadDistance / 10);
					m_lookAheadPoint = m_intersectionPoint + m_crossWpDirection * m_overflowAmount;
					return;
				}
				else
				{
					m_lookAheadPoint = GlobalScript.GetDirection (transform.position, m_currentWaypoint) * lookAheadDistance;
					return;
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
		{
			return false;
		}		 
			
		m_isTurningAround = false; 
		m_prevWaypoint = m_currentWaypoint;
		m_currentWaypoint = m_waypoints [m_currentWaypointIndex].transform.position;
		m_crossWpDirection = GlobalScript.GetDirection (m_prevWaypoint, m_currentWaypoint);
		if (m_waypointIsStrict)
			m_desiredDirection = GlobalScript.GetDirection (transform.position, m_currentWaypoint);
		
		if ((m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ())
		{
			m_waypointIsStrict = (m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().isStrict;
			m_strictWpReachingDistance = (m_waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().reachingDistance;
		}

		if (m_currentWaypointIndex == m_arrayLastIndex)
			m_waypointIsStrict = true;
		
		return true;
	}

	bool CheckWaypointReaching()
	{
		if (m_waypointIsStrict)
		{
			if (GlobalScript.GetDistance (transform.position, m_currentWaypoint) < m_strictWpReachingDistance)
				return true;
		}
		else
		{
			if (GlobalScript.GetDistance (transform.position, m_currentWaypoint) < lookAheadDistance)
				return true;
		}
			
		return false;
	}

	void HandleRouteFinishing()
	{
		switch (movementType)
		{
		case MovementType.path:
			{
				m_routeIsFinished = true;
					if (m_animationScript)
						m_animationScript.Stop ();
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


	void OnDrawGizmos() {
		if (m_drawGizmos)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere (m_lookAheadPoint, 0.5f);
			Gizmos.DrawLine (m_lookAheadPoint, transform.position);
			Gizmos.DrawCube (m_rawLookAheadPoint, Vector3.one);

			Gizmos.color = Color.red;
			Gizmos.DrawSphere (m_intersectionPoint, 1f);
			Gizmos.DrawLine (m_intersectionPoint, transform.position);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (m_prevWaypoint, m_currentWaypoint);
		}
	}
}