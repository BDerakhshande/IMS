$(document).ready(function () {
    $('#moeinTable').DataTable({
        paging: true,
        searching: false,         // حذف جستجو
        ordering: false,
        pageLength: 10,
        lengthChange: false,      // حذف dropdown تعداد ردیف
        language: {
            //url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/fa.json"
        }
    });

    $('#moeinTable tbody').on('mouseenter', 'tr', function () {
        $(this).css('background-color', '#e0f7fa');
    }).on('mouseleave', 'tr', function () {
        $(this).css('background-color', '');
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const table = document.getElementById("moeinTable");
    const rows = table.querySelectorAll("tbody tr");
    let selectedIndex = -1;

    function highlightRow(index) {
        rows.forEach((row, i) => {
            row.classList.toggle("selected", i === index);
        });
    }

    document.addEventListener("keydown", function (event) {
        const key = event.key;

        if (key === "ArrowDown" && selectedIndex < rows.length - 1) {
            selectedIndex++;
            highlightRow(selectedIndex);
            rows[selectedIndex].scrollIntoView({ block: "center", behavior: "smooth" });
            event.preventDefault();
        }

        if (key === "ArrowUp" && selectedIndex > 0) {
            selectedIndex--;
            highlightRow(selectedIndex);
            rows[selectedIndex].scrollIntoView({ block: "center", behavior: "smooth" });
            event.preventDefault();
        }

        if (key === "Enter" && selectedIndex >= 0) {
            const row = rows[selectedIndex];
            if (row && row.onclick) row.onclick();
        }

        if (key === "Escape") {
            history.back();
        }

        if (key === "Delete" && selectedIndex >= 0) {
            const form = rows[selectedIndex].querySelector("form");
            if (form && confirm("آیا از حذف این معین مطمئن هستید؟")) {
                form.submit();
            }
        }

        if ((event.shiftKey && key.toLowerCase() === "e") && selectedIndex >= 0) {
            const editBtn = rows[selectedIndex].querySelector(".edit-btn");
            if (editBtn) {
                editBtn.click();
                event.preventDefault();
            }
        }

        if (event.altKey && event.shiftKey && key.toLowerCase() === "n") {
            const addBtn = document.getElementById("addAccountBtn");
            if (addBtn) {
                addBtn.click();
                event.preventDefault();
            }
        }
    });
});