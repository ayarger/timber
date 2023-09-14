shader_type spatial;

uniform sampler2D albedo_texture;
uniform sampler2D visibility_texture;

varying vec3 world_position;

const vec3 player_position = vec3(46, 0, 18);

const int num_positions = 100;
uniform float test = 100.0f;

uniform sampler2D positions;

vec4 get_data(int pos){
	return texture(positions,vec2(.005f+(float(2*pos))/float(num_positions),.5f))*256.0+texture(positions,vec2(.005f+(float(2*pos+1))/float(num_positions),.5f));
}

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

    // Assign the final color to the output
	ALBEDO = vec3(0.0, 0.0, 0.0);
	
	for(int i = 0; i<50;i++){
		vec4 data = get_data(i);
		float d = distance(world_position, data.rgb);

	    // Check if the x component of the world position is less than 20
	    if (d<data.a) {
		    ALBEDO = final_color.rgb;
		    ALPHA = final_color.a;
	    }
	}
}
