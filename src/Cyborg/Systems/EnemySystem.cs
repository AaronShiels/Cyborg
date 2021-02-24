using System;
using System.Collections.Generic;
using System.Linq;
using Cyborg.Core;
using Cyborg.Entities;
using Cyborg.Utilities;
using Microsoft.Xna.Framework;
using static Cyborg.Utilities.MathsExtensions;

namespace Cyborg.Systems
{
    public class EnemySystem : IUpdateSystem
    {
        private const float _walkingForce = 200f;
        private const float _explosionsPerSecond = 12f;
        private const float _explosionJitter = 10f;
        private const float _deathDuration = 0.5f;

        private readonly IEntityManager _entityManager;
        private readonly IGameState _gameState;

        public EnemySystem(IEntityManager entityManager, IGameState gameState)
        {
            _entityManager = entityManager;
            _gameState = gameState;
        }

        public void Update(GameTime gameTime)
        {
            if (!_gameState.Active)
                return;

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var playerEntities = _entityManager.Entities<Player>();

            foreach (var entity in _entityManager.Entities<Enemy>())
            {
                CalculateState(entity, playerEntities, elapsed);
                ApplyAnimation(entity);
            }
        }

        private void CalculateState(Enemy entity, IEnumerable<Player> playerEntities, float elapsed)
        {
            // Enter dead
            if (entity.Damage.Remaining <= 0 && !entity.State.Dying)
            {
                entity.State.Dying = true;
            }

            // Walking
            if (!entity.State.Dying)
            {
                var enemyToClosestPlayerVector = playerEntities
                    .Select(pe => pe.Body.Position - entity.Body.Position)
                    .OrderBy(v => v.Length())
                    .FirstOrDefault();

                if (enemyToClosestPlayerVector != default)
                {
                    entity.State.Direction = Vector2.Normalize(enemyToClosestPlayerVector);
                    entity.Kinetic.Force = entity.State.Direction * _walkingForce;
                }
            }

            if (entity.State.Dying)
            {
                if (entity.State.DyingElapsed % (1 / _explosionsPerSecond) < 0.0166f)
                {
                    var randomPosition = entity.Body.Position + CreateRandomVector2() * _explosionJitter;
                    _entityManager.Create(Particle.ExplosionConstructor(randomPosition));
                }

                if (entity.State.DyingElapsed > _deathDuration)
                {
                    entity.Destroyed = true;
                }
                else
                {
                    entity.State.DyingElapsed += elapsed;
                    entity.Kinetic.Force = Vector2.Zero;
                }
            }
        }

        private static void ApplyAnimation(Enemy entity)
        {
            var cardinalDirection = entity.State.Direction.ToCardinal();
            if (cardinalDirection.X == 1)
                entity.Sprite.Animation = Enemy.AnimationWalkRight;
            else if (cardinalDirection.X == -1)
                entity.Sprite.Animation = Enemy.AnimationWalkLeft;
            else if (cardinalDirection.Y == 1)
                entity.Sprite.Animation = Enemy.AnimationWalkDown;
            else if (cardinalDirection.Y == -1)
                entity.Sprite.Animation = Enemy.AnimationWalkUp;
        }
    }
}