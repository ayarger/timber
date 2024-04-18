shader_type spatial;

uniform sampler2D fowTexture;
uniform vec3 world_position;

uniform float screenPosX = 0.0;
uniform float screenPosZ = 0.0;

uniform float screenHeight = 5.0;
uniform float screenWidth = 5.0;

uniform sampler2D texture_albedo : hint_albedo;

void fragment()
{
    // Calculate visibility value from fog of war texture.
    float visibility_value = texture(fowTexture, vec2((world_position.x - screenPosX) / screenWidth, (world_position.z - screenPosZ) / screenHeight)).r;

    // Fetch the color of the sprite.
    vec4 tex_color = texture(texture_albedo, UV);

    // Calculate the alpha based on the visibility value.
    float alpha = smoothstep(0.4, 0.6, visibility_value); // Adjust the smoothstep parameters for fading range.

    // Output final color with adjusted alpha.
    ALBEDO = tex_color.rgb;
    ALPHA = alpha * tex_color.a;
}
