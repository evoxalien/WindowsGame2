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
    class Enemy : Entity
    {

        public static Random rand = new Random();
        private int timeUntilStart = 60;
        public bool IsActive { get { return timeUntilStart <= 0; } }
        private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();
        private int PointValue { get; set; }
        private int EnemyType = 0;
        //float MaxAngle = 2;


        public Enemy(Texture2D image, Vector2 position)
        {
            this.image = image;
            Position = position;
            Radius = image.Width / 2f;
            color = Color.Transparent;
        }

        #region EnemyLogic
        IEnumerable<int> FollowPlayer(float acceleration = 1f)
        {
            Vector2 PlayerPosition = PlayerShip.Instance.Position;
            
            Vector2 LastPlayerPosition = PlayerPosition;
            
            while (true)
            {
                int framesPerMove = 10;
                PlayerPosition = PlayerShip.Instance.Position;
                /*
                for (int i = 0; i < framesPerMove; i++)
                {
                    if(i == 0)
                        LastPlayerPosition = PlayerShip.Instance.Position;
                    if(i == 4)
                        PlayerPosition = PlayerShip.Instance.Position;
                    Velocity += (((PlayerPosition + LastPlayerPosition)/2) - Position).ScaleTo(acceleration);
                    if (Velocity != Vector2.Zero)
                        Orientation = Velocity.ToAngle();
                    yield return 0;
                }
                */
                Velocity += (PlayerPosition - Position).ScaleTo(acceleration);
                if (Velocity != Vector2.Zero)
                    Orientation = Velocity.ToAngle();
                yield return 0;
            }
        }


        IEnumerable<int> MoveInASquare()
        {
            const int framesPerSide = 30;
            while (true)
            {
                //Move Right for 30 frames
                for (int i = 0; i < framesPerSide; i++)
                {
                    Velocity = Vector2.UnitX;
                    yield return 0;
                }
                //Move Down
                for (int i = 0; i < framesPerSide; i++)
                {
                    Velocity = Vector2.UnitY;
                    yield return 0;
                }
                //Move Left
                for (int i = 0; i < framesPerSide; i++)
                {
                    Velocity = -Vector2.UnitX;
                    yield return 0;
                }
                //Move up
                for (int i = 0; i < framesPerSide; i++)
                {
                    Velocity = -Vector2.UnitY;
                    yield return 0;
                }
            }
        }
        

        IEnumerable<int> MoveRandomly()
        {

            float direction = rand.NextFloat(0, MathHelper.TwoPi);

            while (true)
            {
                direction += rand.NextFloat(-0.1f, 0.1f);
                direction = MathHelper.WrapAngle(direction);

                for (int i = 0; i < 6; i++)
                {
                    Velocity += MathUtil.FromPolar(direction, 0.4f);
                    Orientation -= 0.05f;

                    var bounds = GameRoot.Viewport.Bounds;
                    bounds.Inflate(-image.Width, -image.Height);

                    //if the enemy is outside the bounds, make it move away from the edge
                    if (!bounds.Contains(Position.ToPoint()))
                        direction = (GameRoot.ScreenSize / 2 - Position).ToAngle() + rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

                    yield return 0;
                }
            }

        }

        IEnumerable<int> FlappyBirdMotion()
        {

            float direction = rand.NextFloat(0, MathHelper.TwoPi);
            const int framesBetweenDips = 60;

            while (true)
            {
               float LastOrientation = Orientation;
               
                for (int i = 0; i < framesBetweenDips/6; i++)
                {
                    Velocity += Vector2.UnitX/4;
                    Orientation = Velocity.ToAngle();
                    yield return 0;
                }
                for (int i = 0; i < framesBetweenDips / 3 + rand.Next(0, 100); i++)
                {
                    Velocity += (Vector2.UnitY + Vector2.UnitX)/2;
                    Orientation = Velocity.ToAngle();
                    yield return 0;
                }
                for (int i = 0; i < framesBetweenDips / 2 + rand.Next(0, 10); i++)
                {
                    Velocity += (-Vector2.UnitY + Vector2.UnitX)/2;
                    Orientation = Velocity.ToAngle();
                    yield return 0;
                }

                if (Velocity != Vector2.Zero) {
                    Orientation = Velocity.ToAngle();
                }
                Velocity *= .75f;
            }

        }
        #endregion
        
        #region DelcaringEnemyTypes
        public static Enemy CreateSeeker(Vector2 position)
        {
            var enemy = new Enemy(Art.Seeker, position);
            enemy.AddBehaviour(enemy.FollowPlayer());
            enemy.PointValue = 25;
            enemy.EnemyType = 1;
             
            return enemy;
        }

        public static Enemy CreateWanderer(Vector2 position)
        {
            var enemy = new Enemy(Art.Wanderer, position);
            enemy.AddBehaviour(enemy.MoveRandomly());
            enemy.PointValue = 10;
            enemy.EnemyType = 2;

            return enemy;
        }

        public static Enemy CreateSquareDance(Vector2 position)
        {
            var enemy = new Enemy(Art.Wanderer, position);
            enemy.AddBehaviour(enemy.MoveInASquare());
            //enemy.AddBehaviour(enemy.FollowPlayer());
            enemy.PointValue = 1;
            enemy.EnemyType = 3;

            return enemy;
        }

        public static Enemy CreateFlappyMinion(Vector2 position)
        {
            var enemy = new Enemy(Art.FlappyBirdMinion, position);
            enemy.AddBehaviour(enemy.FlappyBirdMotion());
            enemy.PointValue = 2;
            enemy.EnemyType = 4;
            return enemy;
        }
        #endregion
        
        #region AdditionalFunctions
        public void Effect()
        {
            for (int i = 0; i < 20; i++)
            {
                float speed = 6f * (1f - 1 / rand.NextFloat(1f, 10f));
                var state = new ParticleState()
                {
                    Velocity = rand.NextVector2(speed, speed),
                    Type = ParticleType.Enemy,
                    LengthMultiplier = 1f
                };
                GameRoot.grid.ApplyExplosiveForce(2.5f, Position, 75);
                if (EnemyType == 1 || EnemyType == 4)
                    GameRoot.ParticleManager.CreateParticle(Art.LineParticle, Position, Color.FromNonPremultiplied(255, 255, 128, 155), 190, new Vector2(1.0f), state);
                else if (EnemyType == 2)
                    GameRoot.ParticleManager.CreateParticle(Art.LineParticle, Position, Color.FromNonPremultiplied(255, 128, 128, 155), 190, new Vector2(1.0f), state);
                else
                    GameRoot.ParticleManager.CreateParticle(Art.LineParticle, Position, Color.LightGreen, 190, new Vector2(1.0f), state);
            }
        }


        public void WasShot()
        {
            PlayerStatus.AddPoints(PointValue);
            PlayerStatus.IncreaseCombo();
            //Sound.Explosion.Play(0.5f, rand.NextFloat(-0.2f, 0.2f), 0);
            IsExpired = true;

            Effect();

        }


        public void HandleCollision(Enemy other)
        {
            var d = Position - other.Position;
            Velocity += 10 * d / (d.LengthSquared() + 1);
        }


        private void AddBehaviour(IEnumerable<int> behavior)
        {
            behaviours.Add(behavior.GetEnumerator());
        }


        private void ApplyBehaviors()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (!behaviours[i].MoveNext())
                    behaviours.RemoveAt(i--);
            }
        }
        #endregion

        #region Update
        public override void Update()
        {
            if (timeUntilStart <= 0)
            {
                //Enemy Behavior logic goes here
                if (timeUntilStart <= 0)
                    ApplyBehaviors();
            }
            else
            {
                timeUntilStart--;
                color = Color.White * (1 - timeUntilStart / 60f);
            }

            Position += Velocity;
            Position = Vector2.Clamp(Position, Size / 2, GameRoot.ScreenSize - Size / 2);

            Velocity *= 0.75f;

        }
        #endregion
    }
}
