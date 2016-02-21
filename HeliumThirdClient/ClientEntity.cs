﻿using System;

namespace HeliumThirdClient
{
    class ClientEntity
    {
        public long UID { get; }

        public double X { get; private set; }
        public double Y { get; private set; }

        private int MovingToX;
        private int MovingToY;
        private double MovementSpeed;

        public ClientEntity(HeliumThird.Events.EntityUpdate e)
        {
            UID = e.UID;
            UpdateState(e);
        }

        public void UpdateState(HeliumThird.Events.EntityUpdate e)
        {
            X = e.X;
            Y = e.Y;

            MovingToX = e.TargetX;
            MovingToY = e.TargetY;
            MovementSpeed = e.MoveSpeed;
        }

        public void Update(double dt)
        {
            double dx = MovingToX - X;
            double dy = MovingToY - Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance <= MovementSpeed * dt)
            {
                X = MovingToX;
                Y = MovingToY;
            }
            else
            {
                X += dx / distance * MovementSpeed * dt;
                Y += dy / distance * MovementSpeed * dt;
            }
        }
    }
}