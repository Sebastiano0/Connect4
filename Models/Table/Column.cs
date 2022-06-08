using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Connect4.Models
{
    public class Column
    {

        public int Id { get; set; }
        public List<Cell> Cells { get; set; }
        public string Match_Name { get; set; }
        public bool IsFull(ManageViewModels db, int column)
        {//ritorna true se la colonna è piena(ha 6 celle)
            Cells = db.Cells
                                .Where(c => c.Match.Equals(Match_Name))
                                .Where(index => index.Column == column)
                                .ToList();
            return Cells.Count == 6;
        }

        public Column() { }

        public Column(string name)
        {
            Match_Name = name;
            Cells = new List<Cell>();
        }
    }
}