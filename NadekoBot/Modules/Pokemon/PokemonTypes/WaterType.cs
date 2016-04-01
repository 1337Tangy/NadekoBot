﻿using NadekoBot.Modules.Pokemon.PokeTypes;
using System.Collections.Generic;

namespace NadekoBot.Modules.Pokemon.PokemonTypes
{
    class WaterType : PokeType
    {
        static readonly string name = "WATER";
        public static int numType = 2;

        public double Multiplier(PokeType target)
        {
            switch (target.Name)
            {

                case "FIRE": return 2;
                case "WATER": return 0.5;
                case "GRASS": return 0.5;
                case "GROUND": return 2;
                case "ROCK": return 2;
                case "DRAGON": return 0.5;
                default: return 1;
            }
        }
        List<string> moves = new List<string>();

        public string Name => name;

        public string Image => "💦";

        public int Num => numType;
    }
}
