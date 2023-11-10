shader_type spatial;

uniform sampler2D albedo_texture;
uniform sampler2D visibility_texture;
uniform sampler2D lowVisibility_texture;

varying vec3 world_position;

const vec3 player_position = vec3(46, 0, 18);

uniform float screenPosX = 0.0f;
uniform float screenPosZ = 0.0f;

uniform float screenHeight = 5.0f;
uniform float screenWidth = 5.0f;


uniform sampler2D fowTexture;

//vec4 get_data(int pos){
//	return texture(positions,vec2(.005f+(float(2*pos))/float(num_positions),.5f))*256.0+texture(positions,vec2(.005f+(float(2*pos+1))/float(num_positions),.5f));
//}

void vertex() {
    // Transform the vertex position to world space
    // This is automatically done by Godot's rendering engine
    world_position = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    // Calculate the world position of the current fragment

    
    // Now, you need to convert this world position to a texture coordinate
    // to sample from your visibility texture. This conversion depends on how you've
    // setup your visibility texture, so here's a placeholder.
    //vec2 visibility_uv = vec2(world_pos.x * 0.5, world_pos.z * 0.5);

    // Sample the visibility texture
    //vec4 visibility = texture(visibility_texture, visibility_uv);

    // Sample the albedo texture
    vec4 albedo = texture(albedo_texture, UV);

    // Combine the visibility information with the albedo
    // For example, let's say the visibility texture's red channel stores the visibility factor
    vec4 final_color = albedo; //* visibility.r;
	vec4 fowSample = texture(fowTexture,vec2((world_position.x-screenPosX)/screenWidth,((world_position.z-screenPosZ)/screenHeight)));
	vec4 lvSample = texture(lowVisibility_texture,vec2((world_position.x-screenPosX)/screenWidth,((world_position.z-screenPosZ)/screenHeight)));
	final_color.r *= .8f*lvSample.r+fowSample.r;
	final_color.g *= .8f*lvSample.g+fowSample.g;
	final_color.b *= .8f*lvSample.b+fowSample.b;

    // Assign the final color to the output
	ALBEDO = vec3(0.0, 0.0, 0.0);
//
    ALBEDO = final_color.rgb;
    ALPHA = final_color.a;
	
//	for(int i = 0; i<50;i++){
//		vec4 data = get_data(i);
//		float d = distance(world_position, data.rgb);

	    // Check if the x component of the world position is less than 20
//	    if (d<data.a) {
//	    }
//	}
}
