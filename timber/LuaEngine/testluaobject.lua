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



-- UNSEEN CODE --

-- For testing purposes, this is getting run every 5 seconds
function testluaobject:ready()
	--local parent = self:get_node("../../CustomScriptManager")
	--for i=1,10 do 
	
	WaitForSeconds(1)
	SetDestination(self,-3)
	WaitForSeconds(2)
	SetDestination(self,3)
	--parent.testData = ""..i;
	print("Completed Ready")
	--end
end
