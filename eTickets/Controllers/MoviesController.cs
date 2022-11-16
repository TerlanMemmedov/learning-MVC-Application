using eTickets.Data;
using eTickets.Data.Services;
using eTickets.Data.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eTickets.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class MoviesController : Controller
    {
        private readonly IMoviesService _service;

        public MoviesController(IMoviesService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var movies = await _service.GetAllAsync(c => c.Cinema);

            return View(movies);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _service.GetMovieByIdAsync(id);
            if (movie is null)
            {
                return View("NotFound");
            }

            return View(movie);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Filter(string searchString)
        {
            var allMovies = await _service.GetAllAsync(c => c.Cinema);

            if (!string.IsNullOrEmpty(searchString))
            {
                //in here the name or description need to contain anything, also it can be a letter (you can search with just 'f') //
                var filteredMovie = allMovies.Where(m => m.Name.ToLower().Contains(searchString.ToLower()) || m.Description.ToLower().Contains(searchString.ToLower())).ToList();


                //I dont wanna use that beacuse in here the name or description have to be exactly same with the searchString
                //var filteredMovie = allMovies.Where(m => string.Equals(m.Name, searchString, StringComparison.CurrentCultureIgnoreCase) || string.Equals(m.Description, searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();

                return View("Index", filteredMovie);
            }
            return View("Index", allMovies);
        }

        public async Task<IActionResult> Create()
        {
            var movieDropdownsData = await _service.GetNewMovieDropdownsValues();

            ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
            ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
            ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");


            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(NewMovieVM movie)
        {
            if (!ModelState.IsValid)
            {
                var movieDropdownsData = await _service.GetNewMovieDropdownsValues();

                ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
                ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");


                return View(movie);
            }

            await _service.AddNewMovieAsync(movie);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id)
        {
            var movieForEdit = await _service.GetMovieByIdAsync(id);

            if (movieForEdit is null)
            {
                return View("NotFound");
            }
            
            var movie = new NewMovieVM()
            {
                Id = movieForEdit.Id,
                Name = movieForEdit.Name,
                Price = movieForEdit.Price,
                ImageURL = movieForEdit.ImageURL,
                StartDate = movieForEdit.StartDate,
                EndDate = movieForEdit.EndDate,
                Description = movieForEdit.Description,
                MovieCategory = movieForEdit.MovieCategory,
                CinemaId = movieForEdit.CinemaId,
                ProducerId = movieForEdit.ProducerId,
                ActorIds = movieForEdit.Actors_Movies.Select(n => n.ActorId).ToList()
            };

            var movieDropdowns = await _service.GetNewMovieDropdownsValues();
            ViewBag.Cinemas = new SelectList(movieDropdowns.Cinemas, "Id", "Name");
            ViewBag.Producers = new SelectList(movieDropdowns.Producers, "Id", "FullName");
            ViewBag.Actors = new SelectList(movieDropdowns.Actors, "Id", "FullName");

            return View(movie);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id ,NewMovieVM movieData)
        {
            if (id != movieData.Id)
            {
                return View("NotFound");
            }

            if (!ModelState.IsValid)
            {
                var movieDropdowns = await _service.GetNewMovieDropdownsValues();
                ViewBag.Cinemas = new SelectList(movieDropdowns.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdowns.Producers, "Id", "FullName");
                ViewBag.Actors = new SelectList(movieDropdowns.Actors, "Id", "FullName");
                
                return View(movieData);
            }

            await _service.UpdateMovieAsync(movieData);
            return RedirectToAction("Index");
        }
    }
}
