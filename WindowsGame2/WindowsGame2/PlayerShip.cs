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
    class PlayerShip : Entity
    {
        private static PlayerShip instance;

        public static int WeaponLevel = 1;

        int framesUntilRespawn = 0;
        int cooldownFrames = 6;
        int cooldownRemaining = 0;
        public static Random rand = new Random();
        public bool IsDead { get { return framesUntilRespawn > 0; } }

        public static PlayerShip Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerShip();
                return instance;
            }
        }

        private PlayerShip()
        {
            image = Art.Player;
            Position = GameRoot.ScreenSize / 2;
            Radius = 10;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsDead)
                base.Draw(spriteBatch);
        }

        public void Kill()
        {
            
            PlayerStatus.RemoveLife();
            framesUntilRespawn = 60;
            framesUntilRespawn = PlayerStatus.isGameOver ? 300 : framesUntilRespawn;
            EnemySpawner.Reset();
        }

        public override void Update()
        {
            if (IsDead)
            {
                framesUntilRespawn--;
                return;
            }
            if (PlayerStatus.isGameOver)
                PlayerStatus.Reset();

            const float speed = 8;
            Velocity = speed * Input.GetMovementDirection();
            Position += Velocity;
            Position = Vector2.Clamp(Position, Size / 2, GameRoot.ScreenSize - Size / 2);

            if (Velocity.LengthSquared() > 0)
                Orientation = Velocity.ToAngle();

            var aim = Input.GetAimDirection();
            if (aim.LengthSquared() > 0 && cooldownRemaining <= 0)
            {
                cooldownRemaining = cooldownFrames;
                float aimAngle = aim.ToAngle();
                Quaternion aimQuat = Quaternion.CreateFromYawPitchRoll(0, 0, aimAngle);

                float randomSpread = rand.NextFloat(-0.04f, 0.04f) + rand.NextFloat(-0.04f, 0.04f);
                Vector2 vel = MathUtil.FromPolar(aimAngle + randomSpread, 11F);
                Vector2 offset;

                if (WeaponLevel == 1)
                {
                    cooldownFrames = 4;
                    offset = Vector2.Transform(new Vector2(35, 0), aimQuat);
                    EntityManager.Add(new Bullet(Position + offset, vel));
                }
                else if (WeaponLevel == 2)
                {
                    cooldownFrames = 6;

                    offset = Vector2.Transform(new Vector2(35, -8), aimQuat);
                    EntityManager.Add(new Bullet(Position + offset, vel));

                    offset = Vector2.Transform(new Vector2(35, 8), aimQuat);
                    EntityManager.Add(new Bullet(Position + offset, vel));
                }
                else if (WeaponLevel >= 3)
                {
                    cooldownFrames = 3;
                    offset = Vector2.Transform(new Vector2(35, -8), aimQuat);
                    EntityManager.Add(new Bullet(Position + offset, vel));

                    offset = Vector2.Transform(new Vector2(35, 8), aimQuat);
                    EntityManager.Add(new Bullet(Position + offset, vel));

                }
                
                
            }

            if(cooldownRemaining > 0)
                cooldownRemaining --;
        }
    }
}
