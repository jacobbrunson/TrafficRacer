﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Gui
{
	class ScoreUI
	{
		private static SpriteFont Font;

		private int Score;
		private MessageQueue points = new MessageQueue();

		public void ShowPoints(int amount)
		{
			Random r = new Random();

			Message m = new Message(String.Format("+{0:n0}", amount));
			m.Offset = new Vector2((float)r.NextDouble() * 50, r.Next(0, 2) == 0 ? -10 : 30);

			points.Enqueue(m);
		}

		public void Update(GameTime gameTime, int score)
		{
			Score = score;
			points.Update(gameTime);
		}

		public void Render(SpriteBatch spriteBatch, int viewportWidth, int viewportHeight)
		{
			Vector2 scorePos = new Vector2(viewportWidth - 200, viewportHeight - 50);
			spriteBatch.DrawString(Font, "Score: " + Score, scorePos, Color.Blue, 0, Vector2.Zero, 2, SpriteEffects.None, 0);

			foreach (Message m in points)
			{
				Color color = new Color(1f, 0.2f, 0.2f);
				spriteBatch.DrawString(Font, m.Text, scorePos + m.Offset, color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Font = content.Load<SpriteFont>("font");
		}

	}
}