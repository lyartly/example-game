﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;

    private GameObject attackTarget;
    private float lastAttackTime;

    private bool isDead;

    private float stopDistance;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }
    void Start()
    {
        //MousseManager.Instance.OnMouseClicked += MoveToTarget;
        //MousseManager.Instance.OnEnemyClicked += EventAttack;

        
        SaveManager.Instance.LoadPlayerData();
    }
    void OnEnable()
    {
        MousseManager.Instance.OnMouseClicked += MoveToTarget;
        MousseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    void OnDisable()
    {
        if (!MousseManager.Isintialized) return; 
        MousseManager.Instance.OnMouseClicked -= MoveToTarget;
        MousseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    void Update()
    {
        isDead = characterStats.CurrentHealth == 0;
        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
        }

        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;

        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;
        //Attack
        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //reset attackTime
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStates = attackTarget.GetComponent<CharacterStats>();

            targetStates.TakeDamage(characterStats, targetStates);
        }
    }
}
