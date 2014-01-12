function flat(level)
	spawn("BgStandsWithPhysics", vector(6, 0, 0.66))
	
	lyra = spawnBgPony("LyraSitting", vector(7.1, 0.3, 0.5))
	bonbon = spawnBgPony("BonBon", vector(6.6, 0.3, 0.52))
	daisy = spawnBgPony("Daisy", vector(3.3, -0.18, 0.6))
	sparkler = spawnBgPony("Sparkler", vector(8.6, -0.18, 0.7))
	linky = spawnBgPony("Linky", vector(5.6, 1.2, -0.8))
	carrot = spawnBgPony("CarrotTop", vector(5, 0.6, 0))
	rose = spawnBgPony("Rose", vector(4, 0, 0.9))
	diamond = spawnBgPony("Diamond", vector(9.1, -0.18, 0.9))
	lily = spawnBgPony("Lily", vector(2.6, -0.18, 0.7))
	cloudkicker = spawnBgPony("CloudKickerF", vector(2, 0.4, 0))
	flora = spawnBgPony("Flora", vector(1.4, -0.18, -0.7))
	berry = spawnBgPony("BerryPunch", vector(6, 0.6, 0))
	cherry = spawnBgPony("Cherry", vector(9.8, -0.18, 1))
	sun = spawnBgPony("SunF", vector(6.4, 1.6, -0.6))
	dizzy = spawnBgPony("DizzyF", vector(4.8, 1.5, -0.7))
	
	bonbon:Sit()
	daisy:Stand()
	daisy:Cheer()
	sparkler:Stand()
	sparkler:Cheer()
	linky:ChangeAnimation("Cheer2")
	carrot:Sit()
	carrot:Cheer()
	rose:Sit()
	diamond:Stand()
	lily:Stand()
	cloudkicker:Fly()
	flora:Stand()
	berry:Sit()
	cherry:Stand()
	sun:Fly()
	dizzy:Fly()
	
	local x, z
	local id = 0
	local pone
	
	for x = -29, -20 do
		for z = -29, -20 do
			pone = spawnBgPonyByNum(id, vector(x, -0.18, z))
			pone:Stand()
			
			id = id + 1
		end
	end
end
