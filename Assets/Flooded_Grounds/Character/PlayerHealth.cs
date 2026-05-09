using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100;
    private int _currentHP;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;        // przeciągnij Slider z Canvas
    [SerializeField] private Image fillImage;         // obrazek wypełnienia paska

    [Header("Kolory paska")]
    [SerializeField] private Color colorFull = Color.green;
    [SerializeField] private Color colorMid = Color.yellow;
    [SerializeField] private Color colorLow = Color.red;

    private Animator _animator;
    private bool _isDead = false;

    // hash animacji — dodaj "IsDead" Bool w Animatorze psa jeśli chcesz
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        _currentHP = maxHP;
        UpdateSlider();
    }

    // ── wywoływane przez EnemyAI gdy atakuje gracza ──
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHP = Mathf.Max(0, _currentHP - damage);
        UpdateSlider();

        Debug.Log($"Gracz otrzymał {damage} obrażeń! HP: {_currentHP}/{maxHP}");

        if (_currentHP <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (_isDead) return;

        _currentHP = Mathf.Min(maxHP, _currentHP + amount);
        UpdateSlider();
    }

    void UpdateSlider()
    {
        if (hpSlider == null) return;

        float ratio = (float)_currentHP / maxHP;
        hpSlider.value = ratio;

        // kolor zmienia się zależnie od HP
        if (fillImage != null)
        {
            if (ratio > 0.6f) fillImage.color = colorFull;
            else if (ratio > 0.3f) fillImage.color = colorMid;
            else fillImage.color = colorLow;
        }
    }

    void Die()
    {
        _isDead = true;
        Debug.Log("Gracz zginął!");

        if (_animator != null)
            _animator.SetBool(IsDead, true);

        // wyłącz sterowanie
        GetComponent<Character>().enabled = false;

        // opcjonalnie: pokaż ekran Game Over po 2 sekundach
        // Invoke(nameof(ShowGameOver), 2f);
    }
}