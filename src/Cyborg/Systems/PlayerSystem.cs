using System;
using System.Collections.Generic;
using System.Linq;
using Cyborg.Components;
using Cyborg.Core;
using Cyborg.Entities;
using Cyborg.Utilities;
using Microsoft.Xna.Framework;

namespace Cyborg.Systems
{
    public class PlayerSystem : IUpdateSystem
    {
        private const float _walkingForce = 600f;
        private const float _attackDuration = 0.25f;
        private const float _attackKnockbackVelocity = 400f;
        private const float _dashDuration = 0.75f;
        private const float _dashInstantaneousVelocity = 400f;
        private readonly IEntityManager _entityManager;
        private readonly IGameState _gameState;

        public PlayerSystem(IEntityManager entityManager, IGameState gameState)
        {
            _entityManager = entityManager;
            _gameState = gameState;
        }

        public void Update(GameTime gameTime)
        {
            if (!_gameState.Active)
                return;

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entity in _entityManager.Entities<Player>())
            {
                CalculateState(entity, elapsed);
                ApplyAnimation(entity);

                if (entity.State.Attacking)
                {
                    var attackBounds = new Sector(entity.Body.Position.ToRoundedPoint(), entity.State.AttackRadius, entity.State.AttackAngles.Minimum, entity.State.AttackAngles.Maximum);

                    foreach (var enemyEntity in _entityManager.Entities<Enemy>())
                        ApplyAttack(entity, attackBounds, enemyEntity);
                }
            }
        }

        private static void CalculateState(Player entity, float elapsed)
        {
            // Finish dash
            if (entity.State.Dashing)
            {
                entity.State.DashElapsed += elapsed;
                if (entity.State.DashElapsed >= _dashDuration)
                    entity.State.Dashing = false;
            }

            // Finish attack
            if (entity.State.Attacking)
            {
                entity.State.AttackElapsed += elapsed;
                if (entity.State.AttackElapsed >= _attackDuration)
                    entity.State.Attacking = false;
            }

            // Enter attack
            if (!entity.State.Attacking && entity.Controller.Pressed.Contains(Button.Attack))
            {
                entity.State.Attacking = true;
                entity.State.AttackElapsed = 0f;
                entity.State.AttackCounter++;

                if (entity.Controller.Joystick != Vector2.Zero)
                    entity.State.Direction = entity.Controller.Joystick;

                var cardinalDirection = entity.State.Direction.ToCardinal();
                double baseAttackAngle = 0;
                if (cardinalDirection.X == 1)
                    baseAttackAngle = 0;
                else if (cardinalDirection.X == -1)
                    baseAttackAngle = Math.PI;
                else if (cardinalDirection.Y == 1)
                    baseAttackAngle = 0.5 * Math.PI;
                else if (cardinalDirection.Y == -1)
                    baseAttackAngle = 1.5 * Math.PI;

                var adjustedAttackAngle = baseAttackAngle + 0.125 * Math.PI * ((entity.State.AttackCounter % 2 == 0) ? 1 : -1);
                var attackSector = (adjustedAttackAngle - 0.3125 * Math.PI, adjustedAttackAngle + 0.3125 * Math.PI);
                entity.State.AttackAngles = attackSector;
            }

            // Enter dash
            if (!entity.State.Attacking && !entity.State.Dashing && entity.Controller.Pressed.Contains(Button.Dash))
            {
                entity.State.Dashing = true;
                entity.State.DashElapsed = 0f;

                if (entity.Controller.Joystick != Vector2.Zero)
                    entity.State.Direction = entity.Controller.Joystick;

                entity.Kinetic.Velocity = entity.State.Direction * _dashInstantaneousVelocity;
            }

            // Walking            
            if (!entity.State.Attacking && !entity.State.Dashing && entity.Controller.Joystick != Vector2.Zero)
            {
                entity.State.Walking = true;
                entity.State.Direction = entity.Controller.Joystick;
                entity.Kinetic.Force = entity.State.Direction * _walkingForce;
            }
            else
            {
                entity.State.Walking = false;
                entity.Kinetic.Force = Vector2.Zero;
            }
        }

        private static void ApplyAnimation(Player entity)
        {
            var cardinalDirection = entity.State.Direction.ToCardinal();
            if (entity.State.Attacking)
            {
                if (cardinalDirection.X == 1)
                    entity.Sprite.Animation = (entity.State.AttackCounter % 2 == 0) ? Player.AnimationAttackRight : Player.AnimationAttack2Right;
                else if (cardinalDirection.X == -1)
                    entity.Sprite.Animation = (entity.State.AttackCounter % 2 == 0) ? Player.AnimationAttackLeft : Player.AnimationAttack2Left;
                else if (cardinalDirection.Y == 1)
                    entity.Sprite.Animation = (entity.State.AttackCounter % 2 == 0) ? Player.AnimationAttackDown : Player.AnimationAttack2Down;
                else if (cardinalDirection.Y == -1)
                    entity.Sprite.Animation = (entity.State.AttackCounter % 2 == 0) ? Player.AnimationAttackUp : Player.AnimationAttack2Up;
            }
            else if (entity.State.Walking)
            {
                if (cardinalDirection.X == 1)
                    entity.Sprite.Animation = Player.AnimationWalkRight;
                else if (cardinalDirection.X == -1)
                    entity.Sprite.Animation = Player.AnimationWalkLeft;
                else if (cardinalDirection.Y == 1)
                    entity.Sprite.Animation = Player.AnimationWalkDown;
                else if (cardinalDirection.Y == -1)
                    entity.Sprite.Animation = Player.AnimationWalkUp;
            }
            else
            {
                if (cardinalDirection.X == 1)
                    entity.Sprite.Animation = Player.AnimationStandRight;
                else if (cardinalDirection.X == -1)
                    entity.Sprite.Animation = Player.AnimationStandLeft;
                else if (cardinalDirection.Y == 1)
                    entity.Sprite.Animation = Player.AnimationStandDown;
                else if (cardinalDirection.Y == -1)
                    entity.Sprite.Animation = Player.AnimationStandUp;
            }
        }

        private static void ApplyAttack(Player playerEntity, Sector attackBounds, Enemy enemyEntity)
        {
            var enemyBounds = enemyEntity.Body.Bounds;
            if (!attackBounds.Intersects(enemyBounds))
                return;

            if (!enemyEntity.Damage.TryApply(1))
                return;

            var knockbackVector = Vector2.Normalize(enemyEntity.Body.Position - playerEntity.Body.Position);
            enemyEntity.Kinetic.Velocity = knockbackVector * _attackKnockbackVelocity;
        }
    }
}