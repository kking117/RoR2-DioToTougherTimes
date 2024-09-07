﻿using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using FlatItemBuff.Items;

namespace FlatItemBuff.Components
{
	public class FinImpact: MonoBehaviour
	{
		private CharacterBody victimBody;
		private CharacterMotor victimMotor;
		private HealthComponent hpComp;
		public CharacterBody attackerBody;
		public int itemCount = 0;

		private float lastSpeed = 0f;
		private float fallCreditGrace = 1f;
		private bool HasImpact = false;
		private void Awake()
		{
			Reset();
		}
		internal void Reset()
		{
			lastSpeed = 0f;
			fallCreditGrace = 1f;
			HasImpact = false;

			victimBody = GetComponent<CharacterBody>();
			if (victimBody)
			{
				hpComp = victimBody.GetComponent<HealthComponent>();
				victimMotor = victimBody.GetComponent<CharacterMotor>();
				if (victimMotor)
                {
					lastSpeed = victimMotor.velocity.magnitude;
				}
			}
		}
		private void OnDestroy()
		{
			if (victimBody)
			{
				StartCooldown();
			}
		}
		private void FixedUpdate()
		{
			if (!victimBody || !hpComp)
			{
				Destroy(this);
				return;
			}
			if (!hpComp.alive)
            {
				Destroy(this);
				return;
			}
			if (victimBody.HasBuff(KnockbackFin.knockMidBuff))
			{
				fallCreditGrace = 1f;
				if (victimMotor.isGrounded)
				{
					OnImpact();
				}
				else
				{
					lastSpeed = victimMotor.velocity.magnitude;
				}
			}
			else
            {
				fallCreditGrace -= Time.fixedDeltaTime;
				if (fallCreditGrace <= 0f)
                {
					Destroy(this);
				}
			}
		}

		internal void OnImpact()
        {
			if (!HasImpact)
            {
				StartCooldown();
				Detonate();
			}
		}
		private void StartCooldown()
		{
			victimBody.RemoveBuff(KnockbackFin.knockMidBuff);
			if (!victimBody.HasBuff(DLC2Content.Buffs.KnockUpHitEnemies))
			{
				for (int i = KnockbackFin.Cooldown; i > 0; i--)
				{
					victimBody.AddTimedBuff(DLC2Content.Buffs.KnockUpHitEnemies, i);
				}
			}
		}
		private void Detonate()
        {
			HasImpact = true;
			if (KnockbackFin.BaseRadius > 0f && attackerBody)
            {
				float blastDamage = attackerBody.damage * KnockbackFin.GetImpactDamage(itemCount);
				float blastRadius = KnockbackFin.GetImpactRadius(itemCount);
				if (blastDamage > 0f)
				{
					bool isCrit = Util.CheckRoll(attackerBody.crit, attackerBody.master);
					float velDmg = Mathf.Max(0f, lastSpeed);
					velDmg = Mathf.InverseLerp((victimBody.jumpPower + 20f) / 4f, 120f, velDmg);
					blastDamage *= Mathf.Lerp(1f, KnockbackFin.MaxDistDamage, velDmg);
					

					Vector3 blastPosition = victimBody.footPosition;
					BlastAttack blastAttack = new BlastAttack();
					if (KnockbackFin.DoStun)
					{
						SetStateOnHurt comp = victimBody.GetComponent<SetStateOnHurt>();
						if (comp)
						{
							if (comp.canBeStunned)
							{
								comp.SetStun(1f);
							}
						}
					}
					blastAttack.position = blastPosition;
					blastAttack.radius = blastRadius;
					blastAttack.baseDamage = blastDamage;
					blastAttack.baseForce = 0f;
					blastAttack.procCoefficient = KnockbackFin.ProcRate;
					blastAttack.crit = isCrit;
					blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
					blastAttack.attacker = attackerBody.gameObject;
					blastAttack.inflictor = null;
					blastAttack.teamIndex = TeamComponent.GetObjectTeam(attackerBody.gameObject);
					blastAttack.damageType = DamageType.Generic;
					blastAttack.damageColorIndex = DamageColorIndex.Item;
					blastAttack.Fire();
					EffectManager.SpawnEffect(KnockbackFin.ImpactEffect, new EffectData
					{
						origin = blastPosition,
						rotation = Util.QuaternionSafeLookRotation(victimBody.transform.forward),
						scale = blastRadius
					}, true);
					EffectManager.SimpleSoundEffect(EntityStates.Croco.BaseLeap.landingSound.index, victimBody.footPosition, true);
				}
			}
		}
	}
}
