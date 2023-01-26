using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MovieClient.Models;
using MovieClient.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MovieClient.Controllers
{
  [Authorize]
  public class MoviesController : Controller
  {
    private readonly string _apikey;
    private readonly MovieClientContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MoviesController(UserManager<ApplicationUser> userManager, MovieClientContext db, IConfiguration configuration)
    {
      _apikey = configuration["TMDB"];
      _userManager = userManager;
      _db = db;
    }

    public IActionResult Index()
    {
        return View(Movie.GetMovies(_apikey));
    }

    [HttpPost]
    public ActionResult CreateOrUpdate(Movie movie, int MovieId)
    {
      // IF movie does not exist, take new movie object, add to db
      // if ((_db.Movies.FirstOrDefault(entry => movie.MovieId == entry.MovieId)) == null)
      // {
      //   _db.Movies.Add(movie);
      //   _db.SaveChanges();
      // }
      
      // check IF movie does exist, _db.Update(movie), redirect to details page
      // movie.Review = movie.Review;
      movie.Rating = (movie.Rating + movie.Rating) / movie.NumberOfRatings;
      movie.NumberOfRatings += 1;
      _db.Update(movie);
      _db.SaveChanges();

      return RedirectToAction("Details", new { id = movie.MovieId });
    }

    public IActionResult Details(int id)
    {
      Movie movie = Movie.GetDetails(id, _apikey);

      if ((_db.Movies.FirstOrDefault(entry => movie.Id == entry.Id)) == null)
      {
        _db.Movies.Add(movie);
        _db.SaveChanges();
      }

      return View(movie);
    }

    [HttpPost]
    public async Task<ActionResult> AddToUser (int inputId)
    { 
      string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);

      User thisUser = _db.Users.FirstOrDefault(entry => entry.UserAccount.Id == currentUser.Id);

      _db.UserMovies.Add(new UserMovie() { MovieId = inputId, UserId = thisUser.UserId});
      _db.SaveChanges();

      return RedirectToAction("Index");
    }

    public IActionResult Search(string query)
    {
      if (query != null)
      {
        return View(Movie.GetBasicSearch(query, _apikey));
      }
      else
      {
        //add error message
        return RedirectToAction("Index");
      }
    } 
    
    [HttpGet, ActionName("AdvSearch")]
    public IActionResult AdvSearch(string param, string query)
    {
      return View(Movie.GetAdvSearch(param, query, _apikey));
    } 

    //new
    public async Task<ActionResult> MyMovies()
    {
      string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
      User thisUser = _db.Users.Include(join => join.JoinEntities).ThenInclude(join => join.Movie).FirstOrDefault(entry => entry.UserAccount.Id == currentUser.Id);
  
      //Movie movie = Movie.GetDetails(id, _apikey);
      // .Include(join => join.JoinEntities).ThenIncude(join => join.)


      // List<Item> model = _db.Items
      //                       .Include(item => item.Category)
      //                       .ToList();
      return View(thisUser);

    }
  }
}

    // [HttpPost]
    // public ActionResult RemoveFromUser (int id)
    // { 
    //   string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //   ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
    //   UserMovie joinEntry = _db.UserMovies.FirstOrDefault(entry => entry.MovieId == id && entry.UserId == currentUser.Id);
    //   _db.UserMovies.Remove(joinEntry);
    //   _db.SaveChanges();

    //  