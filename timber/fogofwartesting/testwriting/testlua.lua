local testlua = {}
testlua.a = 0
testlua.b = 0
testlua.c = 0
testlua.d = 0
function testlua:ready()
	self.a = 1
	self.b = 2
	self.c = 3
	self.d = 4
	self.hello = (self.hello + 1)
	self.a = (self.b + self.c)
	print(self.a)
end
return testlua
