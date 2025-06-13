# Course Generation API - Project Structure

```
course-generation-api/
├── main.py                 # FastAPI application entry point
├── models.py              # Pydantic models for API requests/responses
├── database_setup.py      # Database initialization script
├── database_manager.py    # Database CRUD operations
├── ai_orchestrator.py     # AI model orchestration pipeline
├── test_api.py           # Comprehensive API test suite
├── requirements.txt      # Python dependencies
├── deploy.sh            # Deployment script
├── README.md            # Complete documentation
├── course_generation.db # SQLite database (created after setup)
└── course_outputs/      # Generated course files directory
    ├── {course_id}/
    │   ├── audio/       # Generated audio files
    │   ├── videos/      # Generated video files
    │   └── rag_output.json # RAG model output
    └── ...
```

## Key Files Description

### Core Application Files

- **main.py**: The main FastAPI application containing all API endpoints, CORS configuration, and background task management
- **models.py**: Pydantic models that define the structure of API requests and responses, ensuring type safety
- **database_manager.py**: Comprehensive database operations class with methods for CRUD operations and data retrieval
- **ai_orchestrator.py**: The core AI pipeline that coordinates RAG, TTS, and Lipsync model execution

### Setup and Testing

- **database_setup.py**: Creates the SQLite database schema and inserts sample data for testing
- **test_api.py**: Complete test suite that validates all API endpoints and functionality
- **deploy.sh**: Automated deployment script for easy setup
- **requirements.txt**: All required Python packages with specific versions

### Generated Content

- **course_outputs/**: Directory containing all generated course materials organized by course ID
- **course_generation.db**: SQLite database file containing all course data and metadata

## Quick Start

1. Run the deployment script:
   ```bash
   ./deploy.sh
   ```

2. Start the API server:
   ```bash
   python3 main.py
   ```

3. Access the API documentation:
   ```
   http://localhost:8000/docs
   ```

## Integration Points

### AI Model Integration

Replace the placeholder implementations in `ai_orchestrator.py`:

1. **RAG Model**: Update the `RAGModel.generate_course()` method
2. **TTS Model**: Update the `TTSModel.text_to_speech()` method  
3. **Lipsync Model**: Update the `LipsyncModel.generate_video()` method

### Database Customization

Modify `database_setup.py` to:
- Add new tables or columns
- Change data types or constraints
- Add indexes for performance optimization

### API Extension

Extend `main.py` to:
- Add new endpoints
- Implement authentication
- Add custom middleware
- Configure additional CORS settings

