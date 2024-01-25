otherscript = {}

local mt = {}

mt.__index = function(self, key) 
  if key=="x" then
	return "4"
  else
	return rawget(self, key)
  end
end

mt.__newindex = function(self, key, value)
  rawset(self, key, value)
end

setmetatable(otherscript, mt)

function WaitForSeconds(secs)
	coroutine.yield("W"..secs)
end


-- UNSEEN CODE --

function otherscript:ready()
	--local parent = self:get_node("../../CustomScriptManager")
	for i=1,1000000 do 
		local x = 5+5
		--parent.testData = ""..i;
		print(x)
	end
end

-- UNSEEN CODE --



local coros = {};



function otherscript:startcoro(key)
	coros[key] = coroutine.create(self.ready)
end

function otherscript:runcoro(key)
	local code, res = coroutine.resume(coros[key],self)
	return res
end

function otherscript:collectdata(data)
	self.data=data;
end
