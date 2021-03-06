﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GeometryWars
{
    static class EntityManager
    {
        private static Random rand = new Random();

        //Entity Lists
        static List<Entity> entities = new List<Entity>();
        public static List<Enemy> enemies = new List<Enemy>();
        static List<Bullet> bullets = new List<Bullet>();
        static List<BlackHole> blackHoles = new List<BlackHole>();
        static List<Boss> bosses = new List<Boss>();

        public static IEnumerable<Boss> Bosses { get { return bosses; } }
        public static IEnumerable<BlackHole> BlackHoles { get { return blackHoles; } }
        public static IEnumerable<Enemy> Enemies { get { return enemies; } }

        static bool isUpdating;
        static List<Entity> addedEntities = new List<Entity>();

        //Counts
        public static int Count { get { return entities.Count; } }
        public static int BlackHoleCount { get { return blackHoles.Count; } }

        #region AddingEntities
        private static void AddEntity(Entity entity)
        {
            entities.Add(entity);
            if (entity is Bullet)
                bullets.Add(entity as Bullet);
            else if (entity is Enemy)
                enemies.Add(entity as Enemy);
            else if (entity is BlackHole)
                blackHoles.Add(entity as BlackHole);
            else if (entity is Boss)
                bosses.Add(entity as Boss);

        }

        public static void Add(Entity entity)
        {
            if (!isUpdating)
                AddEntity(entity);
            else
                addedEntities.Add(entity);
        }
        #endregion

        #region BlackHoleRelated
        // Used with Blackholes!

        public static System.Collections.IEnumerable GetNearbyEntities(Vector2 position, float radius)
        {
            return entities.Where(x => Vector2.DistanceSquared(position, x.Position) < radius * radius);
        }
        #endregion

        #region Collision
        private static bool IsColliding(Entity a, Entity b)
        {
            float radius = a.Radius + b.Radius;
            return !a.IsExpired && !b.IsExpired && Vector2.DistanceSquared(a.Position, b.Position) < radius * radius;
        }

        static void HandleCollisions()
        {
            //handle collisions between enemies
            for (int i = 0; i < enemies.Count; i++)
                for (int j = i + 1; j < enemies.Count; j++)
                {
                    if (IsColliding(enemies[i], enemies[j]))
                    {
                        enemies[i].HandleCollision(enemies[j]);
                        enemies[j].HandleCollision(enemies[i]);
                    }
                }

            //handle HandleCollisions between bullets and enemies
            for (int i = 0; i < enemies.Count; i++)
                for (int j = 0; j < bullets.Count; j++)
                {
                    if(IsColliding(enemies[i], bullets[j]))
                    {
                        enemies[i].WasShot();
                        bullets[j].IsExpired = true;
                        for (int p = 0; p < 30; p++)
                            GameRoot.ParticleManager.CreateParticle(Art.LineParticle, bullets[j].Position, Color.LightBlue, 50, new Vector2(0.5f, 0.5f), new ParticleState() { Velocity = rand.NextVector2(0, 9), Type = ParticleType.Bullet, LengthMultiplier = 1 });
                    }
                }
        for(int j = 0; j < bosses.Count; j++)
            for(int i = 0; i < bullets.Count; i++)
                if(IsColliding(bosses[j], bullets[i]))
                {
                    bosses[j].WasShot();
                    bullets[i].IsExpired = true;
                    for (int p = 0; p < 30; p++)
                        GameRoot.ParticleManager.CreateParticle(Art.LineParticle, bullets[i].Position, Color.LightBlue, 50, new Vector2(0.5f, 0.5f), new ParticleState() { Velocity = rand.NextVector2(0, 9), Type = ParticleType.Bullet, LengthMultiplier = 1 });
                }

            //HANDLE COLLISIONS BETWEEN PLAYERS AND ENEMIES

            if (Input.GodMode == false)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].IsActive && IsColliding(PlayerShip.Instance, enemies[i]))
                    {
                        PlayerShip.Instance.Kill();
                        enemies.ForEach(x => x.WasShot());
                        blackHoles.ForEach(x => x.Kill());
                        for (int x = 0; x < bullets.Count; x++)
                            bullets[x].IsExpired = true;
                        break;

                    }
                }
            }
            //Boss boss;
            //boss.FlappyKing();
            // handle collisions with black holes
            for (int i = 0; i < blackHoles.Count; i++)
            {//Colliding with enemies
                for (int j = 0; j < enemies.Count; j++)
                    if (enemies[j].IsActive && IsColliding(blackHoles[i], enemies[j]))
                    {
                        enemies[j].Effect();
                        enemies[j].IsExpired = true;
                        blackHoles[i].hitpoints++;

                        
                        for (int p = 0; p < 10; p++)
                        {
                            float speed = 10f * (1f - 1 / rand.NextFloat(1f, 10f)); ;
                            var state = new ParticleState()
                            {
                                Velocity = rand.NextVector2(speed, speed),
                                Type = ParticleType.Enemy,
                                LengthMultiplier = 1f
                            };
                            
                            GameRoot.ParticleManager.CreateParticle(Art.LineParticle, enemies[j].Position, Color.FromNonPremultiplied(128, 255, 255, 155), 190, new Vector2(1.0f), state);
                        }

                        if (blackHoles[i].hitpoints >= 35)
                        {
                            Vector2 pos = blackHoles[i].Position;
                            for (int p = 0; p < 55; p++)
                            {
                                float speed = 10f * (1f - 1 / rand.NextFloat(1f, 10f)); ;
                                var state = new ParticleState()
                                {
                                    Velocity = rand.NextVector2(speed, speed),
                                    Type = ParticleType.Enemy,
                                    LengthMultiplier = 1f
                                };

                                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, enemies[j].Position, Color.FromNonPremultiplied(128, 255, 255, 155), 190, new Vector2(1.0f), state);
                            }
                            blackHoles[i].Kill();

                            EnemySpawner.Swarm(pos);
                            
                        }
                        
                    }
                for (int j = 0; j < bullets.Count; j++)
                {
                    if (IsColliding(blackHoles[i], bullets[j]))
                    {
                        bullets[j].IsExpired = true;
                        blackHoles[i].WasShot();
                        for (int p = 0; p < 30; p++)
                            GameRoot.ParticleManager.CreateParticle(Art.LineParticle, bullets[j].Position, Color.LightBlue, 50, new Vector2(0.5f, 0.5f), new ParticleState() { Velocity = rand.NextVector2(0, 9), Type = ParticleType.Bullet, LengthMultiplier = 1 });
                    
                    }
                }
                if (Input.GodMode == false)
                {
                    if (IsColliding(PlayerShip.Instance, blackHoles[i]))
                    {
                        PlayerShip.Instance.Kill();
                        enemies.ForEach(x => x.IsExpired = true);
                        blackHoles.ForEach(x => x.Kill());

                        for (int x = 0; x < bullets.Count; x++)
                            bullets[x].IsExpired = true;
                        break;
                    }
                }
            }
        }
        #endregion

        #region Update
        public static void Update()
        {
            isUpdating = true;

            HandleCollisions();

            foreach (var entity in entities)
                entity.Update();

            isUpdating = false;

            foreach (var entity in addedEntities)
                AddEntity(entity);

            addedEntities.Clear();

            // remove any expired entities
            entities = entities.Where(x => !x.IsExpired).ToList();
            bullets = bullets.Where(x => !x.IsExpired).ToList();
            enemies = enemies.Where(x => !x.IsExpired).ToList();
            blackHoles = blackHoles.Where(x => !x.IsExpired).ToList();

        }
        #endregion

        #region Draw
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in entities)
                entity.Draw(spriteBatch);
        }
        #endregion
    }
}
