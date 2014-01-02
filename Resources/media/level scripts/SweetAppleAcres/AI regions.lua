--[[
-- if it's a kart with a computer player, this tells it the next waypoint to go to
local function goTo(currentRegion, nextRegion, body, info)
	local k = getKartFromBody(body)
	if k ~= nil then
		if k.Player.IsComputerControlled then
			k.Player:CalculateNewWaypoint(currentRegion, nextRegion, info)
		end
	end
end

--
	 how to make trigger regions:
	 1) make a box in 3ds max, 2x2x2
	 2) align to world, so when you set the position gizmo, "forward" is -X
	 3) place them all around the level
	 4) use scaling to change their size
	 5) name them "TriggerRegionXX_YY" where XX is a unique ID, and YY is the ID of the next region
	 6) export them all into a .scene file BY THEMSELVES
	 7) use the SceneToTriggerRegion tool on the .scene file
	 8) give the .tr file the same name as the level and put it in /media/worlds/
]]

local region0 = getTriggerRegion("AITriggerRegion0")

local function setInitialRegion(kart, id)
	if kart.Player.IsComputerControlled then
		kart.Player:CalculateNewWaypoint(nil, region0, nil)
	end
end

forEachKart(setInitialRegion)
