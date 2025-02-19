﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.ManagerServices.Abstracts;
using Project.COREMVC.Models.Members.Movies;
using Project.COREMVC.Models.Members.WatchlistTools;
using Project.COREMVC.Models.Members.Genres;
using Project.COREMVC.Models.Members.MemberPageVM;
using Project.COREMVC.Models.SessionService;
using Project.ENTITIES.Models;
using X.PagedList;
using MovieList = Project.COREMVC.Models.Members.WatchlistTools.MovieList;
using Project.BLL.DTOClasses;
using Project.BLL.ManagerServices.Concretes;
using Project.COREMVC.Models.Members.MovieDetailsVM;
using Project.COREMVC.Models.Members.Casts;

namespace Project.COREMVC.Controllers
{
    public class MemberController : Controller
    {
        readonly IGenreManager _genreManager;
        readonly IMovieManager _movieManager;
        readonly IMovieGenreManager _movieGenreManager;
        readonly IMovieCastManager _movieCastManager;
        readonly ICastManager _castManager;
        readonly IMapper _mapper;


        public MemberController(IGenreManager genreManager, IMovieManager movieManager, IMapper mapper, IMovieGenreManager movieGenreManager, ICastManager castManager, IMovieCastManager movieCastManager)
        {
            _genreManager = genreManager;
            _movieManager = movieManager;
            _mapper = mapper;
            _movieGenreManager = movieGenreManager;
            _castManager = castManager;
            _movieCastManager = movieCastManager;
        }

        private List<MovieVM> MovieDtoToVM()
        {
            List<MovieVM> liste = _movieManager.GetActives().Select(x => new MovieVM
            {
                VideoPath = x.VideoPath,
                MovieName = x.MovieName,
                ImagePath = x.ImagePath,
                ID = x.ID
            }).ToList();

            return liste;
        }
        private List<MovieVM> GenreDtoToVm(int? id)
        {
            List<MovieVM> list = _movieManager.GetActives().Where(x => x.MovieGenres.Any(y => y.GenreID == id)).Select(a => new MovieVM
            {
                VideoPath = a.VideoPath,
                MovieName = a.MovieName,
                ImagePath = a.ImagePath,
                ID = a.ID
            }).ToList();

            return list;
        }

        public IActionResult Index(int? page, int? genreID)
        {
            MemberMovieVM memberMovie = new MemberMovieVM()
            {
                Movies = genreID == null ? MovieDtoToVM().ToPagedList(page ?? 1, 9) : GenreDtoToVm(genreID).ToPagedList(page ?? 1, 9),

                Genres = _genreManager.GetActives().Select(x => new GenreVM
                {
                    GenreName = x.GenreName,
                    ID = x.ID
                }).ToList(),
            };
            if (genreID != null) TempData["genreID"] = genreID;//page degistirdigimizde genre'nin bir sonraki sayfada unutulmaması icin gecici bir depolama alanında tutuyoruz.GenreID ilkel bir tip oldugu icin tempdata da saklıyoruz .Viewdata kullanmamamızın sebebi bir sonra ki sayfada tamamen farklı bir request olacagı icin view dan çıkar o yuzden kullanmayız.

            return View(memberMovie);
        }

        public async Task<IActionResult> AddToMovie(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["message"] = "You need to be logged in to add a movie to your watch list.";
                return RedirectToAction("Index"); 
            }
            MovieList m = HttpContext.Session.GetObject<MovieList>("dmovie") == null ? new MovieList() : HttpContext.Session.GetObject<MovieList>("dmovie");

            Movie movieAdd = _mapper.Map<Movie>(await _movieManager.FindAsync(id));
            MovieItem mi = new()
            {
                ID = movieAdd.ID,
                MovieName = movieAdd.MovieName,
                ImagePath = movieAdd.ImagePath
            };
                    
            m.AddToMovie(mi);
            SetMovieList(m);

            TempData["message"] = $"{mi.MovieName} added to movie watch list";

            return RedirectToAction("Index");
        }

        private void SetMovieList(MovieList m)
        {
            HttpContext.Session.SetObject("dmovie", m);           
        }

        public IActionResult MovieListPage()
        {
            if (HttpContext.Session.GetObject<MovieList>("dmovie") == null)
            {
                TempData["message"] = "watclist is currently empty ";
                return RedirectToAction("Index");
            }
            MovieList m = HttpContext.Session.GetObject<MovieList>("dmovie");
            
            return View(m);
            
        }

        public IActionResult DeleteFromMovieList(int id)
        {
            if (GetMovieListFromSession("dmovie") != null)
            {
                MovieList m = GetMovieListFromSession("dmovie");
                m.RemoveMovieList(id);
                SetMovieList(m);
                if (m.GetMovieItems.Count == 0)
                {
                    HttpContext.Session.Remove("dmovie");
                    TempData["message"] = "İzleme listenizdeki bütün filmler çıkarılmıştır.";
                    return RedirectToAction("Index");
                }
                
            }
            return RedirectToAction("MovieListPage");
        }

        MovieList GetMovieListFromSession(string key)
        {
            return HttpContext.Session.GetObject<MovieList>(key);
        }

        public MovieDetailsVM GetMovieById(int id)
        {
            var movie = _movieManager.Where(m => m.ID == id).Select(m => new MovieVM
            {
                ID = m.ID,
                MovieName = m.MovieName,
                Description = m.Description,
                ImagePath = m.ImagePath,
                VideoPath = m.VideoPath
                
                
            }).FirstOrDefault();

            List<CastVM> casts = _movieCastManager.Where(c => c.MovieID == id).Select(c => new CastVM
            {
                ID = c.Cast.ID,
               FirstName  = c.Cast.FirstName,
               LastName = c.Cast.LastName,
               ImagePath = c.Cast.ImagePath

            }).ToList();
            return new MovieDetailsVM
            {
                Movie = movie,
                Casts = casts,
            };

        }
        public IActionResult MovieDetails(int id)
        {
            var movie = GetMovieById(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }
       
        public IActionResult CastDetails(int id)
        {
            var cast = _castManager.Where(c => c.ID == id).Select(c => new CastVM
            {
                ID = c.ID,
                FirstName = c.FirstName,
                LastName = c.LastName,
                ImagePath = c.ImagePath

            }).FirstOrDefault();

            if (cast == null)
            {
                return NotFound();
            }

            return View(cast);

        }
    }
}
