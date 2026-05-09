using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Ruch")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Atak toporkiem")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private Transform axePoint;        // pusty obiekt przy pysku psa
    [SerializeField] private LayerMask enemyLayer;

    [Header("Parametry Animatora")]
    [SerializeField] private string animSpeed = "Speed";
    [SerializeField] private string animAttack = "IsAttacking";

    private CharacterController _cc;
    private Animator _animator;
    private Camera _cam;
    private float _lastAttackTime;
    private bool _isAttacking;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cam = Camera.main;

        _animator.applyRootMotion = false;
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    // ── RUCH ──────────────────────────────────────────
    void HandleMovement()
    {
        // blokuj ruch podczas ataku
        if (_isAttacking) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // ruch względem kamery
        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            // płynny obrót w kierunku ruchu
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            _cc.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        // grawitacja
        _cc.Move(Vector3.down * 9.8f * Time.deltaTime);

        // animacja
        float speed = new Vector2(h, v).magnitude;
        _animator.SetFloat(animSpeed, speed, 0.1f, Time.deltaTime);
    }

    // ── ATAK ──────────────────────────────────────────
    void HandleAttack()
    {
        // lewy przycisk myszy lub przycisk kontrolera
        bool attackInput = Input.GetMouseButtonDown(0) ||
                           Input.GetButtonDown("Fire1");

        if (!attackInput) return;
        if (Time.time - _lastAttackTime < attackCooldown) return;

        _lastAttackTime = Time.time;
        _animator.SetTrigger(animAttack);
        StartCoroutine(AttackCoroutine());
    }

    System.Collections.IEnumerator AttackCoroutine()
    {
        _isAttacking = true;

        // poczekaj chwilę — moment uderzenia w animacji (dopasuj do swojej animacji)
        yield return new WaitForSeconds(0.25f);

        PerformHit();

        // poczekaj do końca animacji ataku
        yield return new WaitForSeconds(0.35f);

        _isAttacking = false;
    }

    void PerformHit()
    {
        // punkt z którego mierzymy zasięg (axePoint lub sam pies)
        Vector3 origin = axePoint != null ? axePoint.position : transform.position;

        // znajdź wszystkich wrogów w zasięgu toporka
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log($"Pies trafił {hit.name} za {attackDamage} HP!");
            }
        }
    }

    // rysuje zasięg ataku w edytorze
    void OnDrawGizmosSelected()
    {
        Vector3 origin = axePoint != null ? axePoint.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}
