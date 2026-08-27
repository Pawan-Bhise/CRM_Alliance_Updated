(function () {
    function byId(id) {
        return document.getElementById(id);
    }

    function fetchJson(url, onSuccess) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== 4) {
                return;
            }

            if (xhr.status >= 200 && xhr.status < 300) {
                onSuccess(JSON.parse(xhr.responseText || '[]'));
            }
        };
        xhr.send();
    }

    function fillSelect(select, items, textKey) {
        if (!select) {
            return;
        }

        while (select.options.length > 1) {
            select.remove(1);
        }

        items.forEach(function (item) {
            var option = document.createElement('option');
            option.value = item.Id;
            option.text = item[textKey];
            select.appendChild(option);
        });
    }

    function initStartPage() {
        var template = byId('templateId');
        var form = byId('formId');
        var customer = byId('customerId');
        var btnStart = byId('btnStartResponse');

        if (!template || !form || !btnStart) {
            return;
        }

        template.addEventListener('change', function () {
            var templateId = template.value;
            fillSelect(form, [], 'Title');
            if (customer) {
                fillSelect(customer, [], 'Name');
            }

            if (!templateId) {
                return;
            }

            fetchJson('/Survey/SurveyResponse/GetForms?templateId=' + encodeURIComponent(templateId), function (items) {
                fillSelect(form, items, 'Title');
            });

            if (customer) {
                fetchJson('/Survey/SurveyResponse/GetCustomers?templateId=' + encodeURIComponent(templateId), function (items) {
                    fillSelect(customer, items, 'Name');
                });
            }
        });

        btnStart.addEventListener('click', function () {
            if (!form.value) {
                alert('Please select a survey form.');
                return;
            }

            var url = '/Survey/SurveyResponse/Fill?formId=' + encodeURIComponent(form.value);
            if (customer && customer.value) {
                url += '&customerId=' + encodeURIComponent(customer.value);
            }

            window.location.href = url;
        });
    }

    function serializeRankingSelections(responseForm) {
        if (!responseForm || typeof $ === 'undefined') {
            return;
        }

        $('.ranking-list').each(function () {
            var $list = $(this);
            var qIdx = $list.data('question-index');
            var $hidden = $('.ranking-hidden[data-question-index="' + qIdx + '"]');
            $hidden.empty();

            var items = $list.children();
            for (var k = 0; k < items.length; k++) {
                var val = $(items[k]).data('value');
                if (val) {
                    var input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = 'Questions[' + qIdx + '].SelectedOptions';
                    input.value = val;
                    $hidden.append(input);
                }
            }
        });
    }

    function updateRatingVisual(input) {
        if (!input || input.name.indexOf('AnswerText') === -1) {
            return;
        }

        var rating = parseInt(input.value, 10);
        var ratingContainer = input.closest('.survey-rating');
        if (!ratingContainer) {
            return;
        }

        var options = ratingContainer.querySelectorAll('.survey-rating-option');
        for (var index = 0; index < options.length; index++) {
            options[index].classList.toggle('is-selected', index < rating);
        }
    }

    function shouldTriggerOnBlur(input) {
        if (!input || !input.name || input.type === 'hidden' || input.type === 'file') {
            return false;
        }

        return input.type === 'text' || input.type === 'textarea' || input.type === 'number' || input.type === 'email' || input.type === 'tel' || input.type === 'date' || input.tagName && input.tagName.toLowerCase() === 'textarea';
    }

    function hasAnswerValue(questionCard) {
        var rankingList = questionCard.querySelector('.ranking-list');
        if (rankingList) {
            var completed = rankingList.getAttribute('data-completed') === 'true';
            if (!completed) {
                return false;
            }

            var rankingInputs = questionCard.querySelectorAll('.ranking-hidden input[name*="SelectedOptions"]');
            if (rankingInputs && rankingInputs.length > 0) {
                return true;
            }
        }

        var inputs = questionCard.querySelectorAll('input, select, textarea');

        for (var idx = 0; idx < inputs.length; idx++) {
            var input = inputs[idx];

            if (!input.name || input.name.indexOf('Questions[') !== 0) {
                continue;
            }

            if (input.type === 'hidden') {
                continue;
            }

            if (/(SurveyQuestionId|QuestionType|DisplayOrder|QuestionText|IsRequired|MinValue|MaxValue|MinLabel|MaxLabel)$/.test(input.name)) {
                continue;
            }

            if (input.type === 'radio' || input.type === 'checkbox') {
                if (input.checked && String(input.value).trim()) {
                    return true;
                }
                continue;
            }

            if (input.type !== 'file' && String(input.value || '').trim()) {
                return true;
            }
        }

        return false;
    }

    function updateConditionalVisibility(responseForm) {
        if (!responseForm) {
            return;
        }

        var questionCards = Array.prototype.slice.call(responseForm.querySelectorAll('.survey-question-card'));
        questionCards.forEach(function (card, index) {
            if (index === 0) {
                card.style.display = '';
                return;
            }

            var previousCard = questionCards[index - 1];
            if (!previousCard) {
                card.style.display = 'none';
                return;
            }

            var previousVisible = previousCard.style.display !== 'none';
            var previousAnswered = hasAnswerValue(previousCard);
            card.style.display = (previousVisible && previousAnswered) ? '' : 'none';
        });
    }

    function initFillPage() {
        var responseForm = byId('surveyResponseForm');
        if (!responseForm) {
            return;
        }

        responseForm.querySelectorAll('.survey-question-card').forEach(function (card) {
            var inputs = card.querySelectorAll('input, select, textarea');
            inputs.forEach(function (input) {
                input.addEventListener('change', function () {
                    updateRatingVisual(input);
                    serializeRankingSelections(responseForm);
                    updateConditionalVisibility(responseForm);
                });

                if (shouldTriggerOnBlur(input)) {
                    input.addEventListener('blur', function () {
                        serializeRankingSelections(responseForm);
                        updateConditionalVisibility(responseForm);
                    });
                } else if (input.type === 'radio' || input.type === 'checkbox') {
                    input.addEventListener('click', function () {
                        updateRatingVisual(input);
                        serializeRankingSelections(responseForm);
                        updateConditionalVisibility(responseForm);
                    });
                } else if (input.type === 'select-one' || input.tagName && input.tagName.toLowerCase() === 'select') {
                    input.addEventListener('change', function () {
                        serializeRankingSelections(responseForm);
                        updateConditionalVisibility(responseForm);
                    });
                }
            });
        });

        responseForm.querySelectorAll('.survey-rating input:checked').forEach(updateRatingVisual);

        updateConditionalVisibility(responseForm);

        responseForm.addEventListener('submit', function (event) {
            // For date fields that only have a date, append current local time so server receives a datetime
            $('.datepicker').each(function () {
                if (this.value && this.value.length === 10) {
                    const now = new Date();
                    this.value += ' ' + now.toTimeString().split(' ')[0];
                }
            });

            // For ranking lists, serialize the ordered items into hidden inputs named Questions[<index>].SelectedOptions
            $('.ranking-list').each(function () {
                var $list = $(this);
                var qIdx = $list.data('question-index');
                // remove any existing hidden inputs for this question
                $('.ranking-hidden[data-question-index="' + qIdx + '"]').empty();
                var items = $list.children();
                for (var k = 0; k < items.length; k++) {
                    var val = $(items[k]).data('value');
                    if (val) {
                        var input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = 'Questions[' + qIdx + '].SelectedOptions';
                        input.value = val;
                        $('.ranking-hidden[data-question-index="' + qIdx + '"]').append(input);
                    }
                }
            });

            var uploads = responseForm.querySelectorAll('.response-upload');
            for (var i = 0; i < uploads.length; i++) {
                var input = uploads[i];
                if (!input.files || input.files.length === 0) {
                    continue;
                }

                var file = input.files[0];
                var ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
                var allowed = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.jpg', '.jpeg', '.png'];
                if (allowed.indexOf(ext) === -1) {
                    event.preventDefault();
                    alert('Unsupported file type: ' + file.name);
                    return;
                }

                if (file.size > 5 * 1024 * 1024) {
                    event.preventDefault();
                    alert('File too large (max 5MB): ' + file.name);
                    return;
                }
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initStartPage();
        initFillPage();

        var responseForm = byId('surveyResponseForm');

        if (typeof $ !== 'undefined' && typeof $.fn.datepicker === 'function') {
            $('.datepicker').datepicker({
                dateFormat: 'yy-mm-dd',
                changeMonth: true,
                changeYear: true,
                yearRange: '1900:2050',
                autoclose: true
            });
        }

        if (typeof $ !== 'undefined' && typeof $.fn.sortable === 'function') {
            $('.ranking-list').sortable({
                placeholder: 'list-group-item placeholder',
                cursor: 'move',
                stop: function () {
                    var $list = $(this);
                    $list.attr('data-completed', 'true');
                    serializeRankingSelections(responseForm);
                    updateConditionalVisibility(responseForm);
                }
            });
            $('.ranking-list').disableSelection();
        }
    });
})();
