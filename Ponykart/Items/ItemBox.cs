using System;
using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.Players;
using Ponykart.Physics;
using PonykartParsers;
using Ponykart.Sound;
using Ponykart.UI;

namespace Ponykart.Items
{
    class ItemBox : LThing
    {
        private LThing box;
        private GameUIManager ui = LKernel.GetG<GameUIManager>();
        public Item contents;
        protected string itemName;

        public ItemBox(ThingBlock block, ThingDefinition def, string _itemName) : base(block, def)
        {
           LKernel.GetG<CollisionReporter>().AddEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Default, OnCol);
           //contents = LKernel.GetG<ItemManager>().SpawnItem(null, _itemName);
           itemName = _itemName;
        }


        void OnCol(CollisionReportInfo info)
        {
            if (info.FirstGroup == PonykartCollisionGroups.Karts)
            {
                foreach (Player p in LKernel.GetG<PlayerManager>().Players)
                {
                    if (info.FirstObject.GetHashCode() == p.Kart.Body.GetHashCode() && info.SecondObject.GetHashCode() == this.Body.GetHashCode())
                    {
                        p.hasItem = true;
                        ui.SetItemLevel(1);
                        ui.SetItemImage(itemName);
                        p.heldItem = itemName;
                        LKernel.GetG<SoundMain>().Play3D("Item Get.wav", p.NodePosition, false);                       
                        //DummyItem dummy = new DummyItem(itemName, p);
                        LKernel.GetG<CollisionReporter>().RemoveEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Default, OnCol);
                        Dispose();
                    }
                }
                
            }
        }
    }

}
