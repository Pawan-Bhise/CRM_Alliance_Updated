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
        var category = byId('categoryId');
        var form = byId('formId');
        var customer = byId('customerId');
        var btnStart = byId('btnStartResponse');

        if (!template || !category || !form || !btnStart) {
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
            if (!category.value) {
                alert('Please select a category.');
                return;
            }

            if (!form.value) {
                alert('Please select a survey form.');
                return;
            }

            var url = '/Survey/SurveyResponse/Fill?formId=' + encodeURIComponent(form.value) + '&categoryId=' + encodeURIComponent(category.value);
            if (customer && customer.value) {
                url += '&customerId=' + encodeURIComponent(customer.value);
            }

            window.location.href = url;
        });
    }

    function initFillPage() {
        var responseForm = byId('surveyResponseForm');
        if (!responseForm) {
            return;
        }

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
            // initialize datepicker and sortable for ranking
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
                    cursor: 'move'
                });
                $('.ranking-list').disableSelection();
            }
    });
})();
