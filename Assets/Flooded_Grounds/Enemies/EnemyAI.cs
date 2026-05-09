using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("HP")]
    [SerializeField] private int maxHP = 100;
    private int _currentHP;

    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _player;
    private float _lastAttackTime;
    private bool _isDead = false;
    private bool _isHurt = false;

    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsDamaged = Animator.StringToHash("IsDamaged");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private enum State { Idle, Chasing, Attacking }
    private State _currentState = State.Idle;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
    }

    void Start()
    {
        _currentHP = maxHP;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogError("Brak obiektu z tagiem 'Player'!");
    }

    void Update()
    {
        if (_player == null || _isDead || _isHurt) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackRange) _currentState = State.Attacking;
        else if (dist <= detectionRange) _currentState = State.Chasing;
        else _currentState = State.Idle;

        switch (_currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Chasing: HandleChasing(); break;
            case State.Attacking: HandleAttacking(); break;
        }
    }

    void HandleIdle()
    {
        _agent.isStopped = true;
        _animator.SetBool(IsRunning, false);
    }

    void HandleChasing()
    {
        _agent.isStopped = false;
        _agent.SetDestination(_player.position);
        _animator.SetBool(IsRunning, _agent.velocity.magnitude > 0.1f);
    }

    void HandleAttacking()
    {
        _agent.isStopped = true;

        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        _animator.SetBool(IsRunning, false);

        if (Time.time - _lastAttackTime >= attackCooldown)
        {
            _lastAttackTime = Time.time;
            _animator.SetTrigger(IsAttacking);
        }
    }

    // ── WYWOŁYWANE PRZEZ DogPlayer.PerformHit() ───────
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHP -= damage;
        Debug.Log($"{name} HP: {_currentHP}/{maxHP}");

        if (_currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtCoroutine());
        }
    }

    System.Collections.IEnumerator HurtCoroutine()
    {
        _isHurt = true;
        _agent.isStopped = true;
        _animator.SetTrigger(IsDamaged);

        // poczekaj na koniec animacji Damage (dopasuj do długości animacji)
        yield return new WaitForSeconds(0.6f);

        _isHurt = false;
    }

    void Die()
    {
        _isDead = true;
        _agent.isStopped = true;
        _animator.SetBool(IsDead, true);

        // wyłącz collider żeby pies nie mógł trafić znowu
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // zniszcz obiekt po 3 sekundach
        Destroy(gameObject, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}