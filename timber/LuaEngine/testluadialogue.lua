testluadialogue = {}

testluadialogue.MyValue = 5
testluadialogue.previousZ = 0;
testluadialogue.dialogueRef = 0
testluadialogue.follow = nil
testluadialogue.startFollow = false



-- UNSEEN CODE --

function testluadialogue:ready()
	Print("I am ready!")
end

-- For testing purposes, this is getting run every 5 seconds
function testluadialogue:update()
	if self.startFollow then
		--Print("start following!")
		if GetDistance(self.follow, self) > 4 then
			SetDestination(self, self.follow.x, self.follow.z)
		end
	elseif GetKeyDown("z") then
		local test = GetDistance(GetNearestActorOfTeam(self, "player"),self) < 8
		if test then
			Print("starting dialogue")
			StartDialogue(self, "Example")
			self.follow = GetNearestActorOfTeam(self, "player")
		end
	end
end

function testluadialogue:custom_signal()
	Print("I received a custom signal!")
end

function testluadialogue:start_follow()
	Print("Start Following")
	self.startFollow = true
end

function testluadialogue:betrayal()
	Print("Betrayal in progress")
	for i=1,2 do
		local temp = Spawn(self,self)
		SetScale(temp, .5)
		SetTeam(temp, "enemy")
	end
	Kill(self)
end

