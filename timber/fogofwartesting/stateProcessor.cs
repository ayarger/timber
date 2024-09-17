using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
public static class StateProcessor

    {
        private static readonly Dictionary<string, ActorState> ActorStateDict =
            new Dictionary<string, ActorState>();

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            ActorStateDict.Clear();

            var allActorStates = Assembly.GetAssembly(typeof(ActorState)).GetTypes()
                .Where(t => typeof(ActorState).IsAssignableFrom(t) && t.IsAbstract == false);
            

            foreach (var s in allActorStates)
            {
                
                ActorState state = Activator.CreateInstance(s) as ActorState;
                if (state != null){
                    //how to set up script
                    // Node stateNode = new Node();
                    // stateNode.SetScript(state.GetScript());
                    GD.Print("Loading state: " + s.Name);
                    ActorStateDict.Add(state.name, state);
                } 
            }

            IsInitialized = true;
        }

        public static ActorState GetState(string targetState) =>
            ActorStateDict[targetState];

    }