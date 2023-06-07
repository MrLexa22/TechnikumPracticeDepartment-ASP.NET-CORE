const el1 = document.getElementsByClassName("modal_window");
const el2 = document.getElementsByClassName("details-modal");
const closeel = document.getElementsByClassName("close_modal_btn");
closeel[0].onclick = function() {
    $(".details-modal").fadeOut("slow");
    $(".modal_window").fadeOut("slow");
};

$(".modal_window").fadeIn("slow");
$(".details-modal").fadeIn("slow");