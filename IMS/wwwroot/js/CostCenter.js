$(document).ready(function () {
    $('tbody').on('click', 'tr.cost-center-row', function (e) {

        // جلوگیری از باز شدن مودال هنگام کلیک روی دکمه‌ها
        if ($(e.target).closest('button, form').length) return;

        var costCenterId = $(this).data('costcenter-id');

        $.ajax({
            url: '/AccountManagement/Definitions/GetSecondTafzilsByCostCenter',
            type: 'GET',
            data: { costCenterId: costCenterId },
            success: function (data) {
                var tableBody = $('#secondTafzilsTableBody');
                tableBody.empty();

                if (!data || data.length === 0) {
                    tableBody.append('<tr><td colspan="3" class="text-center">هیچ حساب تفصیل ۲ یافت نشد.</td></tr>');
                } else {
                    $.each(data, function (index, item) {
                        tableBody.append(`
                            <tr>
                                <td>${item.code}</td>
                                <td>${item.name}</td>
                                <td>${item.tafzil && item.tafzil.name ? item.tafzil.name : '-'}</td>
                            </tr>
                        `);
                    });
                }

                // باز کردن مودال
                var modal = new bootstrap.Modal(document.getElementById('secondTafzilsModal'));
                modal.show();
            },
            error: function () {
                alert('خطایی در بارگذاری حساب‌های تفصیل ۲ رخ داد.');
            }
        });
    });
});
