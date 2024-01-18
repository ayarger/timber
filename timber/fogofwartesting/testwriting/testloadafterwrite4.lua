local testloadafterwrite4 = {}
testloadafterwrite4.a = 0
testloadafterwrite4.b = 0
testloadafterwrite4.c = 0
testloadafterwrite4.d = 0
function testloadafterwrite4:ready()
	self.a = 1
	self.b = 2
	self.c = 3
	self.d = 4
	self.a = (self.b + self.c)
end
return testloadafterwrite4
