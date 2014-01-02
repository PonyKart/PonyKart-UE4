-- load up our types

function barn(lthing)
	local unit_y = vector(0, 1, 0)

	-- these two need to be rotated the other way around
	local topleft = spawn("Barn_Top_Doors", addVectors(lthing.SpawnPosition, vector(-0.59, 1.48, 3.36)))
	setBodyOrientation(topleft.Body, quaternion(0, 1, 0, 0))
	local bottomleft = spawn("Barn_Bottom_Doors", addVectors(lthing.SpawnPosition, vector(-0.59, 0.38, 3.36)))
	setBodyOrientation(bottomleft.Body, quaternion(0, 1, 0, 0))
	
	-- these ones are fine though
	local topright = spawn("Barn_Top_Doors", addVectors(lthing.SpawnPosition, vector(0.9672, 1.48, 3.36)))
	local bottomright = spawn("Barn_Bottom_Doors", addVectors(lthing.SpawnPosition, vector(0.9692, 0.38, 3.36)))
	
	
	-- make hinges
	local TLhinge = hingeConstraint(topleft.Body, lthing.Body, vector(0.75, 0, 0), vector(-1.36, 1.48, 3.3858), unit_y, unit_y)
	local BLhinge = hingeConstraint(bottomleft.Body, lthing.Body, vector(0.75, 0, 0), vector(-1.36, 0.38, 3.3858), unit_y, unit_y)
	
	local TRhinge = hingeConstraint(topright.Body, lthing.Body, vector(0.75, 0, 0), vector(1.737, 1.48, 3.3858), unit_y, unit_y)
	local BRhinge = hingeConstraint(bottomright.Body, lthing.Body, vector(0.75, 0, 0), vector(1.737, 0.38, 3.3858), unit_y, unit_y)
	
	
	-- remember that methods use : and not .!
	TLhinge:SetLimit(1.57, 3.14)
	BLhinge:SetLimit(1.57, 3.14)
	TRhinge:SetLimit(0, 1.57)
	BRhinge:SetLimit(0, 1.57)
	
	-- and then add our constraints to the world
	addConstraint(TLhinge, false)
	addConstraint(BLhinge, false)
	addConstraint(TRhinge, false)
	addConstraint(BRhinge, false)
	
	-- make them stop moving so they're always completely closed
	local vecZero = vector(0, 0, 0)
	
	topleft.Body.AngularVelocity = vecZero
	topleft.Body.LinearVelocity = vecZero
	bottomleft.Body.AngularVelocity = vecZero
	bottomleft.Body.LinearVelocity = vecZero
	topright.Body.AngularVelocity = vecZero
	bottomleft.Body.LinearVelocity = vecZero
	bottomright.Body.AngularVelocity = vecZero
	bottomleft.Body.LinearVelocity = vecZero
end