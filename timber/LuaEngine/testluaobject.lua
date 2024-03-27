testluaobject = {}

testluaobject.MyValue = 5



-- UNSEEN CODE --

function testluaobject:process()
	Print("Process works!")
end

-- For testing purposes, this is getting run every 5 seconds
function testluaobject:testfunc()
	--local parent = self:get_node("../../CustomScriptManager")
	--for i=1,10 do 
	myvariable = false
	WaitForSeconds(1)
	SetDestination(self,self.x-3, self.z)
	Print("I am at x-position: "..self.x)
	WaitForSeconds(2)
	
	SetDestination(self,self.x+3, self.z)
	--parent.testData = ""..i;
	Print("Completed Ready")
	--end
end
