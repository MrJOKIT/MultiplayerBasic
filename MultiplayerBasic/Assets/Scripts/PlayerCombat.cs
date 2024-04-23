using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class BulletType
{
    public string bulletName;
    public Transform serverAttack;
    public Transform clientAttack;
}
public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private BulletType[] bulletTypes;
    [SerializeField] private float delayAttack;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform sfxSpawn;
    [SerializeField] private float attackRadius;
    [SerializeField] private Collider2D playerCollider;
    private float timer;
    private bool onAttack;
    private bool inDelayAttack;
    public NetworkVariable<int> bulletTypeNumber = new NetworkVariable<int>(0);
    private float delayAttackTimeCounter;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.PrimaryFireEvent += Attack;
        inputReader.ChangeBulletEvent += ChangeBullet;
        bulletTypeNumber.OnValueChanged += HandleChangeBulletType;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.PrimaryFireEvent -= Attack;
        inputReader.ChangeBulletEvent -= ChangeBullet;
        bulletTypeNumber.OnValueChanged -= HandleChangeBulletType;
    }

    private void Attack(bool onAttack)
    {
        this.onAttack = onAttack;
    }
    

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        if (timer>0)
        {
            timer -= Time.deltaTime;
        }
        
        if (!onAttack)
        {
            return;
        }
        if(timer > 0){ return; }
        PrimaryAttackServerRpc();
        ClientAttack();

        timer = delayAttack;
    }

    private void ChangeBullet(bool onChange)
    {
        ChangeTypeServerRpc();
    }

    private void IncreaseType(int count)
    {
        var newCount = bulletTypeNumber.Value + count;

        bulletTypeNumber.Value = newCount;
    }
    private void HandleChangeBulletType(int oldType, int newType)
    {
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sfxSpawn.position,attackRadius);
    }
    
    [ServerRpc]
    private void PrimaryAttackServerRpc()
    {
        Transform damageItem = Instantiate(bulletTypes[bulletTypeNumber.Value].serverAttack, sfxSpawn.position, sfxSpawn.rotation);
        Physics2D.IgnoreCollision(playerCollider,damageItem.GetComponent<Collider2D>());
        if(damageItem.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }
        SpawnClientAttackClientRpc();
    }

    [ServerRpc]
    private void ChangeTypeServerRpc()
    {
        bulletTypeNumber.Value += 1;
        if (bulletTypeNumber.Value > bulletTypes.Length - 1 )
        {
            bulletTypeNumber.Value = 0;
        }
    }
    [ClientRpc]
    private void SpawnClientAttackClientRpc()
    {
        if (IsOwner)
        {
            return;
        }
        ClientAttack();
    }
    private void ClientAttack()
    {
        Transform damageItem = Instantiate(bulletTypes[bulletTypeNumber.Value].clientAttack, sfxSpawn.position, sfxSpawn.rotation);
        Physics2D.IgnoreCollision(playerCollider,damageItem.GetComponent<Collider2D>());
    }
}
