using System.Drawing;
using Mogre;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Networking;

namespace Ponykart.UI
{
    class GameUIManager
    {
        public GUI inGameUI;
        PictureBox itembox;
        PictureBox itemimage;
        public GameUIManager()
        {
            UIMain uiMain = LKernel.GetG<UIMain>();

            //This mess gets the height and width of the window for centering UI entities.
            uint uheight, uwidth, colorDepth;
            int height, width;
            RenderWindow window = LKernel.GetG<RenderWindow>();
            window.GetMetrics(out uwidth, out uheight, out colorDepth);
            width = (int)uwidth;
            height = (int)uheight;

            inGameUI = uiMain.GetGUI("ingame gui");
            itembox = inGameUI.GetControl<PictureBox>("itembox");
            //itembox.Top = (height / 2);
            //itembox.Bottom = (height / 2);
            //itembox.Left = (width / 2);
            //itembox.Right = (width / 2);

            itemimage = inGameUI.GetControl<PictureBox>("itemimage");
            //itemimage.Top = (height / 2);
            //itemimage.Bottom = (height / 2);
            //itemimage.Left = (width / 2);
            //itemimage.Right = (width / 2);
        }

        public void SetItemLevel(int level)
        {
            if(level <= 0)
                itembox.Bitmap = new Bitmap("media/gui/items/no item box.png");
            else if(level == 1)
                itembox.Bitmap = new Bitmap("media/gui/items/lv1 box.png");
            else if(level == 2)
                itembox.Bitmap = new Bitmap("media/gui/items/lv2 box.png");
            else if(level >= 3)
                itembox.Bitmap = new Bitmap("media/gui/items/lv3 box.png");
        }

        public void SetItemImage(string name)
        {
            string path = "media/gui/items/" + name + ".png";
            itemimage.Bitmap = new Bitmap(path);
        }
    }
}
