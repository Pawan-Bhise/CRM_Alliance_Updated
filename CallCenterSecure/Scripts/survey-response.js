(function () {
    function byId(id) {
        return document.getElementById(id);
    }

    function fetchJson(url, onSuccess, onError) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== 4) {
                return;
            }

            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    onSuccess(JSON.parse(xhr.responseText || '[]'));
                } catch (error) {
                    if (onError) {
                        onError('Invalid response from server.');
                    }
                }
            } else if (onError) {
                onError('Unable to load data (HTTP ' + xhr.status + ').');
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

    function loadCustomerGrid(templateId, formId) {
        var table = byId('surveyCustomerTable');
        var message = byId('customerGridMessage');
        var customerId = byId('customerId');
        if (!table) {
            return;
        }

        if (typeof $ !== 'undefined' && $.fn.DataTable && $.fn.DataTable.isDataTable(table)) {
            $(table).DataTable().destroy();
        }

        var tbody = table.querySelector('tbody');
        tbody.innerHTML = '';
        if (message) {
            message.textContent = templateId ? 'Loading customers...' : 'Select a survey template to load customers.';
        }

        if (!templateId) {
            return;
        }

        var customerUrl = '/Survey/SurveyResponse/GetCustomers?templateId=' + encodeURIComponent(templateId);
        if (formId) {
            customerUrl += '&formId=' + encodeURIComponent(formId);
        }

        fetchJson(customerUrl, function (items) {
            items.forEach(function (item, index) {
                var row = document.createElement('tr');
                var values = [
                    '<input type="radio" name="selectedCustomer" value="' + item.Id + '" aria-label="Select customer"' + (customerId && customerId.value === String(item.Id) ? ' checked' : '') + '>',
                    '<button type="button" class="btn btn-sm btn-outline-primary btn-edit-customer" data-customer-id="' + item.Id + '" data-call-status-id="' + (item.CallStatusId || '') + '" data-form-status="' + (item.FormStatus || '') + '" data-call-remarks="' + (item.CallRemarks || '').replace(/"/g, '&quot;') + '">Edit</button>',
                    index + 1,
                    item.ClientName,
                    item.Gender,
                    item.Product,
                    item.CustomerCode,
                    item.MobileNumber1,
                    item.MobileNumber2,
                    item.RegionState,
                    item.Branch,
                    item.BusinessCategory,
                    item.ActivitiesSector,
                    item.LoanCycle,
                    item.DisbursedAmount,
                    item.CallStatus,
                    item.FormStatus,
                    item.CallRemarks
                ];

                values.forEach(function (value, valueIndex) {
                    var cell = document.createElement('td');
                    if (valueIndex === 0 || valueIndex === 1) {
                        cell.innerHTML = value;
                    } else {
                        cell.textContent = value === null || typeof value === 'undefined' ? '' : value;
                    }
                    row.appendChild(cell);
                });

                row.querySelector('input[name="selectedCustomer"]').addEventListener('change', function () {
                    if (customerId) {
                        customerId.value = this.value;
                    }
                });
                tbody.appendChild(row);
            });

            if (message) {
                message.textContent = items.length + ' customer(s) loaded.';
            }

            if (typeof $ !== 'undefined' && $.fn.DataTable) {
                $(table).DataTable({
                    pageLength: 10,
                    order: [[3, 'asc']],
                    scrollX: false,
                    autoWidth: false,
                    columnDefs: [
                        { targets: [0, 1, 2, 3, 4, 5, 12, 13, 14, 15, 16, 17], className: 'survey-grid-cell' }
                    ]
                });
            }

            table.querySelectorAll('.btn-edit-customer').forEach(function (button) {
                button.addEventListener('click', function () {
                    openCustomerStatusEditor(this, templateId, formId || '');
                });
            });
        }, function (errorMessage) {
            if (message) {
                message.textContent = errorMessage + ' Check that the survey status tables are installed.';
                message.className = 'text-danger small';
            }
        });
    }

    function openCustomerStatusEditor(button, templateId, formId) {
        if (!formId) {
            alert('Please select a survey form before editing customer status.');
            return;
        }

        var modal = byId('customerStatusModal');
        var customerId = byId('editCustomerId');
        var templateInput = byId('editTemplateId');
        var formInput = byId('editFormId');
        var callStatus = byId('editCallStatusId');
        var formStatus = byId('editFormStatus');
        var remarks = byId('editCallRemarks');
        if (!modal || !customerId || !templateInput || !formInput || !callStatus || !formStatus || !remarks) {
            return;
        }

        customerId.value = button.getAttribute('data-customer-id') || '';
        templateInput.value = templateId || '';
        formInput.value = formId || '';
        remarks.value = button.getAttribute('data-call-remarks') || '';
        formStatus.value = button.getAttribute('data-form-status') || 'Not Started';
        callStatus.innerHTML = '<option value="">Select call status</option>';

        fetchJson('/Survey/SurveyResponse/GetStatusOptions', function (data) {
            (data.CallStatuses || []).forEach(function (status) {
                var option = document.createElement('option');
                option.value = status.Id;
                option.textContent = status.Name;
                callStatus.appendChild(option);
            });
            callStatus.value = button.getAttribute('data-call-status-id') || '';
        });

        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(modal).show();
        } else {
            modal.style.display = 'block';
        }
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
                customer.value = '';
            }
            loadCustomerGrid(templateId, '');

            if (!templateId) {
                return;
            }

            fetchJson('/Survey/SurveyResponse/GetForms?templateId=' + encodeURIComponent(templateId), function (items) {
                fillSelect(form, items, 'Title');
                if (items.length === 1) {
                    form.value = items[0].Id;
                    loadCustomerGrid(templateId, form.value);
                }
            });

        });

        form.addEventListener('change', function () {
            loadCustomerGrid(template.value, form.value);
        });

        loadCustomerGrid(template.value, form.value);

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
        var questionTypeInput = questionCard.querySelector('input[name$=".QuestionType"]');
        var questionType = questionTypeInput ? questionTypeInput.value : '';

        if (questionType === 'Multiple Choice Grid') {
            var multipleChoiceRows = questionCard.querySelectorAll('input[type="radio"][name*="GridAnswers["]');
            var multipleChoiceRowNames = {};
            for (var multipleChoiceIndex = 0; multipleChoiceIndex < multipleChoiceRows.length; multipleChoiceIndex++) {
                multipleChoiceRowNames[multipleChoiceRows[multipleChoiceIndex].name] = true;
            }

            for (var multipleChoiceName in multipleChoiceRowNames) {
                if (!questionCard.querySelector('input[type="radio"][name="' + multipleChoiceName + '"]:checked')) {
                    return false;
                }
            }

            return Object.keys(multipleChoiceRowNames).length > 0;
        }

        if (questionType === 'Checkbox Grid') {
            var checkboxGridRows = questionCard.querySelectorAll('input[type="checkbox"][name*="GridAnswers["]');
            var checkboxGridRowNames = {};
            for (var checkboxGridIndex = 0; checkboxGridIndex < checkboxGridRows.length; checkboxGridIndex++) {
                checkboxGridRowNames[checkboxGridRows[checkboxGridIndex].name] = true;
            }

            var checkboxGridRowsByIndex = {};
            for (var checkboxGridInputIndex = 0; checkboxGridInputIndex < checkboxGridRows.length; checkboxGridInputIndex++) {
                var checkboxGridName = checkboxGridRows[checkboxGridInputIndex].name;
                var rowMatch = checkboxGridName.match(/GridAnswers\[(\d+)\]/);
                if (!rowMatch) {
                    continue;
                }

                checkboxGridRowsByIndex[rowMatch[1]] = checkboxGridRowsByIndex[rowMatch[1]] || [];
                checkboxGridRowsByIndex[rowMatch[1]].push(checkboxGridRows[checkboxGridInputIndex]);
            }

            for (var checkboxGridRowIndex in checkboxGridRowsByIndex) {
                if (!checkboxGridRowsByIndex[checkboxGridRowIndex].some(function (input) { return input.checked; })) {
                    return false;
                }
            }

            return Object.keys(checkboxGridRowsByIndex).length > 0;
        }

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

            if (input.type === 'file') {
                if (input.files && input.files.length > 0) {
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

    function validateRequiredSurveyQuestions(responseForm) {
        if (!responseForm) {
            return true;
        }

        var cards = responseForm.querySelectorAll('.survey-question-card');
        for (var i = 0; i < cards.length; i++) {
            var card = cards[i];
            if (card.style.display === 'none') {
                continue;
            }

            var requiredInput = card.querySelector('input[name$=".IsRequired"]');
            if (!requiredInput || requiredInput.value !== 'True' && requiredInput.value !== 'true') {
                continue;
            }

            if (!hasAnswerValue(card)) {
                var requiredLabel = card.querySelector('.form-label.fw-bold');
                var questionText = requiredLabel ? requiredLabel.textContent.replace(/\*\s*$/, '').trim() : 'This question';
                alert('Please answer the required question: ' + questionText);
                var firstField = card.querySelector('input:not([type="hidden"]), textarea, select');
                if (firstField) {
                    firstField.focus();
                }
                return false;
            }
        }

        return true;
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
            if (!validateRequiredSurveyQuestions(responseForm)) {
                event.preventDefault();
                return;
            }

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
