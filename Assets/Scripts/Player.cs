using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Player : MonoBehaviour
{
    private Collider _body;
    private float _gravity = 0;
    private const float _gravityForce = 20; 
    private const float _minimalGravityUnit = 0.01f;
    private const float _jumpForce = 10;
    private const float _rotationSpeed = 175;
    private const float _speed = 5;
    [SerializeField] private SpriteRenderer _renderer;
    private Vector3 _step {
        get {
            return (_gravity * Vector3.down + _speed * Vector3.right) * Time.deltaTime;
        }
    }
    [SerializeField] private GameObject _explosionPrefab;

    public enum PlayerState
    {
        ACTIVE,
        KILLED,  
    }
    private PlayerState _state = PlayerState.ACTIVE;
    public PlayerState state {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            _renderer.enabled = value == PlayerState.ACTIVE;
        }
    }

    private bool _onGround = false;

    public void Start()
    {
        _body = GetComponent<Collider>();
    }

    private List<Collider> GetCollidedObjects()
    {
        var nearby = new List<Collider>();
        nearby.AddRange(Physics.OverlapSphere(transform.position, 1.5f));
        nearby.RemoveAll((Collider collider) => {
            if (collider == _body) {
                return true;
            }
            Vector3 _d;
            float _di;
            return !Physics.ComputePenetration(_body, transform.position, transform.rotation, collider, collider.transform.position, collider.transform.rotation, out _d, out _di);
        });
        return nearby;
    }

    private void Slide()
    {
        transform.position += _step.y * Vector3.up;
        PlaceOnPlatform();
        transform.position += _step.x * Vector3.right;
        CheckPlayerСollidedWithWall();
    }

    private void PlaceOnPlatform()
    {
        var platform = GetCollidedObjects().Find(MakePredicateWhichCheckingVectorsDot((dot) => {return dot == 0;}));
        if (platform != null)
        {
            transform.position -= _step.y * Vector3.up;
            _onGround = true;
            _gravity = _minimalGravityUnit;
        }
        else
        {
            _onGround = false;
        }
    }

    private void ApplyGravity()
    {
        _gravity += _gravityForce * Time.deltaTime;
    }

    private void ControlByInput()
    {
        if (Input.GetButton("Jump") && _onGround)
        {
            _gravity = -_jumpForce;
        }
    }

    private float _rendererRotation {
        get
        {
            return _renderer.transform.localRotation.eulerAngles.z;
        }
        set
        {
            _renderer.transform.localRotation = Quaternion.Euler(0, 0, value);
        }
    }

    private void RotateSprite()
    {
        if (_onGround)
        {
            _rendererRotation = Mathf.Round(_rendererRotation / 90f) * 90f;
        }
        else
        {
            _rendererRotation -= _rotationSpeed * Time.deltaTime;
        }
    }

    private List<Collider> GetPlatforms()
    {
        return GetCollidedObjects().FindAll((Collider collider) => {
            return collider.GetComponent<AbstractPlatform>() != null;
        });
    }

    private void CheckPlayerСollidedWithWall()
    {
        var platforms = GetPlatforms();
        var wall = platforms.Find(MakePredicateWhichCheckingVectorsDot((dot) => {return dot != 0;}));
        if (wall != null) {
            Kill();
        }
    }

    private void Kill()
    {
        state = PlayerState.KILLED;
        SpawnExplosion();
        StartCoroutine(Respawn());
    }

    private void SpawnExplosion()
    {
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
    }

    private Predicate<Collider> MakePredicateWhichCheckingVectorsDot(Predicate<float> condition)
    {
        return (Collider collider) => {
            var point = Physics.ClosestPoint(transform.position, collider, collider.transform.position, collider.transform.rotation);
            var dot = Vector3.Dot(transform.right, (point - transform.position).normalized);
            return condition.Invoke(dot);
        };
    }

    public void Update()
    {
        if (state == PlayerState.ACTIVE)
        {
            GetCollidedObjects();
            ApplyGravity();
            ControlByInput();
            Slide();
            RotateSprite();
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3);
        transform.position = new Vector3(0, -1.5f, 0);
        state = PlayerState.ACTIVE;
    }
}
