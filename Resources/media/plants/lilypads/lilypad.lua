-- change the lily pad's material to one of the four
-- they're just called Lilypad1, Lilypad2, etc, so giving them a new name is very easy

function lilypad(lthing)
	setOneMaterial(lthing, 0, "Lilypad" .. math.random(4))
end