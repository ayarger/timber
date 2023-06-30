local GameManager = {
	extends = "Node",
}

Engine = {}
Engine.print = function(message)
	print("Engine : " .. message)
end



return GameManager
