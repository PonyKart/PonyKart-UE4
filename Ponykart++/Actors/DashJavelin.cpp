#include "Actors/DashJavelin.h"
#include "Actors/Components/ModelComponent.h"
#include "Actors/Components/RibbonComponent.h"
#include "Actors/Components/SoundComponent.h"
#include "Core/Animation/AnimationBlender.h"
#include "Core/Animation/AnimationManager.h"
#include "Kernel/LKernel.h"
#include "Sound/SoundMain.h"

using namespace Ogre;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;
using namespace Ponykart::Sound;

DashJavelin::DashJavelin(PonykartParsers::ThingBlock* block, PonykartParsers::ThingDefinition* def) : Kart(block, def), jetOpening(0)
{
	ModelComponent* chassis = modelComponents[0];
	// first get rid of the existing animation blender it creates automatically
	LKernel::getG<AnimationManager>()->remove(chassis->animationBlender);
	delete chassis->animationBlender;
	chassis->animationBlender = nullptr;

	Entity* chassisEnt = chassis->getEntity();
	// get our two animation states
	jetMax = chassisEnt->getAnimationState("JetMax");
	jetMax->setEnabled(true);
	jetMax->setWeight(0);
	jetMin = chassisEnt->getAnimationState("JetMin");
	jetMin->setEnabled(true);
	jetMin->setWeight(1);

	chassisEnt->getSkeleton()->setBlendMode(SkeletonAnimationBlendMode::ANIMBLEND_AVERAGE); // we want the two animations to blend together, not add to each other

	jetRibbon = ribbonComponents[0]->getRibbon();

	// sounds
	soundMain = LKernel::getG<SoundMain>();

	idleSound = soundComponents[0]->getSound();
	fullSound = soundComponents[1]->getSound();
	revDownSound = soundMain->getSource("RD_Kart_Rev_Down.ogg");
	revUpSound = soundMain->getSource("RD_Kart_Rev_Up.ogg");

	// convert from linear velocity to KPH
	topSpeedKmHour = defaultMaxSpeed * 3.6f;
	idleState = true;

	LKernel::gRoot->addFrameListener(frameStarted);
}
