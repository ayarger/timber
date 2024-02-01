testluaobject = {}

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

setmetatable(testluaobject, mt)

function WaitForSeconds(secs)
	coroutine.yield("W"..secs)
end


-- UNSEEN CODE --

function testluaobject:ready()
	--local parent = self:get_node("../../CustomScriptManager")
	--for i=1,10 do 
		local x = 5+5
		--parent.testData = ""..i;
		print(x)
	--end
end

value = 0

function testluaobject:on_second()
	if value==0 then
		value = 1
	elseif value==1 then
		value = -1
	elseif value==-1 then
		value = 0
	end
	return "M"..value
end

-- UNSEEN CODE --



local coros = {};



function testluaobject:startcoro(key)
	coros[key] = coroutine.create(self.ready)
end

function testluaobject:runcoro(key)
	local code, res = coroutine.resume(coros[key],self)
	return res
end

function testluaobject:collectdata(data)
	self.data=data;
end
