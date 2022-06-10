using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Connect4.Models
{
    public class Move
    {
        //con mossa(int), ora(timestamp), giocatore
            [Key]
            public int Id { get; set; }
            public int Column { get; set; }
            public string Player { get; set; }
            
            public NextTurnPlayer NextTurnPlayer { get; set; }
            public string TimeStamp { get; set; }
            public string Name_Match { get; set; }
            public Move(int column,NextTurnPlayer nextTurnPlayer, string player, string TimeStamp, string Name_Match)
            {
                Column = column;
                Player = player;
                NextTurnPlayer = nextTurnPlayer;
                this.TimeStamp = TimeStamp;
                this.Name_Match = Name_Match;

            }

        public Move(){}
    }
}