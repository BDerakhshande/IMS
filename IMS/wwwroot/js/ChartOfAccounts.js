$(document).ready(function () {
    const rowsPerPage = 10;  // تعداد ردیف در هر صفحه
    let currentPage = 1;
    let filteredRows = [];
    const $tableBody = $('#accountsTableBody');
    const $rows = $tableBody.find('tr').toArray();

    function renderTable(page, rows) {
        $tableBody.empty();

        const start = (page - 1) * rowsPerPage;
        const end = start + rowsPerPage;
        const rowsToShow = rows.slice(start, end);

        rowsToShow.forEach(row => $tableBody.append(row));

        $('#rowInfo').text(`نمایش ردیف‌های ${start + 1} تا ${Math.min(end, rows.length)} از ${rows.length}`);

        $('#prevBtn').prop('disabled', page === 1);
        $('#nextBtn').prop('disabled', page * rowsPerPage >= rows.length);

        renderPagination(rows.length, page);
    }

    function renderPagination(totalRows, currentPage) {
        const totalPages = Math.ceil(totalRows / rowsPerPage);
        const $pagination = $('#pagination');
        $pagination.empty();

        for (let i = 1; i <= totalPages; i++) {
            const activeClass = i === currentPage ? 'active' : '';
            const pageItem = `<li class="page-item ${activeClass}"><a class="page-link" href="#">${i}</a></li>`;
            $pagination.append(pageItem);
        }

        $('.page-item').click(function (e) {
            e.preventDefault();
            const pageNum = Number($(this).text());
            if (pageNum !== currentPage) {
                currentPage = pageNum;
                renderTable(currentPage, filteredRows);
            }
        });
    }

    function filterRows(query) {
        query = query.trim().toLowerCase();
        if (!query) {
            filteredRows = $rows;
        } else {
            filteredRows = $rows.filter(row => {
                const name = $(row).find('td').eq(1).text().toLowerCase();
                const code = $(row).find('td').eq(2).text().toLowerCase();
                return name.includes(query) || code.includes(query);
            });
        }
    }

    $('#searchInput').on('input', function () {
        currentPage = 1;
        filterRows($(this).val());
        renderTable(currentPage, filteredRows);
    });

    $('#prevBtn').click(function () {
        if (currentPage > 1) {
            currentPage--;
            renderTable(currentPage, filteredRows);
        }
    });

    $('#nextBtn').click(function () {
        const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
        if (currentPage < totalPages) {
            currentPage++;
            renderTable(currentPage, filteredRows);
        }
    });

    // مقداردهی اولیه
    filteredRows = $rows;
    renderTable(currentPage, filteredRows);
});