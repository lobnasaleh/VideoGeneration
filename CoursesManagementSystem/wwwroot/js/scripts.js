// LMS Functionality
//new 5/7/2025
document.addEventListener('DOMContentLoaded', function () {
	// Initialize tooltips
	var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
	tooltipTriggerList.map(function (tooltipTriggerEl) {
		return new bootstrap.Tooltip(tooltipTriggerEl)
	});

	// Quiz functionality
	const quizForms = document.querySelectorAll('.quiz-form');

	quizForms.forEach(form => {
		form.addEventListener('submit', function (e) {
			e.preventDefault();

			// Get all questions in this form
			const questions = this.querySelectorAll('.question-card');
			let score = 0;
			let totalQuestions = questions.length;

			questions.forEach(question => {
				const questionId = question.getAttribute('data-question-id');
				const selectedOption = document.querySelector(`input[name="question-${questionId}"]:checked`);
				const correctAnswer = question.getAttribute('data-correct');

				// Reset previous feedback
				question.querySelectorAll('.list-group-item').forEach(option => {
					option.classList.remove('list-group-item-success', 'list-group-item-danger');
				});

				if (selectedOption) {
					const selectedValue = selectedOption.value;
					const selectedItem = selectedOption.closest('.list-group-item');

					// Highlight correct answer
					const correctItem = question.querySelector(`input[value="${correctAnswer}"]`).closest('.list-group-item');
					correctItem.classList.add('list-group-item-success');

					if (selectedValue === correctAnswer) {
						score++;
					} else {
						// Highlight incorrect selection
						selectedItem.classList.add('list-group-item-danger');
					}
				} else {
					// If no selection was made, just highlight the correct answer
					question.querySelector(`input[value="${correctAnswer}"]`).closest('.list-group-item').classList.add('list-group-item-success');
				}
			});

			// Show result
			const resultDiv = this.querySelector('.quiz-result');
			resultDiv.innerHTML = `<div class="alert alert-info mt-3">Your score: ${score} / ${totalQuestions}</div>`;
			resultDiv.classList.remove('d-none');

			// Scroll to result
			resultDiv.scrollIntoView({ behavior: 'smooth' });

			// Disable form submission after grading
			const submitBtn = this.querySelector('button[type="submit"]');
			submitBtn.disabled = true;
			submitBtn.innerText = 'Quiz Completed';
		});
	});

	// Chapter accordion functionality
	const chapterToggles = document.querySelectorAll('.chapter-toggle');

	chapterToggles.forEach(toggle => {
		toggle.addEventListener('click', function () {
			const icon = this.querySelector('i');
			if (icon.classList.contains('fa-chevron-down')) {
				icon.classList.remove('fa-chevron-down');
				icon.classList.add('fa-chevron-up');
			} else {
				icon.classList.remove('fa-chevron-up');
				icon.classList.add('fa-chevron-down');
			}
		});
	});
});
