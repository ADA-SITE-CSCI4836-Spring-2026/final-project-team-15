using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Player HP with UnityEvents so UI / VFX / audio can react without coupling.
/// Drop on the Player GameObject. Wire OnDamaged / OnHealed / OnDied in the
/// inspector or call from other scripts via TakeDamage() / Heal().
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Seconds of invulnerability after taking damage. 0 disables.")]
    [SerializeField] private float iFrameDuration = 0.5f;
    [Tooltip("HP lost per second over time, independent of damage events. 0 disables.")]
    [SerializeField] private float passiveDrainRate = 5f;

    [Header("Campfire Healing")]
    [Tooltip("When enabled, standing near a campfire restores HP instead of draining it.")]
    [SerializeField] private bool healNearCampfires = true;
    [Tooltip("HP restored per second near a campfire. If 0, uses the passive drain rate.")]
    [SerializeField] private float campfireHealRate = 0f;
    [Tooltip("Distance from a campfire where healing starts.")]
    [SerializeField] private float campfireHealRadius = 8f;
    [Tooltip("Optional explicit campfire transforms. If empty, objects named 'Campfire' are found automatically.")]
    [SerializeField] private Transform[] campfires;

    [Header("Game Over")]
    [Tooltip("Show a loss screen automatically when HP reaches 0.")]
    [SerializeField] private bool showLossScreenOnDeath = true;
    [Tooltip("Show the same loss screen when the round timer reaches 0.")]
    [SerializeField] private bool loseWhenTimerEnds = true;
    [Tooltip("Optional explicit round timer. If empty, the first CountdownTimer in the scene is used.")]
    [SerializeField] private CountdownTimer roundTimer;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;   // emits current HP whenever it changes
    public UnityEvent<float> OnDamaged;         // emits damage amount
    public UnityEvent<float> OnHealed;          // emits heal amount
    public UnityEvent OnDied;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public float NormalizedHealth =>
        maxHealth > 0f ? Mathf.Clamp01(CurrentHealth / maxHealth) : 0f;
    public bool IsAlive => CurrentHealth > 0f;

    private float _iFrameTimer;
    private bool _hasDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CacheCampfiresIfNeeded();
    }

    private void Start()
    {
        // Push the initial value to any listeners (HUD) that subscribed in OnEnable.
        OnHealthChanged?.Invoke(CurrentHealth);

        if (loseWhenTimerEnds)
        {
            if (roundTimer == null) roundTimer = FindObjectOfType<CountdownTimer>();
            if (roundTimer != null) roundTimer.OnTimerCompleted.AddListener(Die);
        }
    }

    private void OnDestroy()
    {
        if (roundTimer != null) roundTimer.OnTimerCompleted.RemoveListener(Die);
    }

    private void Update()
    {
        if (_iFrameTimer > 0f) _iFrameTimer -= Time.deltaTime;

        if (!IsAlive) return;

        if (healNearCampfires && IsNearCampfire())
        {
            float healRate = campfireHealRate > 0f ? campfireHealRate : passiveDrainRate;
            Heal(healRate * Time.deltaTime);
            return;
        }

        if (passiveDrainRate > 0f)
        {
            float before = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - passiveDrainRate * Time.deltaTime);
            if (CurrentHealth != before)
            {
                OnHealthChanged?.Invoke(CurrentHealth);
                if (CurrentHealth <= 0f) Die();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f || _iFrameTimer > 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        _iFrameTimer = iFrameDuration;

        OnDamaged?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f) return;

        float before = CurrentHealth;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        float gained = CurrentHealth - before;
        if (gained <= 0f) return;

        OnHealed?.Invoke(gained);
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void SetMaxHealth(float newMax, bool refill = true)
    {
        maxHealth = Mathf.Max(1f, newMax);
        if (refill) CurrentHealth = maxHealth;
        else CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    private void CacheCampfiresIfNeeded()
    {
        if (campfires != null && campfires.Length > 0) return;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        System.Collections.Generic.List<Transform> foundCampfires = new System.Collections.Generic.List<Transform>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Campfire" || obj.name.StartsWith("Campfire ("))
                foundCampfires.Add(obj.transform);
        }

        campfires = foundCampfires.ToArray();
    }

    private bool IsNearCampfire()
    {
        if (campfires == null || campfires.Length == 0 || campfireHealRadius <= 0f) return false;

        float radiusSquared = campfireHealRadius * campfireHealRadius;
        Vector3 position = transform.position;
        foreach (Transform campfire in campfires)
        {
            if (campfire == null) continue;

            if ((campfire.position - position).sqrMagnitude <= radiusSquared)
                return true;
        }

        return false;
    }

    private void Die()
    {
        if (_hasDied) return;

        _hasDied = true;
        OnDied?.Invoke();

        if (showLossScreenOnDeath)
            LossScreenOverlay.Show();
    }

    private void OnDrawGizmosSelected()
    {
        if (campfires == null || campfires.Length == 0 || campfireHealRadius <= 0f) return;

        Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.35f);
        foreach (Transform campfire in campfires)
        {
            if (campfire != null) Gizmos.DrawWireSphere(campfire.position, campfireHealRadius);
        }
    }
}
