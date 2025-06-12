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
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text;
using CoursesManagementSystem.ViewModels;

namespace CoursesManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        protected readonly APIResponse response;
        private readonly IHttpClientFactory _httpClientFactory;

        public APIController(IUnitOfWork _unitOfWork, IMapper _mapper, IHttpClientFactory httpClientFactory)
        {
            this._unitOfWork = _unitOfWork;
            this._mapper = _mapper;
            this.response = new APIResponse();
            _httpClientFactory = httpClientFactory;
        }
        //http://localhost:5168/api/API/Content-generated
        [HttpPost("Content-generated")]
        public async Task<ActionResult<APIResponse>> ContentGenerated([FromBody] PhaseTwoGeneratedContentDTO content)
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
                    if (Chapterfound == null)
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
                        if (!string.IsNullOrEmpty(lesson.ScriptText ))
                        {
                            Lessonfound.ScriptText = lesson.ScriptText;
                        }
                        if (!string.IsNullOrEmpty(lesson.VideoStorageURL))
                        {

                            Lessonfound.VideoStorageURL = lesson.VideoStorageURL;
                        }
                        if (!string.IsNullOrEmpty(lesson.AudioStorageURL))
                        {
                            Lessonfound.AudioStorageURL = lesson.AudioStorageURL;
                        }
                       
                        if (lesson.Questions != null)
                        {
                            foreach (var question in lesson.Questions)
                            {
                                Question q= new Question()
                                {
                                    QuestionType=question.QuestionType,
                                    QuestionLevelId=question.QuestionLevelId,
                                    QuestionInstructions=question.QuestionInstructions,
                                    QuestionText=question.QuestionText,
                                    LessonId=lesson.LessonId,
                                    CreatedAt=DateTime.Now,
                                    CreatedBy="Admin",
                                    IsDeleted=false
                                };
                            
                                await _unitOfWork.QuestionRepository.AddAsync(q);
                                await _unitOfWork.CompleteAsync();
                                if (question.Answers != null)
                                {
                                    foreach (var answer in question.Answers)
                                    {
                                        Answer r = new Answer()
                                        {
                                            QuestionId=q.ID,
                                           AnswerText=answer.AnswerText,
                                           IsCorrect=answer.IsCorrect,
                                           CreatedAt= DateTime.Now,
                                           IsDeleted=false

                                        };
                                        await _unitOfWork.AnswerRepository.AddAsync(r);

                                      //  await _unitOfWork.CompleteAsync();
                                    }
                                }
                            }
                        }


                        await _unitOfWork.CompleteAsync();
                    }
                }
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.Created;
                response.Result = "Content saved successfully";
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
        //http://localhost:5168/api/API/Chapters/2/lesson
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
        //http://localhost:5168/api/API/Courses/2/CourseConfigs
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
        //http://localhost:5168/api/API/Courses/11/QuestionsConfigs
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
        //http://localhost:5168/api/API/Courses/AllCoursesDetails
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
                            CourseId=cqq.CourseId,
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


        [HttpGet("Courses/SpecificCourseDetails/{CourseId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //http://localhost:5168/api/API/Courses/SpecificCourseDetails/{CourseId:int}
        public async Task<ActionResult<APIResponse>> getSpecificCourseDetails(int CourseId)
        {
            try
            {
                var course = await _unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && c.ID==CourseId,
            new[] { "Category", "Level", "CourseConfig", "CourseQuestionsConfig.QuestionLevel" }
        );
                if (course == null)
                {
                    response.IsSuccess = false;
                    response.Errors.Add("Course not found");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(response);
                }

              
                var chapters = await _unitOfWork.ChapterRepository.GetAllQuery(ch => !ch.IsDeleted && ch.CourseId==CourseId)
                    .ToListAsync();
                var chapterIds = chapters.Select(ch => ch.ID).ToList();
                var lessons = await _unitOfWork.LessonRepository.GetAllQuery(l => !l.IsDeleted && chapterIds.Contains(l.ChapterId))
                    .ToListAsync();

                var res = new CoursesDetailsDTO
                {
                    BookStorageURL = course.BookStorageURL,
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Name ?? "Unknown",
                    Name = course.Name,
                    CourseId = course.ID,
                    Details = course.Details,
                    LevelId = course.LevelId,
                    LevelName = course.Level?.Name ?? "Unknown",
                    ChaptersCount = course.CourseConfig?.ChaptersCount ?? 0,
                    LessonsCountPerChapter = course.CourseConfig?.LessonsCountPerChapter ?? 0,
                    VideoDurationInMin = course.CourseConfig?.VideoDurationInMin ?? 0,
                    Language = course.CourseConfig?.Language.ToString() ?? "Unknown",
                    Persona = course.CourseConfig?.Persona.ToString() ?? "Unknown",
                    Chapters = chapters
                        .Where(ch => ch.CourseId == course.ID)
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
                    CourseQuestionConfig = course.CourseQuestionsConfig
                        .Where(cq => !cq.IsDeleted)
                        .Select(cqq => new CourseQuestionConfigDTO
                        {
                            QuestionLevelId = cqq.QuestionLevelId,
                            QuestionLevelName = cqq.QuestionLevel?.Name ?? "Unknown",
                            QuestionsCountPerLesson = cqq.QuestionsCountPerLesson
                        }).ToList()
                };



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


        /* http://localhost:5168/api/API/CourseById/15 */
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

        /* http://localhost:5168/api/API/AllCourses */
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



        /* http://localhost:5168/api/API/Courses/15/Chapters */
        [HttpGet("Courses/{courseId:int}/Chapters")]
        public async Task<ActionResult<APIResponse>> GetChaptersByCourseId(int courseId)
        {
            var response = new APIResponse();

            var chapters = await _unitOfWork.ChapterRepository.GetAllAsync(
                c => c.CourseId == courseId && !c.IsDeleted
            );
        

            if (chapters == null || !chapters.Any())
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("No chapters found for this course.");
                return NotFound(response);
            }

            var chapterDtos = _mapper.Map<List<_ChapterDTO>>(chapters);

            response.Result = chapterDtos;
            response.StatusCode = HttpStatusCode.OK;

            return Ok(response);
        }



        /* http://localhost:5168/api/API/CourseDetails/15 *//*//shaghal bas 3amlt comment 3shan a3ml check 3ala ely feeha kol el details bel lessons wa el chapters
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
        }*/


        /* http://localhost:5168/api/API/Content-generated/PhaseOne */
        [HttpPost("Content-generated/PhaseOne")]
        public async Task<ActionResult<APIResponse>> AddCourseContentPhaseOne([FromBody] PhaseOneCreateDTO dto)
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

            foreach (var chapterDto in dto.Chapters)
            {
                Chapter chapter;

                if (chapterDto.ChapterId > 0)
                {
                    chapter = await _unitOfWork.ChapterRepository.GetByIdAsync(chapterDto.ChapterId);
                    if (chapter == null) continue;
                }
                else
                {
                    chapter = _mapper.Map<Chapter>(chapterDto);
                    chapter.CourseId = dto.CourseId;
                    await _unitOfWork.ChapterRepository.AddAsync(chapter);
                }

                if (chapterDto.Lessons != null)
                {
                    foreach (var lessonDto in chapterDto.Lessons)
                    {
                        Lesson lesson;

                        if (lessonDto.LessonId > 0)
                        {
                            lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonDto.LessonId);
                            if (lesson == null) continue;

                            lesson.ScriptText = lessonDto.ScriptText;
                            lesson.VideoStorageURL = lessonDto.VideoStorageURL;
                            lesson.AudioStorageURL = lessonDto.AudioStorageURL;

                            _unitOfWork.LessonRepository.Update(lesson);
                        }
                        else
                        {
                            lesson = _mapper.Map<Lesson>(lessonDto);
                            lesson.ChapterId = chapter.ID;
                            await _unitOfWork.LessonRepository.AddAsync(lesson);
                        }

                        if (lessonDto.Questions != null)
                        {
                            foreach (var questionDto in lessonDto.Questions)
                            {
                                var question = _mapper.Map<Question>(questionDto);
                                question.LessonId = lesson.ID;

                                await _unitOfWork.QuestionRepository.AddAsync(question);
                                await _unitOfWork.CompleteAsync(); // ✅ Flush to get question.ID

                                if (questionDto.Answers != null)
                                {
                                    foreach (var answerDto in questionDto.Answers)
                                    {
                                        var answer = _mapper.Map<Answer>(answerDto);
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
            return CreatedAtAction(nameof(getSpecificCourseDetails), new { id = dto.CourseId }, response);
        }


        /*

         {
          "CourseId": 2,
          "Chapters": [
            {
              "ChapterId": 1,
              "Lessons": [
                {
                  "LessonId": 5,
                  "VideoUrl": "HIIIIIIIIhttps://yourstorageaccount.blob.core.windows.net/videos/lesson_5.mp4"
                },
                {
                  "LessonId": 2,
                  "VideoUrl": "HIIIIIIIIhttps://yourstorageaccount.blob.core.windows.net/videos/lesson_1.mp4"
                }
              ]
            }
          ]
        }

         */





        [HttpPost("create-full-course")]
        public async Task<ActionResult<APIResponse>> CreateFullCourse([FromBody] FinalGeneratedContent dto)
        {
            var response = new APIResponse();
            Console.WriteLine(JsonSerializer.Serialize(dto));


            if (dto.Chapters != null && dto.Chapters.Any())
             {
                   var chapters = _mapper.Map<List<Chapter>>(dto.Chapters);

            foreach (var chapter in chapters)
            {
                   chapter.CourseId = dto.Id;//courseid
                    chapter.CreatedBy= "lobna.mohamed@gmail.com";

                          await _unitOfWork.ChapterRepository.AddAsync(chapter);

                           if (chapter.Lessons != null)
                           {
                               foreach (var lesson in chapter.Lessons)
                               {
                                   lesson.ChapterId = chapter.ID;
                                   lesson.CreatedBy= "lobna.mohamed@gmail.com";
                                   await _unitOfWork.LessonRepository.AddAsync(lesson);

                                    if (lesson.Questions != null)
                                    {
                                       foreach (var question in lesson.Questions)
                                       {
                                           question.LessonId = lesson.ID;
                                           question.CreatedBy= "lobna.mohamed@gmail.com";
                                            await _unitOfWork.QuestionRepository.AddAsync(question);

                                            if (question.Answers != null)
                                           {
                                              foreach (var answer in question.Answers)
                                               {
                                                   answer.QuestionId = question.ID;
                                                   answer.CreatedBy= "lobna.mohamed@gmail.com";
                                                 await _unitOfWork.AnswerRepository.AddAsync(answer);
                                              }
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
            return CreatedAtAction(nameof(getSpecificCourseDetails), new { id = dto.Id }, response);
        }



        //[HttpPost("send-to-ai")]
        //public async Task<IActionResult> SendCourseDataToAi([FromBody] JsonElement courseData)
        //{
        //    var json = courseData.GetRawText();

        //    var client = _httpClientFactory.CreateClient();
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    string aiBackendUrl = "https://httpbin.org/post";

        //    var response = await client.PostAsync(aiBackendUrl, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        return Ok(new { message = "Data sent to AI backend successfully", aiResponse = responseBody });
        //    }
        //    else
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, new { error = "Failed to send data to AI backend", details = error });
        //    }
        //}

        //[HttpPost("send-course/{courseId}")]
        //public async Task<IActionResult> SendCourseToAI(int courseId)
        //{
        //    var course = await _unitOfWork.CourseRepository
        //        .GetCourseWithConfigsAsync(courseId);

        //    if (course == null)
        //        return NotFound("Course not found.");

        //    var courseDto = _mapper.Map<CourseGenerationDTO>(course);

        //    var httpClient = _httpClientFactory.CreateClient();
        //    var json = JsonSerializer.Serialize(courseDto);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync("https://httpbin.org/post", content);
        //    var result = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode((int)response.StatusCode, result);

        //    return Ok(result);
        //}

    }


}

