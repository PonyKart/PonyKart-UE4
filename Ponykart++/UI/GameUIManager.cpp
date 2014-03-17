#include "pch.h"
#include "UI/GameUIManager.h"

using namespace Ponykart::UI;

void GameUIManager::setItemLevel(int level)
{
	/// TODO: Implement setItemLevel once we've got the CEGUI design part done
	/// > IMPLYING WINDOWS FORMS
	//if (level <= 0)
	//	itembox.Bitmap = new Bitmap("media/gui/items/no item box.png");
	//else if (level == 1)
	//	itembox.Bitmap = new Bitmap("media/gui/items/lv1 box.png");
	//else if (level == 2)
	//	itembox.Bitmap = new Bitmap("media/gui/items/lv2 box.png");
	//else if (level >= 3)
	//	itembox.Bitmap = new Bitmap("media/gui/items/lv3 box.png");
	throw std::string("[CRITICAL][NOTIMPLEMENTED][DARKSNYDERPLSFIX]GameUIManager::setItemLevel: Not implemented.");
}

void GameUIManager::setItemImage(const std::string& name)
{
	/// TODO: Implement setItemImage once we've got the CEGUI design part done

	//UIMain uiMain = LKernel.GetG<UIMain>();

	////This mess gets the height and width of the window for centering UI entities.
	//uint uheight, uwidth, colorDepth;
	//int height, width;
	//RenderWindow window = LKernel.GetG<RenderWindow>();
	//window.GetMetrics(out uwidth, out uheight, out colorDepth);
	//width = (int)uwidth;
	//height = (int)uheight;

	//inGameUI = uiMain.GetGUI("ingame gui");
	//itembox = inGameUI.GetControl<PictureBox>("itembox");
	////itembox.Top = (height / 2);
	////itembox.Bottom = (height / 2);
	////itembox.Left = (width / 2);
	////itembox.Right = (width / 2);

	//itemimage = inGameUI.GetControl<PictureBox>("itemimage");
	////itemimage.Top = (height / 2);
	////itemimage.Bottom = (height / 2);
	////itemimage.Left = (width / 2);
	////itemimage.Right = (width / 2);
	throw std::string("[CRITICAL][NOTIMPLEMENTED][DARKSNYDERPLSFIX]GameUIManager::setItemImage: Not implemented.");
}
