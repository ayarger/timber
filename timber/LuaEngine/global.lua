print("Start Init")

global = {}

global.lunajson = require 'lunajson'
local jsonstr = '{"Hello":["lunajson",1.5]}'
local t = global.lunajson.decode(jsonstr)
print(t.Hello[2]) -- prints 1.5
print(global.lunajson.encode(t)) -- prints {"Hello":["lunajson",1.5]}
-- 
global.game_objects = {}
--TODO: Keep track of which coroutines on which game_objects
global.awaiting_coroutines = {}

global.delta_time = 0


global.keywords = {
	x=true,
	z=true,
}

global.unique_number_gen = 0

function GetUniqueId()
	global.unique_number_gen = global.unique_number_gen + 1
	-- just in case
	if global.unique_number_gen > 200000000 then
		global.unique_number_gen = 0
	end
	return "ID"..tostring(global.unique_number_gen)
end

--- Delays execution for secs seconds.
-- If the event is called again while it is waiting, there will be two instances of the event running.
-- @param secs Time to wait in seconds, number.
function WaitForSeconds(secs)
	local time = secs
	while time>0 do
		coroutine.yield("N")
		time = time - global.delta_time
	end
end



-- For anything relating to process
function global:tick(delta)
	global.delta_time = delta
	local c_list = global.awaiting_coroutines
	global.awaiting_coroutines = {}
	global:advance_coroutines(c_list)
	global:receive("process")
end

function global:receive(message)
	-- For now, just iterate through all objects looking for ready functions

	local to_remove = {}
	local new_coroutines = {}
	for i=1,#global.game_objects do
		if global.game_objects[i]==nil then
			table.insert(to_remove,i)
		else
			-- For every game object, find all functions that have the same name as the message
			-- and run them as coroutines.
			-- EG: if message is "Ready", run all ready functions.
			if type(global.game_objects[i][message])=="function" then
				-- FORMAT: Game object, new coroutine, unique coroutine ID identifier
				table.insert(new_coroutines,
					{global.game_objects[i],
					coroutine.create(global.game_objects[i][message]), 
					GetUniqueId()})
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
	local data = {}
	while #coroutine_list>0 do
		local commands = {}
		local to_remove = {}
		for i=1,#coroutine_list do
			-- If there was data returned from C# due to the command, pass it back to the coroutine
			local return_data = data[coroutine_list[i][3]]==nil and 0 or data[coroutine_list[i][3]]
			-- Run the coroutine
			local code, res = coroutine.resume(coroutine_list[i][2],coroutine_list[i][1], return_data)
			if res then
				if res=="N" then
					table.insert(global.awaiting_coroutines,coroutine_list[i])
					table.insert(to_remove, i)
				else
					res.id = coroutine_list[i][3]
					commands[i]=res
				end
			else
				table.insert(to_remove, i)
			end
		end
		for i=1,#to_remove do
			-- Clear coroutines that just finished
			table.remove(coroutine_list,to_remove[i]-i+1)
		end
		-- to be submitted back to C#, last return element is data
		local coroutine_data = {coroutine.yield(global.lunajson.encode(commands))}
		-- data returned from C#
		data = coroutine_data[#coroutine_data]
		
	end
end

function global:register_object(object, name)
	table.insert(global.game_objects,object)
	object.object_name = name
end
