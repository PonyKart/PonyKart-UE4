
--[[local function cutlass(kart, sound)
	sound.PlaybackSpeed = math.min((math.abs(kart.VehicleSpeed) * 0.0075) + 0.7, 3)
end


local function addEngineFunction(kart, id)
	-- the 0 means we want the function to run when the 0th (i.e. the first) sound component is updated
	--if kart.Name == "TwiCutlass" then
		addSoundFrameFunction(kart, 0, cutlass)
	end
end

forEachKart(addEngineFunction)]]--
