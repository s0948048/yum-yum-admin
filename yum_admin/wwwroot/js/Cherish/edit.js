console.log(123)

// ---------【Global】----------------
function downPopUp(jq_popup) {
    jq_popup.parent().hide()
}
function AllTextReset(jq_popup) {
    jq_popup.parent().find('input,textarea').val('')
}
function AllSelectReset(jq_popup) {
    jq_popup.parent().find('select').val('0')
}

// -----------------------------------


// 退回彈出視窗
$('#order-back').on('click', function () {
    $('#back-popup').show()
})
$('#back-popup-confirm').on('click', function () {
    console.log($('#back-popup').find('textarea').val())
    downPopUp($(this))
    AllTextReset($(this))
})
$('#back-popup-close').on('click', function () {
    downPopUp($(this))
    AllTextReset($(this))
})



// 已確認彈出視窗
$('#order-ok').on('click', function () {
    $('#ok-popup').show()
})
$('#ok-popup-confirm').on('click', function () {
    downPopUp($(this))
})
$('#ok-popup-close').on('click', function () {
    downPopUp($(this))
})


// 請他修改
$('#order-edit').on('click', function () {
    $('#edit-popup').show()
})
$('#edit-popup-confirm').on('click', function () {
    downPopUp($(this))
    AllSelectReset($(this))
})
$('#edit-popup-close').on('click', function () {
    downPopUp($(this))
    AllSelectReset($(this))
})