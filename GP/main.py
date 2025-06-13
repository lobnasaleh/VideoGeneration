"""
FastAPI Application for Course Generation System
Main application file with API endpoints
"""

from fastapi import FastAPI, HTTPException, BackgroundTasks, UploadFile, File, Form
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
import uvicorn
import os
import json
from typing import Optional
import asyncio
import logging

from models import (
    CourseGenerationRequest, 
    CourseGenerationResponse, 
    CourseResponseModel,
    ProcessingStatus,
    ErrorResponse
)
from database_manager import DatabaseManager

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize FastAPI app
app = FastAPI(
    title="Course Generation API",
    description="API for generating courses from textbooks using AI models",
    version="1.0.0"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allow all origins for development
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize database manager
db_manager = DatabaseManager()

# Global dictionary to track processing status
processing_status = {}

@app.get("/")
async def root():
    """Root endpoint"""
    return {"message": "Course Generation API is running", "version": "1.0.0"}

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "database": "connected"}

@app.post("/generate-course", response_model=CourseGenerationResponse)
async def generate_course(
    background_tasks: BackgroundTasks,
    title: str = Form(...),
    description: Optional[str] = Form(None),
    num_chapters: int = Form(...),
    num_lessons_per_chapter: int = Form(...),
    num_questions_per_lesson: int = Form(...),
    difficulty_levels: str = Form(...),  # JSON string of list
    question_types: str = Form(...),     # JSON string of list
    textbook: UploadFile = File(...)
):
    """
    Generate a course from uploaded textbook
    This endpoint accepts file upload and form data
    """
    try:
        # Validate file
        if not textbook.filename:
            raise HTTPException(status_code=400, detail="No file uploaded")
        
        # Read textbook content
        textbook_content = await textbook.read()
        textbook_text = textbook_content.decode('utf-8')
        
        # Parse JSON strings
        try:
            difficulty_levels_list = json.loads(difficulty_levels)
            question_types_list = json.loads(question_types)
        except json.JSONDecodeError:
            raise HTTPException(status_code=400, detail="Invalid JSON format for difficulty_levels or question_types")
        
        # Create request object
        request_data = CourseGenerationRequest(
            title=title,
            description=description,
            textbook_content=textbook_text,
            textbook_filename=textbook.filename,
            num_chapters=num_chapters,
            num_lessons_per_chapter=num_lessons_per_chapter,
            num_questions_per_lesson=num_questions_per_lesson,
            difficulty_levels=difficulty_levels_list,
            question_types=question_types_list
        )
        
        # Create course in database
        course_id = db_manager.create_course(
            title=request_data.title,
            description=request_data.description,
            textbook_filename=request_data.textbook_filename
        )
        
        # Initialize processing status
        processing_status[course_id] = {
            "status": "processing",
            "current_step": "Initializing",
            "progress_percentage": 0,
            "estimated_time_remaining": None
        }
        
        # Start background processing
        background_tasks.add_task(process_course_generation, course_id, request_data)
        
        return CourseGenerationResponse(
            success=True,
            message=f"Course generation started. Course ID: {course_id}",
            course_id=course_id
        )
        
    except Exception as e:
        logger.error(f"Error in generate_course: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/course-status/{course_id}", response_model=ProcessingStatus)
async def get_course_status(course_id: int):
    """Get the processing status of a course"""
    if course_id not in processing_status:
        raise HTTPException(status_code=404, detail="Course not found")
    
    status_data = processing_status[course_id]
    return ProcessingStatus(
        course_id=course_id,
        status=status_data["status"],
        current_step=status_data["current_step"],
        progress_percentage=status_data["progress_percentage"],
        estimated_time_remaining=status_data.get("estimated_time_remaining")
    )

@app.get("/course/{course_id}", response_model=CourseResponseModel)
async def get_course(course_id: int):
    """Get the generated course data"""
    try:
        course_data = db_manager.get_course_api_format(course_id)
        
        if not course_data:
            raise HTTPException(status_code=404, detail="Course not found")
        
        return course_data
        
    except Exception as e:
        logger.error(f"Error in get_course: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.delete("/course/{course_id}")
async def delete_course(course_id: int):
    """Delete a course and all its data"""
    try:
        # Note: This would require implementing a delete method in DatabaseManager
        # For now, return a placeholder response
        return {"message": f"Course {course_id} deletion requested"}
        
    except Exception as e:
        logger.error(f"Error in delete_course: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

async def process_course_generation(course_id: int, request_data: CourseGenerationRequest):
    """
    Background task to process course generation
    This function will orchestrate the AI models
    """
    try:
        # Import AI orchestration module (will be created in next phase)
        from ai_orchestrator import AIOrchestrator
        
        orchestrator = AIOrchestrator(db_manager, course_id)
        
        # Update status
        processing_status[course_id].update({
            "current_step": "Starting AI processing",
            "progress_percentage": 5
        })
        
        # Process the course
        await orchestrator.process_course(request_data, processing_status)
        
        # Update final status
        processing_status[course_id].update({
            "status": "completed",
            "current_step": "Course generation completed",
            "progress_percentage": 100,
            "estimated_time_remaining": 0
        })
        
        logger.info(f"Course {course_id} generation completed successfully")
        
    except Exception as e:
        logger.error(f"Error processing course {course_id}: {str(e)}")
        processing_status[course_id].update({
            "status": "failed",
            "current_step": f"Error: {str(e)}",
            "progress_percentage": 0
        })

if __name__ == "__main__":
    # Run the application
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info"
    )

