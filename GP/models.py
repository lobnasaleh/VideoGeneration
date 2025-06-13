"""
Pydantic models for Course Generation API
Defines request and response models for the FastAPI application
"""

from pydantic import BaseModel, Field
from typing import List, Optional
from enum import IntEnum

class QuestionType(IntEnum):
    MCQ = 1
    TRUE_FALSE = 2

class AnswerModel(BaseModel):
    answerText: str
    isCorrect: bool

class QuestionModel(BaseModel):
    questionText: str
    questionInstructions: str
    questionType: int  # 1 for MCQ, 2 for True/False
    questionLevelId: int
    answers: List[AnswerModel]

class LessonModel(BaseModel):
    name: str
    details: str
    scriptText: str
    videoStorageURL: str = ""
    questions: List[QuestionModel]

class ChapterModel(BaseModel):
    name: str
    details: str
    lessons: List[LessonModel]

class CourseResponseModel(BaseModel):
    Id: int
    chapters: List[ChapterModel]

# Request models for course generation
class CourseGenerationRequest(BaseModel):
    title: str = Field(..., description="Title of the course")
    description: Optional[str] = Field(None, description="Description of the course")
    textbook_content: str = Field(..., description="Content of the uploaded textbook")
    textbook_filename: str = Field(..., description="Original filename of the textbook")
    num_chapters: int = Field(..., ge=1, le=20, description="Number of chapters to generate")
    num_lessons_per_chapter: int = Field(..., ge=1, le=10, description="Number of lessons per chapter")
    num_questions_per_lesson: int = Field(..., ge=1, le=20, description="Number of questions per lesson")
    difficulty_levels: List[int] = Field(..., description="List of difficulty level IDs to use")
    question_types: List[int] = Field(..., description="List of question types (1=MCQ, 2=True/False)")

class CourseGenerationResponse(BaseModel):
    success: bool
    message: str
    course_id: Optional[int] = None
    course_data: Optional[CourseResponseModel] = None

# Status models for tracking progress
class ProcessingStatus(BaseModel):
    course_id: int
    status: str  # "processing", "completed", "failed"
    current_step: str
    progress_percentage: int
    estimated_time_remaining: Optional[int] = None  # in seconds

class ErrorResponse(BaseModel):
    success: bool = False
    message: str
    error_code: Optional[str] = None

