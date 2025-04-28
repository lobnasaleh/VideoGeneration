using AutoMapper;
using CoursesManagementSystem.DTOs;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static CoursesManagementSystem.DTOs.CourseDTO;
using System.Net;
using CoursesManagementSystem.DB.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

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

        [HttpPost("Content-generated/Phase2")]
        public async Task<ActionResult<APIResponse>> ContentGeneratedPhase2([FromBody] PhaseTwoGeneratedContentDTO content)
        {
            try
            {
                if (content==null || content.CourseId <= 0)
                {
                    response.IsSuccess = false;
                    response.Errors.Add("Invalid content payload");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return StatusCode(400, response);
                }

                var coursefound=await _unitOfWork.CourseRepository.GetAsync(c=>!c.IsDeleted && c.ID==content.CourseId );
                if (coursefound == null)
                {
                    //validate Course
                    response.IsSuccess = false;
                    response.Errors.Add("Course Not Found");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return StatusCode(404, response);
                }

                foreach (var item in content.Chapters)
                {
                    //validate Chapter
                    var Chapterfound = await _unitOfWork.ChapterRepository.GetAsync(ch => !ch.IsDeleted && ch.ID == item.ChapterId && ch.CourseId==content.CourseId);
                    if (coursefound == null)
                    {
                        response.IsSuccess = false;
                        response.Errors.Add("Chapter Not Found");
                        response.StatusCode = HttpStatusCode.NotFound;
                        return StatusCode(404, response);
                    }

                    foreach (var lesson in item.Lessons)
                    {
                        var Lessonfound = await _unitOfWork.LessonRepository.GetAsync(l => !l.IsDeleted && l.ID == lesson.LessonId && l.ChapterId == item.ChapterId);
                        if (Lessonfound == null)
                        {
                            response.IsSuccess = false;
                            response.Errors.Add("Lesson Not Found");
                            response.StatusCode = HttpStatusCode.NotFound;
                            return StatusCode(404, response);
                        }
                        Lessonfound.VideoStorageURL=lesson.VideoUrl;
                        await _unitOfWork.CompleteAsync();
                    }
                }
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = "Video URL Saved Successfully";
                return Ok(response);

            }
            catch (Exception ex) {

                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(500, response);


            }

        }

        [HttpGet("Chapters/{ChapterId:int}/lesson")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> getLessons(int ChapterId)
        {
            try
            {
                var res=await _unitOfWork.LessonRepository.GetAllQuery(l=>!l.IsDeleted && l.ChapterId == ChapterId)
                    .Select(l=>new LessonDTO
                    {
                        AudioStorageURL = l.AudioStorageURL,
                        Details = l.Details,
                        LessonId=l.ID,
                        Name=l.Name,
                        ScriptText=l.ScriptText,
                        Sort=l.Sort,
                        VideoStorageURL=l.VideoStorageURL,
                    })
                    
                    .ToListAsync();

                if (!res.Any()) { 
                response.IsSuccess = false;
                response.Errors.Add("Could not Find Any Lessons");
                response.StatusCode=HttpStatusCode.NotFound;
                return NotFound(response);

                }

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = res;
                return Ok(response);
            }
            catch (Exception ex) {
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(500,response);

            }

        }

        [HttpGet("Courses/{CourseId:int}/CourseConfigs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> getCourseConfigs(int CourseId)
        {
            try
            {
                // var res = await _unitOfWork.CourseConfigRepository.GetAllAsync(c => !c.IsDeleted && c.CourseId == CourseId);

                var res = await _unitOfWork.CourseConfigRepository.GetAllQuery(c => !c.IsDeleted && c.CourseId == CourseId)
                    .Select(c=>new CourseConfigDTO()
                    {
                        CourseId = c.CourseId,
                        BookStorageURL=c.Course.BookStorageURL,//
                        ChaptersCount=c.ChaptersCount,
                        CourseName=c.Course.Name,//
                        Language=c.Language.ToString(),
                        LessonsCountPerChapter=c.LessonsCountPerChapter,
                        Persona=c.Persona.ToString(),  
                        VideoDurationInMin=c.VideoDurationInMin
                    })
                    
                    .ToListAsync();
                if (!res.Any())
                {
                    response.IsSuccess = false;
                    response.Errors.Add("Could not Find Any Configurations For This Course");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(response);
                }

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = res;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(500, response);

            }

        }
        [HttpGet("Courses/{CourseId:int}/QuestionsConfigs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> getCourseQuestionConfigs(int CourseId)
        {
            try
            {
                var res=await _unitOfWork.CourseQuestionConfigRepository.GetAllQuery(c => !c.IsDeleted && c.CourseId == CourseId)
                    .Select(cq=>new CourseQuestionConfigDTO
                    {
                        QuestionLevelId = cq.QuestionLevelId,
                        CourseId = cq.CourseId,
                        QuestionLevelName=cq.QuestionLevel.Name,
                        QuestionsCountPerLesson=cq.QuestionsCountPerLesson,
                    })
                    
                    .ToListAsync();

                if (!res.Any())
                {
                    response.IsSuccess = false;
                    response.Errors.Add("Could not Find Any Questions Configurations For This Course");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(response);
                }

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = res;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(500, response);

            }

        }

        [HttpGet("Courses/AllCoursesDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> getAllCoursesDetails()
        {
            try
            {
                var courses = await _unitOfWork.CourseRepository.GetAllAsync( c => !c.IsDeleted,
            new[] { "Category", "Level", "CourseConfig", "CourseQuestionsConfig.QuestionLevel" }
        );
                var courseIds = courses.Select(c => c.ID).ToList();
                var chapters = await _unitOfWork.ChapterRepository.GetAllQuery(ch => !ch.IsDeleted && courseIds.Contains(ch.CourseId))
                    .ToListAsync();
                var chapterIds = chapters.Select(ch => ch.ID).ToList();
                var lessons = await _unitOfWork.LessonRepository.GetAllQuery(l => !l.IsDeleted && chapterIds.Contains(l.ChapterId))
                    .ToListAsync();

                var res = courses.Select(c => new CoursesDetailsDTO
                {
                    BookStorageURL = c.BookStorageURL,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category?.Name ?? "Unknown",
                    Name = c.Name,
                    CourseId = c.ID,
                    Details = c.Details,
                    LevelId = c.LevelId,
                    LevelName = c.Level?.Name ?? "Unknown",
                    ChaptersCount = c.CourseConfig?.ChaptersCount ?? 0,
                    LessonsCountPerChapter = c.CourseConfig?.LessonsCountPerChapter ?? 0,
                    VideoDurationInMin = c.CourseConfig?.VideoDurationInMin ?? 0,
                    Language = c.CourseConfig?.Language.ToString() ?? "Unknown",
                    Persona = c.CourseConfig?.Persona.ToString() ?? "Unknown",
                    Chapters = chapters
                        .Where(ch => ch.CourseId == c.ID)
                        .Select(ch => new ChapterDTO
                        {
                            ChapterId = ch.ID,
                            Name = ch.Name,
                            Details = ch.Details,
                            Sort = ch.Sort,
                            Lessons = lessons
                                .Where(l => l.ChapterId == ch.ID)
                                .Select(l => new LessonDTO
                                {
                                    LessonId = l.ID,
                                    Name = l.Name,
                                    Details = l.Details,
                                    ScriptText = l.ScriptText,
                                    VideoStorageURL = l.VideoStorageURL,
                                    AudioStorageURL = l.AudioStorageURL,
                                    Sort = l.Sort
                                }).ToList()
                        }).ToList(),
                    CourseQuestionConfig = c.CourseQuestionsConfig
                        .Where(cq => !cq.IsDeleted)
                        .Select(cqq => new CourseQuestionConfigDTO
                        {
                            QuestionLevelId = cqq.QuestionLevelId,
                            QuestionLevelName = cqq.QuestionLevel?.Name ?? "Unknown",
                            QuestionsCountPerLesson = cqq.QuestionsCountPerLesson
                        }).ToList()
                }).ToList();

                if (!res.Any())
                {
                    response.IsSuccess = false;
                    response.Errors.Add("Could not Find Any Course");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(response);
                }

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = res;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(500, response);

            }

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
