﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes._DataModels {
    class Request : IDataModel {
        public string UserName { get; set; }
        public long UserId { get; set; }
        public string ServerName { get; set; }
        public long ServerId { get; set; }
        [Newtonsoft.Json.JsonProperty("Request")]
        public string RequestText { get; set; }
        [Newtonsoft.Json.JsonProperty("createdAt")]
        public DateTime DateAdded { get; set; }
    }
}
