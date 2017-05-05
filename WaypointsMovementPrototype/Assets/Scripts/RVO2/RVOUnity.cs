using UnityEngine;
using System.Collections.Generic;

public static class RVOExtensions {
	public static Vector3 XY(this RVO.Vector2 _this) {
		return new Vector3(_this.x(), _this.y(), 0);
	}
	public static Vector3 XZ(this RVO.Vector2 _this) {
		return new Vector3(_this.x(), 0, _this.y());
	}
	public static RVO.Vector2 XY(this Vector3 _this) {
		return new RVO.Vector2(_this.x, _this.y);
	}
	public static RVO.Vector2 XZ(this Vector3 _this) {
		return new RVO.Vector2(_this.x, _this.z);
	}
	public static Vector3 SafeNormalized(this RVO.Vector2 _this) {
		float magSqrd = _this.x() * _this.x() + _this.y() * _this.y();
		if (magSqrd > 0.0001f) {
			var mag = Mathf.Sqrt(magSqrd);
			return new Vector3(_this.x() / mag, 0, _this.y() / mag);
		}
		return Vector3.zero;
	}
}

public class RVOUnityMgr : MonoBehaviour {
	public Dictionary<string, RVOUnityInstance> m_instances = new Dictionary<string, RVOUnityInstance>();
	void Update() {
		foreach (var i in m_instances) {
			i.Value.Step();
		}
	}

	//====

	public static RVOUnityMgr Me;
	public static void CheckMe() {
		if (Me == null) {
			var go = new GameObject("RVOMgr");
			Me = go.AddComponent<RVOUnityMgr>();
		}
	}
	public static RVOUnityInstance GetInstance(string _name) {
		CheckMe();

		if (_name == null) _name = "";
		RVOUnityInstance rc;
		if (!Me.m_instances.TryGetValue(_name, out rc)) {
			rc = new RVOUnityInstance(_name);
			Me.m_instances[_name] = rc;
		}
		return rc;
	}
	public static bool InstanceExists(string _name) {
		CheckMe();

		if (_name == null) _name = "";
		return Me.m_instances.ContainsKey(_name);
	}
}

public class RVOUnityInstance {
	public class RVOObstacle {
		static List<RVO.Vector2> s_verts = new List<RVO.Vector2>();
		public Vector3 m_position;
		public Vector3 m_extents;
		public RVOObstacle(Vector3 _pos, Vector3 _extents) {
			m_position = _pos;
			m_extents = _extents;
		}
		public Vector3 Corner(int _c) {
			switch (_c) {
			case 0:
				return m_position - m_extents;
			case 1:
				return m_position + new Vector3(-m_extents.x, m_extents.y, m_extents.z);
			case 2:
				return m_position + m_extents;
			case 3:
				return m_position + new Vector3(m_extents.x, -m_extents.y, -m_extents.z);
			}
			return Vector3.zero;
		}
		public void Add(RVO.Simulator _sim) {
			s_verts.Clear();
			for (int i = 0; i < 4; i ++)
				s_verts.Add(Corner(i).XZ());
			_sim.addObstacle(s_verts);
		}
	}

	//=================

	public string m_name;
	public Vector3 m_side = Vector3.right;
	public Vector3 m_fwd = Vector3.forward; // defaults to XY

	public RVO.Simulator m_instance = new RVO.Simulator();

	public List<RVOUnity> m_agents = new List<RVOUnity>();
	public List<int> m_removes = new List<int>();
	public List<RVOObstacle> m_obstacles = new List<RVOObstacle>();
	public bool m_forceRefresh = false;

	public delegate bool CheckMoveFn(GameObject _who, Vector3 _from, ref Vector3 _to); // returns false if the move is illegal, can override the Up component of the _to vector
	public CheckMoveFn m_checkMoveFn;

	public RVOUnityInstance(string _name) {
		m_name = _name;
		Init();
	}

	float _neighbourDistance = 100.0f;
	private int _maxNeighbours = 50;
	private float _timeHorizon = 50.0f;//40.0f;
	private float _timeHorizonObstacles = 1.0f;//40.0f;
	private float _defaultRadius = 0.5f;
	private float _maxSpeed = 30.0f;//5.0f;
	public void Init() {
		m_instance.setAgentDefaults(_neighbourDistance, _maxNeighbours, _timeHorizon,_timeHorizonObstacles, _defaultRadius, _maxSpeed, new RVO.Vector2());
	}

	public bool CheckMove(GameObject _go, Vector3 _from, ref Vector3 _to) {
		if (m_checkMoveFn == null) return true;
		return m_checkMoveFn(_go, _from, ref _to);
	}

	public void ExecuteRemoves() {
		if (m_removes.Count > 0 || m_forceRefresh) {
			if (m_removes.Count > 0) {
				m_removes.Sort();
				for (int i = m_removes.Count-1; i >= 0; i --)
					m_agents.RemoveAt(m_removes[i]);
				m_removes.Clear();
			}

			m_instance.Clear();
			Init();
			for (int i = 0; i < m_obstacles.Count; i ++)
				m_obstacles[i].Add(m_instance);
			m_instance.processObstacles();
			for (int i = 0; i < m_agents.Count; i ++) {
				m_agents[i].Add();
				if (m_agents[i].Id != i)
					Debug.LogErrorFormat("RVO instance mismatch [rebuild] {0} {1}", i, m_agents[i].Id);
			}
		}
	}

	public Vector3 GetAgentPosition(int _id) {
		var pos = m_instance.getAgentPosition(_id);
		return pos.x() * m_side + pos.y() * m_fwd;
	}
	public void SetAgentPosition(int _id, Vector3 _pos) {
		m_instance.setAgentPosition(_id, new RVO.Vector2(Vector3.Dot(_pos, m_side), Vector3.Dot(_pos, m_fwd)));
	}
	public float DistanceSquaredInPlane(Vector3 _a, Vector3 _b) {
		Vector3 d = _b - _a;
		float side = Vector3.Dot(d, m_side), fwd = Vector3.Dot(d, m_fwd);
		return side*side + fwd*fwd;
	}
	public void SetCheckMoveFn(CheckMoveFn _fn) {
		m_checkMoveFn = _fn;
	}

	float m_nextTimeStep = 1.0f/30.0f;
	public float NextTimeStep { get { return m_nextTimeStep; } }
	public void Step() {
		m_nextTimeStep = Mathf.Clamp(Time.deltaTime, 1.0f/100.0f, 1.0f/15.0f);

		ExecuteRemoves();

		foreach (var i in m_agents)
			i.PreUpdate ();
		
		m_instance.setTimeStep(m_nextTimeStep);
		m_instance.doStepST();

		foreach (var i in m_agents)
			i.Sync();
	}
}

[DisallowMultipleComponent]
public class RVOUnity : MonoBehaviour {
	public static Color s_gizmoColour = new Color(0.7f, 0.8f, 1.0f, 0.75f);

	public static void SetupSim(string _instanceName, Vector3 _side, Vector3 _fwd) {
		var inst = RVOUnityMgr.GetInstance(_instanceName);
		inst.m_side = _side;
		inst.m_fwd = _fwd;
	}
	public static void SetSimDefaults(string _instanceName, float _neighbourDistance, int _maxNeighbours, float _defaultRadius, float _maxSpeed) {
		var inst = RVOUnityMgr.GetInstance(_instanceName);
		inst.m_instance.setAgentDefaults(_neighbourDistance, _maxNeighbours, 10.0f,10.0f, _defaultRadius, _maxSpeed, new RVO.Vector2());
	}
	public static void SetMoveCheck(string _instanceName, RVOUnityInstance.CheckMoveFn _fn) {
		var inst = RVOUnityMgr.GetInstance(_instanceName);
		inst.SetCheckMoveFn(_fn);
	}
	public static void AddAgent(GameObject _go, string _instance = "", float _radius = -1, bool _static = false) {
		var rvo = _go.AddComponent<RVOUnity>();
		rvo.SetInstance(_instance);
		rvo.SetStatic(_static);
		if (_radius != -1)
			rvo.SetRadius(_radius);
	}
	public static bool InstanceExists(string _instanceName) {
		return RVOUnityMgr.InstanceExists(_instanceName);
	}
	public static int NumObstacles(string _instanceName) {
		var inst = RVOUnityMgr.GetInstance(_instanceName);
		return inst.m_obstacles.Count;
	}
	public static void AddObstacle(string _instanceName, Vector3 _pos, Vector3 _extents) {
		var inst = RVOUnityMgr.GetInstance(_instanceName);
		inst.m_obstacles.Add(new RVOUnityInstance.RVOObstacle(_pos, _extents));
		inst.m_forceRefresh = true;
	}

	private RVOUnityInstance m_instance;
	public Vector3 m_pos;

	private Vector3 m_expectedDirection;

	public string m_instanceName;
	public float m_radius;
	public bool m_static;
	public bool m_drawGizmos;
	private int m_id;
	private bool m_lastMoveSuccessful;

	private WNS_AnimationControllerScript m_animationScript;
	private MovementScript m_WNSScript;

	public int Id { get { return m_id; } }
	void Start() {
		Add();
		if (m_id != m_instance.m_agents.Count)
			Debug.LogErrorFormat("RVO instance mismatch {0} {1}", m_instance.m_agents.Count, m_id);
		m_instance.m_agents.Add(this);

		m_animationScript = gameObject.GetComponent<WNS_AnimationControllerScript> ();
		m_WNSScript = gameObject.GetComponent<MovementScript> (); 
		m_pos = m_instance.GetAgentPosition (m_id);
	}
	public void Add() {
		if (m_instance == null) {
			m_instance = RVOUnityMgr.GetInstance(m_instanceName);
		}
		m_id = m_instance.m_instance.addAgent(transform.position.XZ());
		if (m_radius != 0)
			m_instance.m_instance.setAgentRadius(m_id, m_radius);
		ClearVelocity();
	}
	public static Vector3 RotateVectorAboutVector(Vector3 _v, Vector3 _axis, float _radians) {
		float cos = Mathf.Cos(_radians), sin = Mathf.Sin(_radians);
		return _v * cos + Vector3.Cross(_axis, _v) * sin + _axis * (Vector3.Dot(_axis, _v) * (1-cos));
	}
	public static RVO.Vector2 RotateVector(RVO.Vector2 _v, float _radians) {
		float cos = Mathf.Cos(_radians), sin = Mathf.Sin(_radians);
		return _v * cos + new RVO.Vector2(_v.y(), -_v.x()) * sin;
	}
	public void UpdatePosition() {
		if (m_radius != m_instance.m_instance.getAgentRadius (m_id))
			m_instance.m_instance.setAgentRadius (m_id, m_radius);
		var newPos = transform.position.XZ();
		var oldPos = m_instance.GetAgentPosition(m_id).XZ();
		var velocity = (newPos - oldPos) / m_instance.NextTimeStep;

		float randomness = 0;//0.3f;
		float smoothness = 0.9f;

		float theta = UnityEngine.Random.Range(-1.0f, 1.0f) * randomness;
		velocity = RotateVector(velocity, theta);

		var smoothVelocity = m_instance.m_instance.getAgentVelocity(m_id) * smoothness + velocity * (1-smoothness);

		m_instance.m_instance.setAgentVelocity (m_id, smoothVelocity);
		m_instance.m_instance.setAgentPrefVelocity (m_id, velocity);
		m_expectedDirection = velocity.SafeNormalized();
	}
	public void PreUpdate() {
		UpdatePosition ();
	}
	void ClearVelocity() {
		m_instance.m_instance.setAgentVelocity(m_id, new RVO.Vector2());
		m_instance.m_instance.setAgentPrefVelocity(m_id, new RVO.Vector2());
	}

	float GetAngleBetweenVectors(Vector3 first, Vector3 second)
	{
		float dot = Vector3.Dot (first, second) / (first.magnitude * second.magnitude);
		var acos = Mathf.Acos(dot);
		return(acos*180/Mathf.PI);
	}

	private Vector3 m_goalDirection;
	private Vector3 m_rvoDirection;
	private float m_rvoAngle;



	public void Sync() {
		var pos = m_instance.GetAgentPosition(m_id);
		pos.y = transform.position.y;
		m_lastMoveSuccessful = !(m_static || !m_instance.CheckMove(gameObject, m_pos, ref pos) || float.IsNaN(pos.sqrMagnitude));

		if (!m_lastMoveSuccessful)
			m_instance.SetAgentPosition(m_id, transform.position);
		else {

			m_goalDirection = GetDirectionFromOnePointToAnother(m_pos, pos);
			m_rvoDirection = GetDirectionFromOnePointToAnother (m_pos,transform.position);
			m_rvoAngle = Mathf.Abs (GetAngleBetweenVectors (m_goalDirection, m_rvoDirection));
			//UpdateImpatience_v2(m_rvoAngle);

			if (GlobalScript.GetDistance (m_pos, pos) < 0.01f)
				m_impatience += 0.01f;
			else
				m_impatience -= 0.01f;
			m_impatience = Mathf.Clamp01 (m_impatience);
			if (m_impatience > 0) 
			{
				pos = transform.position * m_impatience + pos * (1-m_impatience);
				m_instance.SetAgentPosition(m_id, pos);
			}
//
//			if (m_WNSScript)
//				m_WNSScript.HandlePushBack (ref pos, m_pos);
//
//			if (m_animationScript)
//				m_animationScript.HandleSpeedChange (pos, transform.position, m_pos);

			transform.position = pos;
			m_instance.SetAgentPosition(m_id, pos);
		}
		m_pos = m_instance.GetAgentPosition(m_id);
	}

	private float m_impatience;

	//Mode
	public enum ImpatienceMode { Disabled, Light, Medium, Strong};
	public ImpatienceMode m_impatienceMode;


	private float m_rvoDisposition;
	private float m_rvoDispOverflow;



	private float m_impAngle_Threshold;
	private float m_impAngle_SmallThreshold = 90;
	private float m_impAngle_HighThreshold = 120f;

	private float m_impAngle_Coeff;
	private float m_impAngle_SmallCoeff = 0.00005f;
	private float m_impAngle_HighCoeff = 0.0001f;

	void UpdateImpatience_v2(float angle)
	{
		if (m_impatienceMode != ImpatienceMode.Disabled)
		{
			if (m_impatience < 0.3f)
			{
				m_impAngle_Coeff = m_impAngle_SmallCoeff;
				m_impAngle_Threshold = m_impAngle_SmallThreshold;
			}
			else
			{
				m_impAngle_Coeff = m_impAngle_HighCoeff;
				m_impAngle_Threshold = m_impAngle_HighThreshold;
			}

			m_rvoDispOverflow = angle - m_impAngle_Threshold;
			m_impatience += m_impAngle_Coeff * m_rvoDispOverflow;
			m_impatience = Mathf.Max (m_impatience, 0);
			m_impatience = Mathf.Min (m_impatience, 1);
		}
	}

	void OnDestroy() {
		if (m_instance != null)
		{
			m_instance.m_removes.Add (m_id);
			m_id = -1;
		}
	}

	public RVOUnity SetImpatience(float _imp) {
		m_impatience = _imp;
		return this;
	}
	public RVOUnity SetRadius(float _rad) {
		m_radius = _rad;
		if (m_id != -1)
			m_instance.m_instance.setAgentRadius(m_id, m_radius);
		return this;
	}
	public RVOUnity SetStatic(bool _static) {
		m_static = _static;
		return this;
	}
	public RVOUnity SetInstance(string _instanceName) {
		m_instanceName = _instanceName;
		return this;
	}

	#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (m_drawGizmos)
		{
			if (m_id == -1 || m_instance == null)
				return;

			if (m_id == 0)
			{
				Gizmos.color = Color.black;
				foreach (var o in m_instance.m_obstacles)
				{
					Gizmos.DrawSphere (o.m_position, 0.15f);
					for (int i = 0; i < 4; i++)
					{
						Gizmos.DrawSphere (o.Corner (i), 0.05f);
						Gizmos.DrawLine (o.Corner (i), o.Corner ((i + 1) & 3));
					}
				}
			}

			var pos = m_instance.m_instance.getAgentPosition (m_id).XZ ();
			pos.y = transform.position.y;
			var rad = m_instance.m_instance.getAgentRadius (m_id);
			var imp = m_impatience;
			var clr = s_gizmoColour;
			clr.g *= Mathf.Max (0, 1 - imp * 10);
			clr.b *= 1 - imp;
			Gizmos.color = clr;
			Gizmos.DrawSphere (pos, rad);
			Gizmos.color = Color.red;
			Gizmos.DrawLine (pos, pos + m_instance.m_instance.getAgentVelocity (m_id).XZ ());
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (pos, pos + m_instance.m_instance.getAgentPrefVelocity (m_id).XZ ());
			Gizmos.color = m_static ? Color.red : (m_lastMoveSuccessful ? Color.green : Color.blue);
			Gizmos.DrawSphere (pos, rad * 0.1f);
		}
	}
	#endif



	//Nick's addition
	static float GetDistanceBetweenPoints(Vector3 p1, Vector3 p2){
		Vector2 diff = p2 - p1;
		diff.y = 0;
		return (p2 - p1).magnitude;
	}

	Vector3 GetDirectionFromOnePointToAnother(Vector3 from, Vector3 to)
	{
		Vector3 result = (to - from).normalized;
		result.y = 0;	
		return result;
	}
				
}


			