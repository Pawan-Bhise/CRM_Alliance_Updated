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
            || value === "Ranking"
            || value === "NPS (0-10)"
            || value === "Date & Time Picker"
            || value.toLowerCase() === "ranking"
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

    function updateQuestionVisibility(questionEl) {
        var type = normalizeQuestionType(questionEl.querySelector(".question-type").value);
        var optionWrapper = questionEl.querySelector(".option-wrapper");
        var linearWrapper = questionEl.querySelector(".linear-wrapper");
        var gridWrapper = questionEl.querySelector(".grid-wrapper");

        if (optionWrapper) {
            optionWrapper.style.display = isOptionType(type) ? "block" : "none";
        }

        if (linearWrapper) {
            linearWrapper.style.display = (type === "Linear Scale" || type === "NPS (0-10)" || type.toLowerCase() === "linear scale" || type.toLowerCase() === "nps (0-10)") ? "flex" : "none";
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
        questionEl.querySelector(".btn-remove-question").addEventListener("click", function () {
            questionEl.remove();
            reindexAll();
        });

        questionEl.querySelector(".btn-move-up").addEventListener("click", function () {
            var prev = questionEl.previousElementSibling;
            if (prev) {
                questionEl.parentNode.insertBefore(questionEl, prev);
                reindexAll();
            }
        });

        questionEl.querySelector(".btn-move-down").addEventListener("click", function () {
            var next = questionEl.nextElementSibling;
            if (next) {
                questionEl.parentNode.insertBefore(next, questionEl);
                reindexAll();
            }
        });

        questionEl.querySelector(".question-type").addEventListener("change", function () {
            updateQuestionVisibility(questionEl);
            reindexAll();
        });

        questionEl.querySelector(".btn-add-option").addEventListener("click", function () {
            addOption(questionEl.querySelector(".option-list"));
            reindexAll();
        });

        questionEl.querySelector(".btn-add-grid-row").addEventListener("click", function () {
            addGridRow(questionEl.querySelector(".grid-row-list"));
            reindexAll();
        });

        questionEl.querySelector(".btn-add-grid-column").addEventListener("click", function () {
            addGridColumn(questionEl.querySelector(".grid-column-list"));
            reindexAll();
        });

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

        element.name = name;
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
    }

    function init() {
        var form = byId("surveyFormBuilderForm");
        var container = byId("questionContainer");

        if (!form || !container) {
            return;
        }

        container.querySelectorAll(".question-item").forEach(function (questionEl) {
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

        form.addEventListener("submit", function () {
            reindexAll();
        });

        reindexAll();
    }

    document.addEventListener("DOMContentLoaded", init);
})();
