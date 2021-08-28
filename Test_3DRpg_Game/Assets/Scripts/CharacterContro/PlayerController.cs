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


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }
    void Start()
    {
        MousseManager.Instance.OnMouseClicked += MoveToTarget;
        MousseManager.Instance.OnEnemyClicked += EventAttack;
    }

    void Update()
    {
        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject target)
    {
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
        var targetStates = attackTarget.GetComponent<CharacterStats>();

        targetStates.TakeDamage(characterStats, targetStates);
    }
}
