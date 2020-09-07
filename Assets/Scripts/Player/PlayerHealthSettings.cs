using UnityEngine;

[CreateAssetMenu(menuName = "Player Health Settings", fileName = "New PlayerHealthSettings")]
public class PlayerHealthSettings : ScriptableObject {

    // TODO could this be general health settings? i'll see when i get around to other entities...

    [Header("General Health")]
    [SerializeField] float defaultHealth = 100f;
    [SerializeField] float maxHealth = 200f;

    public float DefaultHealth => defaultHealth;
    public float MaxHealth => maxHealth;

    [Header("Fall Damage")]
    [SerializeField] float minFallDamageHeight = 10f;
    [SerializeField] float oneHundredFallDamageHeight = 30f;
    [SerializeField] float fallDamageReferenceGravity = 29.43f;

    public float MinFallDamageHeight => minFallDamageHeight;
    public float OneHundredFallDamageHeight => oneHundredFallDamageHeight;
    public float FallDamageReferenceGravity => fallDamageReferenceGravity;

    [Header("Drowning")]
    [SerializeField] float breathTime = 30f;
    [SerializeField] float breathRecoveryTime = 5f;
    [SerializeField] float drowningDamagePerSecond = 10f;
    [SerializeField] float drowningDamageInterval = 1f;

    public float BreathTime => breathTime;
    public float BreathRecoveryTime => breathRecoveryTime;
    public float DrowningDamagePerSecond => drowningDamagePerSecond;
    public float DrowningDamageInterval => drowningDamageInterval;
	
}
