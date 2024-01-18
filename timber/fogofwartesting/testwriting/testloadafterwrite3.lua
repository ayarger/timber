local testloadafterwrite3 = {}
testloadafterwrite3.a = 0
testloadafterwrite3.b = 0
testloadafterwrite3.c = 0
testloadafterwrite3.d = 0
function testloadafterwrite3:ready()
	self.a = 1
	self.b = 2
	self.c = 3
	self.d = 4
	self.a = (self.b + self.c)
	print(self.b)
	print(self.b)
	print(self.b)
	print(self.b)
end
return testloadafterwrite3
