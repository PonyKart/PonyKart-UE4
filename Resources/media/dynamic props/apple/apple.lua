-- I figure it's better to split this into two functions. Mostly because when
-- we want to spawn a whole ton of apples, otherwise we'd have to be creating
-- and destroying a whole ton of lthings. With two functions, when we have
-- things that want to spawn apples, they don't have to jump between c# and lua
-- so often, speeding things up.

function apple(lthing)
	spawnApple(lthing.SpawnPosition)
	lthing:Dispose()
end


-- we have four colors of apples: two meshes, two materials, and two textures.
-- one mesh uses the top half of the texture, the other uses the bottom half.
-- one material is for the red texture, the other is for the green one.

-- This is basically a randomiser that picks one mesh and one material
-- out of the options available and creates one of the 4 colors of apple.

function spawnApple(position)
	local num = math.random(4)
	local name

	if num == 1 then
		name = "DarkRedApple"
	elseif num == 2 then
		name = "BrightRedApple"
	elseif num == 3 then
		name = "DarkGreenApple"
	else
		name = "BrightGreenApple"
	end
	
	spawn(name, position)
end

-- lthing - the thing you want to spawn relative to
-- offset - the offset
function spawnApple(lthing, offset)
	local num = math.random(4)
	local name

	if num == 1 then
		name = "DarkRedApple"
	elseif num == 2 then
		name = "BrightRedApple"
	elseif num == 3 then
		name = "DarkGreenApple"
	else
		name = "BrightGreenApple"
	end

	relativeSpawn(name, lthing, offset)
end

local appleRadius = 0.14

-- spawns a bunch of apples vertically
function spawnApples(numberOfApples, position)
	local a
	
	for a = 0, numberOfApples do
		spawnApple(addVectors(position, vector(0, a * appleRadius, 0)))
	end
end

-- we spawn stuff from the origin of the lthing, plus a little offset if we want.
-- This will use positions relative to the lthing, though without taking rotation into count.
-- We could do that if we REALLY wanted to, but we don't, so it's all good
-- Width is X, height is Y, length is Z.

function spawnMoreApples(lthing, offsetX, offsetY, offsetZ, width, height, length)
	local offset = vector(offsetX, offsetY, offsetZ)
	local x, y, z, mx, my, mz
	
	for x = 0, width - 1 do
		-- multiply these by the radius of the apple
		mx = x * appleRadius
		
		for y = 0, height - 1 do
			my = y * appleRadius
			
			for z = 0, length - 1 do
				mz = z * appleRadius
				
				spawnApple(lthing, vector(offsetX + mx, offsetY + my, offsetZ + mz))
			end
		end
	end
end