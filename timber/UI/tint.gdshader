shader_type canvas_item;

uniform vec4 tint_color : hint_color;

void fragment() {
	COLOR.rgb = (tint_color.rgb * tint_color.a) + COLOR.rgb * (1.0 - tint_color.a);
}