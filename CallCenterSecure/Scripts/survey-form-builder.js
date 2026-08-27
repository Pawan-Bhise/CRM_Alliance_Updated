(function () {
    function byId(id) {
        return document.getElementById(id);
    }

    function cloneTemplate(templateId) {
        var template = byId(templateId);
        if (!template) {
            return null;
        }

        // Prefer template.content (supported in modern browsers). Fall back to parsing innerHTML for older browsers.
        if (template.content && template.content.firstElementChild) {
            return template.content.firstElementChild.cloneNode(true);
        }

        // Fallback: create a container and set innerHTML from template's innerHTML
        var wrapper = document.createElement('div');
        wrapper.innerHTML = template.innerHTML.trim();
        var first = wrapper.firstElementChild;
        return first ? first.cloneNode(true) : null;
    }

    function normalizeQuestionType(type) {
        return (type || '').toString().trim();
    }

    function isOptionType(type) {
        var value = normalizeQuestionType(type);
        return value === "Multiple Choice"
            || value === "Checkboxes"
            || value === "Dropdown"
            || value === "NPS (0-10)"
            || value === "Date & Time Picker"
            || value.toLowerCase() === "multiple choice"
            || value.toLowerCase() === "checkboxes"
            || value.toLowerCase() === "dropdown"
            || value.toLowerCase() === "nps (0-10)"
            || value.toLowerCase() === "date & time picker";
    }

    function isGridType(type) {
        var value = normalizeQuestionType(type);
        return value === "Multiple Choice Grid" || value === "Checkbox Grid" || value.toLowerCase() === "multiple choice grid" || value.toLowerCase() === "checkbox grid";
    }

    function renderQuestionPreview(questionEl) {
        var previewSurface = questionEl.querySelector('.preview-surface');
        if (!previewSurface) {
            return;
        }

        var type = normalizeQuestionType(questionEl.querySelector('.question-type').value);
        var text = (questionEl.querySelector('.question-text').value || 'Untitled question').trim();
        var isRequired = questionEl.querySelector('.question-required').checked;

        var html = '<div class="mb-2"><strong>' + (text || 'Untitled question') + (isRequired ? ' *' : '') + '</strong></div>';

        if (type === 'Short Answer' || type === 'Paragraph') {
            html += '<input type="text" class="form-control" value="" placeholder="Sample response" />';
            if (type === 'Paragraph') {
                html += '<textarea class="form-control mt-2" rows="2" placeholder="Long answer"></textarea>';
            }
        } else if (type === 'Multiple Choice' || type === 'Dropdown' || type === 'Checkboxes') {
            var options = Array.prototype.slice.call(questionEl.querySelectorAll('.option-text')).map(function (input) {
                return (input.value || '').trim();
            }).filter(function (v) { return v; });

            if (!options.length) {
                options = ['Option 1', 'Option 2', 'Option 3'];
            }

            if (type === 'Multiple Choice') {
                html += options.map(function (option) {
                    return '<div class="form-check mt-2"><input class="form-check-input" type="radio" disabled /><label class="form-check-label">' + option + '</label></div>';
                }).join('');
            } else if (type === 'Checkboxes') {
                html += options.map(function (option) {
                    return '<div class="form-check mt-2"><input class="form-check-input" type="checkbox" disabled /><label class="form-check-label">' + option + '</label></div>';
                }).join('');
            } else if (type === 'Dropdown') {
                html += '<select class="form-control"><option>' + options.join('</option><option>') + '</option></select>';
            }
        } else if (type === 'Linear Scale' || type === 'NPS (0-10)' || type === 'Ranking') {
            var min = parseInt(questionEl.querySelector('.scale-min-value').value || (type === 'NPS (0-10)' ? '0' : '1'), 10);
            var max = parseInt(questionEl.querySelector('.scale-max-value').value || (type === 'NPS (0-10)' ? '10' : '5'), 10);

            if (type === 'Ranking') {
                var ratingMax = parseInt(questionEl.querySelector('.rating-scale-value').value || '5', 10);
                var ratingOptions = [];
                for (var ratingValue = 1; ratingValue <= ratingMax; ratingValue++) {
                    ratingOptions.push(ratingValue);
                }
                html += '<div class="rating-preview" aria-label="Star rating preview">' + ratingOptions.map(function (value) {
                    return '<span class="rating-preview-item"><span class="rating-star">&#9733;</span><span>' + value + '</span></span>';
                }).join('') + '</div>';
                return previewSurface.innerHTML = html;
            }
            var labels = [
                questionEl.querySelector('.scale-min-label').value || min,
                questionEl.querySelector('.scale-max-label').value || max
            ];

            var range = [];
            for (var v = min; v <= max; v++) {
                range.push('<div class="form-check form-check-inline"><input class="form-check-input" type="radio" disabled /><label class="form-check-label">' + v + '</label></div>');
            }
            html += '<div class="preview-scale">' + range.join('') + '</div>';
            html += '<div class="mt-2 small text-muted">' + labels[0] + ' &nbsp; ' + labels[1] + '</div>';
        } else if (type === 'Date & Time Picker') {
            html += '<input type="text" class="form-control" value="" placeholder="YYYY-MM-DD" />';
        } else if (type === 'Multiple Choice Grid' || type === 'Checkbox Grid') {
            var rows = Array.prototype.slice.call(questionEl.querySelectorAll('.grid-row-text')).map(function (input) { return (input.value || '').trim(); }).filter(Boolean);
            var columns = Array.prototype.slice.call(questionEl.querySelectorAll('.grid-column-text')).map(function (input) { return (input.value || '').trim(); }).filter(Boolean);

            if (!rows.length) rows = ['Row 1', 'Row 2'];
            if (!columns.length) columns = ['Option 1', 'Option 2'];

            html += '<table class="preview-grid-table"><thead><tr><th></th>' + columns.map(function (col) { return '<th>' + col + '</th>'; }).join('') + '</tr></thead><tbody>' + rows.map(function (row) {
                return '<tr><td>' + row + '</td>' + columns.map(function () { return '<td><input type="' + (type === 'Checkbox Grid' ? 'checkbox' : 'radio') + '" disabled /></td>'; }).join('') + '</tr>';
            }).join('') + '</tbody></table>';
        } else {
            html += '<input type="text" class="form-control" value="" placeholder="Sample response" />';
        }

        previewSurface.innerHTML = html;
    }

    function updateQuestionVisibility(questionEl) {
        var type = normalizeQuestionType(questionEl.querySelector(".question-type").value);
        var optionWrapper = questionEl.querySelector(".option-wrapper");
        var linearWrapper = questionEl.querySelector(".linear-wrapper");
        var gridWrapper = questionEl.querySelector(".grid-wrapper");
        var rankingWrapper = questionEl.querySelector(".ranking-wrapper");
        var scaleConfigs = questionEl.querySelectorAll(".scale-config");
        var isRanking = type === "Ranking" || type.toLowerCase() === "ranking";

        if (optionWrapper) {
            optionWrapper.style.display = isOptionType(type) ? "block" : "none";
        }

        if (linearWrapper) {
            linearWrapper.style.display = (type === "Linear Scale" || type === "NPS (0-10)" || isRanking || type.toLowerCase() === "linear scale" || type.toLowerCase() === "nps (0-10)") ? "flex" : "none";
        }

        scaleConfigs.forEach(function (scaleConfig) {
            scaleConfig.style.display = isRanking ? "none" : "block";
        });

        if (rankingWrapper) {
            rankingWrapper.style.display = isRanking ? "block" : "none";
        }

        if (gridWrapper) {
            gridWrapper.style.display = isGridType(type) ? "block" : "none";
        }

        if (isOptionType(type)) {
            var listEl = questionEl.querySelector('.option-list');
            if (listEl && listEl.children.length === 0) {
                addOption(listEl);
                addOption(listEl);
            }
        }

        if (type === "NPS (0-10)" || type.toLowerCase() === "nps (0-10)") {
            var minEl = questionEl.querySelector('.scale-min-value');
            var maxEl = questionEl.querySelector('.scale-max-value');
            if (minEl && !minEl.value) minEl.value = '0';
            if (maxEl && !maxEl.value) maxEl.value = '10';
        }

        if (isRanking) {
            var rankingMinEl = questionEl.querySelector('.scale-min-value');
            var rankingMaxEl = questionEl.querySelector('.scale-max-value');
            var ratingScaleEl = questionEl.querySelector('.rating-scale-value');
            if (rankingMinEl) rankingMinEl.value = '1';
            if (rankingMaxEl && !rankingMaxEl.value) rankingMaxEl.value = '5';
            if (ratingScaleEl && !ratingScaleEl.value) ratingScaleEl.value = '5';
        }

        renderQuestionPreview(questionEl);
    }

    function addOption(listEl) {
        var optionEl = cloneTemplate("optionTemplate");
        if (optionEl) {
            listEl.appendChild(optionEl);
        }
    }

    function addGridRow(listEl) {
        var rowEl = cloneTemplate("gridRowTemplate");
        if (rowEl) {
            listEl.appendChild(rowEl);
        }
    }

    function addGridColumn(listEl) {
        var columnEl = cloneTemplate("gridColumnTemplate");
        if (columnEl) {
            listEl.appendChild(columnEl);
        }
    }

    function wireQuestionEvents(questionEl) {
        var btnRemove = questionEl.querySelector(".btn-remove-question");
        if (btnRemove) {
            btnRemove.addEventListener("click", function () {
                questionEl.remove();
                reindexAll();
            });
        }

        var btnUp = questionEl.querySelector(".btn-move-up");
        if (btnUp) {
            btnUp.addEventListener("click", function () {
                var prev = questionEl.previousElementSibling;
                if (prev) {
                    questionEl.parentNode.insertBefore(questionEl, prev);
                    reindexAll();
                }
            });
        }

        var btnDown = questionEl.querySelector(".btn-move-down");
        if (btnDown) {
            btnDown.addEventListener("click", function () {
                var next = questionEl.nextElementSibling;
                if (next) {
                    questionEl.parentNode.insertBefore(next, questionEl);
                    reindexAll();
                }
            });
        }

        var qType = questionEl.querySelector(".question-type");
        if (qType) {
            qType.addEventListener("change", function () {
                updateQuestionVisibility(questionEl);
                reindexAll();
            });
        }

        var qText = questionEl.querySelector(".question-text");
        if (qText) {
            qText.addEventListener("input", function () {
                renderQuestionPreview(questionEl);
                updateAllParentSelectLabels();
            });
            qText.addEventListener("blur", function () {
                updateAllParentSelectLabels();
            });
        }

        var parentSelect = questionEl.querySelector('.conditional-parent-question');
        if (parentSelect) {
            parentSelect.addEventListener('focus', function () {
                populateConditionalParentOptions(questionEl);
            });
        }

        var qReq = questionEl.querySelector(".question-required");
        if (qReq) {
            qReq.addEventListener("change", function () {
                renderQuestionPreview(questionEl);
            });
        }

        var sMin = questionEl.querySelector(".scale-min-value"); if (sMin) sMin.addEventListener("input", function () { renderQuestionPreview(questionEl); });
        var sMax = questionEl.querySelector(".scale-max-value"); if (sMax) sMax.addEventListener("input", function () { renderQuestionPreview(questionEl); });
        var sMinL = questionEl.querySelector(".scale-min-label"); if (sMinL) sMinL.addEventListener("input", function () { renderQuestionPreview(questionEl); });
        var sMaxL = questionEl.querySelector(".scale-max-label"); if (sMaxL) sMaxL.addEventListener("input", function () { renderQuestionPreview(questionEl); });
        var ratingScale = questionEl.querySelector(".rating-scale-value"); if (ratingScale) ratingScale.addEventListener("change", function () { renderQuestionPreview(questionEl); });

        var inputs = questionEl.querySelectorAll(".option-text, .grid-row-text, .grid-column-text");
        if (inputs && inputs.length) {
            inputs.forEach(function (input) {
                input.addEventListener("input", function () {
                    renderQuestionPreview(questionEl);
                });
            });
        }

        var btnToggle = questionEl.querySelector(".btn-toggle-collapse");
        if (btnToggle) {
            btnToggle.addEventListener("click", function () {
                var isCollapsed = questionEl.classList.toggle('collapsed');
                var btn = questionEl.querySelector('.btn-toggle-collapse');
                if (btn) {
                    btn.textContent = isCollapsed ? 'Expand' : 'Collapse';
                    btn.setAttribute('aria-expanded', String(!isCollapsed));
                }
            });
        }

        // Preview button: render and toggle preview surface
        var btnPreview = questionEl.querySelector('.btn-preview');
        if (btnPreview) {
            btnPreview.addEventListener('click', function () {
                var preview = questionEl.querySelector('.question-preview');
                if (preview) {
                    // re-render preview before toggling
                    renderQuestionPreview(questionEl);
                    preview.classList.toggle('d-none');
                } else {
                    renderQuestionPreview(questionEl);
                }
            });
        }

        var btnAddOpt = questionEl.querySelector(".btn-add-option");
        if (btnAddOpt) {
            btnAddOpt.addEventListener("click", function () {
                addOption(questionEl.querySelector(".option-list"));
                reindexAll();
                renderQuestionPreview(questionEl);
            });
        }

        var btnAddGridRow = questionEl.querySelector(".btn-add-grid-row");
        if (btnAddGridRow) {
            btnAddGridRow.addEventListener("click", function () {
                addGridRow(questionEl.querySelector(".grid-row-list"));
                reindexAll();
            });
        }

        var btnAddGridCol = questionEl.querySelector(".btn-add-grid-column");
        if (btnAddGridCol) {
            btnAddGridCol.addEventListener("click", function () {
                addGridColumn(questionEl.querySelector(".grid-column-list"));
                reindexAll();
            });
        }

        questionEl.addEventListener("click", function (event) {
            if (event.target.classList.contains("btn-remove-option")) {
                event.target.closest(".option-item").remove();
                reindexAll();
            }

            if (event.target.classList.contains("btn-remove-grid-row")) {
                event.target.closest(".grid-row-item").remove();
                reindexAll();
            }

            if (event.target.classList.contains("btn-remove-grid-column")) {
                event.target.closest(".grid-column-item").remove();
                reindexAll();
            }
        });

        updateQuestionVisibility(questionEl);
    }

    function setName(element, name, value) {
        if (!element) {
            return;
        }

        if (name === null) {
            element.removeAttribute('name');
        } else {
            element.name = name;
        }
        if (typeof value !== "undefined") {
            element.value = value;
        }
    }

    function reindexAll() {
        var container = byId("questionContainer");
        if (!container) {
            return;
        }

        var questionItems = container.querySelectorAll(".question-item");
        questionItems.forEach(function (questionEl, questionIndex) {
            questionEl.querySelector(".question-title").textContent = "Question " + (questionIndex + 1);

            setName(questionEl.querySelector(".question-id"), "Questions[" + questionIndex + "].Id");
            setName(questionEl.querySelector(".question-order"), "Questions[" + questionIndex + "].DisplayOrder", questionIndex + 1);
            setName(questionEl.querySelector(".question-text"), "Questions[" + questionIndex + "].QuestionText");
            setName(questionEl.querySelector(".question-type"), "Questions[" + questionIndex + "].QuestionType");
            setName(questionEl.querySelector(".question-required"), "Questions[" + questionIndex + "].IsRequired");
            setName(questionEl.querySelector(".conditional-parent-question"), "Questions[" + questionIndex + "].ConditionalParentQuestionIndex");
            setName(questionEl.querySelector(".conditional-parent-option"), "Questions[" + questionIndex + "].ConditionalParentOptionText");

            var optionItems = questionEl.querySelectorAll(".option-item");
            optionItems.forEach(function (optionEl, optionIndex) {
                setName(optionEl.querySelector(".option-id"), "Questions[" + questionIndex + "].Options[" + optionIndex + "].Id");
                setName(optionEl.querySelector(".option-order"), "Questions[" + questionIndex + "].Options[" + optionIndex + "].DisplayOrder", optionIndex + 1);
                setName(optionEl.querySelector(".option-text"), "Questions[" + questionIndex + "].Options[" + optionIndex + "].OptionText");
            });

            var gridRows = questionEl.querySelectorAll(".grid-row-item");
            gridRows.forEach(function (rowEl, rowIndex) {
                setName(rowEl.querySelector(".grid-row-id"), "Questions[" + questionIndex + "].GridRows[" + rowIndex + "].Id");
                setName(rowEl.querySelector(".grid-row-order"), "Questions[" + questionIndex + "].GridRows[" + rowIndex + "].DisplayOrder", rowIndex + 1);
                setName(rowEl.querySelector(".grid-row-text"), "Questions[" + questionIndex + "].GridRows[" + rowIndex + "].Text");
            });

            var gridColumns = questionEl.querySelectorAll(".grid-column-item");
            gridColumns.forEach(function (columnEl, columnIndex) {
                setName(columnEl.querySelector(".grid-column-id"), "Questions[" + questionIndex + "].GridColumns[" + columnIndex + "].Id");
                setName(columnEl.querySelector(".grid-column-order"), "Questions[" + questionIndex + "].GridColumns[" + columnIndex + "].DisplayOrder", columnIndex + 1);
                setName(columnEl.querySelector(".grid-column-text"), "Questions[" + questionIndex + "].GridColumns[" + columnIndex + "].Text");
            });

            setName(questionEl.querySelector(".scale-min-value"), "Questions[" + questionIndex + "].MinValue");
            setName(questionEl.querySelector(".scale-max-value"), "Questions[" + questionIndex + "].MaxValue");
            setName(questionEl.querySelector(".scale-min-label"), "Questions[" + questionIndex + "].MinLabel");
            setName(questionEl.querySelector(".scale-max-label"), "Questions[" + questionIndex + "].MaxLabel");
            var isRanking = normalizeQuestionType(questionEl.querySelector(".question-type").value).toLowerCase() === "ranking";
            setName(questionEl.querySelector(".rating-scale-value"), isRanking ? "Questions[" + questionIndex + "].MaxValue" : null);
            if (isRanking) {
                setName(questionEl.querySelector(".scale-min-value"), null, 1);
                setName(questionEl.querySelector(".scale-max-value"), null);
                setName(questionEl.querySelector(".scale-min-label"), null);
                setName(questionEl.querySelector(".scale-max-label"), null);
            }
        });

        container.querySelectorAll('.question-item').forEach(function (questionEl) {
            populateConditionalParentOptions(questionEl);
        });
    }

    function populateConditionalParentOptions(questionEl) {
        var select = questionEl.querySelector('.conditional-parent-question');
        if (!select) {
            return;
        }

        var container = byId('questionContainer');
        if (!container) {
            return;
        }

        var currentQuestionItem = questionEl;
        var questionItems = Array.prototype.slice.call(container.querySelectorAll('.question-item'));
        var currentIndex = questionItems.indexOf(currentQuestionItem);

        var selectedValue = select.value || select.getAttribute('data-selected-value');

        select.innerHTML = '<option value="">-- Select parent question --</option>';
        questionItems.forEach(function (item, index) {
            if (index === currentIndex) {
                return;
            }

            var text = (item.querySelector('.question-text').value || 'Untitled question').trim();
            var opt = document.createElement('option');
            opt.value = index;
            opt.textContent = (index + 1) + '. ' + text;
            select.appendChild(opt);
        });

        if (selectedValue !== null && typeof selectedValue !== 'undefined' && selectedValue !== '') {
            select.value = selectedValue;
        }
    }

    function updateAllParentSelectLabels() {
        var container = byId('questionContainer');
        if (!container) {
            return;
        }

        Array.prototype.slice.call(container.querySelectorAll('.question-item')).forEach(function (questionEl) {
            populateConditionalParentOptions(questionEl);
        });
    }

    function addQuestion() {
        var container = byId("questionContainer");
        var questionEl = cloneTemplate("questionTemplate");
        if (!container || !questionEl) {
            return;
        }

        container.appendChild(questionEl);
        wireQuestionEvents(questionEl);
        reindexAll();
        updateAllParentSelectLabels();
    }

    function init() {
        var form = byId("surveyFormBuilderForm");
        var container = byId("questionContainer");

        if (!form || !container) {
            return;
        }

        container.querySelectorAll(".question-item").forEach(function (questionEl) {
            var selectedValue = questionEl.querySelector('.conditional-parent-question') && questionEl.querySelector('.conditional-parent-question').value;
            if (selectedValue !== null && typeof selectedValue !== 'undefined' && selectedValue !== '') {
                questionEl.querySelector('.conditional-parent-question').setAttribute('data-selected-value', selectedValue);
            }
            populateConditionalParentOptions(questionEl);
            wireQuestionEvents(questionEl);
        });

        // Ensure option lists are present for any option-based questions on initial load
        container.querySelectorAll(".question-item").forEach(function (questionEl) {
            var type = questionEl.querySelector('.question-type').value;
            var listEl = questionEl.querySelector('.option-list');
            if (isOptionType(type) && listEl && listEl.children.length === 0) {
                addOption(listEl);
                addOption(listEl);
                // Make sure visibility is correct and indexes update
                updateQuestionVisibility(questionEl);
                reindexAll();
            }
        });

        var addBtn = byId("btnAddQuestion");
        if (addBtn) {
            addBtn.addEventListener("click", addQuestion);
        }

        var floatingBtn = document.createElement('button');
        floatingBtn.type = 'button';
        floatingBtn.className = 'btn btn-success btn-lg floating-add-question';
        floatingBtn.textContent = '+ Add Question';
        floatingBtn.addEventListener('click', addQuestion);

        var floatWrap = document.createElement('div');
        floatWrap.className = 'floating-action-bar';
        floatWrap.appendChild(floatingBtn);
        document.body.appendChild(floatWrap);

        var bottomButton = document.createElement('div');
        bottomButton.className = 'bottom-add-question';
        bottomButton.innerHTML = '<button type="button" class="btn btn-success btn-sm">+ Add Question</button>';
        bottomButton.querySelector('button').addEventListener('click', addQuestion);
        container.parentNode.insertBefore(bottomButton, null);

        form.addEventListener("submit", function () {
            reindexAll();
        });

        reindexAll();
        updateAllParentSelectLabels();
    }

    document.addEventListener("DOMContentLoaded", init);
})();
