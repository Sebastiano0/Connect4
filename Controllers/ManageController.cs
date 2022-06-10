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
        public ActionResult Index(Match match/*, string id = ""*/)
        {
            //Uri uri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);
            //id = HttpUtility.ParseQueryString(uri.Query).Get("id");
                
            //if (id != null)
            //{
            //    string searchString = id;
            //    var matches = from m in db.Matches
            //                  select m;

            //    if (!String.IsNullOrEmpty(searchString))
            //    {
            //        matches = db.Matches.Where(s => s.Name.Contains(searchString));
            //    }
            //    return View(matches.ToList());
            //}
            
            match = match.GetMatch(match, db);

            if (User.Identity.GetUserName() != null && match == null)
            {
                return View(db.Matches.ToList());
            } 
            
            if(match != null)
            {
                match.Winner = "Partita interrotta";
                match.State = State.Conclusa;
                db.Entry(match).State = EntityState.Modified;
                db.SaveChanges();
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
            match.Data = DateTime.UtcNow.ToString("dd-MM-yyyy");
            match.SetNextTurnPlayer();
            match.UsernamePlayer1 = User.Identity.GetUserName();

            //controllo se la partita è contro il computer
            if (match.VersusComputer)
            {

                match.UsernamePlayer2 = "Computer";
                match.State = State.InCorso;
                match.NextTurnPlayer = NextTurnPlayer.Giocatore1;
            }

            //controllo che sia stato inserito il nome e che non esista già
            if (!ModelState.IsValid || db.Matches.Find(match.Name) != null || string.IsNullOrWhiteSpace(match.Name))
            {
                ModelState.AddModelError(string.Empty, "Questo nome è già associato a un'altra partita");
                return View(match);
            }

            //aggiungo le colonne alla partita
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
            //controllo che match esista e che non stia partecipando di nuovo il giocatore1
            if (match == null || User.Identity.GetUserName() == match.UsernamePlayer1)
            {
                TempData["AlertMessage"] = "Partita non trovata";
                return RedirectToAction("Index");
            }

            //imposto le variabili della partita
            match.State = State.InCorso;
            match.UsernamePlayer2 = User.Identity.GetUserName();
            
            db.Entry(match).State = EntityState.Modified;
            db.SaveChanges();

            ShowMatchHub.BroadcastMatch();
            
            TempData["AlertMessage"] = "Partecipazione alla partita avvenuta con successo";
            return RedirectToAction("Index");
        }


        public ActionResult Game(Match match)
        {   
            string user = User.Identity.GetUserName();
            bool player1Logged = user == match.UsernamePlayer1;
            bool player2Logged = user == match.UsernamePlayer2;

            //controllo se il match esiste o il giocatore ne fa parte
            if (match == null || (!player1Logged && !player2Logged))
            {
                TempData["AlertMessage"] = "Partita non trovata, controlla se è presente nella tua lista";
                return RedirectToAction("Index");
            }

            match = match.GetMatch(match, db);
            ViewBag.table = match.DrawTable(match);

            //controllo se il giocatore loggato ha perso
            if ((match.Winner != null) && ((player2Logged && match.Winner == match.UsernamePlayer1) || (player1Logged && match.Winner == match.UsernamePlayer2)))
            {
                TempData["AlertMessage"] = "Hai perso";                
            } 
            
            ShowMatchHub.UpdateTable();
            return View();
        }


        [HttpPost]
        public ActionResult Game(Match match, string column)
        {
            match = match.GetMatch(match, db);
            int valueOfMove = match.MakeMove(match, int.Parse(column), db);

            //controllo le conseguenza della mossa
            switch (valueOfMove)
            {
                case 3:
                    break;

                case -1:
                    TempData["AlertMessage"] = "Aspetta il tuo turno!";
                    break;

                case 2:
                    TempData["AlertMessage"] = "Hai vinto la partita!";
                    break;

                case 0:
                    TempData["AlertMessage"] = "La colonna è piena";
                    break;
                
                case 1:
                    TempData["AlertMessage"] = "Nessuno ha vinto, è un pareggio!";
                    break;
                
                default: throw new Exception("Some exception");
            }

            //se la partita è contro il computer e la mossa dell'utente 
            //non ha portato a vittoria o pareggio tocca al computer
            if (match.VersusComputer && (valueOfMove != 1 || valueOfMove != 2))
            {
                valueOfMove = ComputerMove(match);

                //faccio la mossa finchè non vado su una colonna vuota
                while (valueOfMove == 0)
                {
                    valueOfMove = ComputerMove(match);
                }
                
            }

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
            //prendo dal database il match
            Match match = db.Matches.FirstOrDefault(c => c.Name.Equals(Name));
            match = match.GetMatch(match, db);

            //creo la tabella con dati salvati
            ViewBag.table = match.DrawTable(match);
            return PartialView("_GamePartial", match);
        }
    }
}