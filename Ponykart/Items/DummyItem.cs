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
    class DummyItem : LDisposable
    {
        Player focus;
        LThing dummy;
        public DummyItem(string itemType, Player _focus)
        {
            focus = _focus;
            Vector3 pos = focus.NodePosition;
            pos.y += 2;
            dummy = LKernel.GetG<Spawner>().Spawn(itemType, pos);
            Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
        }
        void EveryTenth(object o)
        {
            Vector3 pos = focus.NodePosition;
            pos.y += 2;
            dummy.RootNode.Position = pos;
        }
    }


}
