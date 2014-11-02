using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceFlight
{
    public class ParticleController
    {
        Vector2 position;
        List<Particle> particles;
        Random rnd;
        float life;

        public ParticleController(Vector2 pos, float lifeSeconds)
        {
            position = pos;
            rnd = new Random();
            particles = new List<Particle>();
            life = lifeSeconds;
        }

        public void Update(GameTime updTime)
        {
            if (life > 0)
            {
                Particle part = new Particle(position, new Vector2(rnd.Next(-5, 5)*10, rnd.Next(-5, 5)*10), life);
                particles.Add(part);
            }

            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                p.pos += p.force * (float)updTime.ElapsedGameTime.TotalSeconds;
                p.life -= (float)updTime.ElapsedGameTime.TotalSeconds;
                particles[i] = p;
                if (p.life <= 0)
                    particles.Remove(p);
            }

            life -= (float)updTime.ElapsedGameTime.TotalSeconds;
            if(life <= 0)
                Game1.GetDelegate().particles.Remove(this);
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Particle particle in particles)
                particle.Draw(batch);
        }
    }
}
