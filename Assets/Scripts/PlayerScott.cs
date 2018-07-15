﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerScott : MonoBehaviour
{
    [Title ("Player Movement")]
    [SerializeField] float _moveSpeed = 1.0f;
    [SerializeField] float _decelerationRate = 1.0f;
    [SerializeField] float _deadZone = 0.01f;
    [SerializeField] Rigidbody _rb;
    [SerializeField] Transform _cameraOffset;

    [Title ("Player Rotation")]
    [SerializeField] float _rotationDeadZone = 0.1f;
    [SerializeField] float _rotSpeed = 1.0f;
    [SerializeField] float _rotationOffset = -90.0f;

    [Title ("Player Atttacking")]
    [SerializeField] Transform _firePoint;
    [SerializeField] int _playerDamage;
    [SerializeField] float _attackDuration = 0.5f;
    float _timeUntilLastAttack;
    [SerializeField] float _attackRate = 0.08f;
    [SerializeField] float _attackSphereSize = 1.5f;
    [SerializeField] ParticleSystem _hitEffect;
    float _timeUntilNextAttackRate;
    bool _isAttacking;

    [SerializeField] float _particleEffectRate = 0.3f;
    float _timeUntilNextEffect;

    Collider[] _colliders;
    [SerializeField] LayerMask _enemyLM;

    Vector3 _movement;
    Vector3 _shootDir;

    private Vector3 _mousePos;
    private Vector3 _direction;

    public ParticleSystem _particleSystem;
    public AudioSource _slashSound;

    [Title ("Animations")]
    [SerializeField] Animator _animator;

    void FixedUpdate ()
    {
        PlayerMovement ();
    }

    private void Update ()
    {
        PlayerAttack ();
    }

    private void PlayerMovement ()
    {
        _movement = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
        if (_movement.magnitude > _deadZone)
        {
            _animator.SetBool ("isRunning", true);
            Vector3 newMovement = _cameraOffset.forward * _movement.normalized.z + _cameraOffset.right * _movement.normalized.x;
            newMovement = Vector3.ClampMagnitude (newMovement, 1 / Mathf.Sqrt (2)) * Mathf.Sqrt (2);

            _rb.velocity = newMovement * _moveSpeed * Time.fixedDeltaTime;
        }
        else
        {
            _rb.velocity *= _decelerationRate * Time.fixedDeltaTime;
            _animator.SetBool ("isRunning", false);
        }
        if (!_isAttacking && _rb.velocity.magnitude > _rotationDeadZone)
        {
            transform.rotation = Quaternion.LookRotation (_rb.velocity, Vector3.up);
        }

    }

    private void PlayerRotation ()
    {
        Vector3 positionOnScreen = Camera.main.WorldToViewportPoint (transform.position);
        Vector3 mouseOnScreen = (Vector3) Camera.main.ScreenToViewportPoint (Input.mousePosition);

        float angle = AngleBetweenTwoPoints (positionOnScreen, mouseOnScreen);

        Quaternion _newQuat = Quaternion.Euler (new Vector3 (0f, -angle - _rotationOffset, 0f));
        transform.rotation = Quaternion.Lerp (transform.rotation, _newQuat, Time.time * _rotSpeed);
    }

    private void PlayerAttack ()
    {
        if (_isAttacking && Time.time > _timeUntilLastAttack && _attackDuration != 0)
        {
            _isAttacking = false;
            _timeUntilLastAttack = Time.time + 1 / _attackDuration;
        }

        if (Time.time > _timeUntilNextAttackRate && _attackRate != 0)
        {
            if (Input.GetMouseButtonDown (0))
            {
                Health h;
                _isAttacking = true;
                // AttackAbility ();
                _particleSystem.Emit (1);
                _slashSound.Play ();
                _animator.SetTrigger ("isAttacking");
                _timeUntilNextAttackRate = Time.time + 1 / _attackRate;
                _colliders = Physics.OverlapSphere (_firePoint.position, _attackSphereSize, _enemyLM);

                foreach (var item in _colliders)
                {
                    h = item.GetComponent<Health> ();
                    if (h)
                    {
                        Debug.Log("Damagin");
                        h.Damage (_playerDamage);
                        _hitEffect.Play ();
                    }
                }

                //Debug.Log ("not working");
            }
        }
    }

    void AttackAbility ()
    {
        StartCoroutine (RotateForAttackDuration ());
    }

    IEnumerator RotateForAttackDuration ()
    {
        while (_isAttacking)
        {
            // PlayerRotation ();
            yield return null;
        }
    }

    float AngleBetweenTwoPoints (Vector3 a, Vector3 b)
    {
        return Mathf.Atan2 (a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}