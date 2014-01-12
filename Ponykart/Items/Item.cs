using System;
using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.Players;
using PonykartParsers;

namespace Ponykart.Items
{
    public abstract class Item : LDisposable
    {
        //This is the item's physical presence on the map.
        public LThing Body { get; protected set; }
        public Player User { get; protected set; }
        //public bool HasMesh;

        public Item() { }

        public Item(ref Player user, string itemName)
        {
            User = user;
            try
            {
                Body = LKernel.GetG<Spawner>().Spawn(itemName, User.Kart.ActualPosition + User.Kart.Vehicle.ForwardVector * 5.0f);
            }
            catch (Exception e)
            { }

            OnUse();
            Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
        }

        protected virtual void OnUse()
        {
        }

        protected virtual void EveryTenth(object o)
        {
        }

        public void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;
                Body.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}