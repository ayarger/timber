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


function SetDestination(obj, xdelta, zdelta)
	coroutine.yield(
		{obj=obj.object_name,
		type="M",
		x=xdelta,
		z=zdelta}
		)
end

function Print(str)
	coroutine.yield(
		{obj="global",
		type="P",
		param=str})
end

function Hurt(obj, damage)
	coroutine.yield(
		{obj=obj.object_name,
		type="H",
		damage=damage}
		)
end

function Kill(obj, source)
	coroutine.yield(
		{obj=obj.object_name,
		type="K",
		killSource=source.object_name}
		)
end

--Just float currently
function GetValue(obj,key)
	local coroutine_data = {coroutine.yield(
		{obj=obj,
		type="R",
		param=key})}
	return coroutine_data[#coroutine_data]
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
			if type(global.game_objects[i][message])=="function" then
				table.insert(new_coroutines,{global.game_objects[i],coroutine.create(global.game_objects[i][message])})
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
			local return_data = data[coroutine_list[i][1].object_name]==nil and 0 or data[coroutine_list[i][1].object_name]
			-- Run the coroutine
			local code, res = coroutine.resume(coroutine_list[i][2],coroutine_list[i][1], return_data)
			if res then
				if res=="N" then
					table.insert(global.awaiting_coroutines,coroutine_list[i])
					table.insert(to_remove, i)
				else
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
