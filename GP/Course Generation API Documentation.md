# Course Generation API Documentation

## Overview

The Course Generation API is a comprehensive FastAPI-based backend system designed to automate the creation of educational courses from uploaded textbooks using artificial intelligence models. This system orchestrates multiple AI components including Retrieval-Augmented Generation (RAG), Text-to-Speech (TTS), and Lipsync models to produce complete courses with video lessons and interactive quizzes.

## System Architecture

### Core Components

The system consists of several interconnected components that work together to transform textbook content into engaging video-based courses:

**FastAPI Application (`main.py`)**: The central web server that handles HTTP requests, manages file uploads, and coordinates the course generation process. It provides RESTful endpoints for course creation, status monitoring, and data retrieval.

**Database Layer (`database_setup.py`, `database_manager.py`)**: A SQLite-based persistence layer that stores course metadata, chapter information, lesson content, questions, and answers. The database schema is designed to support the hierarchical structure of courses with proper foreign key relationships.

**AI Orchestration Pipeline (`ai_orchestrator.py`)**: The core processing engine that coordinates the sequential execution of AI models. It manages the workflow from textbook analysis through content generation to media production.

**Data Models (`models.py`)**: Pydantic models that define the structure of API requests and responses, ensuring type safety and data validation throughout the system.

### Database Schema

The database consists of five main tables that represent the hierarchical structure of educational content:

- **courses**: Stores course metadata including title, description, and textbook information
- **chapters**: Contains chapter information linked to specific courses
- **lessons**: Holds lesson details including script text and media file paths
- **questions**: Stores quiz questions associated with lessons
- **answers**: Contains answer options for each question with correctness indicators

## API Endpoints

### Health Check
```
GET /health
```
Returns the system health status and database connectivity information.

### Course Generation
```
POST /generate-course
```
Initiates the course generation process by accepting a textbook file and configuration parameters.

**Request Parameters:**
- `title` (string): Course title
- `description` (string, optional): Course description
- `num_chapters` (integer): Number of chapters to generate
- `num_lessons_per_chapter` (integer): Number of lessons per chapter
- `num_questions_per_lesson` (integer): Number of questions per lesson
- `difficulty_levels` (JSON string): Array of difficulty level IDs
- `question_types` (JSON string): Array of question types (1=MCQ, 2=True/False)
- `textbook` (file): Uploaded textbook file

**Response:**
```json
{
  "success": true,
  "message": "Course generation started. Course ID: 3",
  "course_id": 3,
  "course_data": null
}
```

### Course Status
```
GET /course-status/{course_id}
```
Returns the current processing status of a course generation request.

**Response:**
```json
{
  "course_id": 3,
  "status": "processing",
  "current_step": "Processing lesson: Lesson 1: Topic 1",
  "progress_percentage": 37,
  "estimated_time_remaining": null
}
```

### Course Retrieval
```
GET /course/{course_id}
```
Returns the complete course data in the specified API format.

**Response:**
```json
{
  "Id": 3,
  "chapters": [
    {
      "name": "Chapter 1: AI Fundamentals Course - Part 1",
      "details": "This chapter covers the fundamental concepts of part 1.",
      "lessons": [
        {
          "name": "Lesson 1: Topic 1",
          "details": "This lesson covers topic 1 of chapter 1.",
          "scriptText": "This is the script content for lesson 1...",
          "videoStorageURL": "https://storage.example.com/videos/ch1_lesson1.mp4",
          "questions": [
            {
              "questionText": "Sample MCQ question 1 for lesson 1?",
              "questionInstructions": "Choose the correct option.",
              "questionType": 1,
              "questionLevelId": 7,
              "answers": [
                {
                  "answerText": "Correct answer",
                  "isCorrect": true
                },
                {
                  "answerText": "Wrong answer 1",
                  "isCorrect": false
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

## AI Model Integration

The system is designed with a modular architecture that allows for easy integration of actual AI models. Currently, the system includes placeholder implementations that demonstrate the expected workflow and data flow.

### RAG Model Integration

The RAG (Retrieval-Augmented Generation) model is responsible for analyzing the uploaded textbook content and generating structured course material. To integrate your RAG model, replace the placeholder implementation in the `RAGModel` class within `ai_orchestrator.py`.

**Expected Input:**
- Textbook content as plain text
- Number of chapters to generate
- Number of lessons per chapter
- Number of questions per lesson

**Expected Output:**
A structured dictionary containing chapters, lessons, and questions in the format demonstrated by the mock implementation.

### TTS Model Integration

The Text-to-Speech model converts lesson script text into audio files. Replace the placeholder implementation in the `TTSModel` class.

**Expected Input:**
- Script text for the lesson
- Voice configuration parameters
- Output format specifications

**Expected Output:**
Audio file data that can be saved to the filesystem or uploaded to cloud storage.

### Lipsync Model Integration

The Lipsync model generates video content by synchronizing audio with visual avatars. Replace the placeholder implementation in the `LipsyncModel` class.

**Expected Input:**
- Audio file path or data
- Script text for reference
- Avatar configuration
- Output format specifications

**Expected Output:**
Video file data or URL to the generated video content.

## Installation and Setup

### Prerequisites

- Python 3.11 or higher
- SQLite (included with Python)
- Required Python packages (see requirements.txt)

### Installation Steps

1. **Clone or download the project files** to your desired directory.

2. **Install required dependencies:**
```bash
pip install -r requirements.txt
```

3. **Initialize the database:**
```bash
python database_setup.py
```

4. **Start the FastAPI server:**
```bash
python main.py
```

The server will start on `http://localhost:8000` by default.

### Configuration

The system uses several configuration parameters that can be modified:

- **Database path**: Modify the `db_path` parameter in `DatabaseManager` initialization
- **Server host and port**: Adjust the `uvicorn.run()` parameters in `main.py`
- **Output directories**: Configure the `output_dir` path in `AIOrchestrator`
- **CORS settings**: Modify the CORS middleware configuration for production deployment

## Testing

The system includes a comprehensive test suite that validates all major functionality:

### Running Tests

Execute the test script to verify system functionality:
```bash
python test_api.py
```

The test suite covers:
- Health check endpoint validation
- Course generation with file upload
- Status monitoring throughout the processing pipeline
- Course data retrieval and validation
- Error handling and edge cases

### Test Results

A successful test run will demonstrate:
- Proper file upload handling
- Background task execution
- Database operations
- API response formatting
- Status tracking accuracy

## Production Deployment

### Security Considerations

For production deployment, implement the following security measures:

1. **Authentication and Authorization**: Add JWT-based authentication to protect API endpoints
2. **Input Validation**: Enhance file upload validation to prevent malicious content
3. **Rate Limiting**: Implement request rate limiting to prevent abuse
4. **CORS Configuration**: Restrict CORS origins to specific domains
5. **HTTPS**: Deploy with SSL/TLS encryption
6. **Database Security**: Use environment variables for sensitive configuration

### Scalability Recommendations

1. **Database Migration**: Consider migrating from SQLite to PostgreSQL or MySQL for production workloads
2. **File Storage**: Implement cloud storage (AWS S3, Google Cloud Storage) for media files
3. **Background Processing**: Use Celery with Redis or RabbitMQ for robust background task management
4. **Load Balancing**: Deploy multiple API instances behind a load balancer
5. **Monitoring**: Implement logging, metrics, and health monitoring

### Environment Variables

Create a `.env` file for production configuration:
```
DATABASE_URL=postgresql://user:password@localhost/coursedb
STORAGE_BUCKET=your-cloud-storage-bucket
API_SECRET_KEY=your-secret-key
CORS_ORIGINS=https://yourdomain.com
```

## Error Handling

The system implements comprehensive error handling at multiple levels:

### API Level Errors

- **400 Bad Request**: Invalid input parameters or malformed requests
- **404 Not Found**: Requested course or resource does not exist
- **500 Internal Server Error**: System errors during processing

### Processing Errors

- **File Upload Errors**: Invalid file formats or corrupted uploads
- **AI Model Errors**: Failures in RAG, TTS, or Lipsync processing
- **Database Errors**: Connection issues or constraint violations

### Error Response Format

```json
{
  "success": false,
  "message": "Detailed error description",
  "error_code": "SPECIFIC_ERROR_CODE"
}
```

## Performance Considerations

### Processing Time

Course generation time depends on several factors:
- Textbook length and complexity
- Number of chapters and lessons requested
- AI model processing speed
- Media generation requirements

Typical processing times:
- Small course (2 chapters, 4 lessons): 2-5 minutes
- Medium course (5 chapters, 15 lessons): 10-20 minutes
- Large course (10+ chapters, 30+ lessons): 30+ minutes

### Resource Requirements

- **CPU**: Multi-core processor recommended for AI model execution
- **Memory**: Minimum 8GB RAM, 16GB+ recommended for large courses
- **Storage**: Adequate space for textbook files, audio, and video output
- **Network**: Stable internet connection for cloud storage integration

## Monitoring and Logging

The system includes comprehensive logging for debugging and monitoring:

### Log Levels

- **INFO**: Normal operation events and status updates
- **WARNING**: Non-critical issues that should be monitored
- **ERROR**: Error conditions that require attention
- **DEBUG**: Detailed information for troubleshooting

### Log Locations

- Application logs: Console output and log files
- Database operations: SQLite logs and query performance
- AI processing: Model execution status and timing
- API requests: Request/response logging with timing

## Future Enhancements

### Planned Features

1. **Advanced Question Types**: Support for fill-in-the-blank, matching, and essay questions
2. **Multi-language Support**: Internationalization for global deployment
3. **Course Templates**: Pre-defined course structures for different subjects
4. **Analytics Dashboard**: Course performance and engagement metrics
5. **Integration APIs**: Webhooks and third-party system integration

### Extensibility

The modular architecture supports easy extension:
- **Custom AI Models**: Plugin architecture for different AI providers
- **Output Formats**: Support for additional media formats and platforms
- **Assessment Types**: Expandable question and assessment frameworks
- **Content Sources**: Integration with various textbook and content formats

This documentation provides a comprehensive guide for understanding, deploying, and extending the Course Generation API system. The modular design ensures that individual components can be enhanced or replaced as requirements evolve.

