using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
public static class StateProcessor

    {
        private static readonly Dictionary<string, Type> ActorStateDict =
            new Dictionary<string, Type>();

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            ActorStateDict.Clear();
            
            var actorstates = typeof(ActorState).Assembly.GetTypes();

            var allActorStateTypes = actorstates.Where(t => t.IsSubclassOf(typeof(ActorState)) && t.IsAbstract == false);

            foreach (var s in allActorStateTypes)
            {
                GD.Print("Adding state: " + s.Name);
                ActorStateDict.Add(s.Name, s);
            }

            IsInitialized = true;
        }

        public static Type GetState(string targetState){
            if(ActorStateDict.ContainsKey(targetState)){
                return ActorStateDict[targetState];
            }
            else{
                GD.Print("State not found: " + targetState);
                return null;
            }
        }
            

    }