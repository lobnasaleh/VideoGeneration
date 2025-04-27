using AutoMapper;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public APIController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            this._unitOfWork = _unitOfWork;
            this._mapper = _mapper;
        }


       
    }
}
