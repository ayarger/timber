local otherscript = {}

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

otherscript.awaitaction = signal('arg1')

-- UNSEEN CODE --

function otherscript:ready()
	local parent = self:get_node("../../CustomScriptManager")
	--print("Other script running!")
	self:emit_signal('awaitaction', 'W')
	local x = 5+5
	parent.testData = ""..i;
	self:emit_signal('awaitaction', "E")
end

-- UNSEEN CODE --



local coros = {};

function otherscript:init()
	local parent = self:get_node("../../CustomScriptManager")
	parent:connect('ResumeCoroutine',self,'collectdata')
	--print("Lua Scirpt Initialized")
end


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

return otherscript

