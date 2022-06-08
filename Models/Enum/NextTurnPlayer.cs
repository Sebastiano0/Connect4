using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Connect4.Models
{
        public enum NextTurnPlayer
        {
            [Display(Name = "Payer 1")]
            Player1,
            [Display(Name = "Player 2")]
            Player2
        }
    
}