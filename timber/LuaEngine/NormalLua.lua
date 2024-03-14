local NormalLua = {}

function NormalLua:_ready()  -- `function t:f(...)` is an alias for `function t.f(self, ...)`
  print("Hello World")
end


return NormalLua
