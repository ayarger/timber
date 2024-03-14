testluaobject = {}

testluaobject.MyValue = 5

-- UNSEEN CODE --

-- For testing purposes, this is getting run every 5 seconds
function testluaobject:ready()
	--local parent = self:get_node("../../CustomScriptManager")
	--for i=1,10 do 
	
	WaitForSeconds(1)
	SetDestination(self,self.x-3)
	Print("I am at x-position: "..self.x)
	WaitForSeconds(2)
	SetDestination(self,self.x+3)
	--parent.testData = ""..i;
	Print("Completed Ready")
	--end
end

global_mt = {}
testluaobject.object_name = "lmao"


global_mt.__index = function(self, key) 
  if key=="x" then
	return GetValue(rawget(self,"object_name"),key)
  else
	return rawget(self, key)
  end
end

global_mt.__newindex = function(self, key, value)
  rawset(self, key, value)
end

setmetatable(testluaobject, global_mt)

