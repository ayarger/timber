local SaySomethingComponentLua = {
	extends = "Node",
}


function SaySomethingComponentLua:_ready()  -- `function t:f(...)` is an alias for `function t.f(self, ...)`
	  Engine.print("ready from lua!")
end

function SaySomethingComponentLua:_process()
	  Engine.print("process from lua!")
end

return SaySomethingComponentLua
