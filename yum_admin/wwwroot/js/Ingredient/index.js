// got token
var token_url = '/ingredients/getToken';
var HTTPToken = null;
fetch(token_url)
	.then(response => response.json())
	.then(data => {
		HTTPToken = data.token;
	})

//----    預設function  ----------------
// 視窗關閉、表單重設
function downPopUp(jq_popup) {
	jq_popup.parent().hide()
}
function AllReset(jq_input) {
	jq_input.find('input').val('')
	jq_input.find('select').val('0')
}
function AllEmpty(jq_div) {
	jq_div.empty();
}
// 生成新標籤放到彈出視窗
// 參數1.JQ物件，要放在哪個div
// 參數2.要放的物件資料來源
function PutDataTo_cg_PopUp(jq_insertDiv, ele) {
	var id = $('<input type="textbox">').val($(ele).val()).css('display', 'none')
	var name = $('<span class="me-2">').append($(ele).parent().next().next().text().trim())
	var attr = $('<span>').append($(ele).parent().next().next().next().text().trim())
	var cgIngredient = $('<p>').append($(id)).append($(name)).append($(attr))
	jq_insertDiv.append($(cgIngredient))
}
//--------------------------------------


// 勾選起來~~~
$('#left-site,#insert-site').on('click', 'tr', function (e) { // 第二個參數是觸發元素的子選擇器，綁定在這個子元素身上
	if ($(e.target).is('button') || $(e.target).is('a')) {
		// 如果點擊的是按鈕，直接返回，不做其他操作
		return;
	}
	var boxcheck = $(this).find('input[type="checkbox"]').prop('checked')
	$(this).find('input[type="checkbox"]').prop('checked', !boxcheck)
})



// 把食材換成原本有的
$('#cg-btn').click(function () {

	// 抓取被選取的食材資料
	$('.left-tr').find('input[type="checkbox"]:checked').each(function (idx, ele) {
		PutDataTo_cg_PopUp($('#cg-popup-insert'),ele)
	})

	// 目標只能1種，大於1的要擋掉
	if ($('.right-tr').find('input[type="checkbox"]:checked').length > 1) {
		alert(`結果食材最多一種。`);
		AllEmpty($('#cg-popup-insert,#cg-popup-toFood'))
		return;
	}
	// 目標只能1種，小於1的要警告
	if ($('.right-tr').find('input[type="checkbox"]:checked').length < 1) {
		alert(`並未選擇結果食材。`);
		AllEmpty($('#cg-popup-insert,#cg-popup-toFood'))
		return;
	}
	// 抓取要替換成的食材資料
	$('.right-tr').find('input[type="checkbox"]:checked').each(function (idx, ele) {
		PutDataTo_cg_PopUp($('#cg-popup-toFood'), ele)
	})

	$('#cg-popup').show()
})
$('#cg-popup-close').click(function () {
	downPopUp($(this))
	AllEmpty($('#cg-popup-insert,#cg-popup-toFood'))
})
$('#cg-popup-confirm').click(function () {
	var originFood = [];
	var afterFood = 0;
	$('#cg-popup-insert').find('input').each(function (idx, ele) {
		originFood.push($(ele).val())
	})
	afterFood = $('#cg-popup-toFood').find('input').val()

	console.log(originFood)
	console.log(afterFood)
	// 使用 AJAX 發送請求時附加防護令牌
	$.ajax({
		url: '/ingredients/Replace/',
		method: 'POST',
		contentType: 'application/json',
		headers: { 'RequestVerificationToken': HTTPToken }, // 添加防護令牌
		data: JSON.stringify({ originFood: originFood, afterFood: afterFood }),
		success: function (response) {
			alert(`替換成功：${response.message}`);
			//window.location.href = response.redirectUrl;
		},
		error: function (xhr) {
			alert(`替換成功：, ${xhr.responseJSON.message}`)
		}
	})


	downPopUp($(this))
	AllEmpty($('#cg-popup-insert,#cg-popup-toFood'))
})




// 把新的食材存取下來！
$('.left-tr').find('button').click(function () {
	// 重設
	AllReset($('#store-popup'))

	// 抓ID
	$('#store-popup').find('input[type="text"]').val($(this).prop('id').split('-')[0])
	$('#store-popup').show()
})
$('#store-popup-close').click(function () {
	downPopUp($(this))
	AllReset($('#store-popup'))
})
$('#store-popup-confirm').click(function () {

	// ajax

	downPopUp($(this))
	AllReset($('#store-popup'))
})



// 把該食材刪除！
$('#del-btn').click(function () {

	// 抓取被選取的食材資料
	$('.left-tr').find('input[type="checkbox"]:checked').each(function (idx, ele) {
		var id = $('<input>').css('display', 'none').val($(ele).val())
		var name = $('<span class="me-2">').append($(ele).parent().next().next().text().trim())
		var attr = $('<span>').append('其他')
		var cgIngredient = $('<p>').append($(id)).append($(name)).append($(attr))
		$('#del-popup-insert').append($(cgIngredient))
		// console.log($('#del-popup-insert').find('input').val())
	})
	$('#del-popup').show()
})
$('#del-popup-close').click(function () {
	downPopUp($(this))
	AllEmpty($('#del-popup-insert'))
})
$('#del-popup-confirm').click(function () {
	var delId = []
	$('#del-popup-insert').find('input').each(function (idx, ele) {
		delId.push($(ele).val())
	})

	// 使用 AJAX 發送請求時附加防護令牌
	$.ajax({
		url: '/ingredients/delete/',
		method: 'POST',
		contentType: 'application/json',
		headers: { 'RequestVerificationToken': HTTPToken }, // 添加防護令牌
		data: JSON.stringify(delId),
		success: function (response) {
			alert(`刪除成功：${response.message}`);
			window.location.href = response.redirectUrl;
		},
		error: function (xhr) {
			alert(`刪除失敗：, ${xhr.responseJSON.message}`)
		}
	})

	downPopUp($(this))
	AllEmpty($('#del-popup-insert'))
})




// 檢查是否是一樣的篩選
var Newmatchattr = '';
var Newmatchname = '';
// 右側篩選
$('#food-filter').click(function (e) {
	e.preventDefault();
	e.stopPropagation();

	var matchattr = $('#match-attr').val()
	var matchname = $('#match-name').val()
	if (matchattr == Newmatchattr && matchname == Newmatchname) {
		return;
	}
		// 使用 AJAX 發送請求時附加防護令牌
		$.ajax({
			url: '/ingredients/FoodResult/',
			method: 'POST',
			contentType: 'application/json',
			headers: { 'RequestVerificationToken': HTTPToken }, // 添加防護令牌
			data: JSON.stringify({ attrId: matchattr, name: matchname }),
			success: function (response) {
				//console.log(`查詢成功`, response);
				$('#insert-site').html(response)
				Newmatchattr = matchattr;
				Newmatchname = matchname;
			},
			error: function (xhr) {
				console.log(`查詢失敗：`, xhr.responseJSON.message);
			}
		});		
		
})


