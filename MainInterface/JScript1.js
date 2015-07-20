//function click_minutes_small() {
//	$.ajax({
//		type:"POST",
//		url:"/backend.php",
//		data:{click_minutes_small:1},
//		success:function(e){
//			$("#click_minutes_small").html(
//				1==e.search("click_minutes_small 1")
//					?'<img src="/images/2min.png"/>'
//					: 2==e.search("click_minutes_small 2")
//						?'<img src="/images/3min.png"/>'
//						: 3==e.search("click_minutes_small 3")
//							?'<img src="/images/4min.png"/>'
//							: 4==e.search("click_minutes_small 4")
//								?'<img src="/images/5min.png"/>'
//								: 5==e.search("click_minutes_small 5")
//									?'<img src="/images/1min.png"/>'
//									:'<img src="/images/time_added.png"/>')},
//		error:function(){
//			$("#click_minutes_small").html("Sorry, there was an Error.")}
//	})
//}

//function flick_minutes() { $.ajax({ type: "POST", url: "/backend.php", data: { click_minutes_small: 1} }); }