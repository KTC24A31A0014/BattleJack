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
    public void TakeDamage(int amount)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        if (CurrentHp == 0) Die();
    }

    // 回復(もはや吸血)(最大HPを超えない)
    public void Heal(int baseAmount)
    {
        int healAmount = Mathf.RoundToInt(baseAmount * healMultiplier);
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + healAmount);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} の死亡");
        // ここにGameOver処理
    }

    public bool IsDead() => CurrentHp <= 0;
}
