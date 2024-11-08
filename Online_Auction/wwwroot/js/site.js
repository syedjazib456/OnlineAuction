$(document).ready(function () {
    var url = window.location.pathname;
    console.log(url);
    var navlink = $('.nav-link');

    navlink.each(function () {
        var path = $(this).attr("href");
        var navItem = $(this).closest('.nav-item');

        if (path === url) {
            navItem.addClass('active')
        }
        else {
            navItem.removeClass('active')
        }
    })
});


var category_form = $('#add-category');
var btn = $('#add-category-btn');

category_form.hide();
btn.on('click', function () {
    category_form.slideToggle();
})


$(document).ready(function () {
    function UpdateProductsStatus() {
        $.ajax({
            url: "/Home/CheckProductForSale",
            method: 'POST',
            success: function (data) {
                if (data) {
                    console.log("product update successfully")
                }
            }
        });
    }

    UpdateProductsStatus();

    setInterval(UpdateProductsStatus, 3600000);

    function getNotification() {
        $.ajax({
            url: "/Home/GetNotification",
            method: 'GET',
            dataType: 'json',
            success: function (data) {
                if (data && data.length > 0) {

                    $('#notification').empty();

                    data.forEach(function (notification) {
                        var notificationClass = notification.NotificationStatus === 'unread' ? 'unread-notification' : '';

                        var baseUrl = window.location.origin;
                        var profileUrl = baseUrl + '/Home/Profile?id=' + notification.Link;


                        var notificationElement = `<div class="notification-item-wrapper d-flex align-items-center">
                                <a data-notification-id="${notification.NotificationId}" class="notification-item ${notificationClass}" href="${profileUrl}" style="border-radius: 0px;">
                                    ${notification.NotificationMessage}
                                </a>
                                <button data-noti-id="${notification.NotificationId}" class="delete-notification-btn">X</button>
                            </div>`;

                        $('#notification').append(notificationElement);
                    });

                    $('.unread-notification').click(function (e) {
                        var notificationId = $(this).data('notification-id');
                        sendNotificationClickRequest(notificationId);
                    });

                }
                else {
                    $('#notification').empty();
                    var notificationElement = `<a class="notification-item border border-bottom" style="border-radius:0px;">No new notifications</a>`;

                    $('#notification').append(notificationElement);
                }
            }
        });
    }

    getNotification();

    $(document).on('click', '.delete-notification-btn', function () {
        console.log("btn clicked")
        var notiid = $(this).data('noti-id');

        $.ajax({
            url: "/Home/DeleteNotification",
            method: 'POST',
            data: { notiid: notiid },
            dataType: 'json',
            success: function (response) {
                console.log(response);
                if (response) {
                    window.location.reload();
                }
            },
            error: function (xhr, status, error) {
                console.error(xhr, status, error);
            }
        });
    });



    function sendNotificationClickRequest(notificationId) {
        $.ajax({
            url: "/Home/NotificationClick",
            method: 'POST',
            data: { id: notificationId },
            dataType: 'json',
            success: function (response) {
                console.log("Notification Readed")
            },
            error: function (xhr, status, error) {
                // Handle error response if needed
            }
        });
    }
});
