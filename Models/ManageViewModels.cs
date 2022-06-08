using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Connect4.Models
{

    public class ManageViewModels : DbContext
    {
        public DbSet<Match> Matches { get; set; }      
        public DbSet<Column> Columns { get; set; }     
        public DbSet<Cell> Cells { get; set; }
        public DbSet<Move> Moves { get; set; }
    }
}