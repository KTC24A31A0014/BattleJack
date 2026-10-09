using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] private int maxHp;
    [SerializeField] private float healMultiplier = 1.0f;

    public int MaxHp => maxHp;
    public int CurrentHp {  get; private set; }


    private void Awake()
    {
        CurrentHp = maxHp;
    }

    // ダメージを受ける
    public int TakeDamage(int amount)
    {
        int before = CurrentHp;

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        if (CurrentHp == 0) Die();

        return before - CurrentHp;
    }

    // 回復(もはや吸血)(最大HPを超えない)
    public int Heal(int baseAmount)
    {
        int before = CurrentHp;

        int healAmount = Mathf.RoundToInt(baseAmount * healMultiplier);
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + healAmount);

        return CurrentHp - before;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} の死亡");
        // ここにGameOver処理
    }

    public bool IsDead() => CurrentHp <= 0;
}
