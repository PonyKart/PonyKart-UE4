-- we don't need to spawn a whole new apple like with apple.thing and apple.lua,
-- so all this does is take the flower barrel we already created and change its
-- material to one of the random colors.


function flowerbarrel(lthing)
	local num = math.random(3)
	local name

	if num == 1 then
		name = "FlowerBarrelGreen"
	elseif num == 2 then
		name = "FlowerBarrelRed"
	else
		name = "FlowerBarrelYellow"
	end
	
	spawn(name, lthing.SpawnPosition)
	lthing:Dispose()
end