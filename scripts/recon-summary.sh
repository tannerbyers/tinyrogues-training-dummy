#!/bin/bash
set -e

echo "===== TEMPORARY EVENTS ====="
grep -n -C 2 -E \
'DamageTaken|BeforeDamageCalculation|AfterDamageCalculation|ActorTookTrueDamage|PlayerAttacked' \
reverse/types/TemporaryEvents.cs || true

echo
echo "===== DAMAGE ARGS: PUBLIC API ====="
grep -nE '^[[:space:]]*public (unsafe )?(static )?' \
reverse/types/DamageArgs.cs | head -120 || true

echo
echo "===== ENEMY DAMAGE ARGS: PUBLIC API ====="
grep -nE '^[[:space:]]*public (unsafe )?(static )?' \
reverse/types/EnemyDamageArgs.cs | head -100 || true

echo
echo "===== ACTOR: DAMAGE METHODS ====="
grep -nE \
'^[[:space:]]*public unsafe .*(InflictDamage|QueueDamage|InflictTrueDamage|FixedDamage|SetInvincible|ApplyStatusEffect|GetDamageTypesToDeal|Health|Hearts)[[:space:]]*[(]?' \
reverse/types/Actor.cs || true

echo
echo "===== ACTOR: IMPORTANT PROPERTIES ====="
grep -nE \
'^[[:space:]]*public unsafe .*(Health|Hearts|Invinc|ActorType|Creature|IsDead|Dead|Alive|Status)' \
reverse/types/Actor.cs | head -100 || true

echo
echo "===== ENEMY: PUBLIC API ====="
grep -nE '^[[:space:]]*public (unsafe )?(static )?' \
reverse/types/Enemy.cs | head -120 || true

echo
echo "===== TARGET INTERFACE ====="
grep -nE \
'^[[:space:]]*(public|[A-Za-z_].*[({;])' \
reverse/types/ITargetActor.cs | head -80 || true

echo
echo "===== DAMAGE TRIGGER ====="
grep -nE \
'^[[:space:]]*public (unsafe )?(static )?' \
reverse/types/ActorTakesDamageTrigger.cs | head -100 || true

echo
echo "===== PLAYER ATTACK ====="
grep -nE \
'^[[:space:]]*public unsafe .*(Instance|CurrentWeapon|EquippedWeapon|IsAttacking|Attack|OnPlayerAttacked|Weapon)' \
reverse/types/PlayerAttack.cs || true

echo
echo "===== CLASS SELECTION ====="
grep -nE \
'^[[:space:]]*public (unsafe )?(static )?.*(Instance|Player|Open|Close|Select|Enable|Disable|Start|State)' \
reverse/types/PlayerClassSelectionController.cs | head -100 || true

echo
echo "===== BONFIRE ====="
grep -nE \
'^[[:space:]]*public (unsafe )?(static )?.*(Instance|Player|Enable|Disable|Interact|Click|Open|Close)' \
reverse/types/BonfireButton.cs | head -100 || true
