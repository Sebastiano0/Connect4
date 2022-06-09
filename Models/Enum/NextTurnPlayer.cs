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
            [Display(Name = "Giocatore 1")]
            Giocatore1,
            [Display(Name = "Giocatore 2")]
            Giocatore2
        }
    
}