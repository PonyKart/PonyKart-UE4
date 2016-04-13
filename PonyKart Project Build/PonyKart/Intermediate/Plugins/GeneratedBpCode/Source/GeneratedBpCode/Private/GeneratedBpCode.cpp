// Fill out your copyright notice in the Description page of Project Settings.

#include "GeneratedBpCode.h"
#include "ModuleInterface.h"
#include "ModuleManager.h"

class FGeneratedBpCodeModule : public IModuleInterface
{	
	virtual void StartupModule() override
	{
			
	}
};

IMPLEMENT_MODULE( FGeneratedBpCodeModule, GeneratedBpCode );
