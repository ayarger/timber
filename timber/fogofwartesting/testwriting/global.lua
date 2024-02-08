global = {}

-- 
global.game_objects = {}
--TODO: Keep track of which coroutines on which game_objects
global.awaiting_coroutines = {}

global.delta_time = 0

function WaitForSeconds(secs)
	local time = secs
	while time>0 do
		coroutine.yield("N")
		time = time - global.delta_time
	end
end

function SetDestination(obj, xdelta)
	coroutine.yield(obj.object_name..":M"..xdelta)
end

-- For anything relating to process
function global:tick(delta)
	global.delta_time = delta
	local c_list = global.awaiting_coroutines
	global.awaiting_coroutines = {}
	global:advance_coroutines(c_list)
	--global:receive("process")
end

function global:receive(message)
	-- For now, just iterate through all objects looking for ready functions
	print(message)
	local to_remove = {}
	local new_coroutines = {}
	for i=1,#global.game_objects do
		if global.game_objects[i]==nil then
			table.insert(to_remove,i)
		else
			if global.game_objects[i].ready~=nil then --REPLACE READY LATER
				table.insert(new_coroutines,{global.game_objects[i],coroutine.create(global.game_objects[i].ready)})
			end
		end
	end
	if #to_remove > 0 then
		for i=1,#to_remove do
			--TODO, also clear coroutines running on object
			table.remove(global.game_objects,to_remove[i]-i+1)
		end
	end
	global:advance_coroutines(new_coroutines)
end

function global:advance_coroutines(coroutine_list)
	--run coroutines until completion or "N"
	while #coroutine_list>0 do
		local commands = ''
		local to_remove = {}
		for i=1,#coroutine_list do
			local code, res = coroutine.resume(coroutine_list[i][2],coroutine_list[i][1])
			if res then
				if res=="N" then
					table.insert(global.awaiting_coroutines,coroutine_list[i])
					table.insert(to_remove, i)
				else
					commands = commands..res.."\n"
				end
			else
				table.insert(to_remove, i)
			end
		end
		for i=1,#to_remove do
			-- Clear coroutines that just finished
			table.remove(coroutine_list,to_remove[i]-i+1)
		end
		-- to be submitted back to C#
		coroutine.yield(commands)
	end
end

function global:register_object(object, name)
	table.insert(global.game_objects,object)
	object.object_name = name
end
