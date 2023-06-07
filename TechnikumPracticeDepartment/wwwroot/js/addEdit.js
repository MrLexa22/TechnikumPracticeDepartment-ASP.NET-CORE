function checkSaveBTN() {
    document.getElementsByClassName('submit_btn')[0].style.display = 'none';
    let elements = document.getElementsByClassName('inputs_values');
    for (i = 0; i < elements.length; i++) {
        if (elements[i].style.display == 'grid') {
            document.getElementsByClassName('submit_btn')[0].style.display = 'block';
            return;
        }
    }
}
checkSaveBTN();

function edit(NameVal, boolea) {
    if (boolea) {
        document.getElementsByClassName('inputs_' + NameVal)[0].style.display = 'grid';
        document.getElementById('img_edit_' + NameVal).style.display = 'none';
        document.getElementById('img_cross_' + NameVal).style.display = 'block';
        document.getElementById('editBTN_' + NameVal).setAttribute('onclick', "edit('" + NameVal + "', false);");
    } else {
        document.getElementsByClassName('inputs_' + NameVal)[0].style.display = 'none';
        document.getElementById('img_edit_' + NameVal).style.display = 'block';
        document.getElementById('img_cross_' + NameVal).style.display = 'none';
        document.getElementById('editBTN_' + NameVal).setAttribute('onclick', "edit('" + NameVal + "', true);");

        document.getElementById('input_' + NameVal).value = document.getElementById('old_' + NameVal).textContent;
    }
    checkSaveBTN();
}

function editDatePicker(NameVal, boolea) {
    if (boolea) {
        document.getElementsByClassName('inputs_' + NameVal)[0].style.display = 'grid';
        document.getElementById('img_edit_' + NameVal).style.display = 'none';
        document.getElementById('img_cross_' + NameVal).style.display = 'block';
        document.getElementById('editBTN_' + NameVal).setAttribute('onclick', "editDatePicker('" + NameVal + "', false);");

        var date = document.getElementById('old_' + NameVal).textContent;
        if (date == null || date == "") {
            document.getElementById('input_' + NameVal).value = new Date();
        }
        else {
            var fields = date.split('.');
            var data = fields[0];
            var month = fields[1];
            var year = fields[2];
            document.getElementById('input_' + NameVal).value = year + "-" + month + "-" + data;
            //console.log(document.getElementById('input_' + NameVal).value);
        }
    } else {
        document.getElementsByClassName('inputs_' + NameVal)[0].style.display = 'none';
        document.getElementById('img_edit_' + NameVal).style.display = 'block';
        document.getElementById('img_cross_' + NameVal).style.display = 'none';
        document.getElementById('editBTN_' + NameVal).setAttribute('onclick', "editDatePicker('" + NameVal + "', true);");

        var date = document.getElementById('old_' + NameVal).textContent;
        if (date == null || date == "") {
            document.getElementById('input_' + NameVal).value = new Date();
        }
        else {
            var fields = date.split('.');
            var data = fields[0];
            var month = fields[1];
            var year = fields[2];
            document.getElementById('input_' + NameVal).value = year + "-" + month + "-" + data;
            //console.log(document.getElementById('input_' + NameVal).value);
        }
    }
    checkSaveBTN();
}

function search_group() {
    let input = document.getElementById('search-field_orgped').value;
    input = input.toLowerCase();
    let x = document.getElementsByClassName('search__element_orgped');

    for (i = 0; i < x.length; i++) {
        if (!x[i].innerHTML.toLowerCase().includes(input)) {
            if (!x[i].classList.contains('name_spec'))
                x[i].style.display = "none";
        } else {
            x[i].style.display = "flex";
        }
    }
}

function edit_Select(NameVal, boolea) {
    if (boolea) {
        document.getElementsByClassName('inputs_' + NameVal)[0].style.display = 'grid';
        document.getElementById('img_edit_' + NameVal).style.display = 'none';
        document.getElementById('img_cross_' + NameVal).style.display = 'block';
        document.getElementById('editBTN_' + NameVal).setAttribute('onclick', "edit_Select('" + NameVal + "', false);");
    } else {
        document.getElementsByClassName('inputs_' + NameVal)[0].style.display = 'none';
        document.getElementById('img_edit_' + NameVal).style.display = 'block';
        document.getElementById('img_cross_' + NameVal).style.display = 'none';
        document.getElementById('editBTN_' + NameVal).setAttribute('onclick', "edit_Select('" + NameVal + "', true);");

        var selectElement = $('#select_' + NameVal).eq(0);
        var selectize = selectElement.data('selectize');
        if (!!selectize) selectize.setValue(document.getElementById('old_' + NameVal).getAttribute('old_selected_' + NameVal));

        let element_li = document.querySelectorAll('ul.select_element_ul_' + NameVal + ' li');
        for (i = 0; i < element_li.length; i++) {
            element_li[i].className = "";
        }
    }
    checkSaveBTN();
}