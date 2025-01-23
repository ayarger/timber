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
	team=true
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


-- For anything relating to process
function global:single_receive(actor, message)
	local new_coroutines = {}
	
	if type(actor[message])=="function" then
		-- FORMAT: Game object, new coroutine, unique coroutine ID identifier
		local new_coro = {actor,
			coroutine.create(actor[message]), 
			GetUniqueId()}
		new_coroutines[new_coro] = true
	end
	global:advance_coroutines(new_coroutines)
end
function global:receive_specified(actors, message)
	local new_coroutines = {}
	for _, obj in pairs(actors) do
		if global.game_objects[obj.object_name] then
			-- For every game object, find all functions that have the same name as the message
			-- and run them as coroutines.
			-- EG: if message is "Ready", run all ready functions.
			if type(obj[message])=="function" then
				-- FORMAT: Game object, new coroutine, unique coroutine ID identifier
				local new_coro = {obj,
					coroutine.create(obj[message]), 
					GetUniqueId()}
				new_coroutines[new_coro] = true
			end
		end
	end
	global:advance_coroutines(new_coroutines)
end

function global:receive(message)
	-- Iterate through all functions

	local to_remove = {}
	local new_coroutines = {}
	for name, obj in pairs(global.game_objects) do
		if obj then
			-- For every game object, find all functions that have the same name as the message
			-- and run them as coroutines.
			-- EG: if message is "Ready", run all ready functions.
			if type(obj[message])=="function" then
				-- FORMAT: Game object, new coroutine, unique coroutine ID identifier
				local new_coro = {obj,
					coroutine.create(obj[message]), 
					GetUniqueId()}
				new_coroutines[new_coro] = true
			end
		end
	end
	--if #to_remove > 0 then
	--	for i=1,#to_remove do
	--		--TODO, also clear coroutines running on object
	--		table.remove(global.game_objects,to_remove[i]-i+1)
	--	end
	--end
	--RawPrint(new_coroutines)
	global:advance_coroutines(new_coroutines)
end

function global:advance_coroutines(coroutine_list)
	--run coroutines until completion or "N"
	local data = {}
	local amount = 1
	while amount>0 do
		local commands = {}
		local to_remove = {}
		amount = 0
		for coro, real in pairs(coroutine_list) do
			if real then
				-- If there was data returned from C# due to the command, pass it back to the coroutine
				--RawPrint(coro[3])
				local return_data = data[coro[3]]==nil and 0 or data[coro[3]]
				-- Run the coroutine
				local code, res = coroutine.resume(coro[2],coro[1], return_data)
				if res then
					if res=="N" then
						global.awaiting_coroutines[coro] = true
						coroutine_list[coro] = false
					else
						res.id = coro[3]
						amount = amount + 1
						commands[amount]=res
					end
				else
					coroutine_list[coro] = false
				end
			end
		end
		-- to be submitted back to C#, last return element is data
		--RawPrint(commands)
		local coroutine_data = {coroutine.yield(global.lunajson.encode(commands))}
		-- data returned from C#
		data = coroutine_data[#coroutine_data]
		
	end
end

function global:register_object(object, name)
	global.game_objects[name] = object
	object.object_name = name
end
