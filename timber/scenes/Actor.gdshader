shader_type spatial;

uniform sampler2D fowTexture;
uniform vec3 world_position;

uniform float screenPosX = 0.0;
uniform float screenPosZ = 0.0;

uniform float screenHeight = 5.0;
uniform float screenWidth = 5.0;

uniform sampler2D texture_albedo : hint_albedo;
uniform float alpha_cutout_threshold : hint_range(0, 1) = 0.5;
uniform float static_amount = 0.0; // Amount of static pattern (0 to 1)
uniform float apply_red_tint = 0.0; //apply red tint when dealt damage
// Hash function for procedural noise
float hash(vec2 p) {
    p = fract(p * vec2(123.4, 789.6));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

void fragment()
{
    // Calculate visibility value from fog of war texture.
    float visibility_value = texture(fowTexture, vec2((world_position.x - screenPosX) / screenWidth, (world_position.z - screenPosZ) / screenHeight)).r;

    // Fetch the color of the sprite.
    vec4 tex_color = texture(texture_albedo, UV);
	
	vec3 redTint = vec3(1.0, 0.0, 0.0); // Red color vector
	float intensity = 0.5f;
    vec3 resultColor = mix(tex_color.rgb, redTint, apply_red_tint*intensity);

    // Calculate the alpha based on the visibility value.
    float alpha = smoothstep(0.4, 0.6, visibility_value); // Adjust the smoothstep parameters for fading range.

    // Output final color with adjusted alpha.
    ALBEDO = resultColor * visibility_value;
    ALPHA = alpha * tex_color.a;
}