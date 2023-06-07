const monthNames = ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
    "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
];
document.getElementById("date-day").innerHTML = String(new Date().getDate()).padStart(2, '0');
document.getElementById("date-mont").innerHTML = monthNames[new Date().getMonth()].toString();
document.getElementById("date-year").innerHTML = new Date().getFullYear();