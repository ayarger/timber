local Actor = {
	extends = "Node",
}
function Actor:_ready()
  print(_VERSION)
end

return Actor
