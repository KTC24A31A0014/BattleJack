using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] private int maxHp;

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
    public void Heal(int amount)
    {
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} の死亡");
        // ここにGameOver処理
    }

    public bool IsDead() => CurrentHp <= 0;
}
