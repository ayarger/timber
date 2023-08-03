using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class GameDefinition
{
    public string game_name;
    public TitleScreenDefinition title_screen_definition;
    public UIDefinition ui_definition;
    public ProgressionDefinition progression_definition;
    public List<StatisticDefinition> statistic_definitions;
    public List<ActorDefinition> actor_definitions;
    public List<ActorAestheticDefinition> actor_aesthetic_definitions;
}

[Serializable]
public class TitleScreenDefinition
{
    public string title_screen_background_image;
    public string title_screen_logo_image;

    public static void UserEdit()
    {
        ArborResource.PerformEditType(typeof(TitleScreenDefinition));
    }
}

[Serializable]
public class UIDefinition
{
    public string cursor_image;
    public string primary_font;
}

[Serializable]
public class ProgressionDefinition
{
    public int initial_continue_count = 3;
    public List<SceneDefinition> scene_definitions = new List<SceneDefinition>();
}

[Serializable]
public class SceneDefinition
{
    public Guid main_character;
    public string scene_name;
    public string intro_image;

    public List<AudioClipDefinition> idle_background_tracks = new List<AudioClipDefinition>();
    public List<AudioClipDefinition> combat_background_tracks = new List<AudioClipDefinition>();

    public string victory_image;
    public string victory_text;

    public string defeat_image;
    public string defeat_text;
}

[Serializable]
public class LayoutDefinition
{
    public int width;
    public int length;

    public List<ActorInstanceDefinition> actor_instances = new List<ActorInstanceDefinition>();
}

[Serializable]
public class ActorInstanceDefinition
{
    public Guid actor_definition_id;
    public List<StatisticsInstanceDefinition> overriden_properties = new List<StatisticsInstanceDefinition>();
}

[Serializable]
public class AudioClipDefinition
{
    public string filename;
    public float volume;
}

[Serializable]
public class ActorDefinition
{
    public Guid id;
    public string name;
    public Guid aesthetic_definition_id;
    public List<StatisticsInstanceDefinition> statistics = new List<StatisticsInstanceDefinition>();
}

[Serializable]
public class StatisticsInstanceDefinition
{
    public Guid statistic_id;
    public float value;
}

[Serializable]
public class ActorAestheticDefinition
{
    public Guid id;
    public Guid parent_id;
    public float size_factor = 1.0f;
    public List<string> idle_frames;
    public List<string> attack_frames;
    public string movement_frame_1;
    public string movement_frame_2;
    public string damage;
    public string pre_ko;
    public string ko;
    public string celebration;
    public string lives_icon;
    public string focus;
}

[Serializable]
public class StatisticDefinition
{
    public Guid id;
    public string name;
    public string icon;
    public float default_val;
    public float max;
    public float min;
}