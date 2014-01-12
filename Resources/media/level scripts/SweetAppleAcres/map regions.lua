if getOption("ModelDetail") == "Medium" or getOption("ModelDetail") == "Low" then

	local show
	local hide
	local player = playerKart()
	local currentRegion

	-- only run this part if our model detail level is set to "Medium"
	-- a high model detail setting doesn't want imposters
	if getOption("ModelDetail") == "Medium" then
	
		-- we have static, instanced, and imposters to deal with
		show = function(regionName)
			setInstancedGeometryVisibility(regionName, true)
			setStaticGeometryVisibility(regionName, true)
			setRegionNodeVisibility(regionName, true)
			
			setImposterVisibility(regionName, false)
		end

		hide = function(regionName)
			setInstancedGeometryVisibility(regionName, false)
			setStaticGeometryVisibility(regionName, false)
			setRegionNodeVisibility(regionName, false)
			
			setImposterVisibility(regionName, true)
		end
	else
		-- no imposters this time
		-- no static or instanced geometry
		
		show = function(regionName)
			setRegionNodeVisibility(regionName, true)
		end
		
		hide = function(regionName)
			setRegionNodeVisibility(regionName, false)
		end
	end

	-- returns whether the given shape is the player's kart, since we only want the player to be affecting geometry
	local function checkPlayer(shape)
		return player.Body == shape
	end

	-------------------------------------------

	-- show buckets when entering, show bridge when leaving
	local function startTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Start" then
			currentRegion = "Start"
		
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				show("Buckets")
				hide("Ramp")
				hide("River")
				hide("Treehouse")
				hide("Bridge")
				show("Barn")
			end
		end
	end

	-- show ramp when entering, hide barn and bridge when leaving
	local function cliffTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Cliff" then
			currentRegion = "Cliff"
		
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				show("Buckets")
				show("Ramp")
				hide("River")
				hide("Treehouse")
				show("Bridge")
				show("Barn")
			end
		end
	end

	-- show river when entering, hide start when leaving
	local function bucketsTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Buckets" then
			currentRegion = "Buckets"
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				show("Buckets")
				show("Ramp")
				show("River")
				hide("Treehouse")
				hide("Bridge")
				hide("Barn")
			end
		end
	end

	-- show treehouse when entering, hide cliff when leaving
	local function rampTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Ramp" then
			currentRegion = "Ramp"
		
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				show("Buckets")
				show("Ramp")
				show("River")
				show("Treehouse")
				hide("Bridge")
				hide("Barn")
			end
		end
	end

	-- hide buckets when entering, hide ramp when leaving
	local function riverTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "River" then
			currentRegion = "River"
			
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				hide("Buckets")
				show("Ramp")
				show("River")
				show("Treehouse")
				hide("Bridge")
				hide("Barn")
			end
		end
	end

	-- show bridge when entering, hide river when leaving
	local function treehouseTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Treehouse" then
			currentRegion = "Treehouse"
			
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				hide("Buckets")
				hide("Ramp")
				show("River")
				show("Treehouse")
				show("Bridge")
				hide("Barn")
			end
		end
	end

	-- show barn when entering, hide treehouse when leaving
	local function bridgeTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Bridge" then
			currentRegion = "Bridge"
		
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				hide("Buckets")
				hide("Ramp")
				hide("River")
				show("Treehouse")
				show("Bridge")
				show("Barn")
			end
		end
	end

	-- show start and cliff when entering, hide bridge when leaving
	local function barnTriggerRegion(region, shape, flags, info)
		if checkPlayer(shape) and currentRegion ~= "Barn" then
			currentRegion = "Barn"
			
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				hide("Buckets")
				hide("Ramp")
				hide("River")
				hide("Treehouse")
				show("Bridge")
				show("Barn")
			end
		end
	end

	-- none of our regions are rotated, so they all get the identity matrix
	local iden = quat(0, 0, 0, 1)

	-- create our regions!
	createBoxTriggerRegion("startTriggerRegion", startTriggerRegion, vec(25.6931, 11.0725, 12.1476), vec(-36.0742, 8.6546, 63.8988), iden)
	createBoxTriggerRegion("cliffTriggerRegion", cliffTriggerRegion, vec(13.0378, 14.1297, 42.2379), vec(-63.6846, 14.1296, 15.0298), iden)
	createBoxTriggerRegion("bucketsTriggerRegion", bucketsTriggerRegion, vec(24.8806, 12.611, 40.1298), vec(-67.2862, 10.405, -67.918), iden)
	createBoxTriggerRegion("rampTriggerRegion", rampTriggerRegion, vec(33.7646, 13.3166, 31.1008), vec(-48.4896, 13.3166, -139.3554), iden)
	createBoxTriggerRegion("riverTriggerRegion", riverTriggerRegion, vec(32.6753, 13.7902, 31.5204), vec(18.6218, 11.2186, -138.86), iden)
	createBoxTriggerRegion("treehouseTriggerRegion", treehouseTriggerRegion, vec(20.188, 14.479, 47.474), vec(69.792, 14.479, -90.076), iden)
	createBoxTriggerRegion("bridgeTriggerRegion", bridgeTriggerRegion, vec(35.644, 10.817, 31.289), vec(29.866, 8.012, -10.667), iden)
	createBoxTriggerRegion("barnTriggerRegion", barnTriggerRegion, vec(30.7805, 8.9325, 30.7805), vec(20.333, -6.712, 51.463), iden)

	-- hide some things when we start up
	hide("Bridge")
	hide("Treehouse")
	hide("River")
	hide("Ramp")
	hide("Buckets")
	
	-- unhide all of the trees
	--[[if getOption("ModelDetail") == "Low" then
		setImposterVisibility("Start", true)
		setImposterVisibility("Cliff", true)
		setImposterVisibility("Buckets", true)
		setImposterVisibility("Ramp", true)
		setImposterVisibility("River", true)
		setImposterVisibility("Treehouse", true)
		setImposterVisibility("Bridge", true)
		setImposterVisibility("Barn", true)
	end]]
end