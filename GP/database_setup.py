"""
Database Schema Design for Course Generation System

Based on the API structure and RAG output, the database needs to store:
1. Course metadata
2. Chapters with details
3. Lessons with script text and video URLs
4. Questions with answers

Tables:
1. courses - Main course information
2. chapters - Chapter information linked to courses
3. lessons - Lesson information linked to chapters
4. questions - Questions linked to lessons
5. answers - Answer options linked to questions
"""

import sqlite3
import os
from datetime import datetime

def create_database(db_path="course_generation.db"):
    """Create SQLite database with all required tables"""
    
    # Remove existing database if it exists
    if os.path.exists(db_path):
        os.remove(db_path)
    
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    # Create courses table
    cursor.execute("""
        CREATE TABLE courses (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT,
            description TEXT,
            textbook_filename TEXT,
            total_chapters INTEGER,
            total_lessons INTEGER
        )
    """)
    
    # Create chapters table
    cursor.execute("""
        CREATE TABLE chapters (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            course_id INTEGER NOT NULL,
            name TEXT ,
            details TEXT,      
            chapter_order INTEGER ,
            FOREIGN KEY (course_id) REFERENCES courses (id) ON DELETE CASCADE
        )
    """)
    
    # Create lessons table
    cursor.execute("""
        CREATE TABLE lessons (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            chapter_id INTEGER NOT NULL,
            name TEXT ,
            details TEXT,
            script_text TEXT ,
            audio_path TEXT,
            video_storage_url TEXT,
            lesson_order INTEGER ,
            FOREIGN KEY (chapter_id) REFERENCES chapters (id) ON DELETE CASCADE
        )
    """)
    
    # Create questions table
    cursor.execute("""
        CREATE TABLE questions (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            lesson_id INTEGER NOT NULL,
            question_text TEXT ,
            question_instructions TEXT,
            question_type INTEGER NOT NULL,  -- 1 for MCQ, 2 for True/False
            question_level_id INTEGER NOT NULL,
            FOREIGN KEY (lesson_id) REFERENCES lessons (id) ON DELETE CASCADE
        )
    """)
    
    # Create answers table
    cursor.execute("""
        CREATE TABLE answers (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            question_id INTEGER NOT NULL,
            answer_text TEXT NOT NULL,
            is_correct BOOLEAN NOT NULL DEFAULT FALSE,
            FOREIGN KEY (question_id) REFERENCES questions (id) ON DELETE CASCADE
        )
    """)
    
    # Create indexes for better performance
    cursor.execute("CREATE INDEX idx_chapters_course_id ON chapters(course_id)")
    cursor.execute("CREATE INDEX idx_lessons_chapter_id ON lessons(chapter_id)")
    cursor.execute("CREATE INDEX idx_questions_lesson_id ON questions(lesson_id)")
    cursor.execute("CREATE INDEX idx_answers_question_id ON answers(question_id)")
    
    conn.commit()
    conn.close()
    
    print(f"Database created successfully at: {db_path}")
    return db_path

def insert_sample_data(db_path="course_generation.db"):
    """Insert sample data to test the database schema"""
    
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    # Insert sample course
    cursor.execute("""
        INSERT INTO courses (title, description, textbook_filename, total_chapters, total_lessons)
        VALUES (?, ?, ?, ?, ?)
    """, ("AI Fundamentals", "A comprehensive course on AI basics", "ai_textbook.pdf", 3, 6))
    
    course_id = cursor.lastrowid
    
    # Insert sample chapter
    cursor.execute("""
        INSERT INTO chapters (course_id, name, details, chapter_order)
        VALUES (?, ?, ?, ?)
    """, (course_id, "Chapter 1: Basics of AI", "This chapter covers the basics of AI.", 1))
    
    chapter_id = cursor.lastrowid
    
    # Insert sample lesson
    cursor.execute("""
        INSERT INTO lessons (chapter_id, name, details, script_text, video_storage_url, lesson_order)
        VALUES (?, ?, ?, ?, ?, ?)
    """, (chapter_id, "Lesson 1: What is AI?", "Understanding the definition and scope of AI.", 
          "Artificial Intelligence (AI) refers to the simulation of human intelligence in machines.", 
          "https://storage.example.com/videos/lesson1.mp4", 1))
    
    lesson_id = cursor.lastrowid
    
    # Insert sample question
    cursor.execute("""
        INSERT INTO questions (lesson_id, question_text, question_instructions, question_type, question_level_id, question_order)
        VALUES (?, ?, ?, ?, ?, ?)
    """, (lesson_id, "What does AI stand for?", "Choose the correct option.", 1, 7, 1))
    
    question_id = cursor.lastrowid
    
    # Insert sample answers
    answers = [
        ("Artificial Intelligence", True, 1),
        ("Automated Interface", False, 2),
        ("Analog Input", False, 3)
    ]
    
    for answer_text, is_correct, order in answers:
        cursor.execute("""
            INSERT INTO answers (question_id, answer_text, is_correct, answer_order)
            VALUES (?, ?, ?, ?)
        """, (question_id, answer_text, is_correct, order))
    
    conn.commit()
    conn.close()
    
    print("Sample data inserted successfully")

if __name__ == "__main__":
    # Create database and insert sample data
    db_path = create_database()
    insert_sample_data(db_path)
    
    # Verify the database structure
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    # Get table names
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
    tables = cursor.fetchall()
    print(f"\nCreated tables: {[table[0] for table in tables]}")
    
    # Check sample data
    cursor.execute("SELECT COUNT(*) FROM courses")
    course_count = cursor.fetchone()[0]
    print(f"Sample courses: {course_count}")
    
    conn.close()

