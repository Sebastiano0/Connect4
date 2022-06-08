using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Connect4.Models
{
    public class Cell
    {
        
        public int Player { get; set; }
        [Key]
        public int Value { get; set; }
        //0/null--> vuota, 1--> gioc1, 2--> gioc2
        public int Column { get; set; }
        public string Match { get; set; }
        public int Column_Id { get; set; }

        public bool IsEmpty(int value)
        {//se mi ritorna falso richiamo un altro metodo che mi dice che giocatore la occupa
            return !Convert.ToBoolean(value);
        }

        public Cell(string match, int column, int columnId) {
            this.Match = match;
            this.Column = column;
            this.Column_Id = columnId;
        }

        public Cell() { }
    }
}