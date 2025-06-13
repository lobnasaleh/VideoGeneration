from fastapi import FastAPI
from fastapi.responses import JSONResponse
from pydantic import BaseModel
from typing import List
import json
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, HttpUrl
from typing import List
app = FastAPI()




# @app.get("/")
# def read_root():
#     return {"Hello": "World"}

# @app.get("https://edrak-app.azurewebsites.net/api/API/Courses/AllCoursesDetails") # This handles GET requests to /
# async def root():
#     print("GET request received at /")
#     return {"message": "FastAPI server is running!"}

@app.post("/simple-post")
async def simple_post():
    """
    The simplest POST endpoint.
    It just confirms a POST request was received.
    """
    return {"message": "POST request received successfully!"}

@app.get("/status")
def get_status():
    return {"status": "Model server is up and running"}



# # Pydantic model for courseQuestionConfig items
# class QuestionConfig(BaseModel):
#     QuestionLevelId: int
#     QuestionLevelName: str
#     QuestionsCountPerLesson: int

# # Pydantic model for the full course data
# class CourseData(BaseModel):
#     courseId: int
#     name: str
#     details: str
#     bookStorageURL: str
#     chaptersCount: int
#     lessonsCountPerChapter: int
#     videoDurationInMin: int
#     language: str
#     persona: str
#     courseQuestionConfig: List[QuestionConfig]

# @app.post("/receive-data")
# async def receive_data(data: CourseData):
#     """
#     Endpoint to receive and process course data via POST request.
#     Validates the JSON payload and logs the data.
#     """
#     try:
#         # Log the received data
#         print("\n--- New POST Request Received ---")
#         print("Course Data:")
#         print(json.dumps(data.dict(), indent=2))

#         # Example processing (e.g., extract specific fields)
#         course_id = data.courseId
#         course_name = data.name
#         question_configs = data.courseQuestionConfig

#         # Optional: Add logic to process or store the data
#         # For example, save to a database or perform calculations
#         # db.save_course(data.dict())  # Placeholder for database storage

#         print(f"Processed course: ID={course_id}, Name={course_name}, Question Configs={len(question_configs)}")
#         print("--- End Request ---")

#         # Return success response
#         return JSONResponse(
#             {
#                 "status": "success",
#                 "message": f"Course data for {course_name} received and processed.",
#                 "received_course_id": course_id
#             },
#             status_code=200
#         )
#     except Exception as e:
#         # Log and return error response
#         print(f"Error processing request: {e}")
#         return JSONResponse(
#             {"status": "error", "message": f"Error processing course data: {str(e)}"},
#             status_code=500
#         )





# ----------------------------POSTMAN TEST-------------

# class CourseQuestionConfig(BaseModel):
#     QuestionLevelId: int
#     QuestionLevelName: str
#     QuestionsCountPerLesson: int

# class CourseData(BaseModel):
#     courseId: int
#     name: str
#     details: str
#     bookStorageURL: HttpUrl
#     chaptersCount: int
#     lessonsCountPerChapter: int
#     videoDurationInMin: int
#     language: str
#     persona: str
#     courseQuestionConfig: List[CourseQuestionConfig]

# @app.post("/courses/")
# async def receive_course_data(course_data: CourseData):
#     # Log or process the received data
#     print("Received course data:", course_data)

#     # You can add logic here to save the data to a database or pass to a model
#     return {"message": "Course data received successfully", "courseName": course_data.name}
# ---------------------------------------------------------------

from fastapi import FastAPI
from pydantic import BaseModel, Field
from typing import List



class CourseQuestionConfigToAiDTO(BaseModel):
    QuestionsCountPerLesson: int = Field(..., alias="QuestionsCountPerLesson")
    QuestionLevelId: int = Field(..., alias="QuestionLevelId")
    QuestionLevelName: str = Field(..., alias="QuestionLevelName")

class CourseGenerationDTO(BaseModel):
    CourseId: int = Field(..., alias="CourseId")
    Name: str = Field(..., alias="Name")
    Details: str = Field(..., alias="Details")
    BookStorageURL: str = Field(..., alias="BookStorageURL")  # not HttpUrl since it's a relative path
    ChaptersCount: int = Field(..., alias="ChaptersCount")
    LessonsCountPerChapter: int = Field(..., alias="LessonsCountPerChapter")
    VideoDurationInMin: int = Field(..., alias="VideoDurationInMin")
    Language: str = Field(..., alias="Language")
    Persona: str = Field(..., alias="Persona")
    CourseQuestionConfig: List[CourseQuestionConfigToAiDTO] = Field(..., alias="CourseQuestionConfig")

    class Config:
        allow_population_by_field_name = True
        allow_population_by_alias = True

@app.post("/receive-course-config")
async def receive_course_config(course: CourseGenerationDTO):
    print("✅ Received course:")
    print(f"  CourseId: {course.CourseId}")
    print(f"  Name: {course.Name}")
    print(f"  ChaptersCount: {course.ChaptersCount}")
    print(f"  Questions: {[q.QuestionLevelName for q in course.CourseQuestionConfig]}")

    return {
        "message": "Course received successfully",
        "CourseId": course.CourseId
    }

# ------------------------------------------------------------------

from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
import requests

# ------------ SCHEMA DEFINITIONS ------------

class Answer(BaseModel):
    answerText: str
    isCorrect: bool

class Question(BaseModel):
    questionText: str
    questionInstructions: str
    questionType: int
    questionLevelId: int
    answers: List[Answer]

class Lesson(BaseModel):
    name: str
    details: str
    scriptText: str
    videoStorageURL: str
    questions: List[Question]

class Chapter(BaseModel):
    name: str
    details: str
    lessons: List[Lesson]

class FinalGeneratedContent(BaseModel):
    Id: int
    chapters: List[Chapter]

# ------------ API SETUP ------------
from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
import requests

# @app.post("/send-course-to-dotnet")
# def send_course():
#     fake_payload = {
#         "Id": 57,
#         "chapters": [
#             {
#                 "name": "Chapter 1: Basics of AI",
#                 "details": "This chapter covers the basics of AI.",
#                 "lessons": [
#                     {
#                         "name": "Lesson 1: What is AI?",
#                         "details": "Understanding the definition and scope of AI.",
#                         "scriptText": "Artificial Intelligence (AI) refers to the simulation of human intelligence in machines.",
#                         "videoStorageURL": "https://storage.example.com/videos/lesson1.mp4",
#                         "questions": [
#                             {
#                                 "questionText": "What does AI stand for?",
#                                 "questionInstructions": "Choose the correct option.",
#                                 "questionType": 1,
#                                 "questionLevelId": 7,
#                                 "answers": [
#                                     { "answerText": "Artificial Intelligence", "isCorrect": True },
#                                     { "answerText": "Automated Interface", "isCorrect": False },
#                                     { "answerText": "Analog Input", "isCorrect": False }
#                                 ]
#                             },
#                             {
#                                 "questionText": "AI simulates human intelligence in machines.",
#                                 "questionInstructions": "Select true or false.",
#                                 "questionType": 2,
#                                 "questionLevelId": 8,
#                                 "answers": [
#                                     { "answerText": "True", "isCorrect": True },
#                                     { "answerText": "False", "isCorrect": False }
#                                 ]
#                             }
#                         ]
#                     }
#                 ]
#             }
#         ]
#     }

#     dotnet_api_url = "https://edrak-app.azurewebsites.net/api/API/create-full-course"
#     headers = { "Content-Type": "application/json" }

#     response = requests.post(dotnet_api_url, json=fake_payload, headers=headers)

#     return {
#         "status_code": response.status_code,
#         "response_text": response.text
#     }

import requests

# This data is what they generated after processing (fake sample below)
final_course_data = {
    "Id": 57,  # courseId
    "chapters": [
        {
            "name": "Chapter 1: Basics of AI",
            "details": "This chapter covers the basics of AI.",
            "lessons": [
                {
                    "name": "Lesson 1: What is AI?",
                    "details": "Understanding the definition and scope of AI.",
                    "scriptText": "AI refers to the simulation of human intelligence in machines.",
                    "videoStorageURL": "https://storage.example.com/videos/lesson1.mp4",
                    "questions": [
                        {
                            "questionText": "What does AI stand for?",
                            "questionInstructions": "Choose the correct option.",
                            "questionType": 1,
                            "questionLevelId": 7,
                            "answers": [
                                {"answerText": "Artificial Intelligence", "isCorrect": True},
                                {"answerText": "Automated Interface", "isCorrect": False}
                            ]
                        }
                    ]
                }
            ]
        }
    ]
}

# URL of your .NET endpoint
url = "https://edrak-app.azurewebsites.net/api/API/create-full-course"  # Replace with your real endpoint

# Send it
response = requests.post(url, json=final_course_data)

# Debug
print("Response:", response.status_code)
print("Content:", response.text)