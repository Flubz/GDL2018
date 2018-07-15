﻿using System.Collections;
using DG.Tweening;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (Seeker))]
public class EnemyAI : MonoBehaviour
{
	[HideInInspector] public bool _pathIsEnded = false;

	[Title ("AI")]
	[SerializeField] float _updateRate = 2f;
	[SerializeField] float _speed = 4f;
	[SerializeField] float _nextWaypointDist = 3f;

	[Title ("Chase AI")]
	[SerializeField] float _distToStartChasePlayer = 10.0f;
	[SerializeField] float _distToStopChasePlayer = 15.0f;
	[SerializeField] float _distToStartDamaging = 3.0f;
	[SerializeField] float _searchForPlayerSeconds = 1.5f;
	[SerializeField] int _damageAmount = 1;
	[SerializeField] Transform _armature;

	[Title ("Effects")]
	[SerializeField] AudioSource _audioSource;
	[SerializeField] Animator _animator;

	Path _path;
	Seeker _seeker;
	Rigidbody _rb;
	Transform _target;

	int _currentWaypoint = 0;
	float _distanceToTarget;

	void Awake ()
	{
		//_audioSource = GetComponent<AudioSource> ();
		_seeker = GetComponent<Seeker> ();
		_rb = GetComponent<Rigidbody> ();
		_target = null;
		StartCoroutine (SearchForPlayer ());
	}

	public void StartChasingPlayer (Transform target_)
	{
		//_audioSource.Play ();
		_target = target_;
		if (_target == null) return;
		_seeker.StartPath (transform.position, _target.position, OnPathComplete);
		StartCoroutine (UpdatePath ());
	}

	public void StopChasingPlayer ()
	{
		StartCoroutine (SearchForPlayer ());
		_target = null;
		_rb.velocity = Vector3.zero;
	}

	private void LateUpdate ()
	{
		_armature.position = transform.position;
		_armature.rotation = transform.rotation;
	}

	IEnumerator UpdatePath ()
	{
		if (_target != null)
		{
			_animator.SetBool ("_isMoving", true);
			_seeker.StartPath (transform.position, _target.position, OnPathComplete);
			_distanceToTarget = Vector3.Distance (transform.position, _target.position);

			if (_distanceToTarget < _distToStartDamaging)
			{
				_animator.SetBool ("_justAttacked", true);
				_target.GetComponent<Health> ().Damage (_damageAmount);
			}
			else _animator.SetBool ("_justAttacked", false);

			if (_distanceToTarget >= _distToStopChasePlayer) StopChasingPlayer ();

			yield return new WaitForSeconds (_updateRate);
			StartCoroutine (UpdatePath ());
		}
		else
		{
			_animator.SetBool ("_isMoving", false);
		}
	}

	public void OnPathComplete (Path p)
	{
		if (p.error) gameObject.SetActive (false);

		if (!p.error)
		{
			_path = p;
			_currentWaypoint = 0;
		}
	}

	void FixedUpdate ()
	{
		if (_target == null) return;
		if (_path == null) return;

		if (_currentWaypoint >= _path.vectorPath.Count)
		{
			_pathIsEnded = true;
			return;
		}

		_pathIsEnded = false;

		Vector3 dir = (_path.vectorPath[_currentWaypoint] - transform.position).normalized;

		_rb.velocity = dir * _speed * Time.fixedDeltaTime;
		transform.DOLookAt (_target.position, 1.0f);

		float dist = Vector3.Distance (transform.position, _path.vectorPath[_currentWaypoint]);
		if (dist < _nextWaypointDist)
		{
			_currentWaypoint++;
			return;
		}
	}

	IEnumerator SearchForPlayer ()
	{
		GameObject sResult = GameObject.FindGameObjectWithTag ("Player");
		if (sResult == null)
		{
			yield return new WaitForSeconds (_searchForPlayerSeconds);
			StartCoroutine (SearchForPlayer ());
		}
		if (Vector3.Distance (transform.position, sResult.transform.position) <= _distToStartChasePlayer)
		{
			StartChasingPlayer (sResult.transform);
		}
		else
		{
			yield return new WaitForSeconds (_searchForPlayerSeconds);
			StartCoroutine (SearchForPlayer ());
		}
	}
}