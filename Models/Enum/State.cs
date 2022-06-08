using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Connect4.Models
{
    public enum State
    {
        [Display(Name = "In partenza")]
        InPartenza,
        [Display(Name = "In corso")]
        InCorso,
        Conclusa,
    }
}