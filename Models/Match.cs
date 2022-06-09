using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Globalization;
using System.Web;
using System.Data.Entity;
using System.Web.UI;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Connect4.Models
{
    public class Match
    {
        public List<Column> Columns { get; set; }

        [Key]
        [Required]
        public string Name { get; set; }//nome partita
                
        public string Data { get; set; }
        [Display(Name = "Next turn player")]
        
        public NextTurnPlayer NextTurnPlayer { get; set; }
        
        /// <summary>
        /// Stato attuale della partita
        /// </summary>
        public State State { get; set; }//In partenza, In corso, Conclusa
        
        public string UsernamePlayer1 { get; set; }//lo inserisco per ogni utente quando crea o entra nella partita
        
        public string UsernamePlayer2 { get; set; }
        
        public string Winner { get; set; }//null nessuno, sennò nome giocatore
        
        [Display (Name ="Gioca contro il computer")]
        public bool VersusComputer  { get; set; }
        
        public List<Move> Moves { get; set; }

        public int CanWin { get; set; }//se uguale a 7 qualcuno può vincere

        public Match()
        {
            Moves = new List<Move>();
            Columns = new List<Column>();
        }

        public bool CanPartecipate(Match match)
        {
            return match.UsernamePlayer2 == null && match.UsernamePlayer1 != HttpContext.Current.User.Identity.Name;
        }

        public bool IsPartecipating(Match match)
        {
            return match.UsernamePlayer1 == HttpContext.Current.User.Identity.Name || match.UsernamePlayer2 == HttpContext.Current.User.Identity.Name;
        }

        public bool CurrentPlayer(Match match)
        {//se il prossimo turno è del giocatore
            return ((match.UsernamePlayer1 == HttpContext.Current.User.Identity.Name) && match.NextTurnPlayer == NextTurnPlayer.Giocatore1) || ((match.UsernamePlayer2 == HttpContext.Current.User.Identity.Name) && match.NextTurnPlayer == NextTurnPlayer.Giocatore2);
        }

        public bool CanPlay(Match match)
        {
            return (match.State.Equals(State.InCorso)) && match.UsernamePlayer2 != null && (match.UsernamePlayer1 == HttpContext.Current.User.Identity.Name || match.UsernamePlayer2 == HttpContext.Current.User.Identity.Name);
            //return !(match.State.Equals("In corso")) && match.UsernamePlayer2 != null && (match.UsernamePlayer1 == HttpContext.Current.User.Identity.Name || match.UsernamePlayer2 == HttpContext.Current.User.Identity.Name);

        }

        //public NextTurnPlayer SetNextTurnPlayer()
        public void SetNextTurnPlayer()
        {
            //prenod un numero random di una serie di numeri random
            Random rnd = new Random();
            rnd.Next();
            List<int> random = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                random.Add(rnd.Next());
            }//se è pari allora il primo giocatore è Player 1, sennò Player 2
            if ((rnd.Next() % 2) == 0)
            {
                NextTurnPlayer = NextTurnPlayer.Giocatore1;
            }
            else
            {
                NextTurnPlayer = NextTurnPlayer.Giocatore2;
            }
        }

        public Match GetMatch(Match match, ManageViewModels db)
        {
            match.Columns = db.Columns
                                .Where(c => c.Match_Name.Equals(match.Name))
                                .ToList();

            match.Moves = db.Moves.Where(m => m.Name_Match.Equals(match.Name)).ToList();


            for (int i = 0; i < match.Columns.Count; i++)
            {
                int column = match.Columns[i].Id;

                match.Columns[i].Cells = db.Cells.Where(c => c.Column_Id == column).ToList();
            }

            return db.Matches.Single(m => m.Name == match.Name);
        }

        private bool CheckVictory(int value, int player)
        {
            return value == player;
        }

        public int CheckVictory(Match match, int player, ManageViewModels db)
        {//ritorna 1 se ha vinto gioc1, 2 gioc2, 0 se nessuno
            int value = 0;
            for (int i = 0; i < 6; i++)
            {//controllo sequenza orizzontale
                value = CheckVictory(match, 0, 4, 0, i, player, db);
                if (CheckVictory(value, player))
                {//controllo se il giocatore ha vinto
                    return player;
                }
            }
            

            for (int i = 0; i < 7; i++)
            {//controllo sequenza verticale
                value = CheckVictory(match, 2, 4, i, 0, player, db);
                if (CheckVictory(value, player))
                {
                    return player;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 7; j++)
                {   //controllo sequenza obliqua verso l'alto
                    value = CheckVictory(match, 1, 4, j, i, player, db);
                    
                    if (CheckVictory(value, player))
                    {
                        return player;
                    }
                    //controllo sequenza obliqua verso basso
                    value = CheckVictory(match, -1, 4, j, 6 - i - 1, player, db);
                    if (CheckVictory(value, player))
                    {
                        return player;
                    }
                }
            }

            return value;
        }

        public int CheckVictory(Match match, int direction, int remainingPoint, int column, int cell, int player, ManageViewModels db)
        {//ritorna 1 se ha vinto gioc1, 2 gioc2, 0 se nessuno
            //controllo che la cella non sia nulla
            match = match.GetMatch(match, db);
            //System.Windows.Forms.MessageBox.Show("Remaining point:" + remainingPoint + " player: " + player);
            if (match.Columns[column].Cells.Count == 0 || match.Columns[column].Cells.Count <= cell)
            {
                return 0;
            }
            if (match.Columns[column].Cells[cell].Player == player)
            {
                remainingPoint--;
                if (remainingPoint == 0)
                {
                    return player;
                }
            }
            else
            {
                remainingPoint = 4;
            }
            if(column == 6 || (cell == 5 && direction != -1))
            {
                return 0;
            }
            switch (direction)
            {
                case 0://controllo orizzontale
                    if (remainingPoint == 4 && column > 3)
                    {//se sono dopo la 4° colonna e mancano ancora 4 punti non serve controllare (impossibile fare 4 di fila)
                        return 0;
                    }
                    return CheckVictory(match, 0, remainingPoint, ++column, cell, player, db);
                case 1://controllo obliquo verso alto
                    if ((column > 4 && (cell == 0)) || (cell >= 3 && remainingPoint == 4))
                    {//se sono sulla la 5° colonna e celle 0 o 5 non serve più controllare
                        return 0;
                    }
                    return CheckVictory(match, 1, remainingPoint, ++column, ++cell, player, db);
                case -1://controllo obliquo verso il basso
                    if ((column > 4 && (cell == 5)) || (cell <= 2 && remainingPoint == 4) || cell<=0)
                    {
                        return 0;
                    }
                    return CheckVictory(match, -1, remainingPoint, ++column, --cell, player, db);
                case 2://controllo verticale
                    if (remainingPoint == 4 && cell > 2)
                    {//se sono sopra la 3° cella e mancano ancora 4 punti non serve controllare (impossibile fare 4 di fila)
                        return 0;
                    }
                    return CheckVictory(match, 2, remainingPoint, column, ++cell, player, db);
                default:
                    break;
            }
            return 0;
        }

        public bool IsFinished(Match match)
        {//controlla se la partita è finita
            return match.State.Equals(State.Conclusa);
            //return match.State.Equals("Conclusa");
        }

       
        public int MakeMove(Match match, int column, ManageViewModels db)
        {   //ritorno un intero così posso gestire gli alert nel controller -- -1 non tuo turno, 0 colonna piena, 1 pareggio, 2 vittoria, 3 non è successo niente
            int valueToReturn = 3;
            match = match.GetMatch(match, db);
            //Controllo che sia il turno del giocatore loggato
            if (!match.VersusComputer && ((match.NextTurnPlayer == NextTurnPlayer.Giocatore2 && match.UsernamePlayer1 == HttpContext.Current.User.Identity.Name) || (match.NextTurnPlayer == NextTurnPlayer.Giocatore1 && match.UsernamePlayer2 == HttpContext.Current.User.Identity.Name)))
            {
                return -1;
            }

            //Controllo che la colonna selezionata non sia piena
            if (!match.Columns[column].IsFull(db, column))
            {//controllo di chi è il turno e metto il valore di chi ha fatto la mossa nella cella
                Cell cell = new Cell(match.Name, column, match.Columns[column].Id);
                

                int player = 1 + (int)match.NextTurnPlayer;
                //Aggiungo la mossa alla lista
                DateTime date = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
                match.Moves.Add(new Move(column, match.NextTurnPlayer, date.ToString("dd/MM/yyyy HH:mm:ss"), match.Name));
                //cambio il prossimo giocatore e inserisco nella cella chi l'ha messa in campo
                if (match.NextTurnPlayer == NextTurnPlayer.Giocatore1)
                {
                    match.Columns[column].Cells.Add(cell);
                    match.Columns[column].Cells[match.Columns[column].Cells.Count-1].Player = 1;
                    match.NextTurnPlayer = NextTurnPlayer.Giocatore2;
                }
                else
                {
                    match.Columns[column].Cells.Add(cell);
                    match.Columns[column].Cells[match.Columns[column].Cells.Count-1].Player = 2;
                    match.NextTurnPlayer = NextTurnPlayer.Giocatore1;
                }
                match.CanWin++;
                db.SaveChanges();
                //per controllare la vittoria (solo quando sono state inserite almeno 7 mosse)
                if (match.CanWin >= 7)
                {
                    int vittoria = match.CheckVictory(match, player, db);
                    if (vittoria == player)
                    {//qualcuno ha vinto
                        if (match.VersusComputer && match.NextTurnPlayer == NextTurnPlayer.Giocatore1)
                        {
                            match.Winner = "Computer";
                        } else
                        {
                            match.Winner = HttpContext.Current.User.Identity.Name;
                        }
                        
                        //System.Windows.Forms.MessageBox.Show("You won the game!");
                        match.State = State.Conclusa;
                        valueToReturn = 2;
                    }
                    
                    if(match.CanWin == 42)
                    {//partita finita in parità
                        match.Winner = "Pareggio";
                        //System.Windows.Forms.MessageBox.Show("No one has won, it's a draw!");
                        match.State = State.Conclusa;
                        valueToReturn = 1;
                    }
                    db.SaveChanges();
                }
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("This column is empty");
                valueToReturn = 0;
            }
            return valueToReturn;
        }

        //controllo se la cella è occupata dal giocatore corrente
        public bool PlayerHasOccupied(Match match, int j, int i, int player)
        {

            if (match.Columns.Count > 0 && match.Columns[j].Cells != null && i < match.Columns[j].Cells.Count)
            {
                return match.Columns[j].Cells[i].Player == player;
            }
            else
            {
                return false;
            }
        }

        public string GetNextPlayer(Match match)
        {
            if(match.NextTurnPlayer == NextTurnPlayer.Giocatore1)
            {
                return match.UsernamePlayer1;
            } else
            {
                return match.UsernamePlayer2;
            }
        }

        public string DrawTable(Match match)
        {
            string table = "";
            if (match.Winner == null)
            {
                table += "<h4>Prossimo giocatore:" + GetNextPlayer(match) + "</h4>";
            }
            table += "<table>";
            for (int i = 5; i >= 0; i--)
            {
                table += "<tr>";
                for (int j = 6; j >= 0; j--)
                {
                    if (match.PlayerHasOccupied(match, j, i, 1))
                    {
                        table += "<td class=\"Player1\">";
                    }
                    else if (match.PlayerHasOccupied(match, j, i, 2))
                    {
                        table += "<td class=\"Player2\">";
                    }
                    else
                    {
                        table += "<td>";
                    }
                    table += "</td>";
                }
                table += "</tr>";
            }
            table += "<tr class= \"button\">";
            for (int i = 0; i < 7; i++)
            {
                table += "<td class = \"button\"><input type=\"submit\" name=\"column\" value=\" " + i + "\" class=\"btn btn-default\" style=\"display: block; margin: auto;\"" + DisableButton(match) + " /></td>";
            }
            table += "</tr></table ><h3> Mosse </h3><ul>";
            //table += "<tr class= \"button\"><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"0\" class=\"btn btn-default\" /></td><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"1\" class=\"btn btn-default\" /></td><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"2\" class=\"btn btn-default\" /></td><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"3\" class=\"btn btn-default\" /></td><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"4\" class=\"btn btn-default\" /></td><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"5\" class=\"btn btn-default\" /></td><td class = \"button\"><input type=\"submit\" name=\"column\" value=\"6\" class=\"btn btn-default\" /></td></tr></table><h3>Mosse</h3><ul>";
            //aggiungo anche le mosse efettuate
            for(int i = 0; i < match.Moves.Count; i++)
            {
                table += "<li class=" + match.Moves[i].Player + ">" + match.Moves[i].Player +", at column " + (1+match.Moves[i].Column) + " at " + match.Moves[i].TimeStamp + "</li>";
            }
            table += "</ul>";
            return table;
        }
        
        private string DisableButton(Match match)
        {
            if(match.Winner != null)
            {
                return "disabled";
            }
            return string.Empty;
        }
    }
}