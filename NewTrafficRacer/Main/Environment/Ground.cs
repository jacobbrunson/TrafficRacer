﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Utility;
using SharpNoise.Modules;

namespace NewTrafficRacer.Environment

{
	public class Ground
	{

        public float positionY;
        public int offsetX;

        int width = 10;
        const int length = 4;
        Vector2 scale = new Vector2(Road.Size / length);

        float[,] heightMap;
        VertexPositionColorNormal[] vertices;

        Effect effect;

        static Perlin noiseSource = new Perlin
        {
            Seed = new Random().Next()
        };

        public struct VertexPositionColorNormal : IVertexType
        {
            public Color Color;
            public Vector3 Normal;
            public Vector3 Position;

            public static readonly VertexElement[] VertexElements = {
                new VertexElement(0, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
            };

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return new VertexDeclaration(VertexElements); }
            }
        }

        public Ground(ContentManager content, float Y, int width = 10, int offsetX = 0, float biomeScale = 100)
        {
            this.width = width;
            this.offsetX = offsetX;
            heightMap = new float[width + 1, length + 1];
            vertices = new VertexPositionColorNormal[width * length * 6]; //we are intentionally duplicating vertices, hence the * 6 and lack of indexing
            positionY = Y;

            effect = content.Load<Effect>("effect");

            var random = new Random();

            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= length; y++)
                {
                    float value = ((float)noiseSource.GetValue(x * 0.5f, (y + positionY / scale.Y) * 0.5f, 0.5f) + 0) / 1;
                    if (Math.Abs(x - width / 2 - offsetX) * scale.X < Road.Size * 0.6f)
                    {
                        value = 0;
                    }
                    heightMap[x, y] = value;
                }
            }

            Vector2[] offsets = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    int i = (width * y + x) * 6;

                    VertexPositionColorNormal vertex;

                    for (int j = 0; j < 6; j++)
                    {
                        float ox = x + offsets[j].X;
                        float oy = y + offsets[j].Y;
                        float height = heightMap[(int)ox, (int)oy];
                        vertex = new VertexPositionColorNormal();
                        vertex.Position = new Vector3(ox * scale.X, oy * scale.Y, height * 1.5f);
                        Vector3[] colors =
                        {
                            new Vector3(0.2f, (heightMap[(int)ox, (int)oy]+1)/4 + 0.3f, 0.3f),
                            new Vector3(height.Map(-1, 1, 0.4f, 0.9f), 0.4f, 0.3f),
                            new Vector3(height.Map(-1, 1, 0.7f, 0.9f), height.Map(-1, 1, 0.6f, 0.8f), height.Map(-1, 1, 0.2f, 0.5f)),
                        };
                        float val = Math.Abs(y + positionY) / biomeScale % colors.Length;

                        int primary = (int)val;
                        int secondary = (primary + 1) % colors.Length;
                        float blendAmt = 1 - (float)Math.Pow((val - primary) * 2 - 1f, 2);
                        if (val - primary < 0.8f)
                        {
                            blendAmt = 1;
                        }

                        Vector3 blended = colors[primary] * blendAmt + colors[secondary] * (1 - blendAmt);
                        Color color = new Color(blended.X, blended.Y, blended.Z);
                        vertex.Color = color;

                        vertices[i + j] = vertex;
                    }
                }
            }

            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3 u = vertices[i].Position - vertices[i + 1].Position;
                Vector3 v = vertices[i].Position - vertices[i + 2].Position;
                Vector3 normal = Vector3.Cross(v, u);
                normal.Normalize();

                Color color = Color.White;
                //color.R = (byte)((vertices[i + 0].Color.R + vertices[i + 1].Color.R + vertices[i + 2].Color.R) / 3);
                //color.G = (byte)((vertices[i + 0].Color.G + vertices[i + 1].Color.G + vertices[i + 2].Color.G) / 3);
                //color.B = (byte)((vertices[i + 0].Color.B + vertices[i + 1].Color.B + vertices[i + 2].Color.B) / 3);

                color.R = Math.Max(Math.Max(vertices[i + 0].Color.R, vertices[i + 1].Color.R), vertices[i + 2].Color.R);
                color.G = Math.Max(Math.Max(vertices[i + 0].Color.G, vertices[i + 1].Color.G), vertices[i + 2].Color.G);
                color.B = Math.Max(Math.Max(vertices[i + 0].Color.B, vertices[i + 1].Color.B), vertices[i + 2].Color.B);

                for (int j = 0; j < 3; j++)
                {
                    vertices[i + j].Normal = normal;
                    vertices[i + j].Color = color;
                }
            }
        }

        public void Render(GraphicsDevice graphics, Effect effect)
		{
			var prevTechnique = effect.CurrentTechnique;

			Matrix world = Matrix.CreateTranslation(Vector3.Left * ((float)width * scale.X / 2f + offsetX * scale.X) + Vector3.UnitY * positionY);
			effect.CurrentTechnique = effect.Techniques["ShadowedTerrain"];
			effect.Parameters["World"].SetValue(world);
			effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["LightMatrix"].SetValue(world * TrafficGame.lighting.View * TrafficGame.lighting.Projection);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				graphics.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
			}

			effect.CurrentTechnique = prevTechnique;
		}
	}
}