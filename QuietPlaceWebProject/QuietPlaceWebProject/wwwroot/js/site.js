// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function sendNotification(title, options) {
	// Проверка поддержки браузером уведомлений
	if (!("Notification" in window)) {
		alert("This browser does not support desktop notification");
	}

	// Проверка разрешения на отправку уведомлений
	else if (Notification.permission === "granted") {
		// Если разрешено, то создаём уведомление
		let notification = new Notification(title, options);
	}

	// В противном случае, запрашиваем разрешение
	else if (Notification.permission !== 'denied') {
		Notification.requestPermission(function (permission) {
			// Если пользователь разрешил, то создаём уведомление
			if (permission === "granted") {
				let notification = new Notification(title, options);
			}
		});
	}

	// В конечном счёте, если пользователь отказался от получения
	// уведомлений, то стоит уважать его выбор и не беспокоить его
	// по этому поводу.
}