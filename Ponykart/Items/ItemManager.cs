using System;
using System.Linq;
using System.Collections.Generic;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;
using Mogre;


namespace Ponykart.Items
{

    class ItemManager
    {
        //This const governs the frequency of itemdrops.
        //On average, an item should drop every n seconds.
        const int ITEMFREQ = 10;
        public List<Item> activeItems = new List<Item>();
        public List<ItemBox> boxes = new List<ItemBox>();
        public bool spawning = false;
        Random rand = new Random();

        private List<string> itemNames = new List<string>();

        public ItemManager() {
			Launch.Log("[Loading] Creating ItemManager...");
			//LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
            Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
            itemNames.Add("SmartApple");
            itemNames.Add("SpeedMuffin");
		}

        public Item SpawnItem(Player user, string itemName)
        {
            Item spawnedItem = null;

            //There is probably a better way to do this.
            switch(itemName)
            {
                case "SmartApple":
                    {
                        spawnedItem = new SmartApple(user);
                        activeItems.Add(spawnedItem);
                    } break;
                case "SpeedMuffin":
                    {
                        spawnedItem = new SpeedMuffin(ref user);
                        activeItems.Add(spawnedItem);
                    } break;
                default:
                    {

                    }break;
            }

            //if(spawnedItem!= null)
            return spawnedItem;
        }
        public void RequestBox(Vector3 pos)
        {
            ItemBox box;
            box = LKernel.GetG<Spawner>().Spawn<ItemBox>("Barrel", pos, (t, d) => new ItemBox(t, d, GetRandomItem()));
            boxes.Add(box);
        }
        void EveryTenth(object o)
        {
            if (spawning && rand.Next(ITEMFREQ * 10) == 1)
            {
                Vector3 spawnpos = LKernel.GetG<PlayerManager>().MainPlayer.NodePosition;
                spawnpos.y += 2;
                RequestBox(spawnpos);
            }
        }
        void OnLevelUnload(LevelChangedEventArgs eventArgs)
        {
            if (eventArgs.OldLevel.Type == LevelType.Race)
            {
                //Clean up any remaining items
                activeItems.Clear();
                boxes.Clear();
            }
        }
        private string GetRandomItem()
        {
            int r = rand.Next(itemNames.Count);
            return itemNames[r];
        }
    }


}
