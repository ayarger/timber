
using Godot;

public class ComputerScreen : Sprite
{
    private ShaderMaterial _shaderMaterial;

    float current_strength = 0.0f;
    float desired_strength = 0.0f;

    public override void _Ready()
    {
        _shaderMaterial = (ShaderMaterial)Material;

        // Set the texture size uniform
        Vector2 textureSize = new Vector2(Texture.GetWidth(), Texture.GetHeight());
        _shaderMaterial.SetShaderParam("texture_size", textureSize);
    }

    public override void _Process(float delta)
    {
        if (EditModeManager.edit_mode)
        {
            desired_strength = 0.5f;
        }
        else
        {
            desired_strength = 0.0f;
        }

        current_strength += (desired_strength - current_strength) * 0.1f;
        _shaderMaterial.SetShaderParam("effect_strength", current_strength);
    }
}

