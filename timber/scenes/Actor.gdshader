shader_type spatial;

uniform sampler2D fowTexture;

uniform vec3 world_position;

uniform float screenPosX = 0.0f;
uniform float screenPosZ = 0.0f;

uniform float screenHeight = 5.0f;
uniform float screenWidth = 5.0f;

uniform sampler2D texture_albedo : hint_albedo;
uniform float alpha_cutout_threshold : hint_range(0, 1) = 0.5;
uniform float static_amount = 0.0; // Amount of static pattern (0 to 1)


// Hash function for procedural noise
float hash(vec2 p) {
    p = fract(p * vec2(123.4, 789.6));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

void fragment() {
	float visibility_value = texture(fowTexture,vec2((world_position.x-screenPosX)/screenWidth,((world_position.z-screenPosZ)/screenHeight))).r;
    vec4 tex_color = texture(texture_albedo, UV);
	ALBEDO = vec3(tex_color.r, tex_color.g, tex_color.b) * visibility_value;
	
	if(visibility_value < 0.001f)
		discard;
	
    // Perform alpha cutout
    ALPHA_SCISSOR = alpha_cutout_threshold;
    if(tex_color.a < 0.5f)
        discard;

	/*
    // Generate noise based on UV and time
    float noise = hash(UV * 30.0 + vec2(0, TIME * 5.0));

    // Apply static effect
    vec3 static_color = mix(tex_color.rgb, tex_color.rgb + noise * static_amount, static_amount);

    // Output the final color
    ALBEDO = static_color;*/
}
