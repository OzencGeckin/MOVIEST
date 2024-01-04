﻿using AutoMapper;
using Project.BLL.DTOClasses;
using Project.BLL.ManagerServices.Abstracts;
using Project.DAL.Repositories.Abstracts;
using Project.ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BLL.ManagerServices.Concretes
{
    public class GenreManager : BaseManager<GenreDTO, Genre>, IGenreManager
    {
        IGenreRepository _grRep;

        public GenreManager(IGenreRepository grRep, IMapper mapper) : base(grRep, mapper)
        {
            _grRep = grRep;
        }
    }
}
