local threshold = 3

local region0, region1, region2, region3, region3a, region4, region5, region6, region7, region8, region9, region10, region11

-- if it's a kart with a computer player, this tells it the next waypoint to go to
local function goTo(currentRegion, nextRegion, body, info)
	local k = getKartFromBody(body)
	if k ~= nil then
		if k.Player.IsComputerControlled then
			k.Player:CalculateNewWaypoint(currentRegion, nextRegion, info)
		end
	end
end

-- due to how trigger regions work, this function is only ever ran when the entering body is in the kart collision group
local function tr0(region, body, flags, info)  goTo(region, region1, body, info) end
local function tr1(region, body, flags, info)  goTo(region, region2, body, info) end
local function tr2(region, body, flags, info)  goTo(region, region3, body, info) end
local function tr3(region, body, flags, info)  goTo(region, region4, body, info) end
local function tr3a(region, body, flags, info) goTo(region, region4, body, info) end
local function tr4(region, body, flags, info)  goTo(region, region5, body, info) end
local function tr5(region, body, flags, info)  goTo(region, region6, body, info) end
local function tr6(region, body, flags, info)  goTo(region, region7, body, info) end
local function tr7(region, body, flags, info)  goTo(region, region8, body, info) end
local function tr8(region, body, flags, info)  goTo(region, region9, body, info) end
local function tr9(region, body, flags, info)  goTo(region, region10, body, info) end
local function tr10(region, body, flags, info) goTo(region, region11, body, info) end
local function tr11(region, body, flags, info) goTo(region, region0, body, info) end

function TestAI(level)
	local iden = quat(0, 0, 0, 1)
	
	-- need to divide the dimensions you get from 3ds max by two if you start with a 1x1x1 box...
	-- use a 2x2x2 box in the future?
	-- oh and remember to make sure you do "align to world" to the first one before you start, otherwise you'll have to mirror them across the X axis (negate the Y and Z parts of the quaternion)
	region0 = createBoxTriggerRegion("TriggerRegion0", tr0, vec(13.4621, 5.496, threshold), vec(0.816162, 5.45602, 3.49136), iden)
	region1 = createBoxTriggerRegion("TriggerRegion1", tr1, vec(13.4621, 5.496, threshold), vec(0.816162, 5.45602, 30.5998), iden)
	region2 = createBoxTriggerRegion("TriggerRegion2", tr2, vec(12.9725, 5.496, threshold), vec(-3.85958, 5.45602, 40.0656), quat(0, 0.382683, -1.67276E-08, 0.92388))
	region3 = createBoxTriggerRegion("TriggerRegion3", tr3, vec(9.24426, 5.496, threshold), vec(-21.1258, 5.45602, 48.8482), quat(0, 0.707107, -3.09086E-08, 0.707107))
	region3a = createBoxTriggerRegion("TriggerRegion3a", tr3a, vec(13.4666, 5.496, 4.49398), vec(-16.88706, 5.45602, 62.6932), iden)
	region4 = createBoxTriggerRegion("TriggerRegion4", tr4, vec(9.96422, 5.496, threshold), vec(-34.923, 5.45602, 39.826), quat(0, 0.843391, -3.68658E-08, 0.5373))
	region5 = createBoxTriggerRegion("TriggerRegion5", tr5, vec(12.7288, 5.496, threshold), vec(-43.7222, 5.45602, 35.3988), quat(0, -0.923879, 4.03841E-08, -0.382684))
	region6 = createBoxTriggerRegion("TriggerRegion6", tr6, vec(10.1005, 5.496, threshold), vec(-48.3024, 5.45602, 25.9978), quat(0, -0.965926, 4.2222E-08, -0.258819))
	region7 = createBoxTriggerRegion("TriggerRegion7", tr7, vec(13.3687, 5.496, threshold), vec(-52.602, 5.45602, 20.1364), quat(0, -1, 4.37114E-08, -1.94707E-07))
	region8 = createBoxTriggerRegion("TriggerRegion8", tr8, vec(8.98008, 5.496, threshold), vec(-48.113, 5.45602, 3.16582), quat(0, -1, 4.37114E-08, -1.94707E-07))
	region9 = createBoxTriggerRegion("TriggerRegion9", tr9, vec(8.98008, 5.496, threshold), vec(-39.248, 5.45602, -5.86082), quat(0, -0.707107, 3.09086E-08, 0.707107))
	region10 = createBoxTriggerRegion("TriggerRegion10", tr10, vec(13.9607, 5.496, threshold), vec(-30.2168, 5.45602, -10.454), quat(2.51215E-15, -0.707107, 3.09086E-08, 0.707107))
	region11 = createBoxTriggerRegion("TriggerRegion11", tr11, vec(12.7355, 5.496, threshold), vec(-3.47664, 5.45602, -5.86306), quat(0, -0.382684, 1.67276E-08, 0.923879))
	
	-- decrease the kart speed so we can watch them better
	function decreaseKartSpeed(kart, id)
		kart.MaxSpeed = kart.MaxSpeed * 0.5
	end
	
	-- give all of the computer-controlled karts an initial region to go to
	function setInitialRegion(kart, id)
		if kart.Player.IsComputerControlled then
			kart.Player:CalculateNewWaypoint(nil, region0, nil)
		end
	end

	forEachKart(decreaseKartSpeed)
	forEachKart(setInitialRegion)
end