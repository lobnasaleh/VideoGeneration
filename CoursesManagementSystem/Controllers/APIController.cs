using AutoMapper;
using CoursesManagementSystem.DTOs;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static CoursesManagementSystem.DTOs.CourseDTO;
using System.Net;
using CoursesManagementSystem.DB.Models;

namespace CoursesManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        protected readonly APIResponse response;

        public APIController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            this._unitOfWork = _unitOfWork;
            this._mapper = _mapper;
            this.response = new APIResponse();
        }




        [HttpGet("CourseById/{CourseId:int}")]
        public async Task<ActionResult<APIResponse>> CourseById(int CourseId)
        {
            var response = new APIResponse(); // create response

            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.ID == CourseId,
                                                   include: new[] { "Category", "Level" });
                

            if (course == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Course not found.");

                return NotFound(response);
            }

            var courseDto = _mapper.Map<CourseDTO>(course);

            response.Result = courseDto;
            response.StatusCode = HttpStatusCode.OK;

            return Ok(response);
        }


        [HttpGet("AllCourses")]
        public async Task<ActionResult<APIResponse>> GetAllCourses()
        {
            var response = new APIResponse(); // create response

            // Get all courses, including Category and Level details
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(
                include: new[] { "Category", "Level" });

            if (courses == null || !courses.Any())
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("No courses found.");

                return NotFound(response);
            }

            // Map courses to CourseDTO
            var courseDtos = _mapper.Map<List<CourseDTO>>(courses);

            // Set response
            response.Result = courseDtos;
            response.StatusCode = HttpStatusCode.OK;

            return Ok(response);
        }


        [HttpGet("Courses/{courseId:int}/Chapters")]
        public async Task<ActionResult<APIResponse>> GetChaptersByCourseId(int courseId)
        {
            var response = new APIResponse();

            var chapters = await _unitOfWork.ChapterRepository.GetAllAsync(
                c => c.CourseId == courseId
            );

            if (chapters == null || !chapters.Any())
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("No chapters found for this course.");
                return NotFound(response);
            }

            var chapterDtos = _mapper.Map<List<ChapterDTO>>(chapters);

            response.Result = chapterDtos;
            response.StatusCode = HttpStatusCode.OK;

            return Ok(response);
        }


        [HttpGet("CourseDetails/{id:int}")]
        public async Task<ActionResult<APIResponse>> GetCourseById(int id)
        {
            var response = new APIResponse();

            var course = await _unitOfWork.CourseRepository.GetAsync(
                c => c.ID == id,
                include: new[] { "Category", "Level", "CourseConfig", "CourseQuestionsConfig.QuestionLevel" }
            );

            if (course == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Course not found.");

                return NotFound(response);
            }

            var courseDto = _mapper.Map<CourseDetailsDTO>(course);

            response.Result = courseDto;
            response.StatusCode = HttpStatusCode.OK;

            return Ok(response);
        }



        [HttpPost("PhaseOne")]
        public async Task<ActionResult<APIResponse>> AddCourseContent([FromBody] PhaseOneCreateDTO dto)
        {
            var response = new APIResponse();

            var course = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseId);
            if (course == null)
            {

                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Course not found.");
                return BadRequest(response);
            }

            var chapters = _mapper.Map<List<Chapter>>(dto.Chapters);

            foreach (var chapter in chapters)
            {
                chapter.CourseId = course.ID;

                await _unitOfWork.ChapterRepository.AddAsync(chapter);

                if (chapter.Lessons != null)
                {
                    foreach (var lesson in chapter.Lessons)
                    {
                        lesson.ChapterId = chapter.ID; // will be assigned after saving, if needed
                        await _unitOfWork.LessonRepository.AddAsync(lesson);

                        if (lesson.Questions != null)
                        {
                            foreach (var question in lesson.Questions)
                            {
                                question.LessonId = lesson.ID;
                                await _unitOfWork.QuestionRepository.AddAsync(question);

                                if (question.Answers != null)
                                {
                                    foreach (var answer in question.Answers)
                                    {
                                        answer.QuestionId = question.ID;
                                        await _unitOfWork.AnswerRepository.AddAsync(answer);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return CreatedAtAction(nameof(GetCourseById), new { id = dto.CourseId }, response);
        }


        [HttpPost("create-full-course")]
        public async Task<ActionResult<APIResponse>> CreateFullCourse([FromBody] CreateFullCourseDTO dto)
        {
            var response = new APIResponse();
            // 1. Map DTO to Course
            var course = _mapper.Map<Course>(dto);

            await _unitOfWork.CourseRepository.AddAsync(course);

            // 2. Save first to generate Course ID (if needed)
            await _unitOfWork.CompleteAsync();

            // 3. Map Chapters with the CourseId
            if (dto.Chapters != null && dto.Chapters.Any())
            {
                var chapters = _mapper.Map<List<Chapter>>(dto.Chapters);

                foreach (var chapter in chapters)
                {
                    chapter.CourseId = course.ID;

                    await _unitOfWork.ChapterRepository.AddAsync(chapter);

                    if (chapter.Lessons != null)
                    {
                        foreach (var lesson in chapter.Lessons)
                        {
                            lesson.ChapterId = chapter.ID;
                            await _unitOfWork.LessonRepository.AddAsync(lesson);

                            if (lesson.Questions != null)
                            {
                                foreach (var question in lesson.Questions)
                                {
                                    question.LessonId = lesson.ID;
                                    await _unitOfWork.QuestionRepository.AddAsync(question);

                                    if (question.Answers != null)
                                    {
                                        foreach (var answer in question.Answers)
                                        {
                                            answer.QuestionId = question.ID;
                                            await _unitOfWork.AnswerRepository.AddAsync(answer);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 4. Final Save
            await _unitOfWork.CompleteAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return CreatedAtAction(nameof(GetCourseById), new { id = dto.Name }, response);
        }



    }
}
