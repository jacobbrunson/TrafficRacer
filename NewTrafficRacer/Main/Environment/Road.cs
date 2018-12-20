﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Utility;
using tainicom.Aether.Physics2D.Dynamics;

namespace NewTrafficRacer.Environment
{
    class Road
    {
        LinkedList<RoadSegment> Segments = new LinkedList<RoadSegment>();

        public const int NumLanes = 4;
        public const float LaneWidth = 2.45f;
        public const float Size = NumLanes * LaneWidth;

        public int Highlight = -1;
        public double HighlightChangeTime = -10;

        ContentManager content;

        World world;

        int groundWidth = 10;
        int groundOffsetX = 0;
        float biomeScale = 100;

        public Road(ContentManager content, World world, int groundWidth = 10, int groundOffsetX = 0, float biomeScale = 100)
        {
            this.content = content;
            this.world = world;
            this.groundWidth = groundWidth;
            this.groundOffsetX = groundOffsetX;
            this.biomeScale = biomeScale;
            Reset();
        }

        public void Reset()
        {
            Segments.Clear();
            for (int i = 0; i < 10; i++)
            {
                var piece = new RoadSegment(content, world, Size * (i - 1), Highlight, groundWidth, groundOffsetX, biomeScale);
                Segments.AddLast(piece);
            }
        }

        public int GetHighlightAtPlayerPos()
        {
            return Segments.First.Next.Value.HighlightedLane;
        }

        public static int GetLane(float x, float tolerance = 0.1f)
        {
            float f = x.Map(-LaneWidth * NumLanes / 2, LaneWidth * NumLanes / 2, 0, NumLanes);
            int i = (int)f;

            if (Math.Abs(x - GetCenterOfLane(i)) > tolerance)
            {
                return -100;
            }

            return i;
        }

        public void SetHighlightStatus(int lane)
        {
            foreach (RoadSegment segment in Segments)
            {
                segment.SetHighlightStatus(lane);
            }
        }

        public static float GetCenterOfLane(int lane)
        {
            return LaneWidth * (lane - NumLanes / 2) + LaneWidth / 2;
        }

        public void Update(GameTime gameTime, float playerY)
        {
            if (playerY - Segments.First.Value.Y > Size * 2)
            {
                Segments.First.Value.Destroy();
                Segments.RemoveFirst();
                var piece = new RoadSegment(content, world, Segments.Last.Value.Y + Size, Highlight, groundWidth, groundOffsetX, biomeScale);
                Segments.AddLast(piece);
            }

            double d = gameTime.TotalGameTime.TotalSeconds - HighlightChangeTime;
            if (d > 10)
            {
                Highlight = -1;
                HighlightChangeTime = gameTime.TotalGameTime.TotalSeconds;
                Highlight = new Random().Next(4);
            }
        }

        public void Render(GameTime gameTime, GraphicsDevice graphics, Effect effect)
        {
            foreach (RoadSegment p in Segments)
            {
                p.Render(gameTime, graphics, effect);
            }
        }
    }
}

