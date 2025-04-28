using AutoMapper;
using CoursesManagementSystem.DTOs;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


    }
}
