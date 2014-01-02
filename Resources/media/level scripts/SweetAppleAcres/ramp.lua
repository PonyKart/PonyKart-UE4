
local function rampIncSpeed(r, body, flags, i)
	local kart = getKartFromBody(body)
	if kart ~= nil and isEnterFlag(flags) then
		-- for regular speed
		kart.MaxSpeed = kart.DefaultMaxSpeed * 3.5
		
		-- for SAA's slow speed
		--kart.MaxSpeed = 84 -- twi's * 1.2
	end
end

local function rampDecSpeed(r, body, flags, i)
	local kart = getKartFromBody(body)
	if kart ~= nil and isEnterFlag(flags) then
		-- for regular speed
		--kart.MaxSpeed = kart.DefaultMaxSpeed
		
		-- for SAA's slow speed
		kart.MaxSpeed = kart.DefaultMaxSpeed
	end
end

hookFunctionToTriggerRegion("AITriggerRegion25", rampIncSpeed)

local rampDecTR = createBoxTriggerRegion("RampSpeedDecTriggerRegion", rampDecSpeed, vec(2, 9.08414, 14.689), vec(-0.63934, 15.2764, -138.911), quat(-1.38779e-017, 0.999994, -4.37111e-008, -0.00337965))

-- the trigger region over the river, to use when you fall into it
--[[
		<node name="RiverTriggerRegion">
            <position x="-12.4589" y="2.30107" z="-138.952" />
            <scale x="10.9985" y="2.71301" z="32.6104" />
            <rotation qx="0.000143133" qy="0.999202" qz="0.0397857" qw="-0.00359472" />
            <entity name="RiverTriggerRegion" castShadows="true" receiveShadows="true" meshFile="TriggerRegion000.mesh">
                <subentities>
                    <subentity index="0" materialName="NoMaterial" />
                </subentities>
            </entity>
        </node>
		]]