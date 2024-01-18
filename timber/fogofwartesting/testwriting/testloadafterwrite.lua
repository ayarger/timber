local testloadafterwrite = {}
testloadafterwrite.a = 0
testloadafterwrite.b = 0
testloadafterwrite.c = 0
testloadafterwrite.d = 0
function testloadafterwrite:ready()
	self.a = 1
	self.b = 2
	self.c = 3
	self.d = 4
	self.hello = (self.hello + 1)
	self.a = (self.b + self.c)
end
return testloadafterwrite
