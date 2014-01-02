using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ponykart.Players;
using Mogre;
using Ponykart.Sound;

namespace Ponykart.Items
{
    class BigApple : Item
    {
        private Vector3 origin;

        public BigApple(Player user) : base(ref user, "BigApple")
        {
            origin = user.NodePosition; 
        }

        protected override void OnUse()
        {
            base.OnUse();
            LKernel.GetG<SoundMain>().Play3D("Apple Firing.mp3", origin, false);

            Vector3 itemVel = User.Kart.Vehicle.ForwardVector;
            itemVel *= User.Kart.VehicleSpeed + 1000.0f;
            itemVel.y = 300.0f;
            Body.Body.ApplyCentralImpulse(itemVel);
        }

        protected override void EveryTenth(object o)
        {
            base.EveryTenth(o);
        }
    }
}
