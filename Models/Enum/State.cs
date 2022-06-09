using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Connect4.Models
{
    /// <summary>
    /// Valori dello stato di un partita
    /// </summary>
    public enum State
    {
        /// <summary>
        /// In attesa del secondo giocatore
        /// </summary>
        [Display(Name = "In partenza")]
        InPartenza,

        /// <summary>
        /// Parita in corso
        /// </summary>
        [Display(Name = "In corso")]
        InCorso,

        /// <summary>
        /// Partita terminata
        /// </summary>
        Conclusa,
    }
}