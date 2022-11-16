using eTickets.Data.Base;
using eTickets.Data.ViewModels;
using eTickets.Models;
using Microsoft.EntityFrameworkCore;

namespace eTickets.Data.Services
{
    public class MoviesService : EntityBaseRepository<Movie>, IMoviesService
    {
        private readonly AppDbContext _context;

        public MoviesService(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddNewMovieAsync(NewMovieVM data)
        {
            var movie = new Movie()
            {
                Name = data.Name,
                Description = data.Description,
                Price = data.Price,
                ImageURL = data.ImageURL,
                CinemaId = data.CinemaId,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                MovieCategory = data.MovieCategory,
                ProducerId = data.ProducerId
            };

            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();


            //Add Mvie Actors
            foreach (var item in data.ActorIds)
            {
                var new_actors_movies = new Actor_Movie()
                {
                    ActorId = item,
                    MovieId = movie.Id
                };

                await _context.Actors_Movies.AddAsync(new_actors_movies);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Movie> GetMovieByIdAsync(int id)
        {
            var movie = _context.Movies
                .Include(c => c.Cinema)
                .Include(p => p.Producer)
                .Include(am => am.Actors_Movies).ThenInclude(a => a.Actor)
                .FirstOrDefault(n => n.Id == id);

            return movie;
        }

        public async Task<NewMovieDropdownsVM> GetNewMovieDropdownsValues()
        {
            var response = new NewMovieDropdownsVM()
            {
                Actors = await _context.Actors.OrderBy(a => a.FullName).ToListAsync(),
                Producers = await _context.Producers.OrderBy(p => p.FullName).ToListAsync(),
                Cinemas = await _context.Cinemas.OrderBy(c => c.Name).ToListAsync()
            };

            return response;
        }

        public async Task UpdateMovieAsync(NewMovieVM data)
        {
            var movieDb = await _context.Movies.FirstOrDefaultAsync(m => m.Id == data.Id);

            if (movieDb != null)
            {
                movieDb.Name = data.Name;
                movieDb.Description = data.Description;
                movieDb.Price = data.Price;
                movieDb.ImageURL = data.ImageURL;
                movieDb.CinemaId = data.CinemaId;
                movieDb.StartDate = data.StartDate;
                movieDb.EndDate = data.EndDate;
                movieDb.MovieCategory = data.MovieCategory;
                movieDb.ProducerId = data.ProducerId;
                await _context.SaveChangesAsync();
            }

            //Deleting old Actor_Movies for changing them with new ones
            var old_actor_movie = await _context.Actors_Movies.Where(n => n.MovieId == data.Id).ToListAsync();
            _context.Actors_Movies.RemoveRange(old_actor_movie);
            await _context.SaveChangesAsync();

            foreach (var item in data.ActorIds)
            {
                var new_actor_movies = new Actor_Movie()
                {
                    ActorId = item,
                    MovieId = data.Id
                };
                await _context.Actors_Movies.AddAsync(new_actor_movies);
            }
            await _context.SaveChangesAsync();
        }
    }
}
