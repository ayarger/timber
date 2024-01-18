local testloadafterwrite2 = {}
testloadafterwrite2.a = 0
testloadafterwrite2.b = 0
testloadafterwrite2.c = 0
testloadafterwrite2.d = 0
function testloadafterwrite2:ready()
	self.a = 1
	self.b = 2
	self.c = 3
	self.d = 4
	self.a = (self.b + self.c)
end
return testloadafterwrite2
