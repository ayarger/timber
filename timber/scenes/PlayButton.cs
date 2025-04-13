using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayButton : Button
{
	public async void OnPressed()
    {// If there is a game id in the url, go directly to the game. TODO: Later, disable the editor.
        if (OS.GetName() == "Web" || OS.GetName() == "HTML5")
        {
            Dictionary<string, string> web_query_parameters = new Dictionary<string, string>();
            string web_query_string = (string)JavaScript.Eval("window.location.search");
            web_query_string = web_query_string.Substring(1, web_query_string.Length - 1);

            if (web_query_string != null && web_query_string != "")
            {
                GD.Print("Web parameters : ");
                string[] split_params = web_query_string.Split('=', '&');
                for (int i = 0; i < split_params.Length; i += 2)
                {
                    GD.Print(split_params[i] + " = " + split_params[i + 1]);
                    web_query_parameters[split_params[i]] = split_params[i + 1];
                }
            }
            if (web_query_parameters.ContainsKey("default_mod"))
            {
                TransitionSystem.RequestTransition(@"res://Main.tscn");
                return;
            }
        }
        GetNode<LoginPopup>("LoginPopup").Open();
		//if (await Utilities.IsConnectedToInternet(this))
		//{
		//	GD.Print("Internet connection detected. Launching game...");
		//	TransitionSystem.RequestTransition(@"res://Main.tscn");
		//}
		//else
		//{
		//	GD.Print("No internet connection detected.");
		//}
	}
}
