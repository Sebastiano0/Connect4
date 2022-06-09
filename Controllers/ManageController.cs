using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Connect4.Models;
using System.Threading;
using System.Net;
using System.Data.Entity;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace Connect4.Controllers
{
    class X :Object
    { }

    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ManageViewModels db = new ManageViewModels();
        private Random rnd = new Random();


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }

            private set 
            { 
                _signInManager = value; 
            }
        }


        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: /Manage/Index
        public ActionResult Index()
        {
            if (User.Identity.GetUserName() != null)
            {
                return View(db.Matches.ToList());
            } 
            else
            {
                return RedirectToAction("Login", "Account");
            }
            
        }


        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(Match match)
        {
            match.State = State.InPartenza;
            //match.State = "In partenza";
            match.Data = DateTime.UtcNow.ToString("dd-MM-yyyy");
            //match.NextTurnPlayer = match.SetNextTurnPlayer();
            match.SetNextTurnPlayer();

            //match.NextTurnPlayer = "Player 1";
            match.UsernamePlayer1 = User.Identity.GetUserName();

            if (match.VersusComputer)
            {

                match.UsernamePlayer2 = "Computer";
                match.State = State.InCorso;
                match.NextTurnPlayer = NextTurnPlayer.Player1;
            }

            //controllo che sia stato inserito il nome e che non esista già
            if (!ModelState.IsValid || db.Matches.Find(match.Name) != null || string.IsNullOrWhiteSpace(match.Name))
            {
                ModelState.AddModelError(string.Empty, "This name is already associated to another game");
                return View(match);
            }

            for (int i = 0; i < 7; i++)
            {
                match.Columns.Add(new Column(match.Name));
            }

            db.Matches.Add(match);
            db.SaveChanges();
            ShowMatchHub.BroadcastMatch();
            return RedirectToAction("Index");
        }

        public ActionResult Partecipate(Match match)
        {
            //if (string.IsNullOrWhiteSpace(Name))
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //Match match = db.Matches.Find(Name);
            if (match == null || User.Identity.GetUserName() == match.UsernamePlayer1)
            {
                return HttpNotFound();
            }
            match.State = State.InCorso;//cambio lo stato della partita
            //match.State = "In corso";
            match.UsernamePlayer2 = User.Identity.GetUserName();
            db.Entry(match).State = EntityState.Modified;//se non si lascia non si salvano i valori modificati
            db.SaveChanges();
            ShowMatchHub.BroadcastMatch();
            //System.Windows.Forms.MessageBox.Show("Successful enrolled to match");
            TempData["AlertMessage"] = "Successful enrolled to the match";
            return RedirectToAction("Index");
        }

        public ActionResult Game(Match match)
        {   
            string user = User.Identity.GetUserName();
            bool player1Logged = user == match.UsernamePlayer1;
            bool player2Logged = user == match.UsernamePlayer2;
            if (match == null || (!player1Logged && !player2Logged))
            {
                return HttpNotFound();
            }
            match = match.GetMatch(match, db);
            ViewBag.table = match.DrawTable(match);
            //controllo se il giocatore loggato ha perso
            if ((player2Logged && match.Winner == match.UsernamePlayer1) || (player1Logged && match.Winner == match.UsernamePlayer2))
            {
                
                Thread.Sleep(1000);
                
                TempData["AlertMessage"] = "You have lose";
                
                //System.Windows.Forms.MessageBox.Show("You have lose");
                return RedirectToAction("Index");
            } 
            /*else if((player2Logged && match.Winner == match.UsernamePlayer2) || (player1Logged && match.Winner == match.UsernamePlayer1) || (match.Winner == "Drawn"))
            {
                return RedirectToAction("Index");//controllo se ha vinto o se è finita in parità
            }*/
            
            ShowMatchHub.UpdateTable();
            return View();
        }

        [HttpPost]
        public ActionResult Game(Match match, string column)
        {
            match = match.GetMatch(match, db);
            int valueOfMove = match.MakeMove(match, int.Parse(column), db);
            switch (valueOfMove)
            {//controllo le conseguenza della mossa
                case 0:
                    TempData["AlertMessage"] = "This column is empty";
                    break;
                case 1:
                    TempData["AlertMessage"] = "No one has won, it's a draw!";
                    break;
                case 2:
                    TempData["AlertMessage"] = "You won the game!";
                    break;
                case 3:
                    break;
                case -1:
                    TempData["AlertMessage"] = "Wait your turn please";
                    break;
                default: throw new Exception("Mi sono dimenticato di un valore Cavolo!!!!!");
            }
            if (match.VersusComputer && (valueOfMove != 1 || valueOfMove != 2))
            {
                valueOfMove = ComputerMove(match);
                while (valueOfMove == 0)
                {//faccio la mossa finchè non vado su una colonna vuota
                    valueOfMove = ComputerMove(match);
                }
                
            }
            //ShowMatchHub.UpdateTable();
            //match = match.GetMatch(match, db);
            return RedirectToAction("Game", match);
        }

        public int ComputerMove(Match match)
        {            
            return match.MakeMove(match, rnd.Next(0, 7), db);            
        }

        public ActionResult GetUpdateData()
        {
            return PartialView("_IndexPartial", db.Matches.ToList());
        }

        public ActionResult GetUpdateTable(string Name)
        {
            //prendo il nome del match
            //string matchName = Request.Url.AbsoluteUri.Split('?')[1].Split('=')[1];
            //string url = Request.Url.ToString().Split('?')[1].Split('&')[1];

            Match match = db.Matches.FirstOrDefault(c => c.Name.Equals(Name));
            //creo la tabella con dati salvati
            match = match.GetMatch(match, db);
            ViewBag.table = match.DrawTable(match);
            return PartialView("_GamePartial", match);
            //return PartialView("_GamePartial");
        }
    }
}