

local particle = createParticleSystem("twhdust", "dust_for_twh")
particle.Emitting = false

local node = getRootSceneNode():CreateChildSceneNode()
node.Position = vector(0, 0, 100)
node:AttachObject(particle)


local function onKeyPress(ke)
	if keyCode(ke.key) == "KC_SPACE" then
		particle.Emitting = true
	end
end

local function onKeyRelease(ke)
	if keyCode(ke.key) == "KC_SPACE" then
		particle.Emitting = false
	end
end

local inputMain = getInputMain()
inputMain.OnKeyboardPress_Anything:Add(onKeyPress)
inputMain.OnKeyboardRelease_Anything:Add(onKeyRelease)
