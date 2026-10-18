// Initialize interactive Flatpickr Date Picker with Month & Year Dropdown navigation
document.addEventListener("DOMContentLoaded", function () {
    initFlatpickrDatepickers();
});

function initFlatpickrDatepickers() {
    if (typeof flatpickr !== "undefined") {
        flatpickr("input[type='date'], .datepicker", {
            dateFormat: "Y-m-d",
            altInput: true,
            altFormat: "d/m/Y",
            allowInput: true,
            animate: true,
            monthSelectorType: "dropdown",
            onReady: function (selectedDates, dateStr, instance) {
                initYearSelectDropdown(instance);
            },
            onMonthChange: function (selectedDates, dateStr, instance) {
                updateYearSelectValue(instance);
            },
            onYearChange: function (selectedDates, dateStr, instance) {
                updateYearSelectValue(instance);
            },
            onOpen: function (selectedDates, dateStr, instance) {
                updateYearSelectValue(instance);
            }
        });
    }
}

function initYearSelectDropdown(instance) {
    var container = instance.monthElements && instance.monthElements[0] ? instance.monthElements[0].parentNode : null;
    if (!container) return;

    var numInput = instance.currentYearElement;
    if (!numInput || container.querySelector('.flatpickr-year-select')) return;

    var select = document.createElement('select');
    select.className = 'flatpickr-year-select form-select form-select-sm d-inline-block w-auto py-0 px-2 ms-1 border-primary fw-bold';
    select.style.cursor = 'pointer';
    select.style.fontSize = '0.875rem';

    var currentYear = new Date().getFullYear();
    var startYear = currentYear - 30;
    var endYear = currentYear + 30;

    for (var y = startYear; y <= endYear; y++) {
        var opt = document.createElement('option');
        opt.value = y;
        opt.textContent = y;
        if (y === instance.currentYear) {
            opt.selected = true;
        }
        select.appendChild(opt);
    }

    select.addEventListener('change', function () {
        var selectedYr = parseInt(this.value, 10);
        instance.changeYear(selectedYr);
    });

    instance.customYearSelect = select;
    numInput.style.display = 'none';
    if (numInput.nextSibling) {
        numInput.parentNode.insertBefore(select, numInput.nextSibling);
    } else {
        numInput.parentNode.appendChild(select);
    }
}

function updateYearSelectValue(instance) {
    if (instance && instance.customYearSelect) {
        instance.customYearSelect.value = instance.currentYear;
    }
}
