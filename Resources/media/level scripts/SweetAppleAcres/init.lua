local function onLap(kart, lapCount)
	create2DSound("lap.wav", false)
end

local function onFinish(kart)
	kart.Player.IsControlEnabled = false
	kart.Acceleration = 0
end

local function onPlayerFinish(kart)
	create2DSound("Crowd Two.mp3", false)
end

local function decreaseKartSpeed(kart, id)
 	kart.MaxSpeed = kart.MaxSpeed / 1.1
end

function SweetAppleAcres(level)
 	create2DSound("SAA Ambience.ogg", true)
 	
 	create3DSound("Mountain Stream1.ogg", vector(56.0908, 0, -42.0108), true)
 	create3DSound("Mountain Stream1.ogg", vector(-20.1831, 8.16656, -138.733), true)
 	
 	forEachKart(decreaseKartSpeed)
	
	hookFunctionToLapEvent(onLap)
	hookFunctionToFinishEvent(onFinish)
	hookFunctionToPlayerFinishEvent(onPlayerFinish)
end
