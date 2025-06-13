"""
Database operations module for Course Generation System
Handles all CRUD operations for the SQLite database
"""

import sqlite3
import json
from typing import List, Dict, Optional, Tuple
from datetime import datetime

class DatabaseManager:
    def __init__(self, db_path: str = "course_generation.db"):
        self.db_path = db_path
    
    def get_connection(self):
        """Get database connection"""
        conn = sqlite3.connect(self.db_path)
        conn.row_factory = sqlite3.Row  # Enable dict-like access to rows
        return conn
    
    def create_course(self, title: str, description: str = None, textbook_filename: str = None) -> int:
        """Create a new course and return its ID"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        cursor.execute("""
            INSERT INTO courses (title, description, textbook_filename, total_chapters, total_lessons)
            VALUES (?, ?, ?, 0, 0)
        """, (title, description, textbook_filename))
        
        course_id = cursor.lastrowid
        conn.commit()
        conn.close()
        
        return course_id
    
    def create_chapter(self, course_id: int, name: str, details: str, chapter_order: int) -> int:
        """Create a new chapter and return its ID"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        cursor.execute("""
            INSERT INTO chapters (course_id, name, details, chapter_order)
            VALUES (?, ?, ?, ?)
        """, (course_id, name, details, chapter_order))
        
        chapter_id = cursor.lastrowid
        conn.commit()
        conn.close()
        
        return chapter_id
    
    def create_lesson(self, chapter_id: int, name: str, details: str, script_text: str, 
                     lesson_order: int, audio_path: str = None, video_storage_url: str = None) -> int:
        """Create a new lesson and return its ID"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        cursor.execute("""
            INSERT INTO lessons (chapter_id, name, details, script_text, audio_path, video_storage_url, lesson_order)
            VALUES (?, ?, ?, ?, ?, ?, ?)
        """, (chapter_id, name, details, script_text, audio_path, video_storage_url, lesson_order))
        
        lesson_id = cursor.lastrowid
        conn.commit()
        conn.close()
        
        return lesson_id
    
    def create_question(self, lesson_id: int, question_text: str, question_instructions: str,
                       question_type: int, question_level_id: int, question_order: int) -> int:
        """Create a new question and return its ID"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        cursor.execute("""
            INSERT INTO questions (lesson_id, question_text, question_instructions, question_type, question_level_id, question_order)
            VALUES (?, ?, ?, ?, ?, ?)
        """, (lesson_id, question_text, question_instructions, question_type, question_level_id, question_order))
        
        question_id = cursor.lastrowid
        conn.commit()
        conn.close()
        
        return question_id
    
    def create_answer(self, question_id: int, answer_text: str, is_correct: bool, answer_order: int) -> int:
        """Create a new answer and return its ID"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        cursor.execute("""
            INSERT INTO answers (question_id, answer_text, is_correct, answer_order)
            VALUES (?, ?, ?, ?)
        """, (question_id, answer_text, is_correct, answer_order))
        
        answer_id = cursor.lastrowid
        conn.commit()
        conn.close()
        
        return answer_id
    
    def update_lesson_media_paths(self, lesson_id: int, audio_path: str = None, video_storage_url: str = None):
        """Update lesson with audio and video paths after AI processing"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        if audio_path and video_storage_url:
            cursor.execute("""
                UPDATE lessons SET audio_path = ?, video_storage_url = ?, updated_at = CURRENT_TIMESTAMP
                WHERE id = ?
            """, (audio_path, video_storage_url, lesson_id))
        elif audio_path:
            cursor.execute("""
                UPDATE lessons SET audio_path = ?, updated_at = CURRENT_TIMESTAMP
                WHERE id = ?
            """, (audio_path, lesson_id))
        elif video_storage_url:
            cursor.execute("""
                UPDATE lessons SET video_storage_url = ?, updated_at = CURRENT_TIMESTAMP
                WHERE id = ?
            """, (video_storage_url, lesson_id))
        
        conn.commit()
        conn.close()
    
    def update_course_totals(self, course_id: int):
        """Update course with total chapters and lessons count"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        # Count chapters
        cursor.execute("SELECT COUNT(*) FROM chapters WHERE course_id = ?", (course_id,))
        total_chapters = cursor.fetchone()[0]
        
        # Count lessons
        cursor.execute("""
            SELECT COUNT(*) FROM lessons l 
            JOIN chapters c ON l.chapter_id = c.id 
            WHERE c.course_id = ?
        """, (course_id,))
        total_lessons = cursor.fetchone()[0]
        
        # Update course
        cursor.execute("""
            UPDATE courses SET total_chapters = ?, total_lessons = ?, updated_at = CURRENT_TIMESTAMP
            WHERE id = ?
        """, (total_chapters, total_lessons, course_id))
        
        conn.commit()
        conn.close()
    
    def get_course_with_full_structure(self, course_id: int) -> Dict:
        """Get complete course structure with chapters, lessons, and questions"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        # Get course info
        cursor.execute("SELECT * FROM courses WHERE id = ?", (course_id,))
        course_row = cursor.fetchone()
        
        if not course_row:
            conn.close()
            return None
        
        course = dict(course_row)
        course['chapters'] = []
        
        # Get chapters
        cursor.execute("""
            SELECT * FROM chapters WHERE course_id = ? ORDER BY chapter_order
        """, (course_id,))
        chapters = cursor.fetchall()
        
        for chapter_row in chapters:
            chapter = dict(chapter_row)
            chapter['lessons'] = []
            
            # Get lessons for this chapter
            cursor.execute("""
                SELECT * FROM lessons WHERE chapter_id = ? ORDER BY lesson_order
            """, (chapter['id'],))
            lessons = cursor.fetchall()
            
            for lesson_row in lessons:
                lesson = dict(lesson_row)
                lesson['questions'] = []
                
                # Get questions for this lesson
                cursor.execute("""
                    SELECT * FROM questions WHERE lesson_id = ? ORDER BY question_order
                """, (lesson['id'],))
                questions = cursor.fetchall()
                
                for question_row in questions:
                    question = dict(question_row)
                    question['answers'] = []
                    
                    # Get answers for this question
                    cursor.execute("""
                        SELECT * FROM answers WHERE question_id = ? ORDER BY answer_order
                    """, (question['id'],))
                    answers = cursor.fetchall()
                    
                    question['answers'] = [dict(answer) for answer in answers]
                    lesson['questions'].append(question)
                
                chapter['lessons'].append(lesson)
            
            course['chapters'].append(chapter)
        
        conn.close()
        return course
    
    def get_course_api_format(self, course_id: int) -> Dict:
        """Get course in the API response format"""
        course_data = self.get_course_with_full_structure(course_id)
        
        if not course_data:
            return None
        
        # Transform to API format
        api_response = {
            "Id": course_data['id'],
            "chapters": []
        }
        
        for chapter in course_data['chapters']:
            api_chapter = {
                "name": chapter['name'],
                "details": chapter['details'],
                "lessons": []
            }
            
            for lesson in chapter['lessons']:
                api_lesson = {
                    "name": lesson['name'],
                    "details": lesson['details'],
                    "scriptText": lesson['script_text'],
                    "videoStorageURL": lesson['video_storage_url'] or "",
                    "questions": []
                }
                
                for question in lesson['questions']:
                    api_question = {
                        "questionText": question['question_text'],
                        "questionInstructions": question['question_instructions'],
                        "questionType": question['question_type'],
                        "questionLevelId": question['question_level_id'],
                        "answers": []
                    }
                    
                    for answer in question['answers']:
                        api_answer = {
                            "answerText": answer['answer_text'],
                            "isCorrect": bool(answer['is_correct'])
                        }
                        api_question['answers'].append(api_answer)
                    
                    api_lesson['questions'].append(api_question)
                
                api_chapter['lessons'].append(api_lesson)
            
            api_response['chapters'].append(api_chapter)
        
        return api_response

# Test the database manager
if __name__ == "__main__":
    db_manager = DatabaseManager()
    
    # Test creating a course
    course_id = db_manager.create_course("Test Course", "A test course", "test.pdf")
    print(f"Created course with ID: {course_id}")
    
    # Test getting course in API format
    api_data = db_manager.get_course_api_format(1)  # Get the sample course
    print(f"API format data: {json.dumps(api_data, indent=2)}")

