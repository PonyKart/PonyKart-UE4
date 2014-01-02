using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ponykart.Players;
using Mogre;
using Ponykart.Sound;
using System.Timers;

namespace Ponykart.Items
{
    class SpeedMuffin : Item
    {
        private Vector3 origin;
        private System.Timers.Timer endTimer;
        private float effectTimer;
        private bool used;
        private float defaultSpeed;
        private float defaultAccel;
        //Player User;
        public SpeedMuffin(ref Player user) : base(ref user, "SpeedMuffin")
        {
            origin = user.NodePosition;
            //User = user;
        }

        protected override void OnUse()
        {
            base.OnUse();
            effectTimer = 3;
            LKernel.GetG<SoundMain>().Play3D("Boost Pickup.wav", origin, false);
            defaultSpeed = User.Kart.MaxSpeed;
            defaultAccel = User.Kart.Acceleration;

            endTimer = new System.Timers.Timer(2000);
            endTimer.Elapsed += new ElapsedEventHandler(OnEnd);
            endTimer.Start();
        }

        protected override void EveryTenth(object o)
        {
            if (effectTimer > 1.0f)
            {
                User.Kart.MaxSpeed = defaultSpeed * effectTimer;
                User.Kart.Acceleration = defaultAccel * effectTimer;
                effectTimer -= 0.1f;
            }
            base.EveryTenth(o);
        }
        protected void OnEnd(object source, ElapsedEventArgs e)
        {
            User.Kart.MaxSpeed = defaultSpeed;
            User.Kart.Acceleration = defaultAccel;
        }
    }
}
