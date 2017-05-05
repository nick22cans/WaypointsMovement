using UnityEngine;
using System.Collections;

public class WNS_AnimationControllerScript : MonoBehaviour {

	private Animator m_animator;
	private MovementScript m_WNScript;
	public float m_speedCoeff = 1f;
	private float m_newSpeed;
	private float m_previousFrameSpeed;
	private float delta;

	// Use this for initialization
	void Start () {
		m_animator = GetComponent<Animator> ();
		if (!m_animator)
			print ("Animator is not linked");
		m_WNScript = GetComponent<MovementScript> ();
		if (m_WNScript)
			delta = m_WNScript.m_speed / 20f;
	}


	public void HandleSpeedChange(float displacement)
	{
		if (m_animator)
		{
			m_newSpeed = displacement * m_speedCoeff;
			m_previousFrameSpeed = m_animator.GetFloat ("Speed");

			m_newSpeed = Mathf.Clamp (m_newSpeed, m_previousFrameSpeed - delta, m_previousFrameSpeed + delta);
			m_animator.SetFloat ("Speed", m_newSpeed);
		}
	}

	public void Stop()
	{
		if (m_animator)
		{
			m_animator.SetFloat ("Speed", 0f);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
