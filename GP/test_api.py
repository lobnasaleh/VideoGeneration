"""
Test script for Course Generation API
Tests the main endpoints and functionality
"""

import requests
import json
import time
import os

# API base URL
BASE_URL = "http://localhost:8000"

def test_health_check():
    """Test the health check endpoint"""
    print("Testing health check endpoint...")
    response = requests.get(f"{BASE_URL}/health")
    print(f"Status: {response.status_code}")
    print(f"Response: {response.json()}")
    return response.status_code == 200

def test_course_generation():
    """Test course generation with file upload"""
    print("\nTesting course generation endpoint...")
    
    # Create a sample textbook file
    textbook_content = """
    Chapter 1: Introduction to Artificial Intelligence
    
    Artificial Intelligence (AI) is a branch of computer science that aims to create intelligent machines.
    These machines can perform tasks that typically require human intelligence, such as visual perception,
    speech recognition, decision-making, and language translation.
    
    The history of AI dates back to the 1950s when Alan Turing proposed the famous Turing Test.
    This test was designed to determine whether a machine could exhibit intelligent behavior equivalent
    to, or indistinguishable from, that of a human.
    
    Chapter 2: Machine Learning Fundamentals
    
    Machine Learning is a subset of AI that provides systems the ability to automatically learn
    and improve from experience without being explicitly programmed. It focuses on the development
    of computer programs that can access data and use it to learn for themselves.
    """
    
    # Save textbook content to a file
    with open("sample_textbook.txt", "w") as f:
        f.write(textbook_content)
    
    # Prepare form data
    form_data = {
        "title": "AI Fundamentals Course",
        "description": "A comprehensive course on AI and Machine Learning basics",
        "num_chapters": 2,
        "num_lessons_per_chapter": 2,
        "num_questions_per_lesson": 3,
        "difficulty_levels": json.dumps([7, 8, 9]),
        "question_types": json.dumps([1, 2])  # MCQ and True/False
    }
    
    # Prepare file upload
    files = {
        "textbook": ("sample_textbook.txt", open("sample_textbook.txt", "rb"), "text/plain")
    }
    
    try:
        response = requests.post(f"{BASE_URL}/generate-course", data=form_data, files=files)
        print(f"Status: {response.status_code}")
        print(f"Response: {response.json()}")
        
        if response.status_code == 200:
            course_id = response.json().get("course_id")
            print(f"Course ID: {course_id}")
            return course_id
        else:
            print("Course generation failed")
            return None
            
    except Exception as e:
        print(f"Error: {str(e)}")
        return None
    finally:
        files["textbook"][1].close()
        if os.path.exists("sample_textbook.txt"):
            os.remove("sample_textbook.txt")

def test_course_status(course_id):
    """Test course status endpoint"""
    if not course_id:
        return
    
    print(f"\nTesting course status for course ID: {course_id}")
    
    # Poll status until completion or timeout
    max_attempts = 30
    attempt = 0
    
    while attempt < max_attempts:
        try:
            response = requests.get(f"{BASE_URL}/course-status/{course_id}")
            print(f"Status check {attempt + 1}: {response.status_code}")
            
            if response.status_code == 200:
                status_data = response.json()
                print(f"Status: {status_data['status']}")
                print(f"Current step: {status_data['current_step']}")
                print(f"Progress: {status_data['progress_percentage']}%")
                
                if status_data['status'] in ['completed', 'failed']:
                    break
            
            time.sleep(2)
            attempt += 1
            
        except Exception as e:
            print(f"Error checking status: {str(e)}")
            break

def test_get_course(course_id):
    """Test getting the generated course data"""
    if not course_id:
        return
    
    print(f"\nTesting get course data for course ID: {course_id}")
    
    try:
        response = requests.get(f"{BASE_URL}/course/{course_id}")
        print(f"Status: {response.status_code}")
        
        if response.status_code == 200:
            course_data = response.json()
            print(f"Course ID: {course_data['Id']}")
            print(f"Number of chapters: {len(course_data['chapters'])}")
            
            for i, chapter in enumerate(course_data['chapters']):
                print(f"Chapter {i+1}: {chapter['name']}")
                print(f"  Lessons: {len(chapter['lessons'])}")
                
                for j, lesson in enumerate(chapter['lessons']):
                    print(f"    Lesson {j+1}: {lesson['name']}")
                    print(f"      Questions: {len(lesson['questions'])}")
        else:
            print("Failed to get course data")
            
    except Exception as e:
        print(f"Error: {str(e)}")

def run_all_tests():
    """Run all tests"""
    print("Starting API tests...")
    
    # Test health check
    if not test_health_check():
        print("Health check failed. Make sure the API server is running.")
        return
    
    # Test course generation
    course_id = test_course_generation()
    
    # Test status checking
    test_course_status(course_id)
    
    # Test getting course data
    test_get_course(course_id)
    
    print("\nAll tests completed!")

if __name__ == "__main__":
    run_all_tests()

