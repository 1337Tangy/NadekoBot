﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NadekoBot.Modules.Pokemon;
using NadekoBot.Classes;
using NadekoBot.Classes._DataModels; using NadekoBot.Modules.Pokemon.PokeTypes;

namespace NadekoBot.Modules.Pokemon.PokemonTypes
{
    class ElectricType : IPokeType
    {
        static readonly string name = "ELECTRIC";
        public static int numType = 3;

        public double GetMagnifier(IPokeType target)
        {
            switch (target.GetName())
            {

                case "WATER": return 2;
                case "ELECTRIC": return 0.5;
                case "GRASS": return 2;
                case "GROUND": return 0;
                case "FLYING": return 2;
                case "DRAGON": return 0.5;
                default: return 1;
            }
        }
        List<string> moves = new List<string>();

        


        public string GetName()
        {
            return name;
        }

        
        public string GetImage()
        {
            return "⚡️";
        }

        public int GetNum()
        {
            return numType;
        }
    }
}
